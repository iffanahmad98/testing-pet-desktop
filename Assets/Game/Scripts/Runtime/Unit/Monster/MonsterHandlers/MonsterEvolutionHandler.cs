using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using Spine.Unity;

[Serializable]
public class MonsterEvolutionSaveData
{
    public string monsterId;
    public float timeSinceCreation;
    public int foodConsumed;
    public int interactionCount;
}

[Serializable]
public class MonsterEvolutionHandler
{
    public bool IsEvolving => _isEvolving;
    private bool _isEvolving = false;
    private MonsterController _controller;

    // Evolution tracking
    private float _lastUpdateTime;
    private float _lastEvolutionTime = -1f;
    private float _timeSinceCreation;
    private int _foodConsumed;
    private int _interactionCount;

    public bool CanEvolve => _controller?.MonsterData != null && _controller.MonsterData.canEvolve && !_controller.MonsterData.isFinalEvol;
    public float TimeSinceCreation => _timeSinceCreation;
    public int FoodConsumed => _foodConsumed;
    public int InteractionCount => _interactionCount;

    private ParticleSystem _evolutionParticle;
    private RectTransform _monsterRectTransform;
    private CanvasGroup _evolutionParticleCanvasGroup;

    public MonsterEvolutionHandler(MonsterController controller)
    {
        _controller = controller;
    }

    // UPDATED: Simplified initialization
    private void InitializeEvolutionRequirements()
    {
        if (_controller?.MonsterData?.evolutionRequirements == null)
        {
            Debug.LogWarning($"[Evolution] No evolution requirements found for {_controller?.MonsterData?.monsterName}");
        }
    }

    public void InitializeWithMonsterData()
    {
        InitializeEvolutionRequirements();
    }

    public void InitUIParticles(MonsterUIHandler uiHandler)
    {
        _evolutionParticle = uiHandler.evolutionEffect;
        _evolutionParticleCanvasGroup = uiHandler.evolutionEffectCg;
    }

    // UPDATED: Get evolution requirements directly from MonsterData
    private EvolutionRequirement[] GetAvailableEvolutions()
    {
        if (_controller?.MonsterData?.evolutionRequirements == null)
        {
            return new EvolutionRequirement[0];
        }
        
        // Look for next level evolution (e.g., if current level is 1, look for target level 2)
        var available = _controller.MonsterData.evolutionRequirements
            .Where(req => req.targetEvolutionLevel == _controller.evolutionLevel + 1)
            .ToArray();

        return available;
    }

    public void LoadEvolutionData(float timeSinceCreation, int foodConsumed, int interactionCount)
    {
        _timeSinceCreation = timeSinceCreation;
        _foodConsumed = foodConsumed;
        _interactionCount = interactionCount;
    }

    public void UpdateEvolutionTracking(float deltaTime)
    {
        if (!CanEvolve || _controller?.MonsterData == null) return;

        _timeSinceCreation += deltaTime;
        if (Time.time - _lastUpdateTime >= 5f)
        {
            CheckEvolutionConditions();
            _lastUpdateTime = Time.time;
        }
    }

    public void OnFoodConsumed()
    {
        _foodConsumed++;
        // Check immediately after food consumption
        CheckEvolutionConditions();
    }

    public void OnInteraction()
    {
        _interactionCount++;
        
        // ADD: Delay evolution check to allow animation to complete
        _controller.StartCoroutine(DelayedEvolutionCheck());
    }

    private IEnumerator DelayedEvolutionCheck()
    {
        // Wait for poke animation to complete (usually 1-2 seconds)
        yield return new WaitForSeconds(2.5f);
        
        // Only check if we're not already evolving and in a safe state
        if (!_isEvolving)
        {
            var stateMachine = _controller.GetComponent<MonsterStateMachine>();
            var currentState = stateMachine?.CurrentState ?? MonsterState.Idle;
            
            // Only evolve if back to normal states
            if (currentState == MonsterState.Idle || currentState == MonsterState.Walking)
            {
                CheckEvolutionConditions();
            }
        }
    }

    private void CheckEvolutionConditions()
    {
        if (!CanEvolve || _isEvolving) return;

        // ADD: Debug current state
        Debug.Log($"[Evolution] Checking evolution for {_controller.monsterID} - Level: {_controller.evolutionLevel}");

        if (!IsInSafeStateForEvolution())
        {
            Debug.Log($"[Evolution] Not in safe state for evolution. Current state: {_controller.GetComponent<MonsterStateMachine>()?.CurrentState}");
            return;
        }

        var nextEvolution = GetNextEvolutionRequirement();
        if (nextEvolution == null)
        {
            Debug.Log($"[Evolution] No evolution requirement found for level {_controller.evolutionLevel + 1}");
            return;
        }

        // ADD: Debug requirement checking
        Debug.Log($"[Evolution] Checking requirements - Time: {_timeSinceCreation}/{nextEvolution.minTimeAlive}, Food: {_foodConsumed}/{nextEvolution.minFoodConsumed}, Interactions: {_interactionCount}/{nextEvolution.minInteractions}");
        Debug.Log($"[Evolution] Current stats - Happiness: {_controller.currentHappiness}/{nextEvolution.minCurrentHappiness}, Hunger: {_controller.currentHunger}/{nextEvolution.minCurrentHunger}");

        if (MeetsEvolutionRequirements(nextEvolution))
        {
            Debug.Log($"[Evolution] All requirements met! Triggering evolution from level {_controller.evolutionLevel} to {nextEvolution.targetEvolutionLevel}");
            TriggerEvolution();
        }
        else
        {
            Debug.Log($"[Evolution] Requirements not met yet");
        }
    }

    private bool IsInSafeStateForEvolution()
    {
        var stateMachine = _controller.GetComponent<MonsterStateMachine>();
        if (stateMachine == null) return true;
        
        var currentState = stateMachine.CurrentState;

        // Allow evolution in more states
        bool isSafeState = currentState == MonsterState.Idle ||
                          currentState == MonsterState.Walking;
        
        // Only block truly unsafe states
        bool isUnsafeState = currentState == MonsterState.Jumping ||
                            currentState == MonsterState.Itching ||
                            currentState == MonsterState.Flapping ||
                            currentState == MonsterState.Running ||
                            currentState == MonsterState.Flying ||
                            currentState == MonsterState.Eating;
        
        return isSafeState && !isUnsafeState;
    }

    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        int currentLevel = _controller.evolutionLevel; // e.g., 1

        foreach (var requirement in GetAvailableEvolutions())
        {
            if (requirement.targetEvolutionLevel == currentLevel + 1) // e.g., looking for level 2
            {
                return requirement;
            }
        }

        return null;
    }

    private bool MeetsEvolutionRequirements(EvolutionRequirement requirement)
    {
        bool timeCheck = _timeSinceCreation >= requirement.minTimeAlive;
        if (!timeCheck) return false;

        bool foodCheck = _foodConsumed >= requirement.minFoodConsumed;
        if (!foodCheck) return false;

        bool interactionCheck = _interactionCount >= requirement.minInteractions;
        if (!interactionCheck) return false;

        bool happinessCheck = _controller.currentHappiness >= requirement.minCurrentHappiness;
        if (!happinessCheck) return false;

        bool hungerCheck = _controller.currentHunger >= requirement.minCurrentHunger;
        if (!hungerCheck) return false;

        bool customCheck = requirement.customCondition?.Invoke(_controller) ?? true;

        return customCheck;
    }

    public void TriggerEvolution()
    {
        if (!CanEvolve || _isEvolving) return;

        _isEvolving = true;  // ← Lock evolution AND movement
        
        var oldLevel = _controller.evolutionLevel;
        var newLevel = oldLevel + 1;

        StartSimpleEvolutionEffect(oldLevel, newLevel);
    }

    private void StartSimpleEvolutionEffect(int oldLevel, int newLevel)
    {
        _controller.StartCoroutine(EvolutionSequence(oldLevel, newLevel));
    }

    private IEnumerator EvolutionSequence(int oldLevel, int newLevel)
    {
        _monsterRectTransform = _controller.GetComponent<RectTransform>();

        // ADD: Force monster to idle state during evolution
        var stateMachine = _controller.GetComponent<MonsterStateMachine>();
        
        // Force idle state for evolution
        stateMachine?.ChangeState(MonsterState.Idle);

        yield return _controller.StartCoroutine(PreEvolutionEffects());
        yield return _controller.StartCoroutine(ShowEvolutionParticles());

        _controller.evolutionLevel = newLevel;
        UpdateMonsterID(newLevel);

        yield return _controller.StartCoroutine(PostEvolutionEffects());

        _controller.UpdateVisuals();
        
        // ADD: Allow state machine to resume normal behavior
        // The state machine will naturally transition from idle
        
        OnEvolutionComplete(oldLevel, newLevel);
    }

    private IEnumerator PreEvolutionEffects()
    {
        var originalScale = _monsterRectTransform.localScale;

        // Option 1: Get components from the Spine SkeletonGraphic
        var skeletonGraphic = _controller.GetComponentInChildren<SkeletonGraphic>();
        var image = skeletonGraphic?.GetComponent<Image>(); // Spine uses Image component for rendering
        var canvasGroup = skeletonGraphic?.GetComponent<CanvasGroup>() ?? skeletonGraphic?.gameObject.AddComponent<CanvasGroup>();

        // 1. ANTICIPATION BUILD-UP - Multiple quick pulses
        yield return _controller.StartCoroutine(AnticipationPulses(originalScale, 5, 0.15f));

        // 2. ENERGY CHARGE EFFECT - Color shifting and glow
        yield return _controller.StartCoroutine(EnergyChargeEffect(image, 1.5f));

        // 3. SCREEN FLASH for impact
        yield return _controller.StartCoroutine(ScreenFlashEffect());

        // 5. FINAL TENSION HOLD
        yield return _controller.StartCoroutine(TensionHold(canvasGroup, 0.5f));
    }

    // Helper methods for enhanced effects
    private IEnumerator AnticipationPulses(Vector3 baseScale, int pulseCount, float pulseSpeed)
    {
        for (int i = 0; i < pulseCount; i++)
        {
            float intensity = 1f + (i * 0.05f); // Each pulse slightly bigger
            yield return _controller.StartCoroutine(SinglePulse(baseScale, baseScale * intensity, pulseSpeed));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator SinglePulse(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < duration / 2)
        {
            float t = elapsed / (duration / 2);
            _monsterRectTransform.localScale = Vector3.Lerp(fromScale, toScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0f;
        // Scale back
        while (elapsed < duration / 2)
        {
            float t = elapsed / (duration / 2);
            _monsterRectTransform.localScale = Vector3.Lerp(toScale, fromScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator EnergyChargeEffect(Image image, float duration)
    {
        if (image == null) yield break;
        
        Color originalColor = image.color;
        Color energyColor = new Color(1f, 1f, 0.8f, 1f); // Bright yellowish
        Color glowColor = new Color(0.8f, 0.9f, 1f, 1f);  // Blue-white glow
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Oscillating energy color
            float wave = Mathf.Sin(t * Mathf.PI * 8) * 0.5f + 0.5f;
            Color currentColor = Color.Lerp(originalColor, energyColor, wave * t);
            
            // Add glow effect by increasing brightness
            if (t > 0.7f)
            {
                float glowIntensity = (t - 0.7f) / 0.3f;
                currentColor = Color.Lerp(currentColor, glowColor, glowIntensity);
            }
            
            image.color = currentColor;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Keep energized color for particle phase
        image.color = energyColor;
    }

    private IEnumerator ScreenFlashEffect()
    {
        // Create temporary white overlay for screen flash
        var canvas = _controller.GetComponentInParent<Canvas>();
        if (canvas == null) yield break;
        
        GameObject flashOverlay = new GameObject("EvolutionFlash");
        flashOverlay.transform.SetParent(canvas.transform, false);
        
        var flashImage = flashOverlay.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashImage.raycastTarget = false;
        
        var rectTransform = flashOverlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Flash effect
        float flashDuration = 0.15f;
        float elapsed = 0f;
        
        // Flash in
        while (elapsed < flashDuration / 2)
        {
            float alpha = elapsed / (flashDuration / 2) * 0.8f;
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0f;
        // Flash out
        while (elapsed < flashDuration / 2)
        {
            float alpha = (1f - elapsed / (flashDuration / 2)) * 0.8f;
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        UnityEngine.Object.Destroy(flashOverlay);
    }

    private IEnumerator TensionHold(CanvasGroup canvasGroup, float duration)
    {
        // Subtle opacity oscillation to show energy building up
        float elapsed = 0f;
        float originalAlpha = canvasGroup.alpha;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float oscillation = Mathf.Sin(t * Mathf.PI * 12) * 0.1f; // Fast subtle flicker
            canvasGroup.alpha = Mathf.Clamp01(originalAlpha + oscillation);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = originalAlpha;
    }

    private IEnumerator ShowEvolutionParticles()
    {
        if (_evolutionParticleCanvasGroup == null) yield break;

        // Force stop any existing particles
        if (_evolutionParticle != null)
        {
            _evolutionParticle.Stop(true);
            yield return new WaitForSeconds(0.1f); // Small delay to ensure cleanup
        }

        // Reset canvas group
        _evolutionParticleCanvasGroup.alpha = 0f;

        // Start the main particle system
        if (_evolutionParticle != null)
        {
            _evolutionParticle.Play();
        }

        // Rest of your particle animation...
        float fadeTime = 1.2f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            float t = elapsed / fadeTime;
            _evolutionParticleCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Add the hold time back
        yield return new WaitForSeconds(2f);

        // Enhanced fade out
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            float t = elapsed / fadeTime;
            _evolutionParticleCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_evolutionParticle != null)
        {
            _evolutionParticle.Stop(true);
        }
    }

    private IEnumerator PostEvolutionEffects()
    {
        var image = _controller.GetComponent<Image>();

        // Reset monster to normal color
        if (image != null) _controller.StartCoroutine(ResetMonsterColor(image));

        // REVELATION EFFECT - Sparkle burst
        yield return _controller.StartCoroutine(RevelationSparkles());
    }

    private IEnumerator ResetMonsterColor(Image image)
    {
        Color currentColor = image.color;
        Color targetColor = Color.white;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            image.color = Color.Lerp(currentColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        image.color = targetColor;
    }

    private IEnumerator RevelationSparkles()
    {
        var canvas = _controller.GetComponentInParent<Canvas>();
        if (canvas == null) yield break;
        
        // Create sparkle burst effect around monster
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 50f;
            _controller.StartCoroutine(CreateSparkle(canvas, _monsterRectTransform.position + direction));
        }
        
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator CreateSparkle(Canvas canvas, Vector3 startPosition)
    {
        GameObject sparkle = new GameObject("Sparkle");
        sparkle.transform.SetParent(canvas.transform, false);
        sparkle.transform.position = startPosition;
        
        var sparkleImage = sparkle.AddComponent<Image>();
        sparkleImage.color = new Color(1f, 1f, 0.8f, 1f);
        sparkleImage.raycastTarget = false;
        
        var sparkleRect = sparkle.GetComponent<RectTransform>();
        sparkleRect.sizeDelta = new Vector2(10f, 10f);
        
        // Animate sparkle
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 originalScale = sparkleRect.localScale;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0f, 1f, t);
            float alpha = Mathf.Lerp(1f, 0f, t);
            
            sparkleRect.localScale = originalScale * scale;
            sparkleImage.color = new Color(1f, 1f, 0.8f, alpha);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        UnityEngine.Object.Destroy(sparkle);
    }

    private IEnumerator MultiBounceScale(Vector3 targetScale, float bounceHeight)
    {
        float[] bounces = { bounceHeight, bounceHeight * 0.6f, bounceHeight * 0.3f };
        
        foreach (float bounce in bounces)
        {
            // Up
            yield return _controller.StartCoroutine(ScaleTo(targetScale * bounce, 0.2f));
            // Down
            yield return _controller.StartCoroutine(ScaleTo(targetScale, 0.15f));
        }
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = _monsterRectTransform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _monsterRectTransform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        _monsterRectTransform.localScale = target;
    }

    private void UpdateMonsterID(int newLevel)
    {
        var oldID = _controller.monsterID;
        var parts = _controller.monsterID.Split('_');
        if (parts.Length >= 3)
        {
            _controller.monsterID = $"{parts[0]}_Lv{newLevel}_{parts[2]}"; // Will be Lv2, Lv3, etc.

            var gameManager = ServiceLocator.Get<MonsterManager>();
            if (gameManager != null)
            {
                var savedIDs = SaveSystem.LoadSavedMonIDs();
                if (savedIDs.Contains(oldID))
                {
                    savedIDs.Remove(oldID);
                    savedIDs.Add(_controller.monsterID);
                    SaveSystem.SaveMonIDs(savedIDs);

                    gameManager.RemoveSavedMonsterID(oldID);
                    gameManager.AddSavedMonsterID(_controller.monsterID);
                }
            }

            SaveSystem.DeleteMon(oldID);
        }
        else
        {
            Debug.LogWarning($"[Evolution] Could not update monster ID format for {_controller.monsterID}");
        }
    }

    private void OnEvolutionComplete(int oldLevel, int newLevel)
    {
        Debug.Log($"[Evolution] {_controller.monsterID} evolved from level {oldLevel} to level {newLevel}");
        
        ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.monsterName} evolved to level {newLevel}!", 3f);

        _lastEvolutionTime = Time.time;
        _foodConsumed = 0;
        _interactionCount = 0;
        
        // CRITICAL: Update visuals with new evolution level
        _controller.UpdateVisuals();
        
        _isEvolving = false;
        _controller.SaveMonData();
    }

    // Get evolution progress for UI
    public float GetEvolutionProgress()
    {
        var nextRequirement = GetNextEvolutionRequirement();
        if (nextRequirement == null)
        {
            return 1f;
        }

        float progress = 0f;
        int conditions = 0;

        if (nextRequirement.minTimeAlive > 0)
        {
            progress += Mathf.Clamp01(_timeSinceCreation / nextRequirement.minTimeAlive);
            conditions++;
        }

        if (nextRequirement.minFoodConsumed > 0)
        {
            progress += Mathf.Clamp01((float)_foodConsumed / nextRequirement.minFoodConsumed);
            conditions++;
        }

        if (nextRequirement.minInteractions > 0)
        {
            progress += Mathf.Clamp01((float)_interactionCount / nextRequirement.minInteractions);
            conditions++;
        }

        if (nextRequirement.minCurrentHappiness > 0)
        {
            progress += Mathf.Clamp01(_controller.currentHappiness / nextRequirement.minCurrentHappiness);
            conditions++;
        }        if (nextRequirement.minCurrentHunger > 0)
        {
            progress += Mathf.Clamp01(_controller.currentHunger / nextRequirement.minCurrentHunger);
            conditions++;
        }
        
        float finalProgress = conditions > 0 ? progress / conditions : 1f;

        return finalProgress;
    }
}
