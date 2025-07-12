using UnityEngine;
using UnityEngine.UI;

public class DebugBackground : MonoBehaviour
{
    [Header("Buttons")]
    public Button btnRain;
    public Button btnNight;

    [Header("Targets")]
    public GameObject rainEffect;
    public GameObject nightOverlay;
    public GameObject backgroundLayer;

    [Header("Background Time")]
    public GameObject dayBackground;
    public GameObject eveningBackground;
    public GameObject nightBackground;
    public GameObject overcastBackground;
    private bool isRainOn = false;
    private bool isNightOn = false;
    private bool isBackgroundOn = true;

    void Start()
    {
        btnRain.onClick.AddListener(ToggleRain);
        btnNight.onClick.AddListener(ToggleNight);
        // Inisialisasi
        ApplyState();
    }

    void ToggleRain()
    {
        isRainOn = !isRainOn;
        ApplyRain();
    }

    void ToggleNight()
    {
        isNightOn = !isNightOn;
        ApplyNight();
    }

    void ToggleBackground()
    {
        isBackgroundOn = !isBackgroundOn;
        ApplyBackground();
    }

    void ApplyState()
    {
        ApplyRain();
        ApplyNight();
        ApplyBackground();
    }

    void ApplyRain()
    {
        if (rainEffect != null)
            rainEffect.SetActive(isRainOn);
    }

    void ApplyNight()
    {
        if (nightOverlay != null)
            nightOverlay.SetActive(isNightOn);
    }

    void ApplyBackground()
    {
        if (backgroundLayer != null)
            backgroundLayer.SetActive(isBackgroundOn);
    }

    public void SetBackground(string time)
    {
        dayBackground.SetActive(false);
        eveningBackground.SetActive(false);
        nightBackground.SetActive(false);
        overcastBackground.SetActive(false);

        switch (time.ToLower())
        {
            case "day":
                dayBackground.SetActive(true);
                break;
            case "evening":
                eveningBackground.SetActive(true);
                break;
            case "night":
                nightBackground.SetActive(true);
                break;
            case "overcast":
                overcastBackground.SetActive(true);
                break;
            default:
                Debug.LogWarning("Unknown background time: " + time);
                break;
        }
    }
}