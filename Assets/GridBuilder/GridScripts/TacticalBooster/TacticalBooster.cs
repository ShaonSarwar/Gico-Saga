using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class TacticalBooster : MonoBehaviour
{
    public event EventHandler OnUseTacticalBooster;
    public enum TacticalBoosterUsingState { None, Hand, Hammer, Shuffle} 


    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private GridLogicVisual gridLogicVisual;
    [SerializeField] private UserData userData;
    [SerializeField] private Camera mCamera;
    [SerializeField] private SpriteRenderer board;
    //[SerializeField] private GridDataManager gridDataManager; 

    // Booster Prefabs 
    [SerializeField] private Transform handBoosterPrefab;
    [SerializeField] private Transform hammerBoosterPrefab;
    [SerializeField] private Transform shuffleBoosterPrefab; 

    // Animation Clips 
    [SerializeField] private AnimationClip handHorizontalClip;
    [SerializeField] private AnimationClip handVerticalClip;

    // UI Refferences
    [SerializeField] private Image glowHand;
    [SerializeField] private Image glowHammer;
    [SerializeField] private Image glowShuffle;
    [SerializeField] private GameObject shuffleButton; 


    // fields 
    private int startDragX, startDragY, endX, endY;
    private Vector3 startPosition, endPosition;
    private bool usingTacticalBooster;
    private bool usingHandBooster;
    private bool usingHammerBooster;
    private bool usingShuffleBooster;
    private bool handBoosterHorizontalSwap;
    private TacticalBoosterUsingState state; 

    public bool UsingTacticalBooster { get { return usingTacticalBooster; } }

    private void Awake()
    {
        Input.multiTouchEnabled = false; 
    }

    private void Update()
    {
        if (gridLogicVisual.GetState() != GridLogicVisual.State.WaitingForUser) return; 
        if (gridLogic.TryIsGameOver()) return; 
        if (!usingTacticalBooster) return; 
        switch (state)
        {
            case TacticalBoosterUsingState.None:
                break;
            case TacticalBoosterUsingState.Hand:
                if (usingHandBooster)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (!board.bounds.Contains(GravitimeUtility.GetTouchPosition(mCamera, touch.position))) 
                        {
                            Debug.Log("Outside of Board");
                        }
                        else
                        {
                            switch (touch.phase)
                            {
                                case TouchPhase.Began:
                                    Vector3 startTouchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
                                    gridLogic.GetGridXY(startTouchPosition, out startDragX, out startDragY);
                                    startPosition = startTouchPosition;
                                    break;
                                case TouchPhase.Moved:
                                    break;
                                case TouchPhase.Stationary:
                                    break;
                                case TouchPhase.Ended:
                                case TouchPhase.Canceled:
                                    Vector3 endTouchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
                                    gridLogic.GetGridXY(endTouchPosition, out endX, out endY);

                                    if (endX != startDragX)
                                    {
                                        // different X 
                                        handBoosterHorizontalSwap = true; 
                                        endY = startDragY;
                                        if (endX < startDragX)
                                        {
                                            endX = startDragX - 1;
                                        }
                                        else
                                        {
                                            endX = startDragX + 1;
                                        }
                                    }
                                    else
                                    {
                                        // different Y 
                                        handBoosterHorizontalSwap = false;
                                        endX = startDragX;
                                        if (endY < startDragY)
                                        {
                                            endY = startDragY - 1;
                                        }
                                        else
                                        {
                                            endY = startDragY + 1;
                                        }
                                    }

                                    if (gridLogic.CanSwapGridItemsByTacticalBooster(startDragX, startDragY, endX, endY))
                                    {
                                        CreateHandBooster();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        
                    }
                }
                break;
            case TacticalBoosterUsingState.Hammer:
                if (usingHammerBooster)
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (!board.bounds.Contains(GravitimeUtility.GetTouchPosition(mCamera, touch.position)))
                        {
                            Debug.Log("Outside of board");
                        }
                        else
                        {
                            switch (touch.phase)
                            {
                                case TouchPhase.Began:
                                    Vector3 startTouchPosition = GravitimeUtility.GetTouchPosition(mCamera, touch.position);
                                    gridLogic.GetGridXY(startTouchPosition, out startDragX, out startDragY);

                                    if (gridLogic.HammerBoosterChecklist(startDragX, startDragY))
                                    {
                                        CreateHammerBooster();
                                    }
                                    break;
                                case TouchPhase.Moved:
                                    break;
                                case TouchPhase.Stationary:
                                    break;
                                case TouchPhase.Ended:
                                    break;
                                case TouchPhase.Canceled:
                                    break;
                                default:
                                    break;
                            }
                        }                      
                    }
                }
                break;
            case TacticalBoosterUsingState.Shuffle:
                if (usingShuffleBooster)
                {
                    CreateSHuffleBooster();
                }
                break;
            default:
                break;
        }
    }

    // Unity UI OnClick Button 
    public void ActivateHandBooster()
    {
        CancellHammerSelection();
        CancellShuffleSelection();
        usingHandBooster = !usingHandBooster; 
        if (usingHandBooster)
        {
            usingTacticalBooster = true;
            glowHand.gameObject.SetActive(true); 
            SetState(TacticalBoosterUsingState.Hand);
        }
        else
        {
            CancellAllBoosterSelection(); 
        }
    }

    public void ActivateHammerBooster()
    {
        CancellHandSelection();
        CancellShuffleSelection();
        usingHammerBooster = !usingHammerBooster; 
        if (usingHammerBooster)
        {
            usingTacticalBooster = true;
            glowHammer.gameObject.SetActive(true); 
            SetState(TacticalBoosterUsingState.Hammer);
        }
        else
        {
            CancellAllBoosterSelection(); 
        }
    } 

    public void ActivateShuffleBooster()
    {
        CancellHammerSelection();
        CancellHandSelection();
        usingShuffleBooster = !usingShuffleBooster; 
        if (usingShuffleBooster)
        {
            shuffleButton.SetActive(true); 
        }
        else
        {
            CancellAllBoosterSelection(); 
        }
    }

    public void CreateHandBooster()
    {
        userData.UseHandBooster();
        Transform handTransform = Instantiate(handBoosterPrefab, startPosition, Quaternion.identity);
        handTransform.transform.Find("HandBooster").transform.localPosition = startPosition;

        if (handBoosterHorizontalSwap)
        {
            handTransform.GetComponent<Animation>().clip = handHorizontalClip;
            handTransform.GetComponent<Animation>().AddClip(handHorizontalClip, "HandHorizontal");
            handTransform.GetComponent<Animation>().Play();
        }
        else
        {
            handTransform.GetComponent<Animation>().clip = handVerticalClip;
            handTransform.GetComponent<Animation>().AddClip(handVerticalClip, "HandVertical");
            handTransform.GetComponent<Animation>().Play();
        }
        CancellAllBoosterSelection();
        gridLogicVisual.SwapGridItemsWithBooster(startDragX, startDragY, endX, endY);
        Debug.Log($"Hand Grid Pos: StartX: {startDragX} StartY: {startDragY} EndX: {endX} EndY: {endY}");       
        Destroy(handTransform.gameObject, handVerticalClip.length);     
        OnUseTacticalBooster?.Invoke(this, EventArgs.Empty);
    }


    public void CreateHammerBooster()
    {
        userData.UseHammerBooster();
        GridItemPosition gridItemPosition = gridLogic.GetGridObject(startDragX, startDragY);
        Vector3 position = gridItemPosition.GetWorldPosition();
        Transform hammerTransform = Instantiate(hammerBoosterPrefab, position, Quaternion.identity);
        hammerTransform.transform.Find("Hammer").localPosition = position + new Vector3(0.5f, 0.5f);
        hammerTransform.GetComponent<Animation>().Play();
        float animationDuration = hammerTransform.GetComponent<Animation>().clip.length; 
        if (gridItemPosition.HasGridItem())
        {
            if (gridItemPosition.HasBooster())
            {
                gridLogic.TryDestroyBoosterAfterDelay(gridItemPosition);
            }
            else if (gridItemPosition.GetHasBlocker())
            {
                gridLogic.TryDestroyBlocker(gridItemPosition);
            }
            else if (gridItemPosition.IsRegister())
            {
                gridLogic.TryDestroyRegister(gridItemPosition);
            }
            else
            {
                gridLogic.TryDestroyGridItem(gridItemPosition);
            }

            gridLogicVisual.SetBusyState(animationDuration, () =>
            {
                gridLogicVisual.SetState(GridLogicVisual.State.Liazu);
            }, () => true); 
        }

        Destroy(hammerTransform.gameObject, animationDuration);
        CancellAllBoosterSelection();
        OnUseTacticalBooster?.Invoke(this, EventArgs.Empty);
    }

    public void MakeShuffle()
    {
        usingTacticalBooster = true;
        glowShuffle.gameObject.SetActive(true);
        SetState(TacticalBoosterUsingState.Shuffle);
    }

    public void CreateSHuffleBooster()
    {
        userData.UseShuffleBooster();
        Transform shuffleTransform = Instantiate(shuffleBoosterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        shuffleTransform.GetComponent<Animation>().Play(); 

        gridLogicVisual.SetBusyState(0.8f, () =>
        {
            gridLogicVisual.SetState(GridLogicVisual.State.Suffle);
        }, () => true);

        Destroy(shuffleTransform.gameObject, shuffleTransform.GetComponent<Animation>().clip.length); 
        CancellAllBoosterSelection();
        OnUseTacticalBooster?.Invoke(this, EventArgs.Empty);
    }

    public void SetState(TacticalBoosterUsingState state)
    {
        this.state = state; 
    }

    public void CancellAllBoosterSelection()
    {
        usingHandBooster = false;
        usingHammerBooster = false;
        usingShuffleBooster = false;
        usingTacticalBooster = false; 
        glowHand.gameObject.SetActive(false);
        glowHammer.gameObject.SetActive(false);
        glowShuffle.gameObject.SetActive(false);
        shuffleButton.SetActive(false); 
        SetState(TacticalBoosterUsingState.None); 
    }

    public void CancellHandSelection()
    {
        usingHandBooster = false;
        glowHand.gameObject.SetActive(false); 
    }

    public void CancellHammerSelection()
    {
        usingHammerBooster = false;
        glowHammer.gameObject.SetActive(false); 
    }

    public void CancellShuffleSelection()
    {
        usingShuffleBooster = false;
        glowShuffle.gameObject.SetActive(false);
        shuffleButton.SetActive(false); 
    }
}
