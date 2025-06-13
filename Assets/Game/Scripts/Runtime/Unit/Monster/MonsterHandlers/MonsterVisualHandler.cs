using UnityEngine;
using Spine.Unity;
using System.Collections;

public class MonsterVisualHandler
{
    private MonsterController _controller;
    private SkeletonGraphic _skeletonGraphic;

    public MonsterVisualHandler(MonsterController controller, SkeletonGraphic skeletonGraphic)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
    }

    public void SetSpineDataBasedOnEvolution()
    {
        if (_controller.MonsterData == null || _skeletonGraphic == null) return;

        SkeletonDataAsset targetSkeletonData = GetCurrentSkeletonData();

        if (targetSkeletonData != null)
        {
            // Only change if it's actually different
            if (_skeletonGraphic.skeletonDataAsset != targetSkeletonData)
            {
                _skeletonGraphic.skeletonDataAsset = targetSkeletonData;
                _skeletonGraphic.Initialize(true);

                // Wait a frame before setting animation
                _controller.StartCoroutine(SetAnimationAfterFrame());
            }
        }
    }

    public void ApplyMonsterVisuals()
    {
        if (_controller.MonsterData == null || _skeletonGraphic == null) return;

        SetSpineDataBasedOnEvolution();

        // Ensure animation starts after spine data is set
        if (_skeletonGraphic.skeletonDataAsset != null && _skeletonGraphic.AnimationState != null)
        {
            _skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        }
    }

    private IEnumerator SetAnimationAfterFrame()
    {
        yield return null; // Wait one frame for spine to initialize

        if (_skeletonGraphic.AnimationState != null)
        {
            _skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
        }
    }

    public SkeletonDataAsset GetCurrentSkeletonData()
    {
        if (_controller.MonsterData == null ||
            _controller.MonsterData.monsterSpine == null ||
            _controller.MonsterData.monsterSpine.Length == 0)
        {
            Debug.LogWarning($"[Visual] No spine data available for monster {_controller.monsterID}");
            return null;
        }

        // Clamp to valid array bounds
        int arrayIndex = Mathf.Clamp(_controller.evolutionLevel, 0, _controller.MonsterData.monsterSpine.Length - 1);

        // Log if we're clamping (indicates a configuration issue)
        if (arrayIndex != _controller.evolutionLevel)
        {
            Debug.LogWarning($"[Visual] Evolution level {_controller.evolutionLevel} out of bounds for monster {_controller.monsterID}. Using index {arrayIndex} instead.");
        }

        var spineAsset = _controller.MonsterData.monsterSpine[arrayIndex];
        if (spineAsset == null)
        {
            Debug.LogError($"[Visual] Spine asset at index {arrayIndex} is null for monster {_controller.monsterID}");
        }

        return spineAsset;
    }

    public void UpdateMonsterVisuals()
    {
        ApplyMonsterVisuals();
    }

    public Bounds GetSkeletonBounds()
    {
        if (_skeletonGraphic?.Skeleton == null)
            return new Bounds(_controller.transform.position, Vector2.one * 100f);

        var skeleton = _skeletonGraphic.Skeleton;

        // Calculate world bounds of the skeleton
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        // Get bounds from all bones
        foreach (var bone in skeleton.Bones)
        {
            if (bone == null) continue;

            float worldX = bone.WorldX;
            float worldY = bone.WorldY;

            minX = Mathf.Min(minX, worldX);
            maxX = Mathf.Max(maxX, worldX);
            minY = Mathf.Min(minY, worldY);
            maxY = Mathf.Max(maxY, worldY);
        }

        // If no bones found, use attachment bounds
        if (minX == float.MaxValue)
        {
            float[] vertexBuffer = new float[8];
            skeleton.GetBounds(out minX, out minY, out float width, out float height, ref vertexBuffer);
            maxX = minX + width;
            maxY = minY + height;
        }

        // Convert to Unity world space (Spine Y is inverted)
        Vector2 center = new Vector2((minX + maxX) * 0.5f, -(minY + maxY) * 0.5f);
        Vector2 size = new Vector2(maxX - minX, maxY - minY);

        // Apply skeleton scale
        float scale = _skeletonGraphic.transform.localScale.x;
        center *= scale;
        size *= scale;
        
        // Add to monster's world position
        RectTransform rectTransform = _controller.GetComponent<RectTransform>();
        center += rectTransform.anchoredPosition;

        return new Bounds(center, size);
    }

    public Vector2 GetBackPosition()
    {
        var bounds = GetSkeletonBounds();

        // CHANGED: Spawn from bottom center of monster for downward slide
        Vector2 bottomPosition = new Vector2(
            bounds.center.x, // Use center X instead of back
            bounds.min.y     // Use bottom edge directly
        );

        return bottomPosition;
    }

    public Vector2 GetRandomPositionOutsideBounds()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null && gameManager.gameArea != null)
        {
            var gameAreaRect = gameManager.gameArea;
            Vector2 gameAreaSize = gameAreaRect.sizeDelta;

            // Calculate spawn area (outside monster but inside game area)
            float spawnRadius = 100f;
            float padding = 50f;
            
            Vector2 monsterPos = _controller.GetComponent<RectTransform>().anchoredPosition;
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            Vector2 baseDropPosition = monsterPos + randomDirection * spawnRadius;

            // Clamp to game area
            baseDropPosition.x = Mathf.Clamp(baseDropPosition.x, 
                -gameAreaSize.x / 2 + padding, 
                gameAreaSize.x / 2 - padding);
            baseDropPosition.y = Mathf.Clamp(baseDropPosition.y, 
                -gameAreaSize.y / 2 + padding, 
                gameAreaSize.y / 2 - padding);

            // NEW: Find safe position that avoids ALL objects (coins AND poop)
            return FindSafeSpawnPosition(baseDropPosition);
        }

        return Vector2.zero;
    }

    // Add method to get the starting position for coin arc
    public Vector2 GetCoinLaunchPosition()
    {
        var bounds = GetSkeletonBounds();

        // Launch from the center-top of the monster
        return new Vector2(
            bounds.center.x,
            bounds.max.y + 10f // Slightly above the monster
        );
    }

    // NEW: Get safe poop spawn position that considers bounds
    public Vector2 GetSafePoopSpawnPosition()
    {
        Vector2 backPosition = GetBackPosition();

        // Use the existing bounds handler from the controller
        var boundsHandler = _controller.GetBoundsHandler();
        if (boundsHandler == null)
        {
            // Fallback to monster position if no bounds handler
            return _controller.GetComponent<RectTransform>().anchoredPosition;
        }
        
        var groundBounds = boundsHandler.CalculateGroundBounds();

        // Keep poop spawn within ground bounds with extra padding
        float padding = 80f; // Increased padding
        
        // Clamp more strictly
        backPosition.x = Mathf.Clamp(backPosition.x,
            groundBounds.min.x + padding,
            groundBounds.max.x - padding);
        backPosition.y = Mathf.Clamp(backPosition.y, // Changed from Max to Clamp
            groundBounds.min.y + padding,
            groundBounds.max.y - padding);

        return backPosition;
    }

    // NEW: Handle poop spawn with animation
    public void SpawnPoopWithAnimation(PoopType type)
    {
        Vector2 spawnPosition = GetSafePoopSpawnPosition();
        
        // NEW: Find safe position that doesn't overlap with coins
        spawnPosition = FindSafeSpawnPosition(spawnPosition);
        
        // Spawn poop at safe position
        var poopGameObject = ServiceLocator.Get<GameManager>().SpawnPoopAt(spawnPosition, type);
        
        if (poopGameObject != null)
        {
            _controller.StartCoroutine(AnimatePoopSlide(poopGameObject, spawnPosition));
        }
    }
    
    private IEnumerator AnimatePoopSlide(GameObject poopObject, Vector2 startPosition)
    {
        var poopRect = poopObject.GetComponent<RectTransform>();
        if (poopRect == null) yield break;

        // Animation settings - gravity-like fall
        float slideDuration = 1.2f; // Longer for realistic fall
        AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Accelerating downward

        // Calculate downward slide direction
        Vector2 slideDirection = CalculateSafeSlideDirection(startPosition);
        Vector2 targetPosition = startPosition + slideDirection;
        
        // Set initial position
        poopRect.anchoredPosition = startPosition;
        
        // Animate sliding down with gravity feel
        float elapsedTime = 0f;
        while (elapsedTime < slideDuration && poopObject != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideDuration;
            float curveValue = slideCurve.Evaluate(progress);
            
            Vector2 currentPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);
            poopRect.anchoredPosition = currentPosition;
            
            yield return null;
        }
        
        // Ensure final position
        if (poopObject != null && poopRect != null)
        {
            poopRect.anchoredPosition = targetPosition;
        }
    }

    private Vector2 CalculateSafeSlideDirection(Vector2 startPosition)
    {
        // Use the existing bounds handler from the controller
        var boundsHandler = _controller.GetBoundsHandler();
        if (boundsHandler == null)
        {
            // Fallback to slide down
            return Vector2.down * 50f;
        }
        
        var groundBounds = boundsHandler.CalculateGroundBounds();
        
        float slideDistance = 50f; // Increased for more noticeable downward slide
        float padding = 50f;
        
        // SIMPLIFIED: Only try downward directions
        Vector2[] possibleDirections = {
            Vector2.down,                    // Straight down (preferred)
            new Vector2(-0.3f, -0.9f),      // Slight left-down
            new Vector2(0.3f, -0.9f),       // Slight right-down
        };
        
        foreach (Vector2 direction in possibleDirections)
        {
            Vector2 testPosition = startPosition + direction.normalized * slideDistance;
            
            // Check if position is within bounds
            if (testPosition.x >= groundBounds.min.x + padding && 
                testPosition.x <= groundBounds.max.x - padding && 
                testPosition.y >= groundBounds.min.y + padding)
            {
                return direction.normalized * slideDistance;
            }
        }
        
        // Safe fallback: calculate maximum downward distance
        float maxDownwardDistance = startPosition.y - (groundBounds.min.y + padding);
        maxDownwardDistance = Mathf.Max(20f, Mathf.Min(slideDistance, maxDownwardDistance));
        
        return Vector2.down * maxDownwardDistance;
    }

    // NEW: Find safe spawn position that doesn't overlap
    private Vector2 FindSafeSpawnPosition(Vector2 preferredPosition, int maxAttempts = 5)
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager == null) return preferredPosition;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 testPosition = preferredPosition;
            if (i > 0)
            {
                // Add random offset for subsequent attempts
                testPosition += UnityEngine.Random.insideUnitCircle * 50f;
            }
            
            // Use GameManager's efficient overlap checking
            if (gameManager.IsPositionClearOfObjects(testPosition))
            {
                return testPosition;
            }
        }
        
        // Return original position if no safe position found
        return preferredPosition;
    }
}
