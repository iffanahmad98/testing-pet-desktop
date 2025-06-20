using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

[Serializable]
public class MonsterEvolutionHandler
{
    public bool IsEvolving => _isEvolving;
    private bool _isEvolving = false;
    private MonsterController _controller;

    // Evolution tracking
    private float _lastUpdateTime;
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

    public void InitUIParticles(MonsterUIHandler uiHandler)
    {
        _evolutionParticle = uiHandler.evolutionEffect;
        _evolutionParticleCanvasGroup = uiHandler.evolutionEffectCg;
    }

    private EvolutionRequirement[] GetAvailableEvolutions()
    {
        if (_controller?.MonsterData?.evolutionRequirements == null)
            return new EvolutionRequirement[0];

        return _controller.MonsterData.evolutionRequirements
            .Where(req => req.targetEvolutionLevel == _controller.evolutionLevel + 1)
            .ToArray();
    }

    public void LoadEvolutionData(float timeSinceCreation, int foodConsumed, int interactionCount)
    {
        _timeSinceCreation = timeSinceCreation;
        _foodConsumed = foodConsumed;
        _interactionCount = interactionCount;
    }

    public void UpdateEvolutionTracking(float deltaTime)
    {
        if (!CanEvolve) return;

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
        CheckEvolutionConditions();
    }

    public void OnInteraction()
    {
        _interactionCount++;
        _controller.StartCoroutine(DelayedEvolutionCheck());
    }

    private IEnumerator DelayedEvolutionCheck()
    {
        yield return new WaitForSeconds(2.5f);

        if (!_isEvolving)
        {
            var stateMachine = _controller.GetComponent<MonsterStateMachine>();
            if (stateMachine?.CurrentState == MonsterState.Idle || stateMachine?.CurrentState == MonsterState.Walking)
                CheckEvolutionConditions();
        }
    }

