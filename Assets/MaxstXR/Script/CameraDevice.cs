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
    public class CameraDevice
    {
        /// <summary>
        /// Supported camera type (Mobile only)
        /// </summary>
        public enum CameraType
        {
            /// <summary>
            /// Rear camera
            /// </summary>
            Rear = 0,

            /// <summary>
            /// Face camera
            /// </summary>
            Face = 1,
        }

        /// <summary>
        /// Supported Fusion type 
        /// </summary>
        public enum FusionType
        {
            /// <summary>
            /// ARKit, ARCore camera
            /// </summary>
            ARCamera = 0,
        }

        /// <summary>
        /// Supported camera resolution
        /// </summary>
        public enum CameraResolution
        {
            /// <summary>
            /// 1280 * 720 (16:9 Resolution)
            /// </summary>
            Resolution1280x720,

            /// <summary>
            /// 1920 * 1080 (16:9 Resolution)
            /// </summary>
            Resolution1920x1080,
        }

        /// <summary>
        /// Flip video
        /// </summary>
        public enum FlipDirection
        {
            /// <summary>
            /// Horizontal flip
            /// </summary>
            HORIZONTAL = 0,

            /// <summary>
            /// Vertical flip
            /// </summary>
            VERTICAL = 1
        }

        /// <summary>
        /// API call return value
        /// </summary>
        public enum ResultCode
        {
            /// <summary>
            /// Success
            /// </summary>
            Success = 0,

            /// <summary>
            /// Permission state unknown
            /// </summary>
            CameraPermissionIsNotResolved = 100,

            /// <summary>
            /// No Camera can be usable
            /// </summary>
            CameraDevicedRestriced = 101,

            /// <summary>
            /// Camera permission is not grated
            /// </summary>
            CameraPermissionIsNotGranted = 102,

            /// <summary>
            /// Camera is alreay opened
            /// </summary>
            CameraAlreadyOpened = 103,

            /// <summary>
            /// Camera access exception
            /// </summary>
            CameraAccessException = 104,

            /// <summary>
            /// Camera not exist
            /// </summary>
            CameraNotExist = 105,

            /// <summary>
            /// Camera open timeout
            /// </summary>
            CameraOpenTimeOut = 106,

            /// <summary>
            /// Flash light is not supported
            /// </summary>
            FlashLightUnsupported = 107,

            /// <summary>
            /// Tracker already started
            /// </summary>
            TrackerAlreadyStarted = 200,

            /// <summary>
            /// Unknown error
            /// </summary>
            UnknownError = 1000,
        }

        private static CameraDevice instance = null;

        /// <summary>
        /// Get a CameraDevice instance.
        /// </summary>
        /// <returns>Return the CameraDevice instance</returns>
        public static CameraDevice GetInstance()
        {
            if (instance == null)
            {
                instance = new CameraDevice();
            }
            return instance;
        }

        private int cameraId = 0;
        private int preferredWidth = 0;
        private int preferredHeight = 0;

        private CameraDevice()
        {
        }

        /// <summary>
        /// Start camera preview
        /// </summary>
        public ResultCode Start()
        {
            return CameraDeviceInternal.GetInstance().Start();
        }

        /// <summary>
        /// Start simulation camera preview
        /// </summary>
        /// <param name="folderPath">Simulation file folder path</param>
        public ResultCode Start(string folderPath)
        {
            return (ResultCode)CameraDeviceInternal.GetInstance().Start(folderPath);
        }


        /// <summary>
        /// Flip video background 
        /// </summary>
        /// <param name="direction">Flip direction</param>
        /// <param name="on">True to set. False to rest</param>
        public void FlipVideo(FlipDirection direction, bool on)
        {
            CameraDeviceInternal.GetInstance().FlipVideo((int)direction, on);
        }

        /// <summary>
        /// Check Flip video state
        /// </summary>
        /// <param name="direction">Flip direction</param>
        /// <returns>Diretion state.</returns>
        public bool IsVideoFlipped(FlipDirection direction)
        {
            return CameraDeviceInternal.GetInstance().IsVideoFlipped((int)direction);
        }

        /// <summary>
        /// Stop camera preview
        /// </summary>
		public ResultCode Stop()
        {
            return CameraDeviceInternal.GetInstance().Stop();
        }

        /// <summary>
        /// camera preview width.
        /// </summary>
        /// <returns>Camera width</returns>
        public int GetWidth()
        {
            return CameraDeviceInternal.GetInstance().GetWidth();
        }

        /// <summary>
        /// camera preview height.
        /// </summary>
        /// <returns>Camera height</returns>
        public int GetHeight()
        {
            return CameraDeviceInternal.GetInstance().GetHeight();
        }

        /// <summary>
        /// Get projection matrix
        /// </summary>
        /// <returns>Matrix4x4 projection matrix</returns>
        public Matrix4x4 GetProjectionMatrix()
        {
            return CameraDeviceInternal.GetInstance().GetProjectionMatrix();
        }

        /// <summary>
        /// Get background plane geometry
        /// </summary>
        /// <returns>float[5] background plane geometry</returns>
        public float[] GetBackgroundPlaneGeometry()
        {
            return CameraDeviceInternal.GetInstance().GetBackgroundPlaneGeometry();
        }

        /// <summary>
        /// Set Camera clipping plane.
        /// </summary>
        /// <param name="near">Near plane</param>
        /// <param name="far">Far plane</param>
        public void SetClippingPlane(float near, float far)
        {
            CameraDeviceInternal.GetInstance().SetClippingPlane(near, far);
        }

        /// <summary>
        /// Get flip state
        /// </summary>
        /// <returns>Horizontal flip</returns>
        public bool IsFlipHorizontal()
		{
            return CameraDeviceInternal.GetInstance().IsFlipHorizontal();
		}

		/// <summary>
		/// Get flip state
		/// </summary>
		/// <returns>Vertical flip</returns>
		public bool IsFlipVertical()
		{
            return CameraDeviceInternal.GetInstance().IsFlipVertical();
        }

        /// <summary>
		/// Get fusion camera support state.
		/// </summary>
        /// <param name="type">Support Fusion Type</param>
		/// <returns>True is support, false is not support.</returns>
        public bool IsFusionSupported(FusionType type)
        {
            return CameraDeviceInternal.GetInstance().IsFusionSupported((int)type);
        }
    }
}