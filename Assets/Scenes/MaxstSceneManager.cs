using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using maxstAR;
using UnityEngine.UI;
using System.IO;
using System;

public class MaxstSceneManager : MonoBehaviour
{
	private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;
	private GameObject arCamera = null;

	public List<GameObject> disableObjects = new List<GameObject>();
	public List<GameObject> occlusionObjects = new List<GameObject>();
	private List<VPSTrackable> vPSTrackablesList = new List<VPSTrackable>();

	public Material buildingMaterial;
	public Material runtimeBuildingMaterial;

	public GameObject maxstLogObject;

	public bool isOcclusion = true;
	private string currentLocalizerLocation = "";
	private int currentLocalizerPlaceId = -1;

	private string serverName = "";

	public GameObject arContent;

	public GameObject arrowPrefab;
	private List<GameObject> pathObjects = new List<GameObject>();
	private List<GameObject> arrowItems = new List<GameObject>();
	public float arrowVisibleDistance = 20.0f;

	void Awake()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		
		AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions("android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.CAMERA", "android.permission.ACCESS_FINE_LOCATION", "android.permission.ACCESS_COARSE_LOCATION");
		if (result[0] == AndroidRuntimePermissions.Permission.Granted && result[1] == AndroidRuntimePermissions.Permission.Granted)
			Debug.Log("We have all the permissions!");
		else
			Debug.Log("Some permission(s) are not granted...");

		ARManager arManagr = FindObjectOfType<ARManager>();
		if (arManagr == null)
		{
			Debug.LogError("Can't find ARManager. You need to add ARManager prefab in scene.");
			return;
		}
		else
		{
			arCamera = arManagr.gameObject;
		}

		cameraBackgroundBehaviour = arManagr.GetCameraBackgroundBehaviour();
		if (cameraBackgroundBehaviour == null)
		{
			Debug.LogError("Can't find CameraBackgroundBehaviour.");
			return;
		}

		VPSTrackable[] vPSTrackables = FindObjectsOfType<VPSTrackable>(true);
		if (vPSTrackables != null)
		{
			vPSTrackablesList.AddRange(vPSTrackables);
		}
		else
		{
			Debug.LogError("You need to add VPSTrackables.");
		}

		foreach (GameObject eachObject in disableObjects)
		{
			if(eachObject != null)
            {
				eachObject.SetActive(false);
			}
		}

