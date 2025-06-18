using UnityEngine;

public class MonsterBoundsHandler
{
    private RectTransform _rectTransform;
    private GameManager _gameManager;
    private const float PADDING = 50f;
    private const float GROUND_OFFSET = 10f; // Extra offset from detected ground edge
    
    public MonsterBoundsHandler(RectTransform rectTransform, GameManager gameManager)
    {
        _rectTransform = rectTransform;
        _gameManager = gameManager;
    }
    
    public Vector2 GetRandomTarget()
    {
        // Default to ground target
        return GetGroundTarget();
    }
    
    public Vector2 GetRandomTargetForState(MonsterState state)
    {
        // NEW: For very small areas, use more generous targeting
        if (IsMovementAreaTooSmall())
        {
            return GetRelaxedTarget();
        }
        
        return state switch
        {
            MonsterState.Flying => GetFlyingTarget(),
            MonsterState.Flapping => GetFlyingTarget(), // Keep flapping in air
            // Check if monster is currently in air before deciding target
            MonsterState.Idle => IsCurrentlyFlying() ? GetAirIdleTarget() : GetGroundTarget(),
            MonsterState.Jumping => IsCurrentlyFlying() ? GetAirIdleTarget() : GetGroundTarget(),
            MonsterState.Itching => IsCurrentlyFlying() ? GetAirIdleTarget() : GetGroundTarget(),
            _ => GetGroundTarget() // Walking, Running, Eating stay on ground
        };
    }
    
    private Vector2 GetGroundTarget()
    {
        var bounds = CalculateGroundBounds();
        
        return new Vector2(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
        );
    }
    
    private Vector2 GetFlyingTarget()
    {
        var bounds = CalculateFlyingBounds();
        
        return new Vector2(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
        );
    }
    
    public (Vector2 min, Vector2 max) CalculateMovementBounds()
    {
        // Default to ground bounds
        return CalculateGroundBounds();
    }
    
    public (Vector2 min, Vector2 max) CalculateBoundsForState(MonsterState state)
    {
        return state switch
        {
            MonsterState.Flying => CalculateFlyingBounds(),
            // Keep flying monsters in air for non-movement states
            MonsterState.Idle => CalculateGroundBounds(),
            MonsterState.Jumping => CalculateGroundBounds(),
            MonsterState.Itching => CalculateGroundBounds(),
            _ => CalculateGroundBounds()
        };
    }
    
    // public (Vector2 min, Vector2 max) CalculateGroundBounds()
    // {
    //     var gameAreaRect = _gameManager.gameArea;
    //     Vector2 size = gameAreaRect.sizeDelta;
        
    //     float halfWidth = _rectTransform.rect.width / 2;
    //     float halfHeight = _rectTransform.rect.height / 2;
        
    //     // If game area is too small, return centered bounds
    //     if (IsGameAreaTooSmallForMonster())
    //     {
    //         return GetCenteredBounds();
    //     }
        
    //     // Calculate normal bounds
    //     Vector2 boundsMin = new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING);
    //     Vector2 boundsMax = new Vector2(size.x / 2 - halfWidth - PADDING, -size.y / 2 + (size.y * 0.4f) - halfHeight);
        
    //     // Ensure bounds are valid (min <= max)
    //     if (boundsMin.x > boundsMax.x || boundsMin.y > boundsMax.y)
    //     {
    //         return GetCenteredBounds();
    //     }
        
    //     // Try to ensure minimum movement area
    //     const float MIN_MOVEMENT_AREA = 100f;
        
    //     if (boundsMax.y - boundsMin.y < MIN_MOVEMENT_AREA)
    //     {
    //         float center = (boundsMin.y + boundsMax.y) / 2;
    //         boundsMin.y = center - MIN_MOVEMENT_AREA / 2;
    //         boundsMax.y = center + MIN_MOVEMENT_AREA / 2;
            
    //         // Clamp to game area bounds
    //         boundsMin.y = Mathf.Max(boundsMin.y, -size.y / 2 + halfHeight + PADDING);
    //         boundsMax.y = Mathf.Min(boundsMax.y, size.y / 2 - halfHeight - PADDING);
            
    //         // Final validation - if still invalid, use centered bounds
    //         if (boundsMin.y > boundsMax.y)
    //         {
    //             return GetCenteredBounds();
    //         }
    //     }
        
    //     return (boundsMin, boundsMax);
    // }

