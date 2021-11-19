/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.IO;


namespace maxstAR
{
	[RequireComponent(typeof(Camera))]
	public class ARManager : AbstractARManager
	{
#if UNITY_ANDROID
		private AndroidEngine androidEngine = null;
#endif

		void Awake()
		{
            base.Init();

#if UNITY_ANDROID
			androidEngine = new AndroidEngine();
#elif UNITY_IOS
			//string licenseKey = ConfigurationScriptableObject.GetInstance().LicenseKey;
   //         NativeAPI.maxst_init(licenseKey);
#endif

#if UNITY_EDITOR
			// Find arsdk_version.txt
			// if not exist file create file and write arsdk version
			// if file exist write
			if(!Directory.Exists(Application.streamingAssetsPath))
            {
				Directory.CreateDirectory(Application.streamingAssetsPath);
            }
			System.IO.File.WriteAllText(System.IO.Path.Combine(Application.streamingAssetsPath, "arsdk_version.txt"), MaxstAR.GetVersion());
#endif
        }

		void OnDestroy()
		{
			base.Deinit();

#if UNITY_ANDROID
			androidEngine = null;
#endif
		}
	}
}