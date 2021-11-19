/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEngine;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

#if PLATFORM_ANDROID
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif
#endif

namespace maxstAR
{
    /// <summary>
    /// class for camera device handling
    /// </summary>
    public class CameraDeviceInternal
    {
        private static CameraDeviceInternal instance = null;

        /// <summary>
        /// Get a CameraDevice instance.
        /// </summary>
        /// <returns>Return the CameraDevice instance</returns>
        public static CameraDeviceInternal GetInstance()
        {
            if (instance == null)
            {
                instance = new CameraDeviceInternal();
            }
            return instance;
        }

        private int cameraId = 0;
        private int preferredWidth = 0;
        private int preferredHeight = 0;


        private CameraDeviceInternal()
        {
        }

        /// <summary>
        /// Start camera preview
        /// </summary>
        public CameraDevice.ResultCode Start()
        {
#if PLATFORM_ANDROID
#if UNITY_2018_3_OR_NEWER
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif
#endif
            int cameraType = 0;
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                cameraType = AbstractConfigurationScriptableObject.GetInstance().WebcamType;
            }
            else
            {
                cameraType = (int)AbstractConfigurationScriptableObject.GetInstance().CameraType;
            }

            CameraDevice.CameraResolution cameraResolution = AbstractConfigurationScriptableObject.GetInstance().CameraResolution;
            switch (cameraResolution)
            {
                case CameraDevice.CameraResolution.Resolution1280x720:
                    preferredWidth = 1280;
                    preferredHeight = 720;
                    break;

                case CameraDevice.CameraResolution.Resolution1920x1080:
                    preferredWidth = 1920;
                    preferredHeight = 1080;
                    break;

                default:
                    preferredWidth = 1280;
                    preferredHeight = 720;
                    break;
            }

            Debug.Log("Camera id : " + cameraId);

            return (CameraDevice.ResultCode)NativeAPI.maxst_CameraDevice_start(cameraType, preferredWidth, preferredHeight);
        }

        /// <summary>
        /// Start simulation camera preview
        /// </summary>
        /// <param name="folderPath">Simulation file folder path</param>
        public CameraDevice.ResultCode Start(string folderPath)
        {
            return (CameraDevice.ResultCode)NativeAPI.maxst_CameraDevice_startSimulator(folderPath);
        }

        /// <summary>
        /// Set Camera clipping plane.
        /// </summary>
        /// <param name="near">Near plane</param>
        /// <param name="far">Far plane</param>
        public void SetClippingPlane(float near, float far)
        {
            NativeAPI.maxst_CameraDevice_setClippingPlane(near, far);
        }

        /// <summary>
        /// Flip video background 
        /// </summary>
        /// <param name="direction">Flip direction</param>
        /// <param name="on">True to set. False to rest</param>
        public void FlipVideo(int direction, bool on)
        {
            NativeAPI.maxst_CameraDevice_flipVideo((int)direction, on);
        }

        /// <summary>
        /// Check Flip video state
        /// </summary>
        /// <param name="direction">Flip direction
        public bool IsVideoFlipped(int direction)
        {
            return NativeAPI.maxst_CameraDevice_isVideoFlipped((int)direction);
        }

        /// <summary>
        /// Stop camera preview
        /// </summary>
		public CameraDevice.ResultCode Stop()
        {
            return (CameraDevice.ResultCode)NativeAPI.maxst_CameraDevice_stop();
        }

        /// <summary>
        /// camera preview width.
        /// </summary>
        /// <returns>Camera width</returns>
        public int GetWidth()
        {
            return NativeAPI.maxst_CameraDevice_getWidth();
        }

        /// <summary>
        /// camera preview height.
        /// </summary>
        /// <returns>Camera height</returns>
        public int GetHeight()
        {
            return NativeAPI.maxst_CameraDevice_getHeight();
        }

        /// <summary>
        /// Get projection matrix
        /// </summary>
        /// <returns>Matrix4x4 projection matrix</returns>
        public Matrix4x4 GetProjectionMatrix()
        {
            float[] glProjection = new float[16];

            NativeAPI.maxst_CameraDevice_getProjectionMatrix(glProjection);

            return MatrixUtils.ConvertGLProjectionToUnityProjection(glProjection);
        }

        /// <summary>
        /// Get background plane geometry
        /// </summary>
        /// <returns>float[5] background plane geometry</returns>
        public float[] GetBackgroundPlaneGeometry()
        {
            float[] values = new float[5];
            NativeAPI.maxst_CameraDevice_getBackgroundPlaneGeometry(values);
            return values;
        }

        public float[] GetFusionPose(int type, float[] vpspose, float[] localpose)
        {
            float[] fusionPose = new float[16];
            NativeAPI.maxst_CameraDevice_getFusionPose(type, vpspose, localpose, fusionPose);

            return fusionPose;
        }

        /// <summary>
        /// Get flip state
        /// </summary>
        /// <returns>Horizontal flip</returns>
        public bool IsFlipHorizontal()
        {
            return NativeAPI.maxst_CameraDevice_isVideoFlipped((int)CameraDevice.FlipDirection.HORIZONTAL);
        }

        /// <summary>
        /// Get flip state
        /// </summary>
        /// <returns>Vertical flip</returns>
        public bool IsFlipVertical()
        {
            return NativeAPI.maxst_CameraDevice_isVideoFlipped((int)CameraDevice.FlipDirection.VERTICAL);
        }

        /// <summary>
		/// Get fusion camera support state.
		/// </summary>
        /// <param name="type">Support Fusion Type</param>
		/// <returns>True is support, false is not support.</returns>
        public bool IsFusionSupported(int type)
        {
            return NativeAPI.maxst_CameraDevice_isFusionSupported(type);
        }

        public bool SetCalibrationData(int width, int height, float fx, float fy, float px, float py)
        {
            return NativeAPI.maxst_CameraDevice_setIntrinsic(width, height, fx, fy, px, py);
        }

        public void SetFusionCameraIntrinsic(float fx, float fy, float px, float py)
        {
            NativeAPI.maxst_CameraDevice_setFusionCameraIntrinsic(fx, fy, px, py);
        }

        public bool RequestAsyncImage()
        {
            return NativeAPI.maxst_CameraDevice_requestAsyncImage();
        }

        public void SetAsyncImage(bool isEnable)
        {
            NativeAPI.maxst_CameraDevice_setAsyncImage(isEnable);
        }

        /// <summary>
        /// Set external camera pose and timestamp to AR engine.
        /// </summary>
        /// <param name="pose">Camera pose</param>
        /// <param name="timestamp">Timestamp</param>
        public void SetNewVPSCameraPoseAndTimestamp(float[] pose, ulong timestamp)
        {
            Debug.Log("MaxstAR : " + timestamp);
            NativeAPI.maxst_CameraDevice_setNewVPSCameraPoseAndTimestamp(pose, timestamp);
        }

        /// <summary>
        /// Set external camera image and timestamp to AR engine.
        /// </summary>
        /// <param name="data">Native address of camera image</param>
        /// <param name="length">Lenght of data buffer</param>
        /// <param name="width">Width of camera image</param>
        /// <param name="height">Height of camera image</param>
        /// <param name="format">Color format</param>
        /// <param name="pose">Camera pose</param>
        /// <param name="timestamp">Timestamp</param>
        public void SetNewFrameAndPoseAndTimestamp(byte[] data, int length, int width, int height, ColorFormat format, float[] pose, ulong timestamp)
        {
            NativeAPI.maxst_CameraDevice_setNewFrameAndPoseAndTimestamp(data, length, width, height, (int)format, pose, timestamp);
        }

        /// <summary>
        /// Set external camera image and timestamp to AR engine for sync.
        /// </summary>
        /// <param name="textureIds">Texture id of camera image</param>
        /// <param name="textureLength">Lenght of image texture id</param>
        /// <param name="pose">Camera pose</param>
        /// <param name="timestamp">Timestamp</param>
        public void SetSyncCameraFrameAndPoseAndTimestamp(IntPtr[] textureIds, int textureLength, float[] pose, ulong timestamp, int trackingState, int trackingFailureReason)
        {
            NativeAPI.maxst_CameraDevice_setSyncCameraFrameAndPoseAndTimestamp(textureIds, textureLength, pose, timestamp, trackingState, trackingFailureReason);
        }
    }
}