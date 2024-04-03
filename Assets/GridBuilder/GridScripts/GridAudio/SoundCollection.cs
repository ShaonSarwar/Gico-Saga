using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class contain all of GridAudioSO 
[CreateAssetMenu()]
public class SoundCollection : ScriptableObject
{
    // Gameplay SFX 
    [SerializeField] private GridAudioSO boosterSpawnSFX;
    [SerializeField] private GridAudioSO hammerSFX;
    [SerializeField] private GridAudioSO insulatorDamageSFX; 
    [SerializeField] private GridAudioSO insulatorDestroySFX;
    [SerializeField] private GridAudioSO itemBlastSFX;
    [SerializeField] private GridAudioSO itemFallSFX;
    [SerializeField] private GridAudioSO levelFailedSFX;
    [SerializeField] private GridAudioSO levelSuccessSFX;
    [SerializeField] private GridAudioSO powerBoosterBlastSFX;
    [SerializeField] private GridAudioSO registerDestroySFX;
    [SerializeField] private GridAudioSO registerSpawnSFX;
    [SerializeField] private GridAudioSO swapSFX;
    [SerializeField] private GridAudioSO strippedBoosterBlastSFX;
    [SerializeField] private GridAudioSO wrappedBoosterBlastSFX; 
    [SerializeField] private GridAudioSO spaceshipEngineSFX;
    [SerializeField] private GridAudioSO thalesHandSFX;


    // Menu or UI SFX 
    [SerializeField] private GridAudioSO buttonPressedSFX;
    [SerializeField] private GridAudioSO panelOpenClosedSFX; 


    public GridAudioSO BoosterSpawnSFX { get { return boosterSpawnSFX; } }
    public GridAudioSO HammerSFX { get { return hammerSFX; } }
    public GridAudioSO InsulatorDamageSFX { get { return insulatorDamageSFX; } }
    public GridAudioSO InsulatorDestroySFX { get { return insulatorDestroySFX; } }
    public GridAudioSO ItemBlastSFX { get { return itemBlastSFX; } }
    public GridAudioSO ItemFallSFX { get { return itemFallSFX; } }
    public GridAudioSO LevelFailedSFX { get { return levelFailedSFX; } }
    public GridAudioSO LevelSuccessSFX { get { return levelSuccessSFX; } }
    public GridAudioSO PowerBoosterBlastSFX { get { return powerBoosterBlastSFX; } }
    public GridAudioSO RegisterDestroySFX { get { return registerDestroySFX; } }
    public GridAudioSO RegisterSpawnSFX { get { return registerSpawnSFX; } }
    public GridAudioSO SwapSFX { get { return swapSFX; } }
    public GridAudioSO StrippedBoosterBlastSFX { get { return strippedBoosterBlastSFX; } }
    public GridAudioSO WrappedBoosterBlastSFX { get { return wrappedBoosterBlastSFX; } }
    public GridAudioSO SpaceshipEngineSFX { get { return spaceshipEngineSFX; } }
    public GridAudioSO ThalesHandSFX { get { return thalesHandSFX; } }
    public GridAudioSO ButtonPressedSFX { get { return buttonPressedSFX; } }
    public GridAudioSO PanelOpenCLosedSFX { get { return panelOpenClosedSFX; } }
}
