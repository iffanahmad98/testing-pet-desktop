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
    
    public (Vector2 min, Vector2 max) CalculateGroundBounds()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;
        
        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;
        
        // Fallback to bottom portion of game area if image analysis fails
        float groundHeight = size.y * 0.4f;
        
        return (
            new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING),
            new Vector2(size.x / 2 - halfWidth - PADDING, -size.y / 2 + groundHeight - halfHeight)
        );
    }

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
}