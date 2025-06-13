using UnityEngine;
using Spine.Unity;

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
    
    private System.Collections.IEnumerator SetAnimationAfterFrame()
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
        
        // Get the back position (leftmost point of the skeleton)
        Vector2 backPosition = new Vector2(
            bounds.min.x - 20f, // 20 units behind the back edge
            bounds.center.y - bounds.size.y * 0.3f // Slightly below center (ground level)
        );
        
        return backPosition;
    }
    
    public Vector2 GetRandomPositionOutsideBounds()
    {
        var bounds = GetSkeletonBounds();
        
        // Get a safe position near the bottom of the monster with some offset
        Vector2 dropPosition = new Vector2(
            bounds.center.x + UnityEngine.Random.Range(-bounds.size.x * 0.3f, bounds.size.x * 0.3f), // Slight horizontal variation
            bounds.min.y - 20f // Just below the monster's bottom edge
        );
        
        // Ensure the drop position stays within game area bounds
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null && gameManager.gameArea != null)
        {
            var gameAreaRect = gameManager.gameArea;
            Vector2 gameAreaSize = gameAreaRect.sizeDelta;
            
            // Clamp to game area with padding
            float padding = 30f;
            dropPosition.x = Mathf.Clamp(dropPosition.x, 
                -gameAreaSize.x / 2 + padding, 
                gameAreaSize.x / 2 - padding);
            dropPosition.y = Mathf.Max(dropPosition.y, 
                -gameAreaSize.y / 2 + padding);
        }
        
        return dropPosition;
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
}
