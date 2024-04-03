using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int gridLevelIndex; 
    [SerializeField] private PlayerData playerData;
    //[SerializeField] private PlayerStats playerStats;


    public AvatarSO avatarList; 
    private int newUserRewardMount = 999; 

    public bool UseInGameAvatar { get; set; }
    public Sprite SocialAvatar { get; set; }

    private void Awake()
    {     
        playerData.OnDataChanged += PlayerData_OnDataChanged;
    }


    private void PlayerData_OnDataChanged(object sender, EventArgs e)
    {
        GridSaveSystem.SaveData(sender as PlayerData);
        //playerStats.UpdatePlayerStats(); 
    }

    public void NewUserReward()
    {
        Debug.Log("New User Found! Give some Reward");
        playerData.SetLevelIndex(1);
        playerData.SetAvatarIndex(1);
    }

    public int GetCurrentLevel()
    {
        PlayerPrefs.SetInt("PlayerLevel", playerData.GetLevelIndex()); 
        return playerData.GetLevelIndex(); 
    }
    public void LoadLevel()
    {
        SceneManager.LoadScene(gridLevelIndex); 
    }
}
