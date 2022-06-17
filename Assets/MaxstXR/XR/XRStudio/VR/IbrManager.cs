using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
public partial class IbrManager : MonoBehaviour
{
    public enum PovType : uint
    {
        Primary = 0u, Secondary = 1u
    }

    #region Public Properties
    private TextureManager _textureManager;
    private TextureManager TextureManager {
        get
        {
            if (null == _textureManager)
                _textureManager = FindObjectOfType<TextureManager>(true);
            return _textureManager;
        }
    }

    #endregion

    #region Serialized Fields

    [SerializeField]
    private UnityEvent _onEnable;
    [SerializeField]
    private Material IbrCullBack, IbrCullFront;

    [SerializeField]
    private Color meshColor;

    [SerializeField]
    private Vector4 _frameData = Vector4.zero;
    
    [SerializeField]
    private Matrix4x4[] _frameViewMatrices = new Matrix4x4[2];
    
    [SerializeField]
    private Texture2D[] _frameTextures = new Texture2D[2];
    
    [SerializeField]
    private string[] _frameSpots = new string[2];
    
    [SerializeField]
    private string[] _frameNames = new string[2];

    [SerializeField]
    private Texture2D _frameTextureInEditor = null;

    #endregion

    public async Task StartFrom(IPov pov)
    {
        _frameData.x = 0.0f;
        _frameSpots[0] = pov.Spot;
        _frameNames[0] = pov.Name;
        _frameViewMatrices[0] = pov.WorldToLocalMatrix;
        _frameTextureInEditor = await TextureManager.LoadTexture(pov.Spot, pov.Name);
        IbrCullBack.SetVector("_runtimeData", new Vector4(1, 0, 0, 0));

        UpdateMaterialEditor(IbrCullBack);
        UpdateMaterialEditor(IbrCullFront);
    }

    private void UpdateMaterialEditor(Material m)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (null != _frameTextureInEditor)
            SetFrameTextureCurrent(m, _frameTextureInEditor);
    }


    public void SetPov(IPov pov, PovType povType) {
        var index = (uint) povType;
        _frameSpots[index] = pov.Spot;
        _frameNames[index] = pov.Name;
        _frameViewMatrices[index] = pov.WorldToLocalMatrix;
    }

    public async Task UntilReady(PovType povType)
    {
        var index = (uint)povType;

        TextureManager.ScheduleIfNotLoaded(_frameSpots[index], _frameNames[index]);
        _frameTextures[index] = await TextureManager.GetLoadedTexture(_frameSpots[index], _frameNames[index]);
    }

    #region Unity Messages

    protected virtual void Awake()
    {
        IbrCullBack.SetColor("_Color", new Color(0, 0, 0, 0));
        IbrCullBack.SetVector("_runtimeData", new Vector4(0, 0, 0, 0));
    }

    protected virtual async void OnEnable()
    {
        _frameData.x = 0.0f;

        await UntilReady(PovType.Primary);
        _onEnable.Invoke();
        UpdateMaterials();

    }

    #endregion

    public void HandleAnimationStarted()
    {
        _frameData.x = 0.0f;

        TextureManager.PreventToBeUnloaded(_frameSpots[0], _frameNames[0]);

        UpdateMaterials();
    }

    public void HandleAnimationUpdated(float t)
    {
        _frameData.x = Mathf.Clamp01(t);

        SetFrameData(IbrCullBack, _frameData);
        SetFrameData(IbrCullFront, _frameData);
    }

    public void HandleAnimationFinished()
    {
        _frameData.x = 1.0f;

        TextureManager.AllowToBeUnloaded(_frameSpots[0], _frameNames[0]);

        _frameSpots[0] = _frameSpots[1];
        _frameNames[0] = _frameNames[1];
        _frameViewMatrices[0] = _frameViewMatrices[1];
        _frameTextures[0] = _frameTextures[1];

        UpdateMaterials();
    }

    private void UpdateMaterial(Material m)
    {
        SetFrameData(m, _frameData);
        SetFrameViewMatrices(m, _frameViewMatrices);
        if (null != _frameTextures[0])
            SetFrameTextureCurrent(m, _frameTextures[0]);
        if (null != _frameTextures[1])
            SetFrameTextureNext(m, _frameTextures[1]);
    }

    private void UpdateMaterials()
    {
        UpdateMaterial(IbrCullBack);
        UpdateMaterial(IbrCullFront);
    }

    public static int kCurrTexId = Shader.PropertyToID("_MainTex");
    public static int kNextTexId = Shader.PropertyToID("_MainTex2");
    public static int kFrameDataId = Shader.PropertyToID("frame_Data");
    public static int kFrameViewMatricesId = Shader.PropertyToID("frame_MatrixV");

    public static void SetFrameData(Material material, Vector4 frameData)
    {
        material.SetVector(kFrameDataId, frameData);
    }

    public static void SetFrameViewMatrices(Material material, Matrix4x4[] frameViewMatrices)
    {
        material.SetMatrixArray(kFrameViewMatricesId, frameViewMatrices);
    }

    public static void SetFrameTextureCurrent(Material material, Texture current)
    {
        material.SetTexture(kCurrTexId, current);
    }

    public static void SetFrameTextureNext(Material material, Texture next)
    {
        material.SetTexture(kNextTexId, next);
    }
}
