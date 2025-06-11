using UnityEngine;
using TMPro;

[System.Serializable]
public class MonsterUIHandler
{
    [Header("UI Elements")]
    public GameObject hungerInfo;
    public GameObject happinessInfo;
    public GameObject sickStatusInfo;
    public ParticleSystem evolutionEffect;
    public CanvasGroup evolutionEffectCg;

    private TextMeshProUGUI hungerText;
    private TextMeshProUGUI happinessText;
    private TextMeshProUGUI sickStatusText;

    private CanvasGroup _hungerInfoCg;
    private CanvasGroup _happinessInfoCg;
    private CanvasGroup _sickStatusInfoCg;

    public void Init()
    {
        _hungerInfoCg = hungerInfo.GetComponent<CanvasGroup>();
        _happinessInfoCg = happinessInfo.GetComponent<CanvasGroup>();
        _sickStatusInfoCg = sickStatusInfo.GetComponent<CanvasGroup>();

        hungerText = hungerInfo.GetComponentInChildren<TextMeshProUGUI>();
        happinessText = happinessInfo.GetComponentInChildren<TextMeshProUGUI>();
        sickStatusText = sickStatusInfo.GetComponentInChildren<TextMeshProUGUI>();

        // Start hidden
        _hungerInfoCg.alpha = 0f;
        _happinessInfoCg.alpha = 0f;
        _sickStatusInfoCg.alpha = 0f;
        
        // Initialize evolution effects as hidden
        if (evolutionEffectCg != null)
        {
            evolutionEffectCg.alpha = 0f;
        }
    }

    public void UpdateHungerDisplay(float hunger, bool showUI)
    {
        // Always update the text content (color stays as set in inspector)
        hungerText.text = $"Hunger: {hunger:F1}%";

        // Control visibility based on hover
        _hungerInfoCg.alpha = showUI ? 1f : 0f;
    }

    public void UpdateHappinessDisplay(float happiness, bool showUI)
    {
        // Always update the text content (color stays as set in inspector)
        happinessText.text = $"Happiness: {happiness:F1}%";

        // Control visibility based on hover
        _happinessInfoCg.alpha = showUI ? 1f : 0f;
    }

    public void UpdateSickStatusDisplay(bool isSick, bool showUI)
    {
        // Update the sick status display based on the isSick parameter
        sickStatusText.text = isSick ? "Status: Sick" : "Status: Healthy";
        
        // Control visibility based on hover
        _sickStatusInfoCg.alpha = showUI ? 1f : 0f;
    }
}