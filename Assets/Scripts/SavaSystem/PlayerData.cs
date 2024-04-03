using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerData : MonoBehaviour
{
    public event EventHandler OnDataChanged;
    public event EventHandler OnPlayerDataInitialize;
    private int levelIndex;
    private int avatarIndex;
    private int spaceshipIndex;
    private float soundValue;
    private int gameCurrency;
    private string playerName;
    private int achievementIndex;
    private int weeklyLevelGained; 

    // Inventory Items 
    private int strippedBoosterMount;
    private int wrappedBoosterMount;
    private int powerBoosterMount;
    private int handBoosterMount;
    private int hammerBoosterMount;
    private int shuffleBoosterMount;

    private bool newUserFound; 

    public void InitializePlayerData(SavedData savedData)
    {
        if (savedData != null)
        {
            levelIndex = savedData.levelIndex;
            avatarIndex = savedData.avatarIndex;
            spaceshipIndex = savedData.spaceshipIndex;
            soundValue = savedData.soundValue;
            gameCurrency = savedData.gameCurrency;
            playerName = savedData.playerName;
            achievementIndex = savedData.achievementIndex;
            strippedBoosterMount = savedData.strippedBoosterMount;
            wrappedBoosterMount = savedData.wrappedBoosterMount;
            powerBoosterMount = savedData.powerBoosterMount;
            handBoosterMount = savedData.handBoosterMount;
            hammerBoosterMount = savedData.hammerBoosterMount;
            shuffleBoosterMount = savedData.shuffleBoosterMount; 
        }
    }

    public void FireInitializeEvent()
    {
        EventHandler handler = OnPlayerDataInitialize;
        if (handler != null)
        {
            handler?.Invoke(this, EventArgs.Empty); 
        }
    }

    public void SetLevelIndex(int levelIndex) 
    {
        this.levelIndex = levelIndex;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }

    public void SetWeeklyLevelGained(int levelCount)
    {
        this.weeklyLevelGained = levelCount; 
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void SetAvatarIndex(int avatarIndex) 
    { 
        this.avatarIndex = avatarIndex;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void SetSpaceshipIndex(int spaceshipIndex) 
    { 
        this.spaceshipIndex = spaceshipIndex;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void SetSoundValue(float soundValue) 
    { 
        this.soundValue = soundValue;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void SetPlayerName(string playerName) 
    { 
        this.playerName = playerName;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }
    public void SetAchivementIndex(int achievementIndex) 
    { 
        this.achievementIndex += achievementIndex;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }

    public void IncrementWeeklyLevelGained(int count)
    {
        this.weeklyLevelGained += count;
        OnDataChanged?.Invoke(this, EventArgs.Empty); 
    }

    public void IncreaseGameCurrency(int mount)
    {
        gameCurrency += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseGameCurrency(int mount)
    {
        gameCurrency -= mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    // Set InventoryItems 
    public void IncreaseStrippedBooster(int mount)
    {
        strippedBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseStrippedBooster()
    {
        strippedBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreaseWrappedBooster(int mount)
    {
        wrappedBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseWrappedBooster()
    {
        wrappedBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreasePowerBooster(int mount)
    {
        powerBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UsePowerBooster()
    {
        powerBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreaseHandBooster(int mount)
    {
        handBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseHandBooster()
    {
        handBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreaseHammerBooster(int mount)
    {
        hammerBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseHammerBooster()
    {
        hammerBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreaseShuffleBooster(int mount)
    {
        shuffleBoosterMount += mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseShuffleBooster()
    {
        shuffleBoosterMount -= 1;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetLevelIndex() { return levelIndex; }
    public int GetWeeklyLevelGained() { return weeklyLevelGained; }
    public int GetAvatarIndex() { return avatarIndex; }
    public int GetSpaceshipIndex() { return spaceshipIndex; }
    public float GetSoundValue() { return soundValue; }
    public int GetGameCurrency() { return gameCurrency; }
    public string GetPlayerName() { return playerName; }
    public int GetAchievementIndex() { return achievementIndex; }

    // Get Inventory Items 
    public int GetStrippedBoosterMount() { return strippedBoosterMount; }
    public int GetWrappedBoosterMount() { return wrappedBoosterMount; }
    public int GetPowerBoosterMount() { return powerBoosterMount; }
    public int GetHandBoosterMount() { return handBoosterMount; }
    public int GetHammerBoosterMount() { return hammerBoosterMount; }
    public int GetShuffleBoosterMount() { return shuffleBoosterMount; }

    public void SetStrippedBoosterMount(int mount)
    {
        strippedBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetWrappedBoosterMount(int mount)
    {
        wrappedBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetPowerBoosterMount(int mount)
    {
        powerBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetHandBoosterMount(int mount)
    {
        handBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetHammerBoosterMount(int mount)
    {
        hammerBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetShuffleBoosterMount(int mount)
    {
        shuffleBoosterMount = mount;
        OnDataChanged?.Invoke(this, EventArgs.Empty);
    }
}
