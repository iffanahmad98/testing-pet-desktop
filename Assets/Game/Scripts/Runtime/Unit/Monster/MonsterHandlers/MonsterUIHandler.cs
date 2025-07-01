using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;

[System.Serializable]
public class MonsterUIHandler
{
    [Header("UI Elements")]
    public Image monsterImage;
    public CanvasGroup monsterInfoPanel;

    [Header("VFX")]
    public UIParticle evolutionVFX;
    public Material evolutionMaterial;
    public UIParticle healingVFX;
    // public UIParticle sickVFX;

    [Header("Emoji Expression")]
    public GameObject Expression;
    public Animator emojiAnimator;

    [Header("Stat Bars")]
    public StatBarControl hungerBar;
    public StatBarControl happinessBar;
    public StatBarControl healthBar;

    private CanvasGroup _expressionCg;
    private float _hoverStartTime = 0f;
    private bool _isDisplayingEmoji = false;
    private const float EMOJI_DISPLAY_DELAY = 0.8f;

    public void Initialize(MonsterStatsHandler statsHandler = null, MonsterController monsterController = null)
    {
        monsterInfoPanel.alpha = 0f;

        if (monsterImage != null && monsterController != null)
            monsterImage.sprite = monsterController.GetMonsterIcon();

        // Setup canvas group
        if (Expression != null)
        {
            _expressionCg = Expression.GetComponent<CanvasGroup>();
            _expressionCg.alpha = 0f;
        }

        // Initialize stat bars with values from stats handler or defaults
        float hunger = statsHandler?.CurrentHunger ?? 100f;
        float happiness = statsHandler?.CurrentHappiness ?? 100f;
        float health = statsHandler?.CurrentHP ?? 100f;
        float maxHealth = 100f; // Could add MaxHP property to StatsHandler

        if (hungerBar != null) hungerBar.Initialize(hunger, 100f);
        if (happinessBar != null) happinessBar.Initialize(happiness, 100f);
        if (healthBar != null) healthBar.Initialize(health, maxHealth);

        // Subscribe to events if stats handler provided
        if (statsHandler != null)
        {
            statsHandler.OnHungerChanged += (hunger) => UpdateHungerDisplay(hunger, true);
            statsHandler.OnHappinessChanged += (happiness) => UpdateHappinessDisplay(happiness, true);
            statsHandler.OnSickChanged += (isSick) =>
            {
                UpdateSickStatusDisplay(isSick, true);
                UpdateHealthDisplay(statsHandler.CurrentHP);
            };
        }
    }

    public void UpdateHungerDisplay(float hunger, bool showUI)
    {
        if (emojiAnimator != null)
        {
            emojiAnimator.SetFloat("Hunger", hunger);
        }

        // Update hunger bar
        if (hungerBar != null)
        {
            hungerBar.SetValue(hunger);
        }
    }

    public void UpdateHappinessDisplay(float happiness, bool showUI)
    {
        if (emojiAnimator != null)
        {
            emojiAnimator.SetFloat("Happiness", happiness);
        }

        // Update happiness bar
        if (happinessBar != null)
        {
            happinessBar.SetValue(happiness);
        }
    }

    public void UpdateHealthDisplay(float health)
    {
        if (healthBar != null)
        {
            healthBar.SetValue(health);
        }
    }

    public void UpdateSickStatusDisplay(bool isSick, bool showUI)
    {
        if (emojiAnimator != null)
        {
            emojiAnimator.SetBool("IsSick", isSick);
        }
    }

    public void HideAllUI()
    {
        // Hide all UI elements
        _expressionCg.alpha = 0f;

        // Reset hover state
        _hoverStartTime = 0f;
        _isDisplayingEmoji = false;

        // Hide monster info panel
        monsterInfoPanel.alpha = 0f;
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
                    if (emojiAnimator != null)
                    {
                        emojiAnimator.SetBool("IsSick", isSick);
                    }
                }
            }
        }
    }

    public void ShowMonsterInfo()
    {
        monsterInfoPanel.DOKill();
        monsterInfoPanel.alpha = 0f;
        monsterInfoPanel.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            monsterInfoPanel.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad).SetDelay(4f);
        });
    }

    public void HideMonsterInfo()
    {
        monsterInfoPanel.DOKill();
        monsterInfoPanel.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad);
    }
    public void PlayParticleWithDuration(GameObject particleObject, float duration, MonsterController controller)
    {
        if (particleObject == null) return;

        particleObject.SetActive(false); // reset
        particleObject.SetActive(true);

        // Disable after delay
        if (controller != null)
        {
            controller.StartCoroutine(DeactivateAfterDelay(particleObject, duration));
        }
    }
    public void PlayHealingVFX(MonsterController controller)
    {
        if (healingVFX != null)
        {
            PlayParticleWithDuration(healingVFX.gameObject, 2f, controller);
        }
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) obj.SetActive(false);
    }

}