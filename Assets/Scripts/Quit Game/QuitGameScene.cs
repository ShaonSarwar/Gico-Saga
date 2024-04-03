using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class QuitGameScene : MonoBehaviour
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
        ProgressBarManager.Instance.LoadScene(0); 
    }

    public void Resume()
    {
        quitPanel.SetActive(false); 
    }
}
