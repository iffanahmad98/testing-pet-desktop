using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;
using CartoonFX;
using UnityEngine.UI;
using System.Linq;
using Mono.Cecil.Cil;

public static class MonsterEvolutionSequenceHelper
{
    private static Queue<GameObject> _sparklePool = new Queue<GameObject>();
    private static List<GameObject> _activeSparkles = new List<GameObject>();
    private static GameObject _sparkleParent;
    private static GameObject _evolutionParticle;

    public static Sequence PlayEvolutionUISequence(
        Camera evolveCam,
        SkeletonGraphic spineGraphic,
        UIParticle evolutionParticle,
        SkeletonDataAsset nextEvolutionSkeleton,
        System.Action onEvolutionDataUpdate,
        Vector3 monsterPosition)
    {
        _evolutionParticle = evolutionParticle.gameObject;
        var originalFOV = evolveCam.fieldOfView;
        var originalPosition = MainCanvas.CamRT.anchoredPosition;
        var particle = evolutionParticle.gameObject;
        var particleCg = particle.GetComponent<CanvasGroup>();

        var effect = particle.GetComponent<CFXR_Effect>();
        var shake = effect.cameraShake;
        shake.useMainCamera = false;
        shake.cameras = new List<Camera> { MainCanvas.MonsterCamera };
        shake.enabled = true;

        var seq = DOTween.Sequence();

        // 1. Camera zoom in - 5x slower
        seq.AppendCallback(() =>
        {
            MainCanvas.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            MainCanvas.Canvas.worldCamera = evolveCam;
        });
        seq.Append(MainCanvas.CamRT.DOAnchorPos(monsterPosition, 2.5f).SetEase(Ease.InOutSine));
        seq.Append(evolveCam.DOFieldOfView(2.5f, 4.0f).SetEase(Ease.InOutSine));

        // 2. Play particle and start sparkles
        seq.JoinCallback(() =>
        {
            particle.SetActive(true);
            particleCg.alpha = 0f;
            particleCg.DOFade(1f, 1.0f).SetEase(Ease.InOutSine);
            shake.StartShake();
            StartSparkleEffect(spineGraphic.rectTransform);
        });

        // 3. Create glow cover and change skeleton during glow - 5x scaled timing
        seq.AppendInterval(2.0f); // Wait 2s before glow starts
        seq.AppendCallback(() => {
            CreateGlowCover(spineGraphic, () => {
                // Change skeleton during the glow peak (at 1.5s into the glow)
                spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
                spineGraphic.Initialize(true);
                onEvolutionDataUpdate?.Invoke();
            });
            
            // Stop particles and shake after glow starts
            particle.SetActive(false);
            particleCg.DOFade(0f, 2.5f).SetEase(Ease.InOutSine); // 5x slower fade
            shake.StopShake();
        });

        // 4. Continue sparkles for a bit after evolution (wait for glow to complete)
        seq.AppendInterval(4f); // Wait 4s for glow + 5s for sparkles
        seq.AppendCallback(() =>
        {
            StopSparkleEffect();
        });

        // 5. Camera zoom out IMMEDIATELY after sparkles stop
        seq.Append(evolveCam.DOFieldOfView(originalFOV, 0.8f).SetEase(Ease.InOutSine));
        seq.Join(MainCanvas.CamRT.DOAnchorPos(originalPosition, 0.5f).SetEase(Ease.InOutSine));
        seq.AppendCallback(() =>
        {
            MainCanvas.Canvas.worldCamera = null;
            MainCanvas.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        });

        // 6. Additional 1 seconds after evolution sequence completion
        // seq.AppendInterval(1.0f);

        return seq;
    }

    // Create sparkle sprite programmatically
    private static Sprite CreateSparkleSprite()
    {
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var center = new Vector2(32, 32);
        
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
        image.color = Color.white;
        image.raycastTarget = false;
        
        // Add RectTransform
        var rect = sparkleGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30f, 30f); // Slightly larger for better visibility
        
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
            
            // Random position around the monster (now relative to monster)
            var radius = Random.Range(50f, 150f);
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var offset = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
            
            sparkleRect.anchoredPosition = offset; // Relative to monster position
            sparkleRect.localScale = Vector3.zero;
            
            // Random color tint
            var colors = new Color[] { 
                Color.white, 
                Color.yellow, 
                new Color(1f, 0.8f, 0.9f), // Light pink
                new Color(0.8f, 0.9f, 1f)  // Light blue
            };
            image.color = colors[Random.Range(0, colors.Length)];
            
            _activeSparkles.Add(sparkle);
            
            // Animate sparkle
            var seq = DOTween.Sequence();
            
            // Scale up and fade in
            seq.Append(sparkleRect.DOScale(Random.Range(0.5f, 1.2f), 1.5f).SetEase(Ease.OutBack));
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

    private static void CreateParticleBurst(SkeletonGraphic spineGraphic, System.Action onBurstPeak)
    {
        // Intensify existing particles during transformation
        var particleBurst = DOTween.Sequence();
        
        // Increase particle intensity
        particleBurst.AppendCallback(() => {
            // Scale up particle effects if possible
            var particleTransform = _evolutionParticle.transform;
            particleTransform.DOScale(2f, 0.2f);
            
            // Add extra sparkles during transformation
            for (int i = 0; i < 15; i++) // More sparkles
            {
                CreateSingleSparkle(spineGraphic.rectTransform, i * 0.05f); // Faster spawn
            }
        });
        
        // Peak moment - change skeleton
        particleBurst.AppendInterval(0.3f);
        particleBurst.AppendCallback(() => onBurstPeak?.Invoke());
        
        // Scale back down
        particleBurst.AppendCallback(() => {
            _evolutionParticle.transform.DOScale(1f, 0.3f);
        });
    }

    private static void CreateGlowCover(SkeletonGraphic spineGraphic, System.Action onGlowPeak)
    {
        // Create glow overlay with bright energy effect
        var glowGO = new GameObject("GlowCover");
        var glowImage = glowGO.AddComponent<Image>();
        var glowRect = glowGO.GetComponent<RectTransform>();
        var glowCG = glowGO.AddComponent<CanvasGroup>();
        
        // Create circular glow sprite programmatically
        glowImage.sprite = CreateCircularGlowSprite();
        glowImage.color = Color.yellow; // Bright yellow
        glowImage.raycastTarget = false;
        glowCG.alpha = 0f;
        
        // Parent to monster and size appropriately
        glowGO.transform.SetParent(spineGraphic.rectTransform, false);
        glowRect.anchoredPosition = Vector2.zero;
        
        // Make it 8x larger and circular - MASSIVE glow coverage
        var size = Mathf.Max(spineGraphic.rectTransform.sizeDelta.x, spineGraphic.rectTransform.sizeDelta.y) * 50f; // 8x instead of 2x
        glowRect.sizeDelta = new Vector2(size, size); // Square for circular sprite
        
        // Position in front of monster
        glowGO.transform.SetSiblingIndex(999);
        
        // Glow sequence: grow and brighten -> peak -> fade (5x longer durations)
        var glowSeq = DOTween.Sequence();
        
        // Grow and brighten (1.5s - was 0.3s)
        glowSeq.Append(glowCG.DOFade(1f, 1.5f).SetEase(Ease.InBack));
        glowSeq.Join(glowRect.DOScale(1.5f, 1.5f).SetEase(Ease.InBack)); // This will make it even bigger during animation!
        
        // At peak glow, change the skeleton
        glowSeq.AppendCallback(() => onGlowPeak?.Invoke());
        
        // Hold bright glow (0.5s - was 0.1s)
        glowSeq.AppendInterval(0.5f);
        
        // Fade and shrink (2.0s - was 0.4s)
        glowSeq.Append(glowCG.DOFade(0f, 2.0f).SetEase(Ease.OutQuad));
        glowSeq.Join(glowRect.DOScale(0.8f, 2.0f).SetEase(Ease.OutQuad));
        
        // Destroy glow object when done
        glowSeq.OnComplete(() => Object.Destroy(glowGO));
    }

    // Add this new method to create circular glow sprite
    private static Sprite CreateCircularGlowSprite()
    {
        var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        var center = new Vector2(128, 128);
        var radius = 120f;
        
        // Create circular radial gradient
        for (int x = 0; x < 256; x++)
        {
            for (int y = 0; y < 256; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                // Create smooth radial falloff
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                
                // Apply smoothstep for softer edges
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                
                // Apply another smoothstep for even softer falloff
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
    }
}
