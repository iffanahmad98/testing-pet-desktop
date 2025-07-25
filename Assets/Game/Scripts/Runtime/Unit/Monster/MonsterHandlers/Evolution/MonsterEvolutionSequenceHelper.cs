using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;
using CartoonFX;
using UnityEngine.UI;
using System.Linq;

public static class MonsterEvolutionSequenceHelper
{
    private static Queue<GameObject> _sparklePool = new Queue<GameObject>();
    private static List<GameObject> _activeSparkles = new List<GameObject>();
    private static GameObject _sparkleParent;
    private static GameObject _evolutionParticle;

    public static Sequence PlayEvolutionUISequence(
        Camera evolveCam,
        SkeletonGraphic spineGraphic,
        UIParticle[] evolutionParticles, // Changed to array
        SkeletonDataAsset nextEvolutionSkeleton,
        System.Action onEvolutionDataUpdate
    )
    {
        // Use different particles for different phases
        UIParticle rampUpParticle = (evolutionParticles != null && evolutionParticles.Length > 6) 
            ? evolutionParticles[0] : null;  // Index 6 for ramp up
        UIParticle coverUpParticle = (evolutionParticles != null && evolutionParticles.Length > 7) 
            ? evolutionParticles[8] : null;  // Index 7 for cover up
        UIParticle finishParticle = (evolutionParticles != null && evolutionParticles.Length > 8) 
            ? evolutionParticles[2] : null;  // Index 8 for finish
        
        if (rampUpParticle == null || coverUpParticle == null || finishParticle == null)
        {
            Debug.LogError("Evolution particles missing! Need particles at indices 6, 7, and 8");
            return DOTween.Sequence();
        }

        // Get components for all particles
        var rampUpCg = rampUpParticle.GetComponent<CanvasGroup>();
        var coverUpCg = coverUpParticle.GetComponent<CanvasGroup>();
        var finishCg = finishParticle.GetComponent<CanvasGroup>();

        var originalFOV = evolveCam.fieldOfView;
        var originalPosition = MainCanvas.CamRT.anchoredPosition;

        // Get BiomeManager from ServiceLocator
        var biomeManager = ServiceLocator.Get<BiomeManager>();

        // Store original monster scale, position, AND anchor/pivot settings for restoration
        var originalScale = spineGraphic.rectTransform.localScale;
        var originalPos = spineGraphic.rectTransform.anchoredPosition;
        var originalAnchorMin = spineGraphic.rectTransform.anchorMin;
        var originalAnchorMax = spineGraphic.rectTransform.anchorMax;
        var originalPivot = spineGraphic.rectTransform.pivot;

        // ALSO store parent (MonsterController) original settings
        var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
        var originalParentScale = parentRect.localScale;
        var originalParentPos = parentRect.anchoredPosition;
        var originalParentAnchorMin = parentRect.anchorMin;
        var originalParentAnchorMax = parentRect.anchorMax;
        var originalParentPivot = parentRect.pivot;

        var seq = DOTween.Sequence();

        // 1. Enable darken filter if BiomeManager exists
        seq.AppendCallback(() =>
        {
            if (biomeManager != null)
            {
                biomeManager.ToggleFilters("Darken", true);
            }

            var monsterRect = spineGraphic.rectTransform;
            var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
            
            // Set anchor and pivot to center for proper scaling
            monsterRect.anchorMin = new Vector2(0.5f, 0.5f);
            monsterRect.anchorMax = new Vector2(0.5f, 0.5f);
            monsterRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Also set parent anchor/pivot for proper centering
            parentRect.anchorMin = new Vector2(0.5f, 0.5f);
            parentRect.anchorMax = new Vector2(0.5f, 0.5f);
            parentRect.pivot = new Vector2(0.5f, 0.5f);
        });

        // Phase 1: Move parent MonsterController to center and start scaling (simulates camera focusing)
        parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
        seq.Append(parentRect.DOAnchorPos(new Vector2(0f, -50f), 1.5f).SetEase(Ease.InOutCubic));
        seq.Join(parentRect.DOScale(Vector3.one * 4.0f, 1.5f).SetEase(Ease.InOutCubic));

        // Phase 2: Continue scaling parent bigger (simulates zoom-in effect)  
        seq.Append(parentRect.DOScale(Vector3.one * 4.0f, 2.0f).SetEase(Ease.InOutSine));

        // Add shake to parent during zoom
        seq.JoinCallback(() =>
        {
            parentRect.DOShakePosition(3.5f, strength: 8f, vibrato: 15, randomness: 50f);
        });

        // 2. Play RAMP UP particle during zoom (monster still visible)
        seq.JoinCallback(() =>
        {
            rampUpParticle.gameObject.SetActive(true);
            rampUpCg.alpha = 0f;
            rampUpCg.DOFade(1f, 1.0f).SetEase(Ease.InOutSine);
            
            // Scale shake effect during zoom (apply to parent)
            parentRect.DOShakeScale(3.5f, strength: 0.2f, vibrato: 8, randomness: 90f);
        });

        // 3. Peak zoom moment - transition to cover up particle
        seq.AppendInterval(2.5f); // Wait for initial ramp up
        seq.AppendCallback(() => {
            // Fade out ramp up particle
            rampUpCg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => {
                rampUpParticle.gameObject.SetActive(false);
            });
            
            // Start cover up particle
            coverUpParticle.gameObject.SetActive(true);
            coverUpCg.alpha = 0f;
            coverUpCg.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
            
            // Intense shake at transformation moment
            spineGraphic.rectTransform.DOShakePosition(0.5f, strength: 20f, vibrato: 40, randomness: 90f);
        });
        
