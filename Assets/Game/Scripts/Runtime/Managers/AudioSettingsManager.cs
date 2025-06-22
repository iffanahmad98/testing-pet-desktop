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
        var settings = SaveSystem.GetPlayerConfig().settings;

        cachedMaster = settings.masterVolume;
        cachedBGM = settings.bgmVolume;
        cachedSFX = settings.sfxVolume;
        Debug.Log($"Loaded Audio Settings: Master={cachedMaster}, BGM={cachedBGM}, SFX={cachedSFX}");

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
        var settings = SaveSystem.GetPlayerConfig().settings;

        settings.masterVolume = masterSlider.value;
        settings.bgmVolume = bgmSlider.value;
        settings.sfxVolume = sfxSlider.value;

        cachedMaster = settings.masterVolume;
        cachedBGM = settings.bgmVolume;
        cachedSFX = settings.sfxVolume;

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
}
