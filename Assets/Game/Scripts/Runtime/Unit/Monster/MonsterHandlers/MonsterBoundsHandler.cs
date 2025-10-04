using UnityEngine;

public class MonsterBoundsHandler
{
    private RectTransform _rectTransform;
    private MonsterManager _manager;
    private const float PADDING = 10f;
    private const float FLYING_BUFFER_ZONE = 25f;
    private const float AIR_IDLE_HEIGHT_BUFFER = 50f;
    private const float MIN_MOVEMENT_AREA = 150f;
    private const float GROUND_AREA_HEIGHT_RATIO = 0.4f;
    private const float MINIMAL_Y_MOVEMENT = 20f;
    private const float TINY_Y_MOVEMENT = 5f;
    private const float MINIMAL_X_SPACE = 50f;
    private const float JITTER_RANGE = 10f;
    private const float BOUNDS_SCALE_FACTOR = 0.3f;
    private const float CONSTRAINED_PADDING = 20f;
    private const float SMALL_Y_JITTER = 15f;

    public MonsterBoundsHandler(MonsterManager manager, RectTransform rectTransform)
    {
        _manager = manager;
        _rectTransform = rectTransform;
    }
    
    public Vector2 GetRandomSpawnTarget()
    {
        // Get ground bounds and spawn at the center Y position of ground area
        var bounds = CalculateGroundBounds();
        float centerY = (bounds.min.y + bounds.max.y) / 2f;

        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            centerY
        );
    }
    
    public Vector2 GetRandomTargetForState(MonsterState state)
    {
        if (IsMovementAreaTooSmall())
        {
            return GetConstrainedSpaceTarget();
        }
        
        return state switch
        {
            MonsterState.Flying => GetFlyingTarget(),
            _ => GetGroundTarget() 
        };
    }
    
    private Vector2 GetRandomPositionInBounds((Vector2 min, Vector2 max) bounds)
    {
        // Only move horizontally - use center Y position of bounds
        float centerY = (bounds.min.y + bounds.max.y) / 2f;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            centerY
        );
    }

    private Vector2 GetGroundTarget()
    {
        return GetRandomPositionInBounds(CalculateGroundBounds());
    }

    private Vector2 GetFlyingTarget()
    {
        var bounds = CalculateFlyingBounds();

        // Only move horizontally - use center Y position of bounds
        float centerY = (bounds.min.y + bounds.max.y) / 2f;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            centerY
        );
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
    
    private Vector2 GetConstrainedSpaceTarget()
    {
        var gameAreaRect = _manager.gameAreaRT;
        Vector2 size = gameAreaRect.sizeDelta;

        float halfWidth = _rectTransform.rect.width / 2;
        float availableWidth = size.x - (halfWidth * 2) - (CONSTRAINED_PADDING * 2);

        // Calculate center Y from ground bounds for consistent horizontal movement
        var bounds = CalculateGroundBounds();
        float centerY = (bounds.min.y + bounds.max.y) / 2f;

        // Only move horizontally - use center Y position
        if (availableWidth > MINIMAL_X_SPACE)
        {
            return new Vector2(
                UnityEngine.Random.Range(-size.x / 2 + halfWidth + CONSTRAINED_PADDING,
                                        size.x / 2 - halfWidth - CONSTRAINED_PADDING),
                centerY
            );
        }
        else
        {
            return new Vector2(
                UnityEngine.Random.Range(-JITTER_RANGE, JITTER_RANGE),
                centerY
            );
        }
    }
    
    private (Vector2 min, Vector2 max) CalculateFlyingBounds()
    {
        var gameAreaRect = _manager.gameAreaRT;
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
    
    private bool IsPositionedInAir()
    {
        Vector2 currentPos = _rectTransform.anchoredPosition;
        var groundBounds = CalculateGroundBounds();
        
        // If monster is above ground area, consider it flying
        return currentPos.y > groundBounds.max.y + FLYING_BUFFER_ZONE; 
    }
    
    private Vector2 GetAirIdleTarget()
    {
        var flyingBounds = CalculateFlyingBounds();

        // Only move horizontally - use center Y position of flying bounds
        float centerY = (flyingBounds.min.y + flyingBounds.max.y) / 2f;
        return new Vector2(
            UnityEngine.Random.Range(flyingBounds.min.x, flyingBounds.max.x),
            centerY
        );
    }

    // Check if game area is physically too small for the monster
    public bool IsGameAreaTooSmallForMonster()
    {
        var gameAreaRect = _manager.gameAreaRT;
        Vector2 gameAreaSize = gameAreaRect.sizeDelta;
        
        float monsterWidth = _rectTransform.rect.width;
        float monsterHeight = _rectTransform.rect.height;
        
        // Check if game area is smaller than monster + minimum padding
        float minRequiredWidth = monsterWidth + (PADDING * 2);
        float minRequiredHeight = monsterHeight + (PADDING * 2);
        
        return gameAreaSize.x < minRequiredWidth || gameAreaSize.y < minRequiredHeight;
    }

    // Better area checking that considers monster size
    public bool IsMovementAreaTooSmall()
    {
        // First check if game area is physically too small
        if (IsGameAreaTooSmallForMonster())
            return true;
        
        var bounds = CalculateGroundBounds();
        
        float width = bounds.max.x - bounds.min.x;
        float height = bounds.max.y - bounds.min.y;
        
        // Also check for invalid bounds (when min > max)
        bool invalidBounds = width <= 0 || height <= 0;
        
        return invalidBounds || width < MIN_MOVEMENT_AREA || height < MIN_MOVEMENT_AREA;
    }

    // Safe bounds calculation that handles edge cases
    public (Vector2 min, Vector2 max) CalculateGroundBounds()
    {
        var gameAreaRect = _manager.gameAreaRT;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Calculate X bounds (always use full width)
        Vector2 boundsMin = new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING);
        Vector2 boundsMax = new Vector2(size.x / 2 - halfWidth - PADDING, -size.y / 2 + (size.y * GROUND_AREA_HEIGHT_RATIO) - halfHeight);
        
        // Handle small Y area without affecting X bounds
        if (boundsMax.y <= boundsMin.y) // Invalid Y bounds
        {
            // Keep full X movement, but create minimal Y movement
            float centerY = -size.y / 2 + size.y / 2; // Center of game area
            float minYMovement = MINIMAL_Y_MOVEMENT; // Minimal Y movement area
            
            boundsMin.y = centerY - minYMovement / 2;
            boundsMax.y = centerY + minYMovement / 2;
            
            // Clamp Y to game area bounds
            boundsMin.y = Mathf.Max(boundsMin.y, -size.y / 2 + halfHeight + PADDING);
            boundsMax.y = Mathf.Min(boundsMax.y, size.y / 2 - halfHeight - PADDING);
            
            // If Y is still invalid, center it but keep X bounds
            if (boundsMax.y <= boundsMin.y)
            {
                float safeY = centerY;
                boundsMin.y = safeY - TINY_Y_MOVEMENT;
                boundsMax.y = safeY + TINY_Y_MOVEMENT;
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

    // Better centered bounds that preserves X when possible
    private (Vector2 min, Vector2 max) GetCenteredBounds()
    {
        var gameAreaRect = _manager.gameAreaRT;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Check if we can preserve X movement
        float availableWidth = size.x - (halfWidth * 2) - (PADDING * 2);
        float availableHeight = size.y - (halfHeight * 2) - (PADDING * 2);
        
        if (availableWidth > MINIMAL_X_SPACE) // If we have reasonable X space
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
            float maxRadius = Mathf.Min(size.x, size.y) * BOUNDS_SCALE_FACTOR;
            return (
                new Vector2(-maxRadius, -maxRadius),
                new Vector2(maxRadius, maxRadius)
            );
        }
    }
}