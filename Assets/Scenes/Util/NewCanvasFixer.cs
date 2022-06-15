using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class NewCanvasFixer : MonoBehaviour
{

    float SCALE = 0.0f;
    float DEVICE_FIX = 1.72f;

    void Awake()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        FixCanvasScaleFactor();
    }

#if UNITY_EDITOR

    void Update()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        FixCanvasScaleFactor();
    }
#endif
    private void FixCanvasScaleFactor()
    {

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            //DEVICE_FIX = 1.72f;
            DEVICE_FIX = 1.67f;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //DEVICE_FIX = 1.8f;
            DEVICE_FIX = 1.50f;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            DEVICE_FIX = 1.65f;
        }
        // Get scale needed to maintain physical size

        float physicalScale = (96.0f / 72.0f) * (Screen.dpi / 96.0f);
        float screenDimensionsWidth = (2.54f * Screen.width / Screen.dpi);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            physicalScale = (96.0f / 72.0f) * (iOSDPI.dpi / 96.0f);
            screenDimensionsWidth = (2.54f * Screen.width / iOSDPI.dpi);
        }
        // Get real screen width (physical width, not screen resolution)
        //float screenDimensionsWidth = (2.54f * Screen.width / Screen.dpi);
        // If screen is really small (less than 11cm, such a smartphone), then apply a lower scale, otherwise maintain physical size regardless of screen size/resolution:

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (screenDimensionsWidth > 8)
            {
                DEVICE_FIX = 2.3f;
            }
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            if (screenDimensionsWidth > 8)
            {
                DEVICE_FIX = 1.8f;
            }
        }

        SCALE = physicalScale * 0.75f;

        CanvasScaler cs = GetComponent<CanvasScaler>();
        if (cs.scaleFactor != SCALE)
        {
            cs.scaleFactor = (float)(SCALE / DEVICE_FIX);
        }

    }
}
