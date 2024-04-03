using UnityEngine;

public class UpdatedLevelScrollManager : MonoBehaviour
{
    [SerializeField] private Transform cameraObject; 
    [SerializeField] private Transform cameraLeftLimit;
    [SerializeField] private Transform cameraRightLimit;
    [SerializeField] private Transform endPosition;
    [SerializeField] private Transform startPosition; 
    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private float velocityRange = 5.0f;
    [SerializeField] private float smoothTime = 0.1f;

    [SerializeField] private Transform levelLayer;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject shopPanel;

    private Camera mainCamera;
    private bool isDragging;
    private Vector3 touchStartPosition;
    private Vector3 touchCurrentPosition;
    private Vector3 velocity = Vector3.zero;
    private float touchDuration;
    private Vector3 targetPosition; 

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void ServerManager_OnServerCallCompleted(object sender, System.EventArgs e)
    {
        int levelIndex = playerData.GetLevelIndex();
        if (levelLayer.Find(levelIndex.ToString()) != null)
        {
            cameraObject.transform.position = new Vector3(levelLayer.Find(levelIndex.ToString()).position.x, cameraObject.transform.position.y, cameraObject.transform.position.z);
        }
    }

    private void Update()
    {
        if (!ScrollCheeklist()) return; 
        if (Input.touchCount < 1) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isDragging = true;
                touchStartPosition = GravitimeUtility.GetHorizontalTouchPosition(mainCamera, touch.position);
                touchDuration = 0.0f;
                targetPosition = Vector3.zero;
                break;
            case TouchPhase.Moved:
                if (!isDragging) break;

                touchCurrentPosition = GravitimeUtility.GetHorizontalTouchPosition(mainCamera, touch.position - touch.deltaPosition);
                touchDuration = Time.deltaTime; 

                Vector3 delta = touchStartPosition - touchCurrentPosition;
                delta *= cameraSensitivity;

                if (Mathf.Abs(delta.x) > velocityRange) delta = delta.normalized * velocityRange;

                //delta *= Time.deltaTime;

                targetPosition = cameraObject.position + new Vector3(delta.x, 0f, 0f);
                targetPosition.x = Mathf.Clamp(targetPosition.x, cameraLeftLimit.position.x - startPosition.position.x, endPosition.position.x - cameraRightLimit.position.x);

                if (touchDuration > 0.03f)
                {
                    // For long swipes, add some additional distance to the target position
                    float longSwipeDelta = 3f;
                    if (targetPosition.x > 0) targetPosition.x += longSwipeDelta;
                    else targetPosition.x -= longSwipeDelta;
                }

                //if (targetPosition.x >= endPosition.position.x)
                //    targetPosition = new Vector3(endPosition.position.x, targetPosition.y, targetPosition.z);
                //{
                //}

                //if (targetPosition.x <= startPosition.position.x)
                //{
                //    targetPosition = new Vector3(startPosition.position.x, targetPosition.y, targetPosition.z); 
                //}

                cameraObject.position = Vector3.Lerp(cameraObject.position, targetPosition, cameraSensitivity * Time.deltaTime);

                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                velocity = Vector3.zero;
                touchDuration = 0.0f;
                targetPosition = Vector3.zero; 
                break;
            default:
                break;
        }
    }

    private Vector3 GetHorizontalTouchPosition(Vector2 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        Plane plane = new Plane(Vector3.up, mainCamera.transform.position);
        plane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    private bool ScrollCheeklist()
    {
        if (settingPanel.activeInHierarchy || inventoryPanel.activeInHierarchy || shopPanel.activeInHierarchy || startPanel.activeInHierarchy) return false;
        return true;
    }
}
