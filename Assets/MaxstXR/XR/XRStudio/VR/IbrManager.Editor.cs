#if UNITY_EDITOR

using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public partial class IbrManager : MonoBehaviour
{
    [SerializeField]
    private Texture2D _frameTextureInEditor = null;

    public async Task StartFrom(IPov pov)
    {
        _frameData.x = 0.0f;
        _frameSpots[0] = pov.Spot;
        _frameNames[0] = pov.Name;
        _frameViewMatrices[0] = pov.WorldToLocalMatrix;
        _frameTextureInEditor = await TextureManager.LoadTexture(pov.Spot, pov.Name);
    }

    public void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));

            UpdateMaterialEditor(IbrCullBack);
            UpdateMaterialEditor(IbrCullFront);
        }
    }
    
    private void UpdateMaterialEditor(Material m)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (null != _frameTextureInEditor)
            SetFrameTextureCurrent(m, _frameTextureInEditor);
    }
}

#endif