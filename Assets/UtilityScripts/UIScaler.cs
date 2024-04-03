using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[ExecuteInEditMode]
public class UIScaler : MonoBehaviour
{
    public event EventHandler OnUICanvasResizeWithScreen;
    [SerializeField] private CameraScaler cameraScaler;
    public bool executeInEditor;
    public bool showRes;
    private bool isRuntime; 
    RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;
    private void Awake()
    {
        cameraScaler.OnCameraResizedWithScreen += CameraScaler_OnCameraResizedWithScreen;
       
    }

    private void CameraScaler_OnCameraResizedWithScreen(object sender, EventArgs e)
    {
        isRuntime = true;
        FitCanvasWithScreen(isRuntime);
    }

    private void FitCanvasWithScreen(bool isRuntime)
    {
        rectTransform = GetComponent<RectTransform>();

        safeArea = Screen.safeArea;
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
        if (isRuntime)
        {
            OnUICanvasResizeWithScreen?.Invoke(this, EventArgs.Empty); 
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (executeInEditor) FitCanvasWithScreen(isRuntime);
        if (showRes)
        {
            Debug.Log("Screen Height: " + Screen.height);
            Debug.Log("Screen Width: " + Screen.width);
        }     
    }
#endif
}
