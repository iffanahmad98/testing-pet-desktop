using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour, ISettingsSavable
{
    [Header("Mixer References")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string bgmVolumeParam = "BGMVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("UI References")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    private const float MIN_VOLUME_DB = -80f;
    private const float MAX_VOLUME_DB = 0f;
    private const string VOLUME_PREFIX = "Volume_";
    private float cachedMaster;
    private float cachedBGM;
    private float cachedSFX;
    private bool isMuted;

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeSliders();

    }
    void Start()
    {
        LoadSettings();
    }

    private void InitializeSliders()
    {

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(masterVolumeParam, dbValue);
        masterVolumeText.text = masterSlider.value.ToString();
    }

    public void SetBGMVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(bgmVolumeParam, dbValue);
        bgmVolumeText.text = bgmSlider.value.ToString();
    }

    public void SetSFXVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(sfxVolumeParam, dbValue);
        sfxVolumeText.text = sfxSlider.value.ToString();
    }

    private float ConvertToDecibel(float linear)
    {
        return linear <= 0.0001f ? MIN_VOLUME_DB : Mathf.Log10(linear) * 20f;
    }

    private void OnDestroy()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(SetBGMVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);

        ServiceLocator.Unregister<AudioSettingsManager>();
        PlayerPrefs.Save();
    }
    public void LoadSettings()
    {

        cachedMaster = PlayerPrefs.GetFloat(VOLUME_PREFIX + masterVolumeParam, 1f);
        cachedBGM = PlayerPrefs.GetFloat(VOLUME_PREFIX + bgmVolumeParam, 1f);
        cachedSFX = PlayerPrefs.GetFloat(VOLUME_PREFIX + sfxVolumeParam, 1f);

        if (masterSlider != null)
        {
            masterSlider.value = cachedMaster;
            SetMasterVolume(cachedMaster);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = cachedBGM;
            SetBGMVolume(cachedBGM);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = cachedSFX;
            SetSFXVolume(cachedSFX);
        }
    }

    public void SaveSettings()
    {

        PlayerPrefs.SetFloat(VOLUME_PREFIX + masterVolumeParam, masterSlider.value);
        PlayerPrefs.SetFloat(VOLUME_PREFIX + bgmVolumeParam, bgmSlider.value);
        PlayerPrefs.SetFloat(VOLUME_PREFIX + sfxVolumeParam, sfxSlider.value);

        cachedMaster = masterSlider.value;
        cachedBGM = bgmSlider.value;
        cachedSFX = sfxSlider.value;

        PlayerPrefs.Save();

        Debug.Log("Audio Settings Saved");
    }

    public void RevertSettings()
    {
        if (masterSlider != null)
        {
            masterSlider.value = cachedMaster;
            SetMasterVolume(cachedMaster);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = cachedBGM;
            SetBGMVolume(cachedBGM);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = cachedSFX;
            SetSFXVolume(cachedSFX);
        }

        Debug.Log("Audio Settings Reverted");
    }

    #region Mute
    public void SetMute () {
        if (isMuted) {
            isMuted = false;
            masterMixer.SetFloat("MasterVolume", 0f); // normal
        } else {
            isMuted = true;
            masterMixer.SetFloat("MasterVolume", -80f); // mute
        }
    }

    public bool GetMuted () { // HotelMainUI.cs, FarmMainUI.cs
        return isMuted;
    }
    #endregion
}
