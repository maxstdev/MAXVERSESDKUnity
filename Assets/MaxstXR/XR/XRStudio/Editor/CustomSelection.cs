using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class CustomSelection
{
    private const string MenuPath = "EditorTools/CustomSelection";

    // Change these keys to your liking - just make sure they don't conflict with something
    private const string MakeSelectableKey = "#&s";
    private const string MakeUnselectableKey = "#&u";

    private static bool enabled;

    /// <summary>
    /// Needed so that we could start automatically after an assembly reload
    /// </summary>
    static CustomSelection()
    {
        Start();
    }

    /// <summary>
    /// Whether or not we can start (used to [en, dis]able the Starting mechanism)
    /// </summary>
    [MenuItem(MenuPath + "/Start", true)]
    public static bool CanStart()
    {
        return !enabled; // we can only start if we're stopped
    }

    /// <summary>
    /// Starts the selection check
    /// </summary>
    [MenuItem(MenuPath + "/Start")]
    public static void Start()
    {
        if (!enabled) // Might be a redundant check, but just in case someone wants to start it from code
        {
            enabled = true;
            EditorApplication.update += Update;
        }
    }

    /// <summary>
    /// Whether or not we can stop (used to [en, dis]able the Stopping mechanism)
    /// </summary>
    [MenuItem(MenuPath + "/Stop", true)]
    public static bool CanStop()
    {
        return enabled; // we can only stop if we're started
    }

    /// <summary>
    /// Stops selection check
    /// </summary>
    [MenuItem(MenuPath + "/Stop")]
    public static void Stop()
    {
        if (enabled)
        {
            enabled = false;
            EditorApplication.update -= Update;
        }
    }

    /// <summary>
    /// Makes the current gameObjects selection unselectable by adding the Unselectable component to each gameObject in the selection
    /// </summary>
    [MenuItem(MenuPath + "/Make Selection Unselectable " + MakeUnselectableKey)]
    public static void MakeUnselectable()
    {
        foreach (var go in Selection.gameObjects)
        {
            // Add it only if it's not there
            if (go.GetComponent<Unselectable>() == null)
                go.AddComponent<Unselectable>();
        }
    }

    /// <summary>
    /// Whether or not we can make the selection selectable again
    /// </summary>
    [MenuItem(MenuPath + "/Make Selection Selectable " + MakeSelectableKey, true)]
    public static bool CanMakeSelectable()
    {
        return !enabled;
    }

    /// <summary>
    /// Makes the current gameObjects selection selectable again
    /// Note: Must call Stop first in order to select a selection that has the Unselectable component on it
    /// </summary>
    [MenuItem(MenuPath + "/Make Selection Selectable " + MakeSelectableKey)]
    public static void MakeSelectable()
    {
        Action<Component> destroy = Application.isPlaying ?
            (Action<Component>)Object.Destroy : Object.DestroyImmediate;

        foreach (var go in Selection.gameObjects)
        {
            destroy(go.GetComponent<Unselectable>());
        }
    }

    private static void Update()
    {
        var gos = Selection.gameObjects;
        if (gos != null && gos.Length > 0)
        {
            Selection.objects = gos.Where(g => g.GetComponent<Unselectable>() == null).ToArray();
        }
    }
}