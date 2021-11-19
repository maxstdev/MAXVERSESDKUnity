using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace maxstAR
{

	/// <summary>
	/// The tracking state of the camera.
	/// </summary>
	public enum ARTrackingState
	{
		/** Tracking is not available. */
		ARTrackingStateNotAvailable = 0,
		/** Tracking is Normal. */
		ARTrackingStateNormal = 1,
	};

	/// <summary>
	/// The location recognition state of the camera.
	/// </summary>
	public enum ARLocationRecognitionState
	{
		/** Can't find camera location. */
		ARLocationRecognitionStateNotAvailable = 0,
		/** Location Recognition Complete. */
		ARLocationRecognitionStateNormal = 1,
	};

	/// <summary>
	/// The reason for the camera’s current tracking state.
	/// </summary>
	public enum ARTrackingFailureReason
	{
		/** Tracking is not limited. */
		ARTrackingStateReasonNone = 0,
		/** Tracking is limited due to initialization in progress. */
		ARTrackingStateReasonInitializing = 1,
		/** Tracking is bad state. */
		ARTrackingStateReasonBadState = 2,
		/** Current motion is excessive. */
		ARTrackingStateReasonExcessiveMotion = 3,
		/** Insufficient feature for tracking. */
		ARTrackingStateReasonInsufficientFeatures = 4,
		/** Insufficient Light for tracking. */
		ARTrackingStateReasonInsufficientLight = 5,
		/** Camera is not available */
		ARTrackingStateReasonCameraUnAvailable = 6,
	};

	/// <summary>
	/// Container for tracking data (image, state, transform)
	/// </summary>
	public class ARFrame
	{

		private ulong arFrameCPtr;
		private ulong timeStamp;
		private float[] transform = new float[16];
		private ARTrackingState arTrackingState;
		private ARLocationRecognitionState arLocationRecognitionState;
		private ARTrackingFailureReason arTrackingFailureReason;
		private string arLocalizerLocation;
		private byte[] locationByte = new byte[1000];

		internal ARFrame(ulong cPtr)
		{
			if (cPtr == 0)
			{
				return;
			}

			arFrameCPtr = cPtr;

			timeStamp = NativeAPI.maxst_ARFrame_getTimeStamp(arFrameCPtr);
			arTrackingState = (ARTrackingState)NativeAPI.maxst_ARFrame_getARTrackingState(arFrameCPtr);
			arLocationRecognitionState = (ARLocationRecognitionState)NativeAPI.maxst_ARFrame_getARLocationRecognitionState(arFrameCPtr);
			arTrackingFailureReason = (ARTrackingFailureReason)NativeAPI.maxst_ARFrame_getARTrackingFailureReason(arFrameCPtr);
			
			Array.Clear(locationByte, 0, locationByte.Length);
			NativeAPI.maxst_ARFrame_getARLocalizerLocation(arFrameCPtr, locationByte);
			arLocalizerLocation = Encoding.UTF8.GetString(locationByte).TrimEnd('\0');
		}

		/// <summary>
		/// Get image Timestamp
		/// </summary>
		/// <returns>image timestamp</returns>
		public ulong GetTimeStamp()
		{
			return timeStamp;
		}

		/// <summary>
		/// Get image
		/// </summary>
		/// <returns>image</returns>
		public TrackedImage GetTrackedImage()
		{
			ulong Image_Cptr = 0;
			Image_Cptr = NativeAPI.maxst_ARFrame_getTrackedImage(arFrameCPtr);

			return new TrackedImage(Image_Cptr);
		}

		/// <summary>
		/// Get The tracking state of the camera.
		/// </summary>
		/// <returns>The tracking state of the camera</returns>
		public ARTrackingState GetARTrackingState()
		{
			return arTrackingState;
		}

		/// <summary>
		/// Get The location recognition state of the camera.
		/// </summary>
		/// <returns>The location recognition state of the camera.</returns>
		public ARLocationRecognitionState GetARLocationRecognitionState()
		{
			return arLocationRecognitionState;
		}

		/// <summary>
		/// Get The reason for the camera’s current tracking state.
		/// </summary>
		/// <returns>The reason for the camera’s current tracking state.</returns>
		public ARTrackingFailureReason GetARTrackingFailureReason()
		{
			return arTrackingFailureReason;
		}

		/// <summary>
		/// Get camera transform
		/// </summary>
		/// <returns>transform</returns>
		public Matrix4x4 GetTransform()
		{
			float[] pose = new float[16];
			NativeAPI.maxst_ARFrame_getTransform(arFrameCPtr, pose);
			Matrix4x4 targetPose = MatrixUtils.GetUnityPoseMatrix(pose);

			targetPose = targetPose.inverse;

			Matrix4x4 convertedMatrix = targetPose;

			if (convertedMatrix == Matrix4x4.zero)
				return Matrix4x4.zero;


			Quaternion rotation = Quaternion.Euler(90, 0, 0);
			Matrix4x4 m = Matrix4x4.TRS(new Vector3(0, 0, 0), rotation, new Vector3(1, 1, 1));
			convertedMatrix = m * convertedMatrix;

			Vector3 convertPosition = MatrixUtils.PositionFromMatrix(convertedMatrix);
			Quaternion convertRotation = MatrixUtils.QuaternionFromMatrix(convertedMatrix);
			Quaternion rotation2 = Quaternion.Euler(180, 0, 0) * convertRotation;
			Matrix4x4 m1 = Matrix4x4.TRS(new Vector3(convertPosition.x, -convertPosition.y, -convertPosition.z), rotation2, new Vector3(1, 1, 1));

			return m1;
		}

		public Matrix4x4 GetTransform(Matrix4x4 convertMatrix)
		{

			float[] pose = new float[16];
			NativeAPI.maxst_ARFrame_getTransform(arFrameCPtr, pose);
			Matrix4x4 targetPose = MatrixUtils.GetUnityPoseMatrix(pose);

			targetPose = convertMatrix * targetPose;
			targetPose = targetPose.inverse;

			Matrix4x4 convertedMatrix = targetPose;

			if (convertedMatrix == Matrix4x4.zero)
				return Matrix4x4.zero;


			Quaternion rotation = Quaternion.Euler(90, 0, 0);
			Matrix4x4 m = Matrix4x4.TRS(new Vector3(0, 0, 0), rotation, new Vector3(1, 1, 1));
			convertedMatrix = m * convertedMatrix;

			Vector3 convertPosition = MatrixUtils.PositionFromMatrix(convertedMatrix);
			Quaternion convertRotation = MatrixUtils.QuaternionFromMatrix(convertedMatrix);
			Quaternion rotation2 = Quaternion.Euler(180, 0, 0) * convertRotation;
			Matrix4x4 m1 = Matrix4x4.TRS(new Vector3(convertPosition.x, -convertPosition.y, -convertPosition.z), rotation2, new Vector3(1, 1, 1));

			return m1;
		}

		public string GetARLocalizerLocation()
        {
			return arLocalizerLocation;
        }
	}
}
