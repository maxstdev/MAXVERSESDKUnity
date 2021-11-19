using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.Linq;
using System.IO;

[CustomEditor(typeof(AtnImporter))]
public class AtnImporterEditor : ScriptedImporterEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var atnObject = (AtnObject)assetTarget;
        var inputPath = AssetDatabase.GetAssetPath(target);
        var inputDirectory = Path.GetDirectoryName(inputPath);
        var inputName = Path.GetFileNameWithoutExtension(inputPath);

        if (GUILayout.Button("Export Image Names"))
        {
            var format = "{0}-names.txt";
            var path = Path.Combine(inputDirectory, string.Format(format, inputName));
            File.WriteAllLines(path, atnObject.ImageFileNames);
            AssetDatabase.ImportAsset(path);
        }
    }
}
