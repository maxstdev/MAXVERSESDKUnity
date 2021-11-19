using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

[ScriptedImporter(1, "atn")]
public class AtnImporter : ScriptedImporter
{
    public RegexFilteringRule[] FilteringRules = new[]
    {
        new RegexFilteringRule{Comment = "(1) 네 방향 중 하나만 남깁니다.", Pattern = "_0.png", Operation = RegexFilteringOperation.Include}
    };

    public RegexRenamingRule[] RenamingRules = new[]
    {
        new RegexRenamingRule{Comment = "(1) Insta360Stitcher가 붙이는 타임스탬프를 제거합니다.", Pattern = "(_[0-9]{2}){6}", Replacement = ""},
        new RegexRenamingRule{Comment = "(2) 확장자를 텍스처 파일에 맞도록 변경합니다.", Pattern = "_0.png", Replacement = ".ktx2"},
    };

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var stream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!stream.CanRead)
        {
            Debug.LogError("Failed to import " + ctx.assetPath);
            return;
        }

        var reader = new StreamReader(stream, Encoding.ASCII);
        if (!int.TryParse(reader.ReadLine(), out int numLines))
        {
            Debug.LogError("Failed to import " + ctx.assetPath);
            return;
        }

        var atnInstance = ScriptableObject.CreateInstance<AtnObject>();
        atnInstance.name = Path.GetFileNameWithoutExtension(ctx.assetPath);

        var allLines = ReadLines(reader, numLines).ToArray();
        atnInstance.AllImageNames = allLines;

        var filteredPairs = allLines.Select((name, index) => (name, index)).Where(pair => Filter(pair.name)).ToArray();
        atnInstance.FilteredImageNames = filteredPairs.Select(pair => pair.name).ToArray();

        atnInstance.ImageFileNames = atnInstance.FilteredImageNames.Select(Rename).ToArray();
        atnInstance.Indices = filteredPairs.Select(pair => pair.index).ToArray();

        ctx.AddObjectToAsset(atnInstance.name, atnInstance);
        ctx.SetMainObject(atnInstance);
    }

    private IEnumerable<string> ReadLines(StreamReader reader, int numLines)
    {
        for (var i = 0; i < numLines; ++i)
            yield return reader.ReadLine();
    }

    private bool Filter(string imageFileName)
    {
        if (null == FilteringRules) return true;

        const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
        return FilteringRules.Aggregate(true, (success, rule) =>
        {
            var match = Regex.Match(imageFileName, rule.Pattern, options);
            return success && (rule.Operation switch
            {
                RegexFilteringOperation.Any => true,
                RegexFilteringOperation.Exclude => !match.Success,
                RegexFilteringOperation.Include => match.Success,
                _ => true, // default
            });
        });
    }

    private string Rename(string imageFileName)
    {
        if (null == RenamingRules) return imageFileName;

        return RenamingRules.Aggregate(imageFileName, (name, rule) =>
        {
            return Regex.Replace(name, rule.Pattern, rule.Replacement);
        });
    }
}