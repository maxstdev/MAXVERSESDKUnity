using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using maxstAR;
using JsonFx.Json;
using System;

public class POIController : MonoBehaviour
{
    static string serverURL = "http://dev-api-poi-customer.maxverse.io";
    public static void GetPOI(MonoBehaviour monoBehaviour, string accessToken, int placeId, Action<POIData[]> success, Action fail)
    {
        Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { "Authorization", accessToken},
            { "Content-Type", "application/json"}
        };

        monoBehaviour.StartCoroutine(APIController.GET(serverURL + "/poi/place/" + placeId, headers, 10, (resultString) =>
        {
            if (resultString != "")
            {
                Debug.Log(resultString);
                POIData[] pois = JsonReader.Deserialize<POIData[]>(resultString);
                success(pois);
            }
            else
            {
                fail();
            }
        }));
    }
}
