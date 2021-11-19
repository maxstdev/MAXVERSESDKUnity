/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace maxstAR
{
	/// <summary>
	/// Control AR Engine (Singletone)
	/// </summary>
	public class TrackerManager
	{
		private static TrackerManager instance = null;
        //private TrackingState trackingState = null;
		private byte[] timeBytes = new byte[1000];

		/// <summary>
		/// Get TrackerManager instance
		/// </summary>
		/// <returns></returns>
		public static TrackerManager GetInstance()
		{
			if (instance == null)
			{
				instance = new TrackerManager();

			}

            return instance;
		}

		private TrackerManager()
		{
			
		}

		/// <summary>Start Tracker.</summary>
		public void StartTracker()
		{
            NativeAPI.maxst_TrackerManager_startTracker();
		}

		/// <summary>Stop Tracker.</summary>
		public void StopTracker()
		{
            NativeAPI.maxst_TrackerManager_stopTracker();
        }

		/// <summary>Destroy Tracker.</summary>
		public void DestroyTracker()
		{
            NativeAPI.maxst_TrackerManager_destroyTracker();
        }

		/// <summary>Refresh Tracker.</summary>
		public void RefreshTracker()
		{
			NativeAPI.maxst_TrackerManager_refreshTracker();
		}

		/// <summary>Add the Trackable data to the Map List.</summary>
		/// <param name="trackingFileName">File path of map for map addition.</param>
		/// <param name="isAndroidAssetFile">Map file position for addition. True is in Asset folder.</param>
		public void AddTrackerData(string trackingFileName, bool isAndroidAssetFile = false)
		{
            NativeAPI.maxst_TrackerManager_addTrackerData(trackingFileName, isAndroidAssetFile);
        }

		/// <summary>Delete the Trackable data from the Map List.</summary>
		/// <param name="trackingFileName">trackingFileName map file name. 
		/// This name should be same which added. If set "" (empty) file list will be cleared</param>
		public void RemoveTrackerData(string trackingFileName = "")
		{
            NativeAPI.maxst_TrackerManager_removeTrackerData(trackingFileName);
        }

		public void ReplaceServerIP(string serverIP)
        {
			NativeAPI.maxst_TrackerManager_replaceServerIP(serverIP);
        }

		/// <summary>Load the Trackable data.</summary>
		public void LoadTrackerData()
		{
            NativeAPI.maxst_TrackerManager_loadTrackerData();
        }

		/// <summary>Request ARCore apk.</summary>
		public void RequestARCoreApk()
        {
#if !UNITY_EDITOR
            AndroidJavaClass javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if(currentActivity != null) {
                AndroidJavaClass maxstARClass = new AndroidJavaClass("com.maxst.ar.TrackerManager");

                if(maxstARClass != null) {
                    maxstARClass.CallStatic<string>("requestARCoreApk", currentActivity);
                }
            
            }
#endif
        }

		/// <summary>Update ARFrame.</summary>
		public void UpdateFrame()
		{
			NativeAPI.maxst_TrackerManager_updateFrame();
		}

		/// <summary>Get ARFrame. must be call after UpdateFrame function.</summary>
		/// <returns>ARFrame</returns>
		public ARFrame GetARFrame()
        {
			ulong arFrameCPtr = 0;
			arFrameCPtr = NativeAPI.maxst_TrackerManager_getARFrame();

			return new ARFrame(arFrameCPtr);
		}

		/// <summary>Get Server State.</summary>
		/// <returns>Server Information</returns>
		public string GetServerQueryTime()
		{
			Array.Clear(timeBytes, 0, timeBytes.Length);
			NativeAPI.maxst_TrackerManager_getServerQueryTime(timeBytes);
			
			return Encoding.UTF8.GetString(timeBytes).TrimEnd('\0');
		}
	}
}
