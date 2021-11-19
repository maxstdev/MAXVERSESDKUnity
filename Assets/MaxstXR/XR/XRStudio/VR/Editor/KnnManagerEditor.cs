using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KnnManager))]
public class KnnManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var knnManager = (KnnManager)target;

        if (GUILayout.Button("Subsample"))
        {
            knnManager.Subsample();
        }
    }
}
