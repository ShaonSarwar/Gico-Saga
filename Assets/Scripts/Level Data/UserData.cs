using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class UserData : MonoBehaviour
{
    public event EventHandler OnUserDataInitialize; 
    public event EventHandler OnUserDataChanged;
    private int levelIndex;
    private int avatarIndex;
    private int spaceshipIndex;
    private float soundValue;
    private int gameCurrency;
    private string playerName;
    private int achievementIndex;
    private int weeklyLevelGained; 

    // Tactical Boosters Only Inventory Items which maybe usable in Gameplay/GridScene 
    private int strippedBoosterMount;
    private int wrappedBoosterMount;
    private int powerBoosterMount;
    private int handBoosterMount;
    private int hammerBoosterMount;
    private int shuffleBoosterMount;

    public int GetLevelIndex() { return levelIndex; }
    public int GetAvatarIndex() { return avatarIndex; }
    public int GetSpaceshipIndex() { return spaceshipIndex; }
    public float GetSoundValue() { return soundValue; }
    public int GetGameCurrency() { return gameCurrency; }
    public string GetPlayerName() { return playerName; }
    public int GetAchievementIndex() { return achievementIndex; }
    public int GetWeeklyLevelGained() { return weeklyLevelGained; }

    // Get Inventory Items 
    public int GetStrippedBoosterMount() { return strippedBoosterMount; }
    public int GetWrappedBoosterMount() { return wrappedBoosterMount; }
    public int GetPowerBoosterMount() { return powerBoosterMount; }
    public int GetHandBoosterMount() { return handBoosterMount; }
    public int GetHammerBoosterMount() { return hammerBoosterMount; }
    public int GetShuffleBoosterMount() { return shuffleBoosterMount; }

    public void FireInitializeEvent()
    {
        EventHandler handler = OnUserDataInitialize; 
        if (handler != null)
        {
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public void FireDataChangedEvent()
    {
        EventHandler handler = OnUserDataChanged; 
        if (handler != null)
        {
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetLevelIndex(int level) 
    {
        levelIndex = level;
    }

    public void SetWeeklyLevelGained(int levelCount)
    {
        this.weeklyLevelGained = levelCount;
    }
    public void SetAvatarIndex(int avatarIndex)
    {
        this.avatarIndex = avatarIndex;
    }
    public void SetSpaceshipIndex(int spaceshipIndex)
    {
        this.spaceshipIndex = spaceshipIndex;
    }
    public void SetSoundValue(float soundValue)
    {
        this.soundValue = soundValue;
    }
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }
    public void SetAchivementIndex(int achievementIndex)
    {
        this.achievementIndex = achievementIndex;
    }


    public void SetStrippedBoosterMount(int mount)
    {
        strippedBoosterMount = mount;
    }

    public void SetWrappedBoosterMount(int mount)
    {
        wrappedBoosterMount = mount;
    }

    public void SetPowerBoosterMount(int mount)
    {
        powerBoosterMount = mount;
    }

    public void SetHandBoosterMount(int mount)
    {
        handBoosterMount = mount;
    }

    public void SetHammerBoosterMount(int mount)
    {
        hammerBoosterMount = mount;
    }

    public void SetShuffleBoosterMount(int mount)
    {
        shuffleBoosterMount = mount;
    }

    public void IncreaseLevelIndex(int mount)
    {
        levelIndex += mount;
        FireDataChangedEvent(); 
    }

    public void IncrementWeeklyLevelGained(int count)
    {
        this.weeklyLevelGained += count;
        FireDataChangedEvent();
    }

    public void IncreaseGameCurrency(int currency) 
    {
        gameCurrency += currency;
        FireDataChangedEvent();
    }
    public void UseGameCurrency(int currency)
    {
        gameCurrency -= currency;
        FireDataChangedEvent(); 
    }
    public void IncreaseHandBooster(int mount)
    {
        handBoosterMount += mount;
        FireDataChangedEvent(); 
    }
    public void UseHandBooster()
    {
        handBoosterMount -= 1;
        FireDataChangedEvent(); 
    }
    public void IncreaseHammerBooster(int mount)
    {
        hammerBoosterMount += mount;
        FireDataChangedEvent();
    }
    public void UseHammerBooster()
    {
        hammerBoosterMount -= 1;
        FireDataChangedEvent();
    }
    public void IncreaseShuffleBooster(int mount)
    {
        shuffleBoosterMount += mount;
        FireDataChangedEvent();
    }
    public void UseShuffleBooster()
    {
        shuffleBoosterMount -= 1;
        FireDataChangedEvent();
    }
}
