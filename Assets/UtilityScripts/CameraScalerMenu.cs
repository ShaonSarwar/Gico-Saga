using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[ExecuteInEditMode]
public class CameraScalerMenu : MonoBehaviour
{
    public event EventHandler OnCameraResizedWithScreen;
    // Reference Aspect Ratio
    [SerializeField] private float targetaspect = 16.0f / 9.0f;
    [SerializeField] private CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
    private float scaleheight;
    public bool executeInEditor;

    private bool isRuntime;
    private Camera mCamera;

    // determine the game window's current aspect ratio
    private float windowaspect;
    void Awake()
    {
        isRuntime = true;
        SetCameraRectWithScreen(isRuntime);
    }

    private void SetCameraRectWithScreen(bool isRuntime)
    {
        mCamera = GetComponent<Camera>();
        mCamera.clearFlags = clearFlags;
        windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        scaleheight = windowaspect / targetaspect;

        // if scaled height is less than current height, add letterbox
        if (scaleheight > 1.0f)
        {
            Rect rect = mCamera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            mCamera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = mCamera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            mCamera.rect = rect;
        }


        if (isRuntime)
        {
            OnCameraResizedWithScreen?.Invoke(this, EventArgs.Empty);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (executeInEditor)
        {
            SetCameraRectWithScreen(isRuntime);
        }
    }
#endif
}
