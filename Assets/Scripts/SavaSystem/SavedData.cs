using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[Serializable]
public class SavedData 
{
    public int levelIndex;
    public int avatarIndex;
    public int spaceshipIndex;
    public float soundValue;
    public int gameCurrency;
    public string playerName;
    public int achievementIndex;

    // Inventory Items 
    public int strippedBoosterMount;
    public int wrappedBoosterMount;
    public int powerBoosterMount;
    public int handBoosterMount;
    public int hammerBoosterMount;
    public int shuffleBoosterMount;

    public SavedData( PlayerData data)
    {
        levelIndex = data.GetLevelIndex();
        avatarIndex = data.GetAvatarIndex();
        spaceshipIndex = data.GetSpaceshipIndex();
        soundValue = data.GetSoundValue();
        gameCurrency = data.GetGameCurrency();
        playerName = data.GetPlayerName();
        achievementIndex = data.GetAchievementIndex();
        strippedBoosterMount = data.GetStrippedBoosterMount();
        wrappedBoosterMount = data.GetWrappedBoosterMount();
        powerBoosterMount = data.GetPowerBoosterMount();
        handBoosterMount = data.GetHandBoosterMount();
        hammerBoosterMount = data.GetHammerBoosterMount();
        shuffleBoosterMount = data.GetShuffleBoosterMount(); 
    }

    public SavedData(UserData data)
    {
        levelIndex = data.GetLevelIndex();
        avatarIndex = data.GetAvatarIndex();
        spaceshipIndex = data.GetSpaceshipIndex();
        soundValue = data.GetSoundValue();
        gameCurrency = data.GetGameCurrency();
        playerName = data.GetPlayerName();
        achievementIndex = data.GetAchievementIndex();
        strippedBoosterMount = data.GetStrippedBoosterMount();
        wrappedBoosterMount = data.GetWrappedBoosterMount();
        powerBoosterMount = data.GetPowerBoosterMount();
        handBoosterMount = data.GetHandBoosterMount();
        hammerBoosterMount = data.GetHammerBoosterMount();
        shuffleBoosterMount = data.GetShuffleBoosterMount();
    }
}
