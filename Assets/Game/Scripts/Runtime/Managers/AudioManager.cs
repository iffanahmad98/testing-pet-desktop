using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private int initialPoolSize = 5;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip gameplayBGM;
    [SerializeField] private AudioClip[] situationalBGM;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip menuOpenSFX;
    [SerializeField] private AudioClip notificationSFX;
    [SerializeField] private AudioClip menuCloseSFX;
    [SerializeField] private AudioClip dropCoinSFX;
    [SerializeField] private AudioClip petJumpSFX;
    [SerializeField] private AudioClip collectCoinSFX;
    [SerializeField] private AudioClip buySFX;
    [SerializeField] private AudioClip openingSFX;
    [SerializeField] private AudioClip placingFaciltySFX;
    [SerializeField] private AudioClip torchIgniteSFX;
    [SerializeField] private AudioClip timeKeeperSFX;
    [SerializeField] private AudioClip magicShovelSFX;
    [SerializeField] private AudioClip rainbowPotSFX;
    [SerializeField] private AudioClip medicineSFX;
    [SerializeField] private AudioClip eatingSFX;
    [SerializeField] private AudioClip[] farmAndHotelSFX;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string bgmMixerGroup = "BGM";
    [SerializeField] private string sfxMixerGroup = "SFX";
    [SerializeField] private string uiMixerGroup = "UI";
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private Transform sfxPoolParent;

    private float ambianceVolume = 0.3f;

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeSFXPool();
        InitializeSFXDictionary();
        AssignOutputsToMixerGroups();
        
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplyBGMForScene(newScene);
    }

    private void ApplyBGMForScene(Scene scene)
    {
        Debug.Log("New scene name: " + scene.name);
        switch (scene.name)
        {
            case "Pet":
                PlayMainMenuBGM();
                break;
            case "FarmGame":
                PlayGameplayBGM();
                break;
        }
    }

    private void InitializeSFXPool()
    {
        // Create a parent object to keep the hierarchy clean
        sfxPoolParent = new GameObject("SFX_Pool").transform;
        sfxPoolParent.SetParent(transform);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSourceInPool();
        }
    }
    
    private void AssignOutputsToMixerGroups()
    {
        // Assign main sources to mixer groups
        if (masterMixer != null)
        {
            bgmSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups(bgmMixerGroup)[0];
            uiSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups(uiMixerGroup)[0];
        }
    }

    private AudioSource CreateNewAudioSourceInPool()
    {
        GameObject sourceObj = new GameObject($"SFX_Source_{sfxPool.Count}");
        sourceObj.transform.SetParent(sfxPoolParent);
        AudioSource newSource = sourceObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;

        // Assign to SFX mixer group if available
        if (masterMixer != null)
        {
            var groups = masterMixer.FindMatchingGroups(sfxMixerGroup);
            if (groups.Length > 0)
            {
                newSource.outputAudioMixerGroup = groups[0];
            }
        }

        sfxPool.Enqueue(newSource);
        return newSource;
    }

    private void InitializeSFXDictionary()
    {
        // Add all your SFX to the dictionary here
        sfxDictionary.Add("button_click", buttonClickSFX);
        sfxDictionary.Add("menu_close", menuCloseSFX);
        sfxDictionary.Add("menu_open", menuOpenSFX);
        sfxDictionary.Add("drop_coin", dropCoinSFX);
        sfxDictionary.Add("pet_jump", petJumpSFX);
        sfxDictionary.Add("collect_coin", collectCoinSFX);
        sfxDictionary.Add("buy", buySFX);
        sfxDictionary.Add("opening", openingSFX);
        sfxDictionary.Add("placing_facility", placingFaciltySFX);
        sfxDictionary.Add("torch_ignite", torchIgniteSFX);
        sfxDictionary.Add("time_keeper", timeKeeperSFX);
        sfxDictionary.Add("magic_shovel", magicShovelSFX);
        sfxDictionary.Add("rainbow_pot", rainbowPotSFX);
        sfxDictionary.Add("medicine", medicineSFX);
        sfxDictionary.Add("eating", eatingSFX);
    }

    private void Update()
    {
        // Clean up finished SFX sources
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                ReturnAudioSourceToPool(activeSfxSources[i]);
                activeSfxSources.RemoveAt(i);
            }
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        if (sfxPool.Count == 0)
        {
            // If pool is empty, create a new one
            return CreateNewAudioSourceInPool();
        }
        return sfxPool.Dequeue();
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        source.clip = null;
        sfxPool.Enqueue(source);
    }

    #region BGM Controls
    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    public void PlayGameplayBGM()
    {
        PlayBGM(gameplayBGM);
    }

    public void PlaySituationalBGM(int index)
    {
        if (index >= 0 && index < situationalBGM.Length)
        {
            PlayBGM(situationalBGM[index]);
        }
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }


    #endregion

    #region Enhanced SFX Controls with Pooling
    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitch = 1f, bool loop = false)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = volumeScale;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();
        activeSfxSources.Add(source);
    }

    public void PlaySFX(string sfxKey, float volumeScale = 1f, float pitch = 1f, bool loop = false)
    {
        if (sfxDictionary.TryGetValue(sfxKey, out AudioClip clip))
        {
            PlaySFX(clip, volumeScale, pitch, loop);
        }
        else
        {
            Debug.LogWarning($"SFX with key '{sfxKey}' not found!");
        }
    }

    public void PlayFarmSFX(int index)
    {
        if (index >= 0 && index < farmAndHotelSFX.Length)
        {
            PlaySFX(farmAndHotelSFX[index]);
        }
    }

    public void PlayFarmSFX(int index, float volumeScale = 1f, bool loop = false)
    {
        if (index >= 0 && index < farmAndHotelSFX.Length)
        {
            PlaySFX(farmAndHotelSFX[index], volumeScale, 1, loop);
        }
    }

    public void playFarmAmbiance(string currentTime = "day")
    {
        // currentTime bisa day, evening, night
        StopAllSFX();

        if (currentTime == "day")
        {
            // Farm ambiance daytime is at index 10
            PlayFarmSFX(10, ambianceVolume, true);
        }
        else if (currentTime == "night")
        {
            // Farm ambiance night is at index 11
            PlayFarmSFX(11, ambianceVolume, true);
        }
    }

    public void PlayHotelAmbiance(string currentTime = "day")
    {
        StopAllSFX();

        if (currentTime == "day")
        {
            // Hotel ambiance day is at index 12
            PlayFarmSFX(12, ambianceVolume, true);
        }
        else if (currentTime == "night")
        {
            // hotel ambiance night is at index 13
            PlayFarmSFX(13, ambianceVolume, true);
        }
    }

    public void PlaySFXClip (AudioClip clip) { // HotelEggsCollectionMenu.cs
        PlaySFX(clip);
    }

    public void StopAllSFX()
    {
        foreach (var source in activeSfxSources)
        {
            source.Stop();
            ReturnAudioSourceToPool(source);
        }
        activeSfxSources.Clear();
    }
    #endregion
}