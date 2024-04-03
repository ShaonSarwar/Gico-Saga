using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    [SerializeField] private GameObject quitPanel; 
  
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitPanel.SetActive(true); 
        }
    }

    public void Quit()
    {
        Application.Quit(); 
    }

    public void Resume()
    {
        quitPanel.SetActive(false); 
    }
}
