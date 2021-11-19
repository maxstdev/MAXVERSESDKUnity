using UnityEngine;

using System.IO;
using System.Collections;

using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif


public class XcodeSettingsPostProcesser
{
    [PostProcessBuildAttribute (10)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS) return; 

        ProcessPlist(buildTarget, pathToBuiltProject);
        ProcessProj(buildTarget, pathToBuiltProject);
    }

    public static void ProcessPlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS
        string plistPath = pathToBuiltProject + "/Info.plist";

        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;
        //rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);  // IPlistEditor.cs
        
        // 위치관련 안내문구 추가
        rootDict.SetString("NSLocationWhenInUseUsageDescription", "Use for AR Navigation.");
        rootDict.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "Use for AR Navigation.");

        plist.WriteToFile(plistPath);
#endif
    }

    public static void ProcessProj(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS
        string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        proj.WriteToFile(projPath);
#endif
    }
}