using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class MonsterUIHandler
{
    [Header("UI Elements")]
    public GameObject hungerInfo;
    public GameObject happinessInfo;
    public GameObject sickStatusInfo;
    public ParticleSystem evolutionEffect;
    public CanvasGroup evolutionEffectCg;
    
    [Header("Emoji Expression")]
    public GameObject emojiExpression;
    public Image emojiImage;
    public Color healthyColor = Color.green;
    public Color sickColor = Color.red;
    public Color hungryColor = Color.yellow;
    
    private TextMeshProUGUI hungerText;
    private TextMeshProUGUI happinessText;
    private TextMeshProUGUI sickStatusText;
    
    private CanvasGroup _hungerInfoCg;
    private CanvasGroup _happinessInfoCg;
    private CanvasGroup _sickStatusInfoCg;
    private CanvasGroup _emojiExpressionCg;
    
    private float _hoverStartTime = 0f;
    private bool _isDisplayingEmoji = false;
    private const float EMOJI_DISPLAY_DELAY = 0.8f;

    public void Init()
    {
        _hungerInfoCg = hungerInfo.GetComponent<CanvasGroup>();
        _happinessInfoCg = happinessInfo.GetComponent<CanvasGroup>();
        _sickStatusInfoCg = sickStatusInfo.GetComponent<CanvasGroup>();
        _emojiExpressionCg = emojiExpression.GetComponent<CanvasGroup>();

        hungerText = hungerInfo.GetComponentInChildren<TextMeshProUGUI>();
        happinessText = happinessInfo.GetComponentInChildren<TextMeshProUGUI>();
        sickStatusText = sickStatusInfo.GetComponentInChildren<TextMeshProUGUI>();

        // Start hidden
        _hungerInfoCg.alpha = 0f;
        _happinessInfoCg.alpha = 0f;
        _sickStatusInfoCg.alpha = 0f;
        _emojiExpressionCg.alpha = 0f;
        
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
        
        // Always show when sick, otherwise show only on hover
        _sickStatusInfoCg.alpha = isSick ? 1f : (showUI ? 1f : 0f);
        
        // Update emoji color based on sick status
        if (emojiImage != null)
        {
            emojiImage.color = isSick ? sickColor : healthyColor;
        }
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
        if (_emojiExpressionCg != null)
        {
            _emojiExpressionCg.alpha = 0f;
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
                if (_emojiExpressionCg != null)
                {
                    _emojiExpressionCg.alpha = 1f;
                    // Set color based on sick status
                    if (emojiImage != null)
                    {
                        emojiImage.color = isSick ? sickColor : healthyColor;
                    }
                }
            }
        }
    }
}