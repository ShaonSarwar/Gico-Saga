using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeGameObjectToggle : MonoBehaviour
{
    public void MakeToggle(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy); 
    }
}
