using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
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

    private const float MIN_VOLUME_DB = -80f;
    private const float MAX_VOLUME_DB = 0f;
    private const string VOLUME_PREFIX = "Volume_";

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeSliders();
    }

    private void InitializeSliders()
    {
        float savedMaster = PlayerPrefs.GetFloat(VOLUME_PREFIX + masterVolumeParam, 1f);
        float savedBGM = PlayerPrefs.GetFloat(VOLUME_PREFIX + bgmVolumeParam, 1f);
        float savedSFX = PlayerPrefs.GetFloat(VOLUME_PREFIX + sfxVolumeParam, 1f);

        if (masterSlider != null)
        {
            masterSlider.value = savedMaster;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            SetMasterVolume(savedMaster);
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGM;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            SetBGMVolume(savedBGM);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            SetSFXVolume(savedSFX);
        }
    }

    public void SetMasterVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(masterVolumeParam, dbValue);
        PlayerPrefs.SetFloat(VOLUME_PREFIX + masterVolumeParam, linearValue);
    }

    public void SetBGMVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(bgmVolumeParam, dbValue);
        PlayerPrefs.SetFloat(VOLUME_PREFIX + bgmVolumeParam, linearValue);
    }

    public void SetSFXVolume(float linearValue)
    {
        float dbValue = ConvertToDecibel(linearValue);
        masterMixer.SetFloat(sfxVolumeParam, dbValue);
        PlayerPrefs.SetFloat(VOLUME_PREFIX + sfxVolumeParam, linearValue);
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
}
