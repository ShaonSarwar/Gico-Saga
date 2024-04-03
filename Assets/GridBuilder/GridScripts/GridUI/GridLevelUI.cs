using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class GridLevelUI : MonoBehaviour
{
    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private GridLogicVisual gridLogicVisual; 
    [SerializeField] private UserData userData;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject levelFailedPanel; 
    [SerializeField] private GameObject spaceship;                   // Spaceship 
    [SerializeField] private GridController gridController;
    //[SerializeField] private GridDataManager gridDataManager;
    //[SerializeField] private GameplayShop gameplayShop;
    [SerializeField] private TacticalBooster tacticalBooster; 
    [SerializeField] private Camera mCamera;
    [SerializeField] private Text stateObservation;
    [SerializeField] private TextMeshProUGUI levelObjectiveCount;
    [SerializeField] private Image levelObjectiveIcon;
    [SerializeField] private GameObject rewardedPanel;
    [SerializeField] private Image rewardedBoosterSprite;
    [SerializeField] private List<Sprite> gameplayBoosterSprites;
    [SerializeField] private GameObject rewardedGameplayBoosterAdButton; 

    [SerializeField] private TextMeshProUGUI scoreText; 
    [SerializeField] private TextMeshProUGUI moveCountText;

    [SerializeField] private TextMeshProUGUI levelCompleteScoreText;
    [SerializeField] private TextMeshProUGUI levelFailedScoreText; 
   

    [SerializeField] private TextMeshProUGUI handBoosterCountText;
    [SerializeField] private TextMeshProUGUI hammerBoosterCountText;
    [SerializeField] private TextMeshProUGUI shuffleBoosterCountText;

    [SerializeField] private SpaceshipSO spaceshipList; 

    // Booster Panel Idol 
    // Hand Booster 
    [SerializeField] private GameObject handCount;
    [SerializeField] private GameObject handPurchaseBtn;
    [SerializeField] private GameObject handUseBtn;
    // Hammer Booster 
    [SerializeField] private GameObject hammerCount;
    [SerializeField] private GameObject hammerPurchaseBtn;
    [SerializeField] private GameObject hammerUseBtn;
    // Altar Booster 
    [SerializeField] private GameObject altarCount;
    [SerializeField] private GameObject altarPurchaseBtn;
    [SerializeField] private GameObject altarUseBtn;

    // Progress Bar 
    [SerializeField] private GameObject _progressBarPanelObject;
    [SerializeField] private Image _progressBarObject; 


    private int rewardedBoosterId;

    [SerializeField] private bool unlimitedAdsInGameplay;
    private int adsShownInGameplayCount; 

    private void Awake()
    {
        //if (SceneManager.GetActiveScene().buildIndex == 1)
        //{
        //    ProgressBarManager.Instance._progressBarPanel = _progressBarPanelObject;
        //    ProgressBarManager.Instance._progressBar = _progressBarObject; 
        //}
        adsShownInGameplayCount = 0; 
        rewardedBoosterId = -1;
        userData.OnUserDataInitialize += UserData_OnUserDataInitialize;
        //gridLogic.OnLevelSet += GridLogic_OnLevelSet;
    }

    private void GridLogic_OnLevelSet(object sender, GridLogic.OnLevelSetEventArgs e)
    {
       
    }

    private void Instance_OnInterstitialAdClosed(object sender, EventArgs e)
    {
        ProgressBarManager.Instance.LoadScene(0); 
    }

    private void UserData_OnUserDataInitialize(object sender, EventArgs e)
    {
        Setup();
    }

    private void Setup()
    {
        spaceship.GetComponent<SpriteRenderer>().sprite = spaceshipList.spaceshipList[userData.GetSpaceshipIndex()]; 
        UpdateText();
        userData.OnUserDataChanged += UserData_OnUserDataChanged;
        //userData.FireDataChangedEvent();

        gridLogic.OnScoreChanged += GridLogic_OnScoreChanged;
        gridLogic.OnMoveUsed += GridLogic_OnMoveUsed;
        gridLogic.OnOneLayerCellDestroyed += GridLogic_OnOneLayerCellDestroyed;

        gridLogicVisual.OnGameOver += GridLogicVisual_OnGameOver;
        gridLogicVisual.OnStateChanged += GridLogicVisual_OnStateChanged;

        //gameplayShop.OnGameplayShopPurchaseCompleted += GameplayShop_OnGameplayShopPurchaseCompleted;
        //MediationManager.Instance.OnInterstitialAdClosed += Instance_OnInterstitialAdClosed;
        //MediationManager.Instance.OnGameplayBoosterRewardedAdClosed += Instance_OnGameplayBoosterRewardedAdClosed;
        //gridDataManager.OnInventoryItemUpdateUI += GridDataManager_OnInventoryItemUpdateUI;
        tacticalBooster.OnUseTacticalBooster += TacticalBooster_OnUseTacticalBooster;
        Transform canvasTransform = FindObjectOfType<Canvas>().transform;
        ProgressBarManager.Instance._progressBarPanel = canvasTransform.GetChild(0).Find("ProgressBarPanel").gameObject;
        ProgressBarManager.Instance._progressBar = canvasTransform.GetChild(0).Find("ProgressBarPanel").GetChild(0).GetChild(0).GetComponent<Image>();
    }

    private void TacticalBooster_OnUseTacticalBooster(object sender, EventArgs e)
    {
        UpdateText(); 
    }

    private void GridDataManager_OnInventoryItemUpdateUI(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void Update()
    {
        if (!unlimitedAdsInGameplay)
        {
            if (adsShownInGameplayCount > 0)
            {
                rewardedGameplayBoosterAdButton.SetActive(false); 
            }
        }
    }

    //private void Instance_OnGameplayBoosterRewardedAdClosed(object sender, MediationManager.OnGameplayBoosterRewardedAdClosedEventArgs e)
    //{
    //    rewardedBoosterSprite.sprite = gameplayBoosterSprites[e.boosterId]; 
    //    rewardedPanel.SetActive(true);
    //    rewardedBoosterId = e.boosterId; 
    //}

    private void GameplayShop_OnGameplayShopPurchaseCompleted(object sender, EventArgs e)
    {
        UpdateText();
    }

    private void GridLogic_OnOneLayerCellDestroyed(object sender, EventArgs e)
    {
        UpdateText(); 
    }

    private void GridLogicVisual_OnStateChanged(object sender, EventArgs e)
    {
        stateObservation.text = gridLogicVisual.GetState().ToString();
        Debug.Log(gridLogicVisual.GetState().ToString()); 
    }

    private void GridLogic_OnScoreChanged(object sender, EventArgs e)
    {
        UpdateText(); 
    }

    private void GridLogicVisual_OnGameOver(object sender, EventArgs e)
    {
        if (gridLogic.TryIsGameOver())
        {
            if (gridLogic.IsLevelWin())
            {
                spaceship.GetComponent<Animation>().Play();
                FunctionTimer.Create(() =>
                {
                    levelCompletePanel.SetActive(true);
                    levelCompleteScoreText.text = gridLogic.GetScore().ToString(); 

                    if (gridLogic.GetLevelIndex() == userData.GetLevelIndex())
                    {
                        int nextLevel = userData.GetLevelIndex() + 1;
                        userData.SetLevelIndex(nextLevel);
                    }
                }, 0.25f);                                            
            }
            else
            {
                levelFailedPanel.SetActive(true);
                levelFailedScoreText.text = gridLogic.GetScore().ToString(); 
            }
        }
    }

    private void UserData_OnUserDataChanged(object sender, EventArgs e)
    {
        UpdateText(); 
    }

    private void GridLogic_OnMoveUsed(object sender, EventArgs e)
    {
        UpdateText(); 
    }

    private void UpdateText()
    {
        moveCountText.text = gridLogic.GetMoveCount().ToString();
        handBoosterCountText.text = userData.GetHandBoosterMount().ToString();
        hammerBoosterCountText.text = userData.GetHammerBoosterMount().ToString();
        shuffleBoosterCountText.text = userData.GetShuffleBoosterMount().ToString(); 
        scoreText.text = gridLogic.GetScore().ToString();
        levelObjectiveCount.text = gridLogic.GetRemainingCellToCollect().ToString();
        UpdateBoosterPanelUI(); 
    }

    // Setup Booster Panel UI 
    private void UpdateBoosterPanelUI()
    {
        // Hand
        if (userData.GetHandBoosterMount() > 0)
        {
            handPurchaseBtn.SetActive(false);
            handCount.SetActive(true);
            handUseBtn.SetActive(true);
        }
        else
        {
            handPurchaseBtn.SetActive(true);
            handCount.SetActive(false);
            handUseBtn.SetActive(false);
        }

        // Hammer 
        if (userData.GetHammerBoosterMount() > 0)
        {
            hammerPurchaseBtn.SetActive(false);
            hammerCount.SetActive(true);
            hammerUseBtn.SetActive(true);
        }
        else
        {
            hammerPurchaseBtn.SetActive(true);
            hammerCount.SetActive(false);
            hammerUseBtn.SetActive(false);
        }

        // Altar
        if (userData.GetShuffleBoosterMount() > 0)
        {
            altarPurchaseBtn.SetActive(false);
            altarCount.SetActive(true);
            altarUseBtn.SetActive(true);
        }
        else
        {
            altarPurchaseBtn.SetActive(true);
            altarCount.SetActive(false);
            altarUseBtn.SetActive(false);
        }
    }

    // Next Level Button Functionality 
    public void BackToMenuOnButtonClick()
    {
        //MediationManager.Instance.ShowAdmobInterstial();
        ProgressBarManager.Instance.LoadScene(0);
    }


    public void SetRewardedBoosterInBoard()
    {
        //gridLogic.ReplaceItemWithBoosterDuringGameplay(rewardedBoosterId);
        rewardedBoosterId = -1; 
    }
}
