using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System; 

public class GridController : MonoBehaviour
{ 
    [SerializeField] private UserData userData;

    private void Awake()
    {

    }



 

    public void LoadMenuScene(int index)
    {
        SceneManager.LoadScene(index); 
    }
}
