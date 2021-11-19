using UnityEngine;
using UnityEngine.Events;
using KtxUnity;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class TextureManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UnityEvent<float> OnProgressUpdated;

    [SerializeField]
    private UnityEvent OnProgressCompleted;

    [SerializeField]
    private string TexturesDirectory;

    [SerializeField]
    private string TextureExtension = ".ktx2";

    #endregion

    #region Private Fields

    private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    private HashSet<string> _loadingQueue = new HashSet<string>();

    private HashSet<string> _unloadingQueue = new HashSet<string>();

    private HashSet<string> _loadingInProgress = new HashSet<string>();

    private HashSet<string> _preventedToBeUnloaded = new HashSet<string>();

    private uint _numStartedLoading = 0u;

    private uint _numFinishedLoading = 0u;

    #endregion

    #region Private Properties
    private Dictionary<string, Texture2D> Textures
    {
        get
        {
            if (null == _textures)
            {
                _textures = new Dictionary<string, Texture2D>();
            }
            return _textures;
        }
    }

    private HashSet<string> LoadingQueue
    {
        get
        {
            if (null == _loadingQueue)
            {
                _loadingQueue = new HashSet<string>();
            }
            return _loadingQueue;
        }
    }

    private HashSet<string> UnloadingQueue
    {
        get
        {
            if (null == _unloadingQueue)
            {
                _unloadingQueue = new HashSet<string>();
            }
            return _unloadingQueue;
        }
    }

    private HashSet<string> LoadingInProgress
    {
        get
        {
            if (null == _loadingInProgress)
            {
                _loadingInProgress = new HashSet<string>();
            }
            return _loadingInProgress;
        }
    }

    private HashSet<string> PreventedToBeUnloaded
    {
        get
        {
            if (null == _preventedToBeUnloaded)
            {
                _preventedToBeUnloaded = new HashSet<string>();
            }
            return _preventedToBeUnloaded;
        }
    }

    private uint NumStartedLoading
    {
        get => _numStartedLoading;
        set
        {
            if (value == _numStartedLoading) { return; }
            _numStartedLoading = value;

            // notify progress updated
            OnProgressUpdated.Invoke(Progress);
        }
    }

    private uint NumFinishedLoading
    {
        get => _numFinishedLoading;
        set
        {
            if (value == _numFinishedLoading) { return; }
            _numFinishedLoading = value;

            // notify progress updated
            OnProgressUpdated.Invoke(Progress);

            if (Progress < 1.0f) { return; }

            // notify progress completed
            OnProgressCompleted.Invoke();

            _numStartedLoading = 0u;
            _numFinishedLoading = 0u;
        }
    }

    private float Progress
    {
        get
        {
            return NumStartedLoading > 0u ? NumFinishedLoading / (float)NumStartedLoading : 1.0f;
        }
    }

    private bool HasScheduledLoading => LoadingQueue.Count > 0;

    private bool HasScheduledUnloading => _unloadingQueue.Count > 0;

    #endregion

    #region Unity Messages

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true; // it must be a trigger
        GetComponent<Rigidbody>().isKinematic = true; // it is not physical
    }

    async void Update()
    {
        // Perform loading only once at a time
        await ProcessScheduledLoading();
    }

    void FixedUpdate()
    {
        // Perform unload only if there are no more scheduled loadings
        if (HasScheduledLoading) { return; }

        ProcessScheduledUnloading();
    }

    void OnTriggerEnter(Collider other)
    {
        var texturePath = GetTexturePath(other);
        if (IsNotLoaded(texturePath))
        {
            ScheduleLoading(texturePath);
        }
        else if (WillBeUnloaded(texturePath))
        {
            UnscheduleUnloading(texturePath);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var texturePath = GetTexturePath(other);
        if (IsLoaded(texturePath))
        {
            ScheduleUnloading(texturePath);
        }
        else if (WillBeLoaded(texturePath))
        {
            UnscheduleLoading(texturePath);
        }
    }

    #endregion

    #region Public Methods

    public async Task<Texture2D> LoadTexture(string mapName, string povName, bool updateProgress = false)
    {
        var texturePath = Path.Combine(mapName, povName);
        var texture = await LoadTexture(texturePath, updateProgress);

        if(!Application.isEditor)
        {
            Textures.Add(texturePath, texture);
        }
        
        return texture;
    }

    public async Task UntilLoaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(mapName, povName);
        await UntilLoaded(texturePath);
    }

    public async Task<Texture2D> GetLoadedTexture(string mapName, string povName)
    {
        var texturePath = Path.Combine(mapName, povName);
        await UntilLoaded(texturePath);
        return Textures[texturePath];
    }

    public void ScheduleIfNotLoaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(mapName, povName);
        if (IsNotLoaded(texturePath)) { ScheduleLoading(texturePath); }
    }

    public void PreventToBeUnloaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(mapName, povName);
        PreventedToBeUnloaded.Add(texturePath);
    }

    public void AllowToBeUnloaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(mapName, povName);
        PreventedToBeUnloaded.Remove(texturePath);
    }

    #endregion

    #region Private Methods

    private bool IsLoaded(string texturePath)
    {
        return Textures.ContainsKey(texturePath);
    }

    private bool IsNotLoaded(string texturePath)
    {
        return !IsLoaded(texturePath);
    }

    private async Task UntilLoaded(string texturePath)
    {
        while (IsNotLoaded(texturePath)) { await Task.Yield(); }
    }

    private string GetTexturePath(Collider other)
    {
        var povController = other.transform.parent;
        var povManager = povController.parent;
        var texturePath = Path.Combine(povManager.name, povController.name);
        return texturePath;
    }

    private string Dequeue(HashSet<string> q)
    {
        var v = q.First();
        q.Remove(v);
        return v;
    }

    private bool WillBeUnloaded(string texturePath)
    {
        return UnloadingQueue.Contains(texturePath);
    }

    private bool WillBeLoaded(string texturePath)
    {
        return LoadingQueue.Contains(texturePath);
    }

    private void ScheduleLoading(string texturePath)
    {
        if (LoadingQueue.Contains(texturePath)) { return; }

        if (LoadingInProgress.Contains(texturePath)) { return; }
        
        LoadingQueue.Add(texturePath);
    }
    
    private void UnscheduleLoading(string texturePath)
    {
        LoadingQueue.Remove(texturePath);
    }

    private void ScheduleUnloading(string texturePath)
    {
        UnloadingQueue.Add(texturePath);
    }

    private void UnscheduleUnloading(string texturePath)
    {
        UnloadingQueue.Remove(texturePath);
    }

    private async Task<Texture2D> LoadTexture(string texturePath, bool updateProgress = true)
    {
        texturePath = Path.Combine(TexturesDirectory, texturePath);
        texturePath = Path.ChangeExtension(texturePath, TextureExtension);
        if (updateProgress) { NumStartedLoading += 1u; }
        var textureResult = await new KtxTexture().LoadFromUrl(texturePath, true);
        if (updateProgress) { NumFinishedLoading += 1u; }
        return textureResult.texture;
    }

    private async Task ProcessScheduledLoading()
    {
        var texturePath = string.Empty;
        var isLoaded = true;

        while (LoadingQueue.Count > 0 && isLoaded)
        {
            texturePath = Dequeue(LoadingQueue);
            isLoaded = IsLoaded(texturePath);
        }

        if (isLoaded) { return; }

        LoadingInProgress.Add(texturePath);

        var texture = await LoadTexture(texturePath);
        Textures.Add(texturePath, texture);
        
        LoadingInProgress.Remove(texturePath);
    }

    private void ProcessScheduledUnloading()
    {
        var texturePath = string.Empty;
        var isNotLoaded = true;

        var postponed = new HashSet<string>();
        while (HasScheduledUnloading&& isNotLoaded)
        {
            texturePath = Dequeue(UnloadingQueue);
            if (PreventedToBeUnloaded.Contains(texturePath))
            { 
                postponed.Add(texturePath);
            }
            isNotLoaded = IsNotLoaded(texturePath);
        }
        UnloadingQueue.UnionWith(postponed);

        if (isNotLoaded || postponed.Contains(texturePath)) { return; }

        // unload the texture
        Destroy(_textures[texturePath]);
        Textures.Remove(texturePath);
    }

    #endregion
}