        // Wait for cover up particle to be fully visible
        seq.AppendInterval(1.0f);
        
        // Fade out monster while cover up particle is on
        var monsterCanvasGroup = spineGraphic.GetComponent<CanvasGroup>();
        if (monsterCanvasGroup == null)
        {
            monsterCanvasGroup = spineGraphic.gameObject.AddComponent<CanvasGroup>();
        }
        seq.Append(monsterCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine));
        
        // Change skeleton while monster is invisible (cover up particle still on)
        seq.AppendCallback(() => {
            spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            spineGraphic.Initialize(true);
            onEvolutionDataUpdate?.Invoke();
        });
        
        // Fade monster back in with new skeleton (cover up particle still on)
        seq.Append(monsterCanvasGroup.DOFade(1f, 0.8f).SetEase(Ease.InOutSine));
        
        // Wait a moment, then transition to finish particle
        seq.AppendInterval(1.0f);
        seq.AppendCallback(() => {
            // Fade out cover up particle
            coverUpCg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => {
                coverUpParticle.gameObject.SetActive(false);
            });
            
            // Start finish particle
            finishParticle.gameObject.SetActive(true);
            finishCg.alpha = 0f;
            finishCg.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        });
        
        // Let finish particle play for a while, then fade out
        seq.AppendInterval(2.0f); // Monster visible with finish particle
        seq.AppendCallback(() => {
            // Fade out finish particle
            finishCg.DOFade(0f, 1.5f).SetEase(Ease.InOutSine).OnComplete(() => {
                finishParticle.gameObject.SetActive(false);
            });
        });

        // 4. Wait for finish particle fade to complete
        seq.AppendInterval(2f); // Wait for particle fade

        // 5. SIMULATE ZOOM OUT: Scale back down and return to original position
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>().DOScale(Vector3.one * 1.2f, 1.0f).SetEase(Ease.InOutCubic));
        seq.Join(spineGraphic.rectTransform.DOAnchorPos(originalPos, 1.0f).SetEase(Ease.InOutCubic));
        // ADD: Return parent to original position
        seq.Join(spineGraphic.transform.parent.GetComponent<RectTransform>().DOAnchorPos(originalParentPos, 1.0f).SetEase(Ease.InOutCubic));

        // 6. Final scale to normal size and ensure final positioning
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>().DOScale(originalParentScale, 0.5f).SetEase(Ease.OutBack));

        seq.AppendCallback(() =>
        {
            // Restore original anchor and pivot settings
            spineGraphic.rectTransform.anchoredPosition = originalPos;
            spineGraphic.rectTransform.anchorMin = originalAnchorMin;
            spineGraphic.rectTransform.anchorMax = originalAnchorMax;
            spineGraphic.rectTransform.pivot = originalPivot;
            spineGraphic.rectTransform.localScale = originalScale;
            
            // Restore parent anchor and pivot settings
            var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
            parentRect.anchoredPosition = originalParentPos;
            parentRect.localScale = originalParentScale;
            parentRect.anchorMin = originalParentAnchorMin;
            parentRect.anchorMax = originalParentAnchorMax;
            parentRect.pivot = originalParentPivot;

            // Disable blur effect at the end of evolution
            if (biomeManager != null)
            {
                biomeManager.ToggleFilters("Darken", false);
            }
        });

        return seq;
    }

    // Create sparkle sprite programmatically
    private static Sprite CreateSparkleSprite()
    {
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        
        // Create a 4-pointed star shape
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                var pos = new Vector2(x - 32, y - 32);
                float alpha = 0f;
                
                // Create star pattern - four pointed star
                float angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
                float distance = pos.magnitude;
                
                // Create 4 rays at 0, 90, 180, 270 degrees
                for (int ray = 0; ray < 4; ray++)
                {
                    float rayAngle = ray * 90f;
                    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, rayAngle));
                    
                    if (angleDiff < 15f) // Ray width
                    {
                        float rayAlpha = Mathf.Clamp01(1f - (distance / 25f)); // Ray length
                        rayAlpha *= Mathf.Clamp01(1f - (angleDiff / 15f)); // Ray sharpness
                        alpha = Mathf.Max(alpha, rayAlpha);
                    }
                }
                
                // Add center glow
                float centerGlow = Mathf.Clamp01(1f - (distance / 8f));
                alpha = Mathf.Max(alpha, centerGlow);
                
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }

    private static void InitializeSparklePool()
    {
        if (_sparkleParent == null)
        {
            _sparkleParent = new GameObject("SparklePool");
            _sparkleParent.transform.SetParent(MainCanvas.Canvas.transform, false);
            _sparkleParent.SetActive(false);
        }

        // Pre-populate pool with sparkle objects
        for (int i = 0; i < 20; i++)
        {
            var sparkle = CreateSparkleObject();
            sparkle.transform.SetParent(_sparkleParent.transform, false);
            sparkle.SetActive(false);
            _sparklePool.Enqueue(sparkle);
        }
    }

    private static GameObject CreateSparkleObject()
    {
        var sparkleGO = new GameObject("Sparkle");
        
        // Add Image component for UI rendering
        var image = sparkleGO.AddComponent<Image>();
        image.sprite = CreateSparkleSprite(); // Use native sprite
        image.color = Color.green;
        image.raycastTarget = false;
        
        // Add RectTransform - LARGER SPARKLES
        var rect = sparkleGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(45f, 45f); // Increased from 30f to 45f (1.5x larger)
        
        // Add CanvasGroup for smooth fading
        var cg = sparkleGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        
        return sparkleGO;
    }

    private static GameObject GetSparkleFromPool()
    {
        if (_sparklePool.Count == 0)
        {
            InitializeSparklePool();
        }

        if (_sparklePool.Count > 0)
        {
            var sparkle = _sparklePool.Dequeue();
            sparkle.SetActive(true);
            return sparkle;
        }

        // Fallback: create new sparkle if pool is empty
        return CreateSparkleObject();
    }

    private static void ReturnSparkleToPool(GameObject sparkle)
    {
        sparkle.SetActive(false);
        sparkle.transform.SetParent(_sparkleParent.transform, false); // Return to pool parent
        sparkle.GetComponent<CanvasGroup>().alpha = 0f;
        sparkle.transform.localScale = Vector3.one;
        sparkle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Reset position
        _sparklePool.Enqueue(sparkle);
        _activeSparkles.Remove(sparkle);
    }

    private static void StartSparkleEffect(RectTransform targetTransform)
    {
        if (_sparklePool.Count == 0)
        {
            InitializeSparklePool();
        }

        // Create sparkles around the monster
        for (int i = 0; i < 8; i++)
        {
            CreateSingleSparkle(targetTransform, i * 0.1f);
        }
    }

    private static void CreateSingleSparkle(RectTransform targetTransform, float delay)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            var sparkle = GetSparkleFromPool();
            if (sparkle == null) return;

            var sparkleRect = sparkle.GetComponent<RectTransform>();
            var canvasGroup = sparkle.GetComponent<CanvasGroup>();
            var image = sparkle.GetComponent<Image>();
            
            // Set parent to the monster instead of main canvas
            sparkle.transform.SetParent(targetTransform, false);
            
            // MORE CENTERED - Reduced radius range around the monster
            var radius = Random.Range(25f, 75f); // Reduced from 50f-150f to 25f-75f (more centered)
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var offset = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
            
            sparkleRect.anchoredPosition = offset; // Relative to monster position
            sparkleRect.localScale = Vector3.zero;
            
            // Random color tint - Green shades
            var colors = new Color[] { 
                Color.green,                        // Pure green
                new Color(0.5f, 1f, 0.5f),         // Light green
                new Color(0f, 0.8f, 0.2f),         // Dark green
                new Color(0.2f, 1f, 0.4f),         // Bright lime green
                new Color(0.4f, 0.9f, 0.6f),       // Soft mint green
                new Color(0.1f, 0.7f, 0.3f),       // Forest green
                new Color(0.6f, 1f, 0.8f),         // Very light green
                new Color(0.8f, 1f, 0.2f)          // Yellow-green
            };
            image.color = colors[Random.Range(0, colors.Length)];
            
            _activeSparkles.Add(sparkle);
            
            // Animate sparkle - LARGER SCALE RANGE
            var seq = DOTween.Sequence();
            
            // Scale up and fade in - LARGER SPARKLES
            seq.Append(sparkleRect.DOScale(Random.Range(0.8f, 1.5f), 1.5f).SetEase(Ease.OutBack)); // Increased from 0.5f-1.2f to 0.8f-1.5f
            seq.Join(canvasGroup.DOFade(1f, 1.5f));
            
            // Float upward while rotating (relative to monster)
            seq.Join(sparkleRect.DOAnchorPosY(sparkleRect.anchoredPosition.y + Random.Range(30f, 80f), 7.5f).SetEase(Ease.OutQuad));
            seq.Join(sparkleRect.DORotate(new Vector3(0, 0, Random.Range(180f, 360f)), 7.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            
            // Fade out and scale down
            seq.Append(canvasGroup.DOFade(0f, 2.5f));
            seq.Join(sparkleRect.DOScale(0f, 2.5f).SetEase(Ease.InBack));
            
            // Return to pool when done
            seq.OnComplete(() => ReturnSparkleToPool(sparkle));
        });
    }

    private static void StopSparkleEffect()
    {
        // Stop creating new sparkles, let existing ones finish their animation
        DOTween.Kill("sparkle_creation");
        
        // Optionally fade out all active sparkles quickly
        foreach (var sparkle in _activeSparkles.ToArray())
        {
            if (sparkle != null && sparkle.activeInHierarchy)
            {
                var cg = sparkle.GetComponent<CanvasGroup>();
                cg.DOFade(0f, 1.5f).OnComplete(() => ReturnSparkleToPool(sparkle));
            }
        }
    }
}
