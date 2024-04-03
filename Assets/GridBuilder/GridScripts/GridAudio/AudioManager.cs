using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class TrackInfo
{
    public string Name = string.Empty;
    public AudioMixerGroup group = null;
    public IEnumerator Trackfader = null;
}

public class AudioPoolItem
{
    public GameObject GameObject;
    public Transform Transform;
    public AudioSource AudioSource;
    public float Unimportance = float.MaxValue;
    public bool playing = false;
    public IEnumerator Coroutine = null;
    public ulong ID = 0;
}
public class AudioManager : MonoBehaviour
{
    //private static AudioManager instance;
    //public static AudioManager Instance
    //{
    //    get
    //    {
    //        if (!instance)
    //        {
    //            instance = (AudioManager)FindObjectOfType(typeof(AudioManager));
    //        }
    //        return instance;
    //    }
    //}

    [SerializeField]
    private AudioMixer mixer = null;
    [SerializeField]
    private int maxSounds = 10;

    // private fields
    private Dictionary<string, TrackInfo> tracks = new Dictionary<string, TrackInfo>();
    private List<AudioPoolItem> pools = new List<AudioPoolItem>();
    private Dictionary<ulong, AudioPoolItem> activePools = new Dictionary<ulong, AudioPoolItem>();
    private ulong IDGiver = 0;
    private Transform listenerPos = null;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if (!mixer)
            return;
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup group in groups)
        {
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.Name = group.name;
            trackInfo.group = group;
            trackInfo.Trackfader = null;
            tracks[group.name] = trackInfo;
        }

        // Building pool 
        for (int i = 0; i < maxSounds; i++)
        {
            GameObject go = new GameObject("pool item");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            go.transform.parent = transform;

            AudioPoolItem poolItem = new AudioPoolItem();
            poolItem.GameObject = go;
            poolItem.Transform = go.transform;
            poolItem.AudioSource = audioSource;
            poolItem.playing = false;
            go.SetActive(false);
            pools.Add(poolItem);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        listenerPos = FindObjectOfType<AudioListener>().transform;
        //Debug.Log(listenerPos.name);
    }

    public float GetTrackVolume(string track)
    {
        TrackInfo info;
        if (tracks.TryGetValue(track, out info))
        {
            float volume;
            mixer.GetFloat(track, out volume);
            return volume;
        }
        return float.MinValue;
    }

    public AudioMixerGroup GetAudioMixerGroupFromTrack(string track)
    {
        TrackInfo info;
        if (tracks.TryGetValue(track, out info))
        {
            return info.group;
        }
        return null;
    }

    public void SetTrackVolume(string track, float volume, float fadetime = 0)
    {
        if (!mixer)
            return;
        TrackInfo info;
        if (tracks.TryGetValue(track, out info))
        {
            if (info.Trackfader != null)
                StopCoroutine(info.Trackfader);
            if (fadetime == 0)
            {
                mixer.SetFloat(track, volume);

            }
            else
            {
                info.Trackfader = SetTrackVolumeInternal(track, volume, fadetime);
                StartCoroutine(info.Trackfader);

            }

        }
    }

    private IEnumerator SetTrackVolumeInternal(string track, float volume, float fadetime)
    {
        float startVolume = 0.0f;
        float timer = 0.0f;
        while (timer < fadetime)
        {
            timer += Time.unscaledDeltaTime;
            mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, fadetime / timer));
            yield return null;
        }
        mixer.SetFloat(track, volume);
    }

    // Construction a pool 
    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float unimportance)
    {
        if (poolIndex < 0 || poolIndex >= pools.Count)
            return 0;
        AudioPoolItem poolItem = pools[poolIndex];
        IDGiver++;
        AudioSource source = poolItem.AudioSource;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.clip = clip;
        source.outputAudioMixerGroup = tracks[track].group;
        source.transform.position = position;

        poolItem.playing = true;
        poolItem.Unimportance = unimportance;
        poolItem.ID = IDGiver;
        poolItem.GameObject.SetActive(true);
        source.Play();
        poolItem.Coroutine = StopSoundDelay(IDGiver, source.clip.length);
        StartCoroutine(poolItem.Coroutine);

        activePools[IDGiver] = poolItem;
        return IDGiver;
    }

    private IEnumerator StopSoundDelay(ulong iDGiver, float duration)
    {
        yield return new WaitForSeconds(duration);
        AudioPoolItem activeSound;
        if (activePools.TryGetValue(iDGiver, out activeSound))
        {
            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);
            activePools.Remove(iDGiver);
            activeSound.playing = false;
        }
    }

    public void StopOneShotSound(ulong Id)
    {
        AudioPoolItem activeSound;
        if (activePools.TryGetValue(Id, out activeSound))
        {
            StopCoroutine(activeSound.Coroutine);

            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);
            activePools.Remove(Id);
            activeSound.playing = false;
        }
    }

    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)
    {
        if (!tracks.ContainsKey(track) || clip == null || volume.Equals(0.0f))
            return 0;
        float unimportance = (listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);
        int leastImportanceIndex = -1;
        float leastImportanceValue = float.MaxValue;

        for (int i = 0; i < pools.Count; i++)
        {
            AudioPoolItem poolItem = pools[i];
            if (!poolItem.playing)
                return ConfigurePoolObject(i,
                                            track,
                                            clip,
                                            position,
                                            volume,
                                            spatialBlend,
                                            unimportance);
            else if (poolItem.Unimportance > leastImportanceValue)
            {
                leastImportanceIndex = i;
                leastImportanceValue = poolItem.Unimportance;
            }
        }

        if (leastImportanceValue > unimportance)
            return ConfigurePoolObject(leastImportanceIndex,
                                        track,
                                        clip,
                                        position,
                                        volume,
                                        spatialBlend,
                                        unimportance);
        return 0;
    }

    public IEnumerator PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128)
    {
        yield return new WaitForSeconds(duration);
        PlayOneShotSound(track,
                         clip,
                         position,
                         volume,
                         spatialBlend,
                         priority);
    }
}