    private void CheckEvolutionConditions()
    {
        if (!CanEvolve || _isEvolving) return;

        var nextEvolution = GetNextEvolutionRequirement();
        if (nextEvolution == null) return;

        if (MeetsEvolutionRequirements(nextEvolution))
            TriggerEvolution();
    }

    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        return GetAvailableEvolutions().FirstOrDefault();
    }

    private bool MeetsEvolutionRequirements(EvolutionRequirement req)
    {
        return _timeSinceCreation >= req.minTimeAlive &&
               _foodConsumed >= req.minFoodConsumed &&
               _interactionCount >= req.minInteractions &&
               _controller.currentHappiness >= req.minCurrentHappiness &&
               _controller.currentHunger >= req.minCurrentHunger &&
               (req.customCondition?.Invoke(_controller) ?? true);
    }

    public void TriggerEvolution()
    {
        if (!CanEvolve || _isEvolving) return;

        _isEvolving = true;
        var oldLevel = _controller.evolutionLevel;
        var newLevel = oldLevel + 1;

        _controller.StartCoroutine(EvolutionSequence(newLevel));
    }

    private IEnumerator EvolutionSequence(int targetLevel)
    {
        _monsterRectTransform = _controller.GetComponent<RectTransform>();
        _controller.GetComponent<MonsterStateMachine>()?.ChangeState(MonsterState.Idle);

        // Animation sequence
        yield return BasicVisualEffect();

        // Update monster data
        _controller.evolutionLevel = targetLevel;
        UpdateMonsterID(targetLevel);
        _controller.UpdateVisuals();

        // Show evolution message
        ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.monsterName} evolved to level {targetLevel}!", 3f);

        // IMPORTANT: Save data BEFORE resetting counters
        _controller.SaveMonData();

        // NOW reset counters for next evolution
        _foodConsumed = 0;
        _interactionCount = 0;

        _controller.UpdateVisuals();
        
        // Add a cooldown period where the monster stays idle
        float postEvolutionIdleTime = 2.5f;
        yield return new WaitForSeconds(postEvolutionIdleTime);
        
        _isEvolving = false;
    }

    private void UpdateMonsterID(int newLevel)
    {
        var oldID = _controller.monsterID;
        var parts = _controller.monsterID.Split('_');
        if (parts.Length >= 3)
        {
            // Update the ID in the controller
            _controller.monsterID = $"{parts[0]}_Stage{newLevel}_{parts[2]}";

            // Update the MonsterManager references
            var monsterManager = ServiceLocator.Get<MonsterManager>();
            if (monsterManager != null)
            {
                // Update saved IDs list in SaveSystem
                var savedIDs = SaveSystem.LoadSavedMonIDs();
                if (savedIDs.Contains(oldID))
                {
                    savedIDs.Remove(oldID);
                    savedIDs.Add(_controller.monsterID);
                    SaveSystem.SaveMonIDs(savedIDs);
                    monsterManager.RemoveSavedMonsterID(oldID);
                    monsterManager.AddSavedMonsterID(_controller.monsterID);
                }
            }

            // Remove the old save data
            SaveSystem.DeleteMon(oldID);
        }
    }

    public float GetEvolutionProgress()
    {
        var req = GetNextEvolutionRequirement();
        if (req == null) return 1f;

        float timeProgress = req.minTimeAlive > 0 ? _timeSinceCreation / req.minTimeAlive : 1f;
        float foodProgress = req.minFoodConsumed > 0 ? (float)_foodConsumed / req.minFoodConsumed : 1f;
        float interactionProgress = req.minInteractions > 0 ? (float)_interactionCount / req.minInteractions : 1f;
        float happinessProgress = req.minCurrentHappiness > 0 ? _controller.currentHappiness / req.minCurrentHappiness : 1f;
        float hungerProgress = req.minCurrentHunger > 0 ? _controller.currentHunger / req.minCurrentHunger : 1f;

        return Mathf.Clamp01((timeProgress + foodProgress + interactionProgress + happinessProgress + hungerProgress) / 5f);
    }
    
    private IEnumerator BasicVisualEffect()
    {
        // Store original properties
        Vector3 originalScale = _monsterRectTransform.localScale;
        Vector2 originalPosition = _monsterRectTransform.anchoredPosition;
        
        // Create objects we'll need for zoom effects
        Canvas canvas = _controller.GetComponentInParent<Canvas>();
        if (canvas == null) yield break;
        
        // Create darkening panel
        GameObject darkPanel = new GameObject("FocusPanel");
        darkPanel.transform.SetParent(canvas.transform.GetChild(0), false);
        darkPanel.transform.SetSiblingIndex(1);
        Image panelImage = darkPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0);
        panelImage.raycastTarget = false;
        RectTransform panelRect = darkPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create highlight
        GameObject highlight = new GameObject("MonsterHighlight");
        highlight.transform.SetParent(canvas.transform, false);
        highlight.transform.SetSiblingIndex(_monsterRectTransform.GetSiblingIndex() - 1);
        Image highlightImage = highlight.AddComponent<Image>();
        highlightImage.sprite = CreateCircleSprite();
        highlightImage.color = new Color(1f, 0.95f, 0.7f, 0f);
        RectTransform highlightRect = highlight.GetComponent<RectTransform>();
        highlightRect.anchoredPosition = _monsterRectTransform.anchoredPosition;
        highlightRect.sizeDelta = _monsterRectTransform.sizeDelta * 1.5f;
        
        // Store hierarchy info
        Transform originalParent = _monsterRectTransform.parent;
        int originalSiblingIndex = _monsterRectTransform.GetSiblingIndex();
        
        // Move monster to canvas
        _monsterRectTransform.SetParent(canvas.transform, true);
        _monsterRectTransform.SetAsLastSibling();
        
        // Target values for zoom
        Vector3 targetScale = originalScale * 5f;
        Vector2 canvasCenter = Vector2.zero;
        
        // 1. ZOOM IN EFFECT
        float elapsed = 0f, duration = 1.0f;
        while (elapsed < duration * 0.7f)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / (duration * 0.7f));
            
            // Darken background
            panelImage.color = new Color(0, 0, 0, t * 0.7f);
            
            // Scale up monster
            _monsterRectTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            
            // Move monster to center
            _monsterRectTransform.anchoredPosition = Vector2.Lerp(originalPosition, canvasCenter, t);
            
            // Fade in highlight
            highlightImage.color = new Color(1f, 0.95f, 0.7f, t * 0.4f);
            highlightRect.sizeDelta = _monsterRectTransform.sizeDelta * (2.0f + t * 1.0f);
            highlightRect.anchoredPosition = _monsterRectTransform.anchoredPosition;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Make sure we're exactly at target values
        _monsterRectTransform.localScale = targetScale;
        _monsterRectTransform.anchoredPosition = canvasCenter;
        highlightRect.anchoredPosition = canvasCenter;
        
        // Small pause at full zoom
        yield return new WaitForSeconds(1f);
        
        // 2. PULSE EFFECT (using ZOOMED scale as base)
        for (int i = 0; i < 3; i++)
        {
            // Pulse up with rotation
            elapsed = 0f;
            duration = 0.15f;
            Vector3 startScale = targetScale; // Use zoomed scale
            Vector3 pulseScale = targetScale * 1.2f; // Pulse 20% larger from zoomed
            
            while (elapsed < duration)
            {
                float t = elapsed/duration;
                _monsterRectTransform.localScale = Vector3.Lerp(startScale, pulseScale, t);
                _monsterRectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI) * 5f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Pulse down
            elapsed = 0f;
            startScale = _monsterRectTransform.localScale;
            
            while (elapsed < duration)
            {
                float t = elapsed/duration;
                _monsterRectTransform.localScale = Vector3.Lerp(startScale, targetScale, t); // Back to zoomed scale
                _monsterRectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin((t+1) * Mathf.PI) * 5f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
        }

        // 3. RIPPLE EFFECT (centered)
        if (canvas != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject rippleObj = new GameObject("EvolutionRipple");
                rippleObj.transform.SetParent(canvas.transform, false);
                
                Image rippleImage = rippleObj.AddComponent<Image>();
                rippleObj.AddComponent<Mask>().showMaskGraphic = true;
                rippleImage.sprite = CreateCircleSprite();
                
                Color rippleColor = new Color(1f, 1f, 0.7f, 0.7f);
                rippleImage.color = rippleColor;
                rippleImage.raycastTarget = false;
                
                RectTransform rippleRect = rippleObj.GetComponent<RectTransform>();
                rippleRect.anchoredPosition = canvasCenter; // Use canvas center
                
                // Scale ripple with monster's zoomed size
                float baseSize = 30 * (targetScale.x / originalScale.x);
                rippleRect.sizeDelta = new Vector2(baseSize, baseSize);
                
                // Expand and fade out
                _controller.StartCoroutine(AnimateRipple(rippleRect, rippleImage, 0.8f));
                
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 4. SCREEN FLASH EFFECT
        if (canvas != null)
        {
            GameObject flashObj = new GameObject("EvolutionFlash");
            flashObj.transform.SetParent(canvas.transform, false);
            Image flashImage = flashObj.AddComponent<Image>();
            flashImage.color = new Color(1f, 1f, 0.7f, 0f); // Golden tint
            flashImage.raycastTarget = false;
            
            RectTransform flashRect = flashObj.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;
            
            // Flash in and out with color change
            // float elapsed = 0f, duration = 0.5f;
            while (elapsed < duration)
            {
                float progress = elapsed/duration;
                float alpha = progress < 0.5f ? progress * 2f * 0.8f : (1f - (progress-0.5f) * 2f) * 0.8f;
                
                // Change flash color over time for rainbow effect
                Color flashColor = Color.Lerp(
                    new Color(1f, 1f, 0.7f, alpha), // Gold
                    new Color(0.8f, 1f, 0.8f, alpha), // Green-white
                    progress
                );
                
                flashImage.color = flashColor;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            UnityEngine.Object.Destroy(flashObj);
        }

        // 5. SPARKLE EFFECTS (positioned around center)
        if (canvas != null)
        {
            for (int i = 0; i < 15; i++)
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = UnityEngine.Random.Range(20f, 100f) * (targetScale.x / originalScale.x); // Scale with zoom
                
                Vector2 position = canvasCenter + // Use canvas center
                                new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                
                GameObject sparkle = new GameObject("Sparkle");
                sparkle.transform.SetParent(canvas.transform, false);
                
                Image sparkleImg = sparkle.AddComponent<Image>();
                sparkleImg.sprite = CreateSimpleSparkleSprite();
                sparkleImg.raycastTarget = false;
                
                // Random size and delay - scale with zoom
                float size = UnityEngine.Random.Range(20f, 80f) * (targetScale.x / originalScale.x);
                float delay = UnityEngine.Random.Range(0f, 0.7f);
                
                RectTransform sparkleRect = sparkle.GetComponent<RectTransform>();
                sparkleRect.anchoredPosition = position;
                sparkleRect.sizeDelta = new Vector2(size, size);
                
                _controller.StartCoroutine(AnimateSparkleWithDelay(
                    sparkleRect, sparkleImg, UnityEngine.Random.Range(0.5f, 1.5f), delay));
            }
        }

        // 6. HOVER EFFECT (at center)
        Vector2 startPos = canvasCenter; // Use canvas center
        float hoverTime = 0f, hoverDuration = 1.5f;
        Quaternion originalRotation = _monsterRectTransform.rotation;
        
        while (hoverTime < hoverDuration)
        {
            float progress = hoverTime / hoverDuration;
            
            // Scale hover height with zoom factor
            float hoverHeight = 15f * (targetScale.x / originalScale.x);
            float yOffset = Mathf.Sin(progress * Mathf.PI * 2) * hoverHeight;
            _monsterRectTransform.anchoredPosition = startPos + new Vector2(0, yOffset);
            
            float rotAngle = Mathf.Sin(progress * Mathf.PI * 3) * 8f;
            _monsterRectTransform.rotation = originalRotation * Quaternion.Euler(0, 0, rotAngle);
            
            hoverTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset position to center exactly
        _monsterRectTransform.anchoredPosition = canvasCenter;
        _monsterRectTransform.rotation = originalRotation;
        
        // 7. PARTICLE EFFECT (if available)
        if (_evolutionParticle != null && _evolutionParticleCanvasGroup != null)
        {
            _evolutionParticleCanvasGroup.alpha = 1f;
            _evolutionParticle.Play();
            yield return new WaitForSeconds(2f);
            _evolutionParticleCanvasGroup.alpha = 0f;
            _evolutionParticle.Stop(true);
        }
        
        // 8. FINALLY ZOOM OUT
        elapsed = 0f;
        duration = 1.0f;
        while (elapsed < duration * 0.5f)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / (duration * 0.5f));
            
            // Restore background
            panelImage.color = new Color(0, 0, 0, 0.7f * (1-t));
            
            // Restore monster scale
            _monsterRectTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            
            // Restore monster position
            _monsterRectTransform.anchoredPosition = Vector2.Lerp(canvasCenter, originalPosition, t);
            
            // Fade out highlight
            highlightImage.color = new Color(1f, 0.95f, 0.7f, 0.4f * (1-t));
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restore monster to original parent
        _monsterRectTransform.SetParent(originalParent, true);
        _monsterRectTransform.SetSiblingIndex(originalSiblingIndex);
        
        // Ensure exact reset
        _monsterRectTransform.localScale = originalScale;
        _monsterRectTransform.anchoredPosition = originalPosition;
        
        // Clean up objects
        UnityEngine.Object.Destroy(darkPanel);
        UnityEngine.Object.Destroy(highlight);
    }

    // Helper method to animate ripple
    private IEnumerator AnimateRipple(RectTransform rectTransform, Image image, float duration)
    {
        float elapsed = 0f;
        Vector2 startSize = rectTransform.sizeDelta;
        Vector2 endSize = startSize * 40f; // Increased from 8f
        Color startColor = image.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, t);
            image.color = Color.Lerp(startColor, endColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        UnityEngine.Object.Destroy(rectTransform.gameObject);
    }

    // Helper method to animate sparkles
    private IEnumerator AnimateSparkle(RectTransform rectTransform, Image image, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = startScale * 0.2f;
        Color startColor = new Color(1f, 1f, 0.8f, 1f);
        Color midColor = new Color(1f, 1f, 1f, 0.8f);
        Color endColor = new Color(1f, 1f, 0.8f, 0f);
        image.color = startColor;
        
        // Float upward
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 40f);
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Scale and fade
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            // Color transition: start -> mid -> end
            if (t < 0.5f)
                image.color = Color.Lerp(startColor, midColor, t * 2f);
            else
                image.color = Color.Lerp(midColor, endColor, (t - 0.5f) * 2f);
            
            // Float upward
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            // Rotate slowly
            rectTransform.Rotate(0, 0, Time.deltaTime * 30f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        UnityEngine.Object.Destroy(rectTransform.gameObject);
    }

    // Creates a programmable circle sprite
    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float centerX = resolution / 2f;
        float centerY = resolution / 2f;
        float radius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                if (distance < radius)
                {
                    // Soft circle with feathered edges
                    float alpha = 1f - (distance / radius);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // Creates a simple sparkle shape
    private Sprite CreateSimpleSparkleSprite()
    {
        int resolution = 32;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float centerX = resolution / 2f;
        float centerY = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distX = Mathf.Abs(x - centerX) / (resolution / 2f);
                float distY = Mathf.Abs(y - centerY) / (resolution / 2f);
                
                // Create a star/cross shape
                if (distX < 0.3f || distY < 0.3f)
                {
                    float distance = Mathf.Max(distX, distY);
                    float alpha = 1f - distance * 2f;
                    alpha = Mathf.Clamp01(alpha);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // Helper method to animate sparkles with delay
    private IEnumerator AnimateSparkleWithDelay(RectTransform rectTransform, Image image, float duration, float delay)
    {
        // Initial setup - fully transparent until delay is over
        image.color = new Color(1f, 1f, 0.8f, 0f);
        yield return new WaitForSeconds(delay);
        
        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = startScale * 0.2f;
        
        Color startColor = new Color(1f, 1f, 0.8f, 1f);
        Color midColor = new Color(1f, 1f, 1f, 0.8f);
        Color endColor = new Color(1f, 1f, 0.8f, 0f);
        image.color = startColor;
        
        // Float upward
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(UnityEngine.Random.Range(-20f, 20f), 60f);
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Scale and fade
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            // Color transition: start -> mid -> end
            if (t < 0.5f)
                image.color = Color.Lerp(startColor, midColor, t * 2f);
            else
                image.color = Color.Lerp(midColor, endColor, (t - 0.5f) * 2f);
            
            // Float upward with some randomness
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            // Rotate slowly - vary rotation speed based on size
            float rotSpeed = 30f / rectTransform.sizeDelta.x * 10f;
            rectTransform.Rotate(0, 0, Time.deltaTime * rotSpeed);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        UnityEngine.Object.Destroy(rectTransform.gameObject);
    }
}
