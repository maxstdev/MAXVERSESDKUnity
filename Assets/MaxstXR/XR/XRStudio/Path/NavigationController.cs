using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using maxstAR;
using JsonFx.Json;

//#if UNITY_EDITOR
//using Unity.EditorCoroutines.Editor;
//#endif

public class NavigationController : MonoBehaviour
{
    private static string pathURL = "http://ec2-3-34-147-254.ap-northeast-2.compute.amazonaws.com:5501";

    private Vector3 startPosition;
    private Vector3 endPosition;

    public float arrowPositionDistance = 3.0f;
    public float arrowVisibleDistance = 20.0f;
    public float arrowPathHeight = -0.5f;
    public GameObject arrowPrefab;

    private List<GameObject> pathObjects = new List<GameObject>();
    private List<GameObject> arrowItems = new List<GameObject>();

    public void UpdateVisibleArrow(GameObject arCameraObject)
    {
        foreach(GameObject eachArrowItem in arrowItems)
        {
            Vector3 arCameraPosition = arCameraObject.transform.position;
            Vector3 arrowPosition = eachArrowItem.transform.position;

            float distacne = Vector3.Distance(arCameraPosition, arrowPosition);
            if(distacne > arrowVisibleDistance)
            {
                eachArrowItem.SetActive(false);
            }
            else
            {
                eachArrowItem.SetActive(true);
            }
        }
    }

    public static void FindPath(string startLocation, Vector3 start, string endLocation, Vector3 end,
        Action<Dictionary<string, List<PathModel>>> success, Action fail, MonoBehaviour behaviour, string server_name = "")
    {
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { "Content-Type", "application/json"}
        };

        string startPositionString = start.x + "," + start.z + "," + start.y;
        string endPositionString = end.x + "," + (end.z) + "," + (end.y);
        Dictionary<string, string> parameters = new Dictionary<string, string>()
        {
            { "start_location", startLocation },
            { "start_position", startPositionString},
            { "end_location", endLocation },
            { "end_position", endPositionString},
            { "server_name", server_name}
        };

//#if UNITY_EDITOR
//        EditorCoroutineUtility.StartCoroutine(APIController.POST(pathURL + "/v1/path", headers, parameters, 10, (resultString) =>
//        {
//            if (resultString != "")
//            {
//                PathModel[] paths = JsonReader.Deserialize<PathModel[]>(resultString);
//                Dictionary<string, List<PathModel>> pathDictionary = new Dictionary<string, List<PathModel>>();
//                foreach (PathModel eachPathModel in paths)
//                {
//                    if (!pathDictionary.ContainsKey(eachPathModel.location))
//                    {
//                        pathDictionary[eachPathModel.location] = new List<PathModel>();
//                    }
//                    List<PathModel> pathList = pathDictionary[eachPathModel.location];
//                    if (pathList == null)
//                    {
//                        pathList = new List<PathModel>();
//                    }

//                    pathList.Add(eachPathModel);
//                }
//                success(pathDictionary);
//            }
//            else
//            {
//                fail();
//            }

//        }), behaviour);
//#else
        behaviour.StartCoroutine(APIController.POST(pathURL + "/v1/path", headers, parameters, 10, (resultString) =>
        {
            if(resultString != "")
            {
                PathModel[] paths = JsonReader.Deserialize<PathModel[]>(resultString);
                Dictionary<string, List<PathModel>> pathDictionary = new Dictionary<string, List<PathModel>>();
                foreach(PathModel eachPathModel in paths)
                {
                    if(!pathDictionary.ContainsKey(eachPathModel.location))
                    {
                        pathDictionary[eachPathModel.location] = new List<PathModel>();
                    }
                    List<PathModel> pathList = pathDictionary[eachPathModel.location];
                    if(pathList == null)
                    {
                        pathList = new List<PathModel>();
                    }

                    pathList.Add(eachPathModel);
                }
                success(pathDictionary);
            }
            else
            {
                fail();
            }
        }));
//#endif
    }


    public void MakePath(string start_location, Vector3 start_position, string end_location, Vector3 end_position, VPSTrackable[] trackables, Action fail, string serverName = "")
    {
        startPosition = start_position;
        endPosition = end_position;

        FindPath(start_location, startPosition, end_location, endPosition, (paths) =>
        {
            RemovePaths();

            // Make Navigation Path use paths parameter.
        },
        () =>
        {
            fail();
        }, this);
    }

    public void RemovePaths()
    {
        arrowItems.Clear();
        foreach(GameObject pathObject in pathObjects)
        {
            Destroy(pathObject);
        }
        pathObjects.Clear();
    }

    private GameObject MakeArrowPath(PathModel[] paths, GameObject arContent)
    {
        List<Vector3> vectors = new List<Vector3>();
        foreach (PathModel eachModel in paths)
        {
            vectors.Add(new Vector3(eachModel.x, eachModel.y, eachModel.z));
        }

        List<Vector3> positions = new List<Vector3>();
        List<Vector3> directions = new List<Vector3>();
        CalculateMilestones(vectors, positions, directions, arrowPositionDistance, true);

        List<Vector3> convertVectorPath = new List<Vector3>();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 eachPath = positions[i];
            Vector3 pathPoint = new Vector3(eachPath.x, eachPath.z, eachPath.y);
            convertVectorPath.Add(pathPoint);
        }


        GameObject naviGameObject = new GameObject();
        naviGameObject.name = "Navigation";
        naviGameObject.transform.position = new Vector3(0, 0, 0);
        naviGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        naviGameObject.transform.localScale = new Vector3(1, 1, 1);

        naviGameObject.transform.parent = arContent.transform;

        Dictionary<GameObject, Dictionary<GameObject, float>> intersectionGameObjects = new Dictionary<GameObject, Dictionary<GameObject, float>>();


        for (int i = 1; i < convertVectorPath.Count - 2; i++)
        {
            Vector3 first = convertVectorPath[i] + new Vector3(0.0f, arrowPathHeight, 0.0f);
            Vector3 second = convertVectorPath[i + 1] + new Vector3(0.0f, arrowPathHeight, 0.0f);

            Vector3 vec = first - second;
            vec.Normalize();
            Quaternion q = Quaternion.LookRotation(vec);

            GameObject arrowGameObject = Instantiate(arrowPrefab);
            arrowGameObject.transform.position = first;
            arrowGameObject.transform.eulerAngles = arrowGameObject.transform.eulerAngles + q.eulerAngles;
            arrowGameObject.transform.parent = naviGameObject.transform;
            arrowGameObject.name = "arrow" + i;
     
            arrowItems.Add(arrowGameObject);
        }

        return naviGameObject;
    }
    
    private Vector3 DivideBetweenTwoPoints(in Vector3 from, in Vector3 to, double ratio)
    {
        Vector3 res = new Vector3(0, 0, 0);
        if (ratio < 0.0 || ratio > 1.0)
            return res;

        res = from * (float)(1.0 - ratio) + to * (float)ratio;
        return res;
    }

    private void InterpolateByBezier(out Vector3 p, out Vector3 d, in Vector3 p0, in Vector3 p1, in Vector3 p2, double t)
    {
        double one_minus_t = 1.0 - t;
        p = (float)(one_minus_t * one_minus_t) * p0 + (float)(2.0 * t * one_minus_t) * p1 + (float)(t * t) * p2;
        d = (float)(-2.0 * one_minus_t) * p0 + (float)(2.0 * one_minus_t - 2.0 * t) * p1 + (float)(2.0 * t) * p2;
        d.Normalize();
    }

    private void CalculateMilestones(List<Vector3> path, in List<Vector3> pos, in List<Vector3> dir, double interval, bool useBezier)
    {
        double totalDist = 0.0;
        List<double> dists = new List<double>();
        dists.Add(0.0);
        int finalIndex = path.Count - 1;
        for (int i = 0; i < finalIndex; i++)
        {
            Vector3 cur = path[i];
            Vector3 next = path[i + 1];
            Vector3 diff = next - cur;
            totalDist += diff.magnitude;
            dists.Add(totalDist);
        }

        pos.Clear();
        dir.Clear();

        for (double d = 0.0; d <= totalDist; d += interval)
        {
            int next = 1;
            while (next < dists.Count && d > dists[next])
                next++;

            int cur = next - 1;
            Vector3 nextPoi = path[next];
            Vector3 curPoi = path[cur];

            double len = d - dists[cur];
            double overallLen = dists[next] - dists[cur];
            double ratio = len / overallLen;

            Vector3 po = DivideBetweenTwoPoints(curPoi, nextPoi, ratio);
            Vector3 di = nextPoi - curPoi;
            di.Normalize();

            pos.Add(po);
            dir.Add(di);
        }

        if (useBezier)
        {
            List<Vector3> newPos = pos;
            List<Vector3> newDir = dir;
            for (int i = 1; i < pos.Count - 1; i++)
            {
                int prev = i - 1;
                int cur = i;
                int next = i + 1;

                Vector3 outNewPos = Vector3.zero;
                Vector3 outNewDir = Vector3.zero;
                InterpolateByBezier(out outNewPos, out outNewDir, pos[prev], pos[cur], pos[next], 0.5);
                newPos[i] = outNewPos;
                newDir[i] = outNewDir;
            }
        }
    }
}