    private (Vector2 min, Vector2 max) CalculateFlyingBounds()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Flying can use the full game area
        return (
            new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING),
            new Vector2(size.x / 2 - halfWidth - PADDING, size.y / 2 - halfHeight - PADDING)
        );
    }
    
    public bool IsWithinBoundsForState(Vector2 position, MonsterState state)
    {
        var bounds = CalculateBoundsForState(state);
        return position.x >= bounds.min.x && position.x <= bounds.max.x &&
               position.y >= bounds.min.y && position.y <= bounds.max.y;
    }
    
    // Add new method to check if monster is currently in flying area
    private bool IsCurrentlyFlying()
    {
        Vector2 currentPos = _rectTransform.anchoredPosition;
        var groundBounds = CalculateGroundBounds();
        
        // If monster is above ground area, consider it flying
        return currentPos.y > groundBounds.max.y + 50f; // 50f buffer zone
    }
    
    // Add new method for air idle targets
    private Vector2 GetAirIdleTarget()
    {
        var flyingBounds = CalculateFlyingBounds();
        var groundBounds = CalculateGroundBounds();
        
        // Stay in the upper portion of flying area (above ground)
        float minY = groundBounds.max.y + 100f; // Stay well above ground
        float maxY = flyingBounds.max.y;
        
        return new Vector2(
            UnityEngine.Random.Range(flyingBounds.min.x, flyingBounds.max.x),
            UnityEngine.Random.Range(minY, maxY)
        );
    }

    // NEW: Check if game area is physically too small for the monster
    public bool IsGameAreaTooSmallForMonster()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 gameAreaSize = gameAreaRect.sizeDelta;
        
        float monsterWidth = _rectTransform.rect.width;
        float monsterHeight = _rectTransform.rect.height;
        
        // Check if game area is smaller than monster + minimum padding
        float minRequiredWidth = monsterWidth + (PADDING * 2);
        float minRequiredHeight = monsterHeight + (PADDING * 2);
        
        return gameAreaSize.x < minRequiredWidth || gameAreaSize.y < minRequiredHeight;
    }

    // UPDATED: Better area checking that considers monster size
    public bool IsMovementAreaTooSmall()
    {
        // First check if game area is physically too small
        if (IsGameAreaTooSmallForMonster())
            return true;
        
        var bounds = CalculateGroundBounds();
        const float MIN_MOVEMENT_AREA = 150f;
        
        float width = bounds.max.x - bounds.min.x;
        float height = bounds.max.y - bounds.min.y;
        
        // Also check for invalid bounds (when min > max)
        bool invalidBounds = width <= 0 || height <= 0;
        
        return invalidBounds || width < MIN_MOVEMENT_AREA || height < MIN_MOVEMENT_AREA;
    }

    // UPDATED: Safe bounds calculation that handles edge cases
    public (Vector2 min, Vector2 max) CalculateGroundBounds()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Calculate X bounds (always use full width)
        Vector2 boundsMin = new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING);
        Vector2 boundsMax = new Vector2(size.x / 2 - halfWidth - PADDING, -size.y / 2 + (size.y * 0.4f) - halfHeight);
        
        // NEW: Handle small Y area without affecting X bounds
        if (boundsMax.y <= boundsMin.y) // Invalid Y bounds
        {
            // Keep full X movement, but create minimal Y movement
            float centerY = -size.y / 2 + size.y / 2; // Center of game area
            float minYMovement = 20f; // Minimal Y movement area
            
            boundsMin.y = centerY - minYMovement / 2;
            boundsMax.y = centerY + minYMovement / 2;
            
            // Clamp Y to game area bounds
            boundsMin.y = Mathf.Max(boundsMin.y, -size.y / 2 + halfHeight + PADDING);
            boundsMax.y = Mathf.Min(boundsMax.y, size.y / 2 - halfHeight - PADDING);
            
            // If Y is still invalid, center it but keep X bounds
            if (boundsMax.y <= boundsMin.y)
            {
                float safeY = centerY;
                boundsMin.y = safeY - 5f;  // Tiny Y movement
                boundsMax.y = safeY + 5f;
            }
        }
        
        // Final validation for X bounds
        if (boundsMax.x <= boundsMin.x)
        {
            // Only if X is also impossible, then use centered bounds
            return GetCenteredBounds();
        }
        
        return (boundsMin, boundsMax);
    }

    // UPDATED: Better centered bounds that preserves X when possible
    private (Vector2 min, Vector2 max) GetCenteredBounds()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Check if we can preserve X movement
        float availableWidth = size.x - (halfWidth * 2) - (PADDING * 2);
        float availableHeight = size.y - (halfHeight * 2) - (PADDING * 2);
        
        if (availableWidth > 50f) // If we have reasonable X space
        {
            // Preserve X movement, minimize Y movement
            return (
                new Vector2(-size.x / 2 + halfWidth + PADDING, -10f), // Full X, tiny Y
                new Vector2(size.x / 2 - halfWidth - PADDING, 10f)
            );
        }
        else
        {
            // Both dimensions are too small
            float maxRadius = Mathf.Min(size.x, size.y) * 0.3f;
            return (
                new Vector2(-maxRadius, -maxRadius),
                new Vector2(maxRadius, maxRadius)
            );
        }
    }

    // UPDATED: Better relaxed targeting that preserves X movement
    private Vector2 GetRelaxedTarget()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;
        
        // Always try to use full width if possible
        float padding = 20f;
        float halfWidth = _rectTransform.rect.width / 2;
        
        float availableWidth = size.x - (halfWidth * 2) - (padding * 2);
        
        if (availableWidth > 50f) // Can use full width
        {
            return new Vector2(
                UnityEngine.Random.Range(-size.x / 2 + halfWidth + padding, size.x / 2 - halfWidth - padding),
                UnityEngine.Random.Range(-15f, 15f) // Small Y jitter
            );
        }
        else
        {
            // Both dimensions constrained
            float jitterRange = 10f;
            return new Vector2(
                UnityEngine.Random.Range(-jitterRange, jitterRange),
                UnityEngine.Random.Range(-jitterRange, jitterRange)
            );
        }
    }
}