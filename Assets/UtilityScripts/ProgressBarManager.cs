using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System; 

public class ProgressBarManager : MonoBehaviour
{
    public static ProgressBarManager Instance;
    public GameObject _progressBarPanel { get; set; }
    public Image _progressBar { get; set; }

    private float _target; 

    private void Awake()
    {      
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        Transform canvasTransform = FindObjectOfType<Canvas>().transform;
        _progressBarPanel = canvasTransform.GetChild(0).Find("ProgressBarPanel").gameObject;
        _progressBar = canvasTransform.GetChild(0).Find("ProgressBarPanel").GetChild(0).GetChild(0).GetComponent<Image>(); 
    }

    public void OpenProgressPanel()
    {
        _progressBarPanel.SetActive(true); 
    }

    public async void LoadScene(int sceneIndex)
    {
        Debug.Log("Scene Index: " + sceneIndex);
        _target = 0f;
        _progressBar.fillAmount = 0f;
        _progressBarPanel.SetActive(true);
        var scene = SceneManager.LoadSceneAsync(sceneIndex);
        scene.allowSceneActivation = false;

        do
        {
            await Task.Delay(100);
            _target = Mathf.Clamp01(scene.progress / 0.9f);
            if (scene.progress >= 0.9f && !scene.allowSceneActivation)
            {
                scene.allowSceneActivation = true;
            }
        } while (!scene.isDone);
        await Task.Delay(2000);
        //scene.allowSceneActivation = true;

        if (_progressBarPanel != null)
        {
            _progressBarPanel.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (_progressBar == null || _progressBarPanel == null)
        {
            Transform canvasTransform = FindObjectOfType<Canvas>().transform;
            _progressBarPanel = canvasTransform.GetChild(0).Find("ProgressBarPanel").gameObject;
            _progressBar = canvasTransform.GetChild(0).Find("ProgressBarPanel").GetChild(0).GetChild(0).GetComponent<Image>();
        }

        if(_progressBar != null)
            _progressBar.fillAmount = Mathf.MoveTowards(_progressBar.fillAmount, _target, 3 * Time.deltaTime);
    }
}
