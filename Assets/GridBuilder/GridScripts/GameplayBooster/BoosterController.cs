using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoosterController : MonoBehaviour
{
    [SerializeField] private GridLogic gridLogic;
    [SerializeField] private GridLogicVisual gridLogicVisual;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SoundCollection soundSFXCollection;
    [SerializeField] private Gridpooler gridpooler;

    private Dictionary<GridItem, StrippedBooster> strippedBoosterDictionary;
    private Dictionary<GridItem, PowerBooster> powerBoosterDictionary;
    private Dictionary<GridItem, WrappedBooster> wrappedBoosterDictionary;

    private int stripped;
    private int wrapped;
    private int power; 

    private void Awake()
    {
        strippedBoosterDictionary = new Dictionary<GridItem, StrippedBooster>();
        powerBoosterDictionary = new Dictionary<GridItem, PowerBooster>();
        wrappedBoosterDictionary = new Dictionary<GridItem, WrappedBooster>(); 
        gridLogicVisual.OnRegisterBooster += GridLogicVisual_OnRegisterBooster;
    }

    private void GridLogicVisual_OnRegisterBooster(object sender, GridLogicVisual.OnRegisterBoosterEventArgs e)
    {
        int boosterID = e.gridItem.GetBoosterID();
        switch (boosterID)
        {
            case 1:
                StrippedBooster strippedBooster = new StrippedBooster(e.grid, gridLogic, gridLogicVisual, gridpooler);
                strippedBoosterDictionary[e.gridItem] = strippedBooster;
                stripped += 1;
                break;
            case 2:
                WrappedBooster wrappedBooster = new WrappedBooster(e.grid, gridLogic, gridLogicVisual, gridpooler);
                wrappedBoosterDictionary[e.gridItem] = wrappedBooster;
                wrapped += 1;
                break;
            case 3:
                PowerBooster powerBooster = new PowerBooster(e.grid, gridLogic, gridLogicVisual, gridpooler);
                powerBoosterDictionary[e.gridItem] = powerBooster;
                power += 1;
                break;
            default:
                break;
        }
    }

    public void ActivateBooster(GridItem gridItem, int boosterID, GridItem swapItem)
    {
        switch (boosterID)
        {
            case 1:
                for (int i = 0; i < strippedBoosterDictionary.Keys.Count; i++)
                {
                    if (strippedBoosterDictionary.ContainsKey(gridItem))
                    {
                        StrippedBooster strippedBooster = strippedBoosterDictionary[gridItem];
                        if (strippedBooster != null)
                        {
                            strippedBooster.TryDestroyStrippedBooster(gridItem);
                            audioManager.PlayOneShotSound(
                                soundSFXCollection.StrippedBoosterBlastSFX.AudioGroup,
                                soundSFXCollection.StrippedBoosterBlastSFX.audioClip,
                                gridItem.GetWorldPosition(),
                                soundSFXCollection.StrippedBoosterBlastSFX.Volume,
                                soundSFXCollection.StrippedBoosterBlastSFX.SpatialBlend
                                );
                            strippedBoosterDictionary.Remove(gridItem);
                            Debug.Log("Activate Stripped"); 
                            //break;
                        }
                    }
                }
                break;
            case 2:
                for (int i = 0; i < wrappedBoosterDictionary.Keys.Count; i++)
                {
                    if (wrappedBoosterDictionary.ContainsKey(gridItem))
                    {
                        WrappedBooster wrappedBooster = wrappedBoosterDictionary[gridItem];
                        if (wrappedBooster != null)
                        {
                            wrappedBooster.TryDestroyWrappedBooster(gridItem);
                            audioManager.PlayOneShotSound(
                                soundSFXCollection.WrappedBoosterBlastSFX.AudioGroup,
                                soundSFXCollection.WrappedBoosterBlastSFX.audioClip,
                                gridItem.GetWorldPosition(),
                                soundSFXCollection.WrappedBoosterBlastSFX.Volume,
                                soundSFXCollection.WrappedBoosterBlastSFX.SpatialBlend
                                );
                            wrappedBoosterDictionary.Remove(gridItem);
                            Debug.Log("Activate Wrapped");
                            //break; 
                        }
                    }
                }
                break;
            case 3:
                for (int i = 0; i < powerBoosterDictionary.Keys.Count; i++)
                {
                    if (powerBoosterDictionary.ContainsKey(gridItem))
                    {
                        PowerBooster powerBooster = powerBoosterDictionary[gridItem];
                        if (powerBooster != null)
                        {
                            powerBooster.TryDestroyPowerBooster(gridItem, swapItem);
                            audioManager.PlayOneShotSound(
                                soundSFXCollection.PowerBoosterBlastSFX.AudioGroup,
                                soundSFXCollection.PowerBoosterBlastSFX.audioClip,
                                gridItem.GetWorldPosition(),
                                soundSFXCollection.PowerBoosterBlastSFX.Volume,
                                soundSFXCollection.PowerBoosterBlastSFX.SpatialBlend
                                );
                            powerBoosterDictionary.Remove(gridItem);
                            Debug.Log("Activate Power");
                            //break;
                        }
                    }
                }
                break; 
            default:
                break;
        }
        //switch (boosterID)
        //{
        //    case 1:
        //        foreach (GridItem item in strippedBoosterDictionary.Keys)
        //        {
        //            if (item.GetX() == gridItem.GetX() && item.GetY() == gridItem.GetY())
        //            {
        //                StrippedBooster strippedBooster = strippedBoosterDictionary[item];
        //                if (strippedBooster != null)
        //                {
        //                    strippedBooster.TryDestroyStrippedBooster(item);
        //                    audioManager.PlayOneShotSound(
        //                        soundSFXCollection.StrippedBoosterBlastSFX.AudioGroup,
        //                        soundSFXCollection.StrippedBoosterBlastSFX.audioClip,
        //                        item.GetWorldPosition(),
        //                        soundSFXCollection.StrippedBoosterBlastSFX.Volume,
        //                        soundSFXCollection.StrippedBoosterBlastSFX.SpatialBlend
        //                        );
        //                    strippedBoosterDictionary.Remove(item);
        //                    //break;
        //                }
        //            }
        //        }
        //        break;
        //    case 2:
        //        foreach (GridItem item in wrappedBoosterDictionary.Keys)
        //        {
        //            if (item.GetX() == gridItem.GetX() && item.GetY() == gridItem.GetY())
        //            {
        //                WrappedBooster wrappedBooster = wrappedBoosterDictionary[item];
        //                if (wrappedBooster != null)
        //                {
        //                    wrappedBooster.TryDestroyWrappedBooster(item);
        //                    audioManager.PlayOneShotSound(
        //                        soundSFXCollection.WrappedBoosterBlastSFX.AudioGroup, 
        //                        soundSFXCollection.WrappedBoosterBlastSFX.audioClip, 
        //                        item.GetWorldPosition(), 
        //                        soundSFXCollection.WrappedBoosterBlastSFX.Volume, 
        //                        soundSFXCollection.WrappedBoosterBlastSFX.SpatialBlend
        //                        );
        //                    wrappedBoosterDictionary.Remove(item);
        //                    //break; 
        //                }
        //            }
        //        }
        //        break;
        //    case 3:
        //        foreach (GridItem item in powerBoosterDictionary.Keys)
        //        {
        //            if (item.GetX() == gridItem.GetX() && item.GetY() == gridItem.GetY())
        //            {
        //                PowerBooster powerBooster = powerBoosterDictionary[item];
        //                if (powerBooster != null)
        //                {
        //                    powerBooster.TryDestroyPowerBooster(item, swapItem);
        //                    audioManager.PlayOneShotSound(
        //                        soundSFXCollection.PowerBoosterBlastSFX.AudioGroup,
        //                        soundSFXCollection.PowerBoosterBlastSFX.audioClip,
        //                        item.GetWorldPosition(),
        //                        soundSFXCollection.PowerBoosterBlastSFX.Volume,
        //                        soundSFXCollection.PowerBoosterBlastSFX.SpatialBlend
        //                        );
        //                    powerBoosterDictionary.Remove(item);
        //                    //break;
        //                }
        //            }
        //        }
        //        break; 
        //    default:
        //        break;
        //}      
    }
}
