using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(PovController))]
public class PovControllerEditor : Editor
{
    public override async void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Start From This"))
        {
            var cameraGo = GameObject.FindWithTag("360Camera");
            
            if (null == cameraGo)
            {
                Debug.LogError("Cannot Find an 'active' GameObject with tag '360Camera'!");
                return;
            }

            var self = (PovController)target;
            var parentTransform = self.transform.parent.parent;

            // update camera position
            cameraGo.transform.position = self.transform.position;

            // make the pov become the starting point
            await parentTransform.GetComponent<IbrManager>().StartFrom(self);
            //parentTransform.SendMessage("StartFrom", self, SendMessageOptions.RequireReceiver);

            // apply changes
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
