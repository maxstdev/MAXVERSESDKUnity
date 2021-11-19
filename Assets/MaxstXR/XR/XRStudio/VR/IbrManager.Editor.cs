#if UNITY_EDITOR

using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public partial class IbrManager : MonoBehaviour
{
    public async Task StartFrom(IPov pov)
    {
        _frameData.x = 0.0f;
        _frameSpots[0] = pov.Spot;
        _frameNames[0] = pov.Name;
        _frameViewMatrices[0] = pov.WorldToLocalMatrix;
        _frameTextures[0] = await TextureManager.LoadTexture(pov.Spot, pov.Name);
    }

    public void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));

            UpdateMaterial(IbrCullBack);
            UpdateMaterial(IbrCullFront);
        }
    }
}

#endif