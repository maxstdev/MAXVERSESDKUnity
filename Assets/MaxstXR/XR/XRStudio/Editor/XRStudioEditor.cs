using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(XRStudioController))]
public class XRStudioEditor : Editor
{
    public const string vpsPath = "/../../XRData/XRMap";
    public const string vpsSimulatePath = "/../../XRData/XRSimulationData/";
    public const string vpsServerName = "";

    private int simulate_selectIndex = 0;
    private int before_simulate_ChoiceIndex = -1;

    private int selectIndex = 0;
    private int beforeChoiceIndex = -1;

    public override void OnInspectorGUI()
    {
        XRStudioController xrStudioController = (XRStudioController)target;

        EditorGUILayout.LabelField("XR Map");
        string folderPath = Application.dataPath;
        folderPath = folderPath + vpsPath;
        string[] directories = Directory.GetDirectories(folderPath);

        List<string> directory_name = new List<string>();
        foreach (string directory in directories)
        {
            var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            directory_name.Add(name);
        }
        selectIndex = EditorGUILayout.Popup(xrStudioController.SelectIndex, directory_name.ToArray());

        GUILayout.Space(10);

        EditorGUILayout.LabelField("XR Simulation Data");
        string selectVPSName = directory_name[selectIndex];
        string[] simulate_directories = null;
        if (selectVPSName != "")
        {
            folderPath = Application.dataPath;
            folderPath = folderPath + vpsSimulatePath + selectVPSName;

            if(Directory.Exists(folderPath))
            {
                simulate_directories = Directory.GetDirectories(folderPath);
            }

            if(simulate_directories != null && simulate_directories.Length != 0)
            {
                List<string> simulate_directory_name = new List<string>();
                foreach (string directory in simulate_directories)
                {
                    var name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    simulate_directory_name.Add(name);
                }

                simulate_selectIndex = EditorGUILayout.Popup(xrStudioController.Simulate_SelectIndex, simulate_directory_name.ToArray());
            }
        }
        
        GUILayout.Space(10);

        EditorGUILayout.Separator();
        GUIContent makeContent = new GUIContent("Load XR Map");
        if (GUILayout.Button(makeContent, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(50)))
        {
            xrStudioController.LoadMap();
        }
        GUILayout.Space(10);

        GUIContent clearContent = new GUIContent("Clear");
        if (GUILayout.Button(clearContent, GUILayout.MaxWidth(Screen.width), GUILayout.MaxHeight(50)))
        {
            xrStudioController.Clear();
        }
        GUILayout.Space(10);


        DrawDefaultInspector();

        bool isDirty = false;
        if (selectIndex != beforeChoiceIndex || simulate_selectIndex != before_simulate_ChoiceIndex)
        {
            isDirty = true;
            beforeChoiceIndex = selectIndex;
            xrStudioController.SelectIndex = selectIndex;

            before_simulate_ChoiceIndex = simulate_selectIndex;
            xrStudioController.Simulate_SelectIndex = simulate_selectIndex;
        }

        if (GUI.changed || isDirty)
        {
            xrStudioController.xrPath = directories[selectIndex];

            if(simulate_directories != null)
            {
                if(simulate_directories.Length >= simulate_selectIndex-1)
                {
                    xrStudioController.xrSimulatePath = simulate_directories[simulate_selectIndex];
                }
                else
                {
                    xrStudioController.xrSimulatePath = simulate_directories[0];
                    simulate_selectIndex = 0;
                    before_simulate_ChoiceIndex = simulate_selectIndex;
                    xrStudioController.Simulate_SelectIndex = simulate_selectIndex;

                }
            }
            
            EditorUtility.SetDirty(target);
        }
    }
}
