/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;


namespace maxstAR
{
	/// <summary>
	/// Initialize system environment with app key, screen size and orientation
	/// </summary>
	public abstract class AbstractARManager : MonoBehaviour
	{
		private static AbstractARManager instance = null;

		internal static AbstractARManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<AbstractARManager>();
				}

				return instance;
			}
		}

		private int screenWidth = 0;
		private int screenHeight = 0;
		private ScreenOrientation orientation;
		private float nearClipPlane = 0.0f;
		private float farClipPlane = 0.0f;
		private Camera arCamera = null;

		private CameraBackgroundBehaviour cameraBackground;

        /// <summary>
        /// Intialize sdk
        /// </summary>
        protected void Init()
		{
			if (XRStudioController.Instance.ARMode)
			{
				transform.position = Vector3.one;
				InitInternal();

				if (Application.platform == RuntimePlatform.Android ||
					Application.platform == RuntimePlatform.IPhonePlayer)
				{
					MaxstAR.SetScreenOrientation((int)Screen.orientation);
				}
				else
				{
					MaxstAR.SetScreenOrientation((int)ScreenOrientation.LandscapeLeft);
				}

				MaxstAR.OnSurfaceChanged(Screen.width, Screen.height);
			}
			else
            {

				GetCameraBackgroundBehaviour().gameObject.SetActive(false);
            }
		}

		/// <summary>
		/// Set device orientation and surface size
		/// </summary>
		void InitInternal()
		{
			// If CameraBackgroundBehaviour is not activated when start application, projection matrix 
			// can not be made because screen width and height isn't set properly yet.
			if (screenWidth != Screen.width || screenHeight != Screen.height)
			{
				screenWidth = Screen.width;
				screenHeight = Screen.height;
				MaxstAR.OnSurfaceChanged(screenWidth, screenHeight);
			}

			if (Application.platform == RuntimePlatform.Android ||
				Application.platform == RuntimePlatform.IPhonePlayer)
			{
				MaxstAR.SetScreenOrientation((int)Screen.orientation);
			}
			else
			{
				MaxstAR.SetScreenOrientation((int)ScreenOrientation.LandscapeLeft);
			}

			arCamera = GetComponent<Camera>();

			CameraDevice.GetInstance().SetClippingPlane(arCamera.nearClipPlane, arCamera.farClipPlane);
        }

		/// <summary>
		/// Release sdk
		/// </summary>
		protected void Deinit()
		{
		}

        private void OnApplicationPause(bool pause)
        {
			if(XRStudioController.Instance.ARMode)
            {
				transform.position = Vector3.one;
				transform.eulerAngles = Vector3.zero;
			}
		}

        public Camera GetARCamera()
		{
			return arCamera;
		}

		public CameraBackgroundBehaviour GetCameraBackgroundBehaviour()
        {
			if(cameraBackground == null)
            {
				cameraBackground = GetComponentInChildren<CameraBackgroundBehaviour>();
			}
			return cameraBackground;
        }

		void OnPreRender()
		{
			if (XRStudioController.Instance.ARMode)
			{
				if (screenWidth != Screen.width || screenHeight != Screen.height)
				{
					screenWidth = Screen.width;
					screenHeight = Screen.height;
					MaxstAR.OnSurfaceChanged(screenWidth, screenHeight);
				}

				if (orientation != Screen.orientation)
				{
					orientation = Screen.orientation;

					if (Application.platform == RuntimePlatform.Android ||
						Application.platform == RuntimePlatform.IPhonePlayer)
					{
						MaxstAR.SetScreenOrientation((int)orientation);
					}
				}

				if (nearClipPlane != arCamera.nearClipPlane || farClipPlane != arCamera.farClipPlane)
				{
					nearClipPlane = arCamera.nearClipPlane;
					farClipPlane = arCamera.farClipPlane;
					CameraDevice.GetInstance().SetClippingPlane(arCamera.nearClipPlane, arCamera.farClipPlane);
				}

				arCamera.projectionMatrix = CameraDevice.GetInstance().GetProjectionMatrix();
			}
        }
	}
}