		if (XRStudioController.Instance.ARMode)
		{
			foreach(VPSTrackable vPSTrackable in vPSTrackablesList)
            {
				vPSTrackable.gameObject.SetActive(false);
            }
		}
		else
        {
			this.enabled = false;
        }
	}

	void Start()
	{
		if (isOcclusion)
		{
			foreach (GameObject eachGameObject in occlusionObjects)
			{
				Renderer[] cullingRenderer = eachGameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer eachRenderer in cullingRenderer)
				{
					Material[] materials = eachRenderer.materials;
					for (int i = 0; i < eachRenderer.materials.Length; i++)
					{
						materials[i] = runtimeBuildingMaterial;
						materials[i].renderQueue = 1900;
					}

					eachRenderer.materials = materials;
				}
			}
		}
		else
		{
			foreach (GameObject eachGameObject in occlusionObjects)
			{
				Renderer[] cullingRenderer = eachGameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer eachRenderer in cullingRenderer)
				{
					Material[] materials = eachRenderer.materials;
					for (int i = 0; i < eachRenderer.materials.Length; i++)
					{
						materials[i] = buildingMaterial;
						materials[i].renderQueue = 1900;
					}

					eachRenderer.materials = materials;
				}
			}
		}

		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        {
			string simulatePath = XRStudioController.Instance.xrSimulatePath;

			if (Directory.Exists(simulatePath))
			{
				CameraDevice.GetInstance().Start(simulatePath);
				MaxstAR.SetScreenOrientation((int)ScreenOrientation.Portrait);
			}
		}
		else
        {
			if (CameraDevice.GetInstance().IsFusionSupported(CameraDevice.FusionType.ARCamera))
			{
				CameraDevice.GetInstance().Start();
			}
			else
			{
				TrackerManager.GetInstance().RequestARCoreApk();
			}
		}
		
		TrackerManager.GetInstance().StartTracker();
		//TrackerManager.GetInstance().ReplaceServerIP("{\"vps_server_ip\":\"http://169.254.241.227:5501\"}");
		//TrackerManager.GetInstance().AddTrackerData("{\"vps_server\":\"p0000_p0000_indoor_maxst_f2\"}");
	}

	void Update()
	{
		if(!XRStudioController.Instance.ARMode)
        {
			return;
        }

		TrackerManager.GetInstance().UpdateFrame();

		ARFrame arFrame = TrackerManager.GetInstance().GetARFrame();

		TrackedImage trackedImage = arFrame.GetTrackedImage();

		if (trackedImage.IsTextureId())
		{
			IntPtr[] cameraTextureIds = trackedImage.GetTextureIds();
			cameraBackgroundBehaviour.UpdateCameraBackgroundImage(cameraTextureIds);
		}
		else
		{
			cameraBackgroundBehaviour.UpdateCameraBackgroundImage(trackedImage);
		}

		if (arFrame.GetARLocationRecognitionState() == ARLocationRecognitionState.ARLocationRecognitionStateNormal)
		{
			Matrix4x4 targetPose = arFrame.GetTransform();

			arCamera.transform.position = MatrixUtils.PositionFromMatrix(targetPose);
			arCamera.transform.rotation = MatrixUtils.QuaternionFromMatrix(targetPose);
			arCamera.transform.localScale = MatrixUtils.ScaleFromMatrix(targetPose);

			string localizerLocation = arFrame.GetARLocalizerLocation();


			if (currentLocalizerLocation != localizerLocation)
			{
				currentLocalizerLocation = localizerLocation;
				foreach (VPSTrackable eachTrackable in vPSTrackablesList)
				{
					bool isLocationInclude = false;
					foreach (string eachLocation in eachTrackable.localizerLocation)
					{
						if (currentLocalizerLocation == eachLocation)
						{
							isLocationInclude = true;
							currentLocalizerPlaceId = eachTrackable.placeId;
							break;
						}
					}
					eachTrackable.gameObject.SetActive(isLocationInclude);
				}
			}
		}
		else
		{
			foreach (VPSTrackable eachTrackable in vPSTrackablesList)
			{
				eachTrackable.gameObject.SetActive(false);
			}
			currentLocalizerLocation = "";
		}

		UpdateVisibleArrow(arCamera);
	}

	void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			CameraDevice.GetInstance().Stop();
			TrackerManager.GetInstance().StopTracker();
		}
		else
		{
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
			{
				string simulatePath = XRStudioController.Instance.xrSimulatePath;
				if (Directory.Exists(simulatePath))
				{
					CameraDevice.GetInstance().Start(simulatePath);
					MaxstAR.SetScreenOrientation((int)ScreenOrientation.Portrait);
				}
			}
			else
			{
				if (CameraDevice.GetInstance().IsFusionSupported(CameraDevice.FusionType.ARCamera))
				{
					CameraDevice.GetInstance().Start();
				}
				else
				{
					TrackerManager.GetInstance().RequestARCoreApk();
				}
			}

			TrackerManager.GetInstance().StartTracker();
			//TrackerManager.GetInstance().ReplaceServerIP("{\"vps_server_ip\":\"http://169.254.241.227:5501\"}");
		}
	}

	void OnDestroy()
	{
		CameraDevice.GetInstance().Stop();
		TrackerManager.GetInstance().StopTracker();
		TrackerManager.GetInstance().DestroyTracker();
	}

	public void OnClickGetPOI()
    {
		string accessToken = TrackerManager.GetInstance().GetAccessToken();

		Debug.Log(accessToken);
		if(currentLocalizerPlaceId > 0)
        {
			POIController.GetPOI(this, accessToken, currentLocalizerPlaceId, success:(pois)=> {
				
			},
			fail:()=> {

			} );
		}
		
	}

	public void OnClickNavigation()
    {
		RemovePaths();

		string accessToken = TrackerManager.GetInstance().GetAccessToken();
		string navigationLocation = "";
		if (currentLocalizerLocation != null)
		{
			GameObject trackingObject = null;
			foreach (VPSTrackable eachTrackable in vPSTrackablesList)
			{
				foreach (string eachLocation in eachTrackable.localizerLocation)
				{
					if (currentLocalizerLocation == eachLocation)
					{
						navigationLocation = eachTrackable.navigationLocation;
						trackingObject = eachTrackable.gameObject;
						break;
					}
				}
			}

			if (trackingObject != null)
			{
				NavigationController.FindPath(this, accessToken, navigationLocation, arCamera.transform.position, navigationLocation, new Vector3(-132.99f, 8.42f, 169.13f), 2.0f, vPSTrackablesList.ToArray(),
				(paths) => {
					MakeNavigationArrowContent(paths);
				},
				() => {
					Debug.Log("No Path");
				});
			}
		}
	}

	private void MakeNavigationArrowContent(Dictionary<string, PathModel[]> paths)
    {
		foreach(string eachTrackableName in paths.Keys)
        {
			foreach (VPSTrackable eachTrackable in vPSTrackablesList)
			{
				if(eachTrackable.localizerLocation.Contains(eachTrackableName))
                {
					GameObject naviGameObject = new GameObject();
					naviGameObject.name = "Navigation";
					naviGameObject.transform.position = new Vector3(0, 0, 0);
					naviGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
					naviGameObject.transform.localScale = new Vector3(1, 1, 1);

					naviGameObject.transform.parent = eachTrackable.transform;

					PathModel[] eachPaths = paths[eachTrackableName];
					for (int i = 1; i < eachPaths.Length - 2; i++)
					{
						GameObject arrowGameObject = Instantiate(arrowPrefab);
						arrowGameObject.transform.position = eachPaths[i].position;
						arrowGameObject.transform.eulerAngles = arrowGameObject.transform.eulerAngles + eachPaths[i].rotation.eulerAngles;
						arrowGameObject.transform.parent = naviGameObject.transform;
						arrowGameObject.name = "arrow" + i;

						arrowItems.Add(arrowGameObject);
					}
				}
			}
		}
	}

	private void UpdateVisibleArrow(GameObject arCameraObject)
	{
		foreach (GameObject eachArrowItem in arrowItems)
		{
			Vector3 arCameraPosition = arCameraObject.transform.position;
			Vector3 arrowPosition = eachArrowItem.transform.position;

			float distacne = Vector3.Distance(arCameraPosition, arrowPosition);
			if (distacne > arrowVisibleDistance)
			{
				eachArrowItem.SetActive(false);
			}
			else
			{
				eachArrowItem.SetActive(true);
			}
		}
	}

	private void RemovePaths()
	{
		arrowItems.Clear();
		foreach (GameObject pathObject in pathObjects)
		{
			Destroy(pathObject);
		}
		pathObjects.Clear();
	}
}