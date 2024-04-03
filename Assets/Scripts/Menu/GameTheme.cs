using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTheme : MonoBehaviour
{
    private AudioSource audioSource; 
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
