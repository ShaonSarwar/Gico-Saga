using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ClipBank
{
    public List<AudioClip> clips = new List<AudioClip>();
}

[CreateAssetMenu()] 
public class GridAudioSO : ScriptableObject
{
    // Serialized Fields 
    [SerializeField] private string audioGroup = string.Empty;
    [SerializeField] [Range(0.0f, 1.0f)] private float volume;
    [SerializeField] [Range(0.0f, 1.0f)] private float spatialblend;
    [SerializeField] [Range(0, 256)] private int priority;
    [SerializeField] private List<ClipBank> audioClipBanks = new List<ClipBank>();

    // Properties 
    public string AudioGroup
    {
        get
        {
            return audioGroup;
        }
    }
    public float Volume
    {
        get
        {
            return volume;
        }
    }
    public float SpatialBlend
    {
        get
        {
            return spatialblend;
        }
    }
    public int Priority
    {
        get
        {
            return priority;
        }
    }
    public int BankCount
    {
        get
        {
            return audioClipBanks.Count;
        }
    }

    public AudioClip this[int i]
    {
        get
        {
            if (audioClipBanks == null || audioClipBanks.Count <= i)
                return null;
            if (audioClipBanks[i].clips.Count == 0)
                return null;
            List<AudioClip> clipList = audioClipBanks[i].clips;
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];
            return clip;
        }
    }

    public AudioClip audioClip
    {
        get
        {
            if (audioClipBanks == null || audioClipBanks.Count == 0)
                return null;
            if (audioClipBanks[0].clips.Count == 0)
                return null;
            List<AudioClip> clipList = audioClipBanks[0].clips;
            AudioClip clip = clipList[Random.Range(0, clipList.Count)];
            return clip;
        }
    }
}
