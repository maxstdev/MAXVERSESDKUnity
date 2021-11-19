using UnityEngine;

public class AtnObject : ScriptableObject
{
    public string[] AllImageNames;
    public string[] FilteredImageNames;
    public string[] ImageFileNames;

    /// <summary>
    /// Filtered indices. It can be used for filtering .atm file.
    /// </summary>
    public int[] Indices;
}