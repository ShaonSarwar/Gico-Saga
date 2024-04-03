using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class LevelScrollController : MonoBehaviour
{
    [SerializeField] private Camera mCamera;
    [SerializeField] private GameObject cameraObject; 
    [SerializeField] private float cameraSensitivity;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;
    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Transform levelLayer;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject startPanel; 
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameManager gameManager;
    //[SerializeField] private ServerManager serverManager; 

    protected Plane plane;
    private float leftHorizontal;
    private float rightHorizontal;
    private bool leftEnd;
    private bool righEnd;
    private float current;
    private float target;

    private Vector3 velocity = new Vector3(5f, 0f, 0f); 
    public float smoothTime = 0.3f;
    public Vector3 rayDifference;
    public float velocityRange = 10f;
    private Vector3 delta1;

    private void Awake()
    {
        //serverManager.OnServerCallCompleted += ServerManager_OnServerCallCompleted;       
    }

    private void ServerManager_OnServerCallCompleted(object sender, EventArgs e)
    {
        Input.multiTouchEnabled = false;
        leftHorizontal = startPosition.position.x;
        rightHorizontal = endPosition.position.y;
       
        int levelIndex = playerData.GetLevelIndex();
        if (levelLayer.Find(levelIndex.ToString()) != null)
        {
            cameraObject.transform.position = new Vector3(levelLayer.Find(levelIndex.ToString()).position.x, cameraObject.transform.position.y, cameraObject.transform.position.z);
        }
    }

    private void Update()
    {
        if (!ScrollCheeklist()) return;
        if (left.position.x < startPosition.position.x)
        {
            leftEnd = true;
        }
        else
        {
            leftEnd = false;
        }

        if (right.position.x > endPosition.position.x)
        {
            righEnd = true;
        }
        else
        {
            righEnd = false;
        }

        delta1 = Vector3.zero;

        //Scroll
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            delta1 = TouchPositionDelta(touch);
            rayDifference = delta1; 
            float deltaX = Mathf.Clamp(delta1.x, cameraObject.transform.position.x - startPosition.position.x, endPosition.position.x - cameraObject.transform.position.x);
            delta1 = new Vector3(deltaX, delta1.y, delta1.z);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    break;
                case TouchPhase.Moved:
                    current = 0;
                    target = 0;
                    if (rayDifference.x > 0 && righEnd) return;
                    if (rayDifference.x < 0 && leftEnd) return;

                    target = target == 0 ? 1 : 0;
                    current = Mathf.MoveTowards(current, target, Time.deltaTime * cameraSensitivity);

                    velocity = new Vector3(Mathf.Clamp(velocity.x, -10f, 10f), velocity.y, velocity.z);
                    if (rayDifference.x > 0f)
                    {
                        if (velocity.x < 5f)
                        {
                            velocity = new Vector3(5f, velocity.y, velocity.z);
                        }

                        cameraObject.transform.position = Vector3.SmoothDamp(cameraObject.transform.position, cameraObject.transform.position + delta1, ref velocity, smoothTime);
                        //cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, cameraObject.transform.position + delta1, Time.deltaTime * Time.deltaTime);
                        delta1 = Vector3.zero;
                        //velocity = new Vector3(5f, velocity.y, velocity.z);
                    }
                    else
                    {
                        if (velocity.x > -5f)
                        {
                            velocity = new Vector3(-5f, velocity.y, velocity.z);
                        }

                        cameraObject.transform.position = Vector3.SmoothDamp(cameraObject.transform.position, cameraObject.transform.position - delta1, ref velocity, smoothTime);
                        //cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position, cameraObject.transform.position - delta1, Time.deltaTime * Time.deltaTime);
                        delta1 = Vector3.zero;
                        //velocity = new Vector3(-5f, velocity.y, velocity.z);
                    }
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:               
                    break;
                default:
                    break;
            }
        }
    }


    protected Vector3 TouchPositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = GravitimeUtility.GetHorizontalTouchPosition(mCamera, touch.position - touch.deltaPosition);
        var rayNow = GravitimeUtility.GetHorizontalTouchPosition(mCamera, touch.position);
        return rayBefore - rayNow;
    }

    private bool ScrollCheeklist()
    {
        if (settingPanel.activeInHierarchy || inventoryPanel.activeInHierarchy || shopPanel.activeInHierarchy || startPanel.activeInHierarchy) return false;
        return true; 
    }
}
