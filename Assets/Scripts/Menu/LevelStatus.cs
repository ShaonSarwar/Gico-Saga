using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class LevelStatus : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    //[SerializeField] private ServerManager serverManager; 
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Camera mCamera;
    [SerializeField] private PlayerData playerData; 

    private CircleCollider2D levelCollider; 
    private int levelIndex;
    private bool isLevelHide;
    private bool isLevelSelected; 

    private void Awake()
    {
        levelCollider = GetComponent<CircleCollider2D>(); 
        isLevelHide = true;
        isLevelSelected = false; 
        //serverManager.OnServerCallCompleted += ServerManager_OnServerCallCompleted;
        playerData.OnDataChanged += PlayerData_OnDataChanged;
    }

    private void PlayerData_OnDataChanged(object sender, EventArgs e)
    {
        if (int.TryParse(this.gameObject.name.ToString(), out levelIndex))
        {
            SetLevelStatus();
        }
    }

    private void ServerManager_OnServerCallCompleted(object sender, EventArgs e)
    {
        if (int.TryParse(this.gameObject.name.ToString(), out levelIndex))
        {
            SetLevelStatus();
        }
    }

    private void Update()
    {
        if (!startPanel.activeInHierarchy) {
            isLevelSelected = false;
        }
        else
        {
            isLevelSelected = true; 
        } 
        
        if (isLevelHide) return;
        if (isLevelSelected) return;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
            Vector2 pos = new Vector2(touchPosition.x, touchPosition.y);

            if (levelCollider == Physics2D.OverlapPoint(pos))
            {
                PlayerPrefs.SetInt("PlayerLevel", levelIndex);
                //Debug.Log("Load Level");
                startPanel.SetActive(true);
                isLevelSelected = true; 
            }
        }

    }

    public void SetLevelStatus()
    {
        if (levelIndex <= gameManager.GetCurrentLevel())
        {
            transform.Find("Hide").gameObject.SetActive(false);
            isLevelHide = false; 
        }
        else
        {
            transform.Find("Hide").gameObject.SetActive(true);
            isLevelHide = true; 
        }
    }
}
