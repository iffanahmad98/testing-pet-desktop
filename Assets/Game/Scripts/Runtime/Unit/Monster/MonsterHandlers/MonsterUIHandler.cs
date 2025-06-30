using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;

[System.Serializable]
public class MonsterUIHandler
{
    [Header("UI Elements")]
    public GameObject hungerInfo;
    public GameObject happinessInfo;
    public GameObject sickStatusInfo;

    [Header("Evolution VFX")]
    public UIParticle evolutionVFX;
    public Material evolutionMaterial;
    
    [Header("Emoji Expression")]
    public GameObject Expression;
    public Animator emojiAnimator;

    [Header("Info Card Components")]
    private TextMeshProUGUI hungerText;
    private TextMeshProUGUI happinessText;
    private TextMeshProUGUI sickStatusText;
    
    private CanvasGroup _hungerInfoCg;
    private CanvasGroup _happinessInfoCg;
    private CanvasGroup _sickStatusInfoCg;
    private CanvasGroup _expressionCg;
    
    private float _hoverStartTime = 0f;
    private bool _isDisplayingEmoji = false;
    private const float EMOJI_DISPLAY_DELAY = 0.8f;

    public void Init()
    {
        _hungerInfoCg = hungerInfo.GetComponent<CanvasGroup>();
        _happinessInfoCg = happinessInfo.GetComponent<CanvasGroup>();
        _sickStatusInfoCg = sickStatusInfo.GetComponent<CanvasGroup>();
        _expressionCg = Expression.GetComponent<CanvasGroup>();

        // hungerText = hungerInfo.GetComponentInChildren<TextMeshProUGUI>();
        // happinessText = happinessInfo.GetComponentInChildren<TextMeshProUGUI>();
        // sickStatusText = sickStatusInfo.GetComponentInChildren<TextMeshProUGUI>();

        // Start hidden
        // _hungerInfoCg.alpha = 0f;
        // _happinessInfoCg.alpha = 0f;
        // _sickStatusInfoCg.alpha = 0f;
        
        _expressionCg.alpha = 0f;
    }

    public void UpdateHungerDisplay(float hunger, bool showUI)
    {
        // Always update the text content (color stays as set in inspector)
        // hungerText.text = $"Hunger: {hunger:F1}%";

        // Control visibility based on hover
        // _hungerInfoCg.alpha = showUI ? 1f : 0f;

        // update emoji
        if (emojiAnimator != null)
        {
            emojiAnimator.SetFloat("Hunger", hunger); // Example threshold for hungry state
        }
    }

    public void UpdateHappinessDisplay(float happiness, bool showUI)
    {
        // Always update the text content (color stays as set in inspector)
        // happinessText.text = $"Happiness: {happiness:F1}%";

        // Control visibility based on hover
        // _happinessInfoCg.alpha = showUI ? 1f : 0f;

        // update emoji
        if (emojiAnimator != null)
        {
            emojiAnimator.SetFloat("Happiness", happiness); // Example threshold for happy state
        }
    }

    public void UpdateSickStatusDisplay(bool isSick, bool showUI)
    {
        // Update the sick status display based on the isSick parameter
        // sickStatusText.text = isSick ? "Status: Sick" : "Status: Healthy";
        
        // Always show when sick, otherwise show only on hover
        // _sickStatusInfoCg.alpha = showUI ? 1f : 0f;
        
        // Update emoji color based on sick status
        if (emojiAnimator != null)
        {
            emojiAnimator.SetBool("IsSick", isSick);
        }
    }

    public void HideAllUI()
    {
        // Hide all UI elements
        _hungerInfoCg.alpha = 0f;
        _happinessInfoCg.alpha = 0f;
        _sickStatusInfoCg.alpha = 0f;
        _expressionCg.alpha = 0f;
        
        // Reset hover state
        _hoverStartTime = 0f;
        _isDisplayingEmoji = false;
    }
    
    // Add hover tracking methods
    public void OnHoverEnter()
    {
        _hoverStartTime = Time.time;
        _isDisplayingEmoji = false;
    }
    
    public void OnHoverExit()
    {
        _hoverStartTime = 0f;
        _isDisplayingEmoji = false;
        if (_expressionCg != null)
        {
            _expressionCg.alpha = 0f;
        }
    }
    
    // Call this from Update method in MonsterController
    public void UpdateEmojiVisibility(bool isSick)
    {
        if (_hoverStartTime > 0 && !_isDisplayingEmoji)
        {
            float hoverDuration = Time.time - _hoverStartTime;
            if (hoverDuration >= EMOJI_DISPLAY_DELAY)
            {
                _isDisplayingEmoji = true;
                if (_expressionCg != null)
                {
                    _expressionCg.alpha = 1f;
                    // Set color based on sick status
                    if (emojiAnimator != null)
                    {
                        emojiAnimator.SetBool("IsSick", isSick);
                    }
                }
            }
        }
    }
}