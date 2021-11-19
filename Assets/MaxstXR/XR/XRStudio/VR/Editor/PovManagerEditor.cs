using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PovManager))]
public class PovManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var povManager = (PovManager) target;
        var atm = povManager.Atm;
        var atn = povManager.Atn;

        if (atm.ViewMatrices.Length != atn.ImageFileNames.Length)
        {
            Debug.LogError("Atm and Atn are not compatible!");
            return;
        }

        if (GUILayout.Button("Create All POVs"))
        {
            Clear(povManager.gameObject);
            GameObject mapObject = new GameObject();
            mapObject.name = atn.name;
            mapObject.transform.parent = povManager.gameObject.transform;
            mapObject.transform.localPosition = Vector3.zero;
            mapObject.transform.localRotation = Quaternion.identity;
            mapObject.transform.localScale = Vector3.one;

            PovController povController = null;
            for (var i = 0; i < atn.ImageFileNames.Length; ++i)
            {
                var imageFileName = atn.ImageFileNames[i];
                var viewMatrix = atm.ViewMatrices[i];

                if (viewMatrix.ValidTRS())
                {
                    var o = PrefabUtility.InstantiatePrefab(povManager.PovPrefab);
                    var go = (GameObject)o;
                    go.name = Path.GetFileNameWithoutExtension(imageFileName);

                    var t = go.transform;
                    t.position = new Vector4(viewMatrix.m03, viewMatrix.m13, viewMatrix.m23, 1);
                    t.rotation = viewMatrix.rotation;
                    t.localScale = Vector3.one;
                    t.SetParent(mapObject.transform, false);
                    
                    go.GetComponent<PovController>().PlaceIndicator();

                    if(i == 0)
                    {
                        povController = go.GetComponent<PovController>();
                    }
                }
                else
                {
                    Debug.LogError("Failed to create POV: " + imageFileName);
                }
            }

            povManager.GetComponent<KnnManager>().Subsample();

            if(povController != null)
            {
                
            }
        }

        if (GUILayout.Button("Delete All POVs"))
        {
            Clear(povManager.gameObject);
        }

        if (GUILayout.Button("Place POV Indicators"))
        {
            var controllers = povManager.GetComponentsInChildren<PovController>();
            foreach(var controller in controllers)
                controller.PlaceIndicator();
        }
    }

    private void Clear(GameObject clearObject)
    {
        foreach (Transform child in clearObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        if (clearObject.transform.childCount > 0)
        {
            Clear(clearObject);
        }
    }
}
