using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GicoTutorial : MonoBehaviour
{
    [SerializeField] private UserData userData;
    [SerializeField] private GameObject commandGraphic; 

    // Command Lines 
    public const string NORMAL_SWIPE_COMMAND = "Swipe Items for Match!";
    public const string STRIPPED_COMMAND = "Match 4 Same Color Item for Creating Blue Bomb Booster!";
    public const string wRAPPED_COMMAND = "Match 5 Same Color Item in Two Line for Croos Booster!";
    public const string POWER_COMMAND = "Match 5 Same Color Item in a Single Line for Power Booster";
    public const string HAND_COMMAND = "Use Hand for Swiping any of Two Item!";
    public const string HAMMER_COMMAND = "Use Hammer for Destroy any Item!";
    public const string ALTAR_COMMAND = "Use Altar for Change Whole Board!";
    public const string CELL_RELEASE_COMMAND = "Hit with Booster Around Cell to Release it!";
    public const string CELL_COLLECT_COMMAND = "Match items around Cell to Collect it!";
    public const string TUTORIAL_COMPLETED_COMMAND = "Hurray! You Did It!"; 

    private bool _tutorial; 
    // Start is called before the first frame update
    void Start()
    {
        if (userData.GetLevelIndex() == 1)
        {
            _tutorial = true; 
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
