using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.Linq;

[ScriptedImporter(1, "atm")]
public class AtmImporter : ScriptedImporter
{
    public AtnObject Atn;

    public float NearClipPlane = 0.01f, FarClipPlane = 100.0f;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var stream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var reader = new BinaryReader(stream, Encoding.ASCII);
        if (!stream.CanRead)
        {
            Debug.LogError("Failed to import " + ctx.assetPath);
            return;
        }

        int numPoints = reader.ReadInt32();
        if (0 != numPoints)
        {
            Debug.LogError("Failed to import " + ctx.assetPath);
            return;
        }

        var numCameras = reader.ReadInt32();
        
        var cameraPositions = ReadVector3AsVector4(reader, numCameras, 1.0f).ToArray();
        if (null != Atn && null != Atn.Indices)
            cameraPositions = Atn.Indices.Select(index => cameraPositions[index]).ToArray();
        
        var projectionMatrix = ReadProjectionMatrix(reader, NearClipPlane, FarClipPlane);

        var viewMatrices = ReadViewMatrix(reader, numCameras).ToArray();
        if (null != Atn && null != Atn.Indices)
            viewMatrices = Atn.Indices.Select(index => viewMatrices[index]).ToArray();

        var atmInstance = ScriptableObject.CreateInstance<AtmObject>();
        atmInstance.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
        atmInstance.ProjectionMatrix = projectionMatrix;
        atmInstance.ViewMatrices = viewMatrices;

        ctx.AddObjectToAsset(atmInstance.name, atmInstance);
        ctx.SetMainObject(atmInstance);
    }

    private IEnumerable<Vector4> ReadVector3AsVector4(BinaryReader reader, int count, float w)
    {
        for (var i = 0; i < count; ++i)
            yield return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), w);
    }

    private Matrix4x4 ReadProjectionMatrix(BinaryReader reader, float zNear, float zFar)
    {
        int w = reader.ReadInt32(), h = reader.ReadInt32();
        float fx = reader.ReadSingle(), fy = reader.ReadSingle();
        float px = reader.ReadSingle(), py = reader.ReadSingle();
        return new Matrix4x4()
        {
            m00 = 2.0f * fx / w,
            m11 = 2.0f * -fy / h,
            m02 = (2.0f * px - w) / w,
            m12 = (2.0f * (h - 1.0f - py) - h) / h,
            m22 = (zFar + zNear) / (zFar - zNear),
            m32 = 1.0f,
            m23 = -(2.0f * zFar * zNear) / (zFar - zNear)
        };
    }

    private IEnumerable<Matrix4x4> ReadViewMatrix(BinaryReader reader, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            var columns = ReadVector3AsVector4(reader, 4, 0.0f).ToArray();
            columns[3].w = 1; // affine
            yield return new Matrix4x4(columns[0], columns[1], columns[2], columns[3]);
        }
    }
}