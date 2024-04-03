using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MenuSceneUIScaler : MonoBehaviour
{
    public bool executeInEditor;
    RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        safeArea = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (executeInEditor)
        {
            rectTransform = GetComponent<RectTransform>();

            safeArea = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height); 
            minAnchor = safeArea.position;
            maxAnchor = minAnchor + safeArea.size;
            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
#endif 
}
