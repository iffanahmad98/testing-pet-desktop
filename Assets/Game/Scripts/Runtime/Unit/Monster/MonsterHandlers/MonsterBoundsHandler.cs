using UnityEngine;

public class MonsterBoundsHandler
{
    private RectTransform _rectTransform;
    private GameManager _gameManager;
    private const float PADDING = 50f; // Match SettingsManager padding
    
    public MonsterBoundsHandler(RectTransform rectTransform, GameManager gameManager)
    {
        _rectTransform = rectTransform;
        _gameManager = gameManager;
    }
    
    public Vector2 GetRandomTarget()
    {
        var bounds = CalculateMovementBounds();
        return new Vector2(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
        );
    }
    
    public (Vector2 min, Vector2 max) CalculateMovementBounds()
    {
        var gameAreaRect = _gameManager.gameArea;
        Vector2 size = gameAreaRect.sizeDelta;

        float halfWidth = _rectTransform.rect.width / 2;
        float halfHeight = _rectTransform.rect.height / 2;

        return (
            new Vector2(-size.x / 2 + halfWidth + PADDING, -size.y / 2 + halfHeight + PADDING),
            new Vector2(size.x / 2 - halfWidth - PADDING, size.y / 2 - halfHeight - PADDING)
        );
    }
    
    // Add method to check if position is within bounds
    public bool IsWithinBounds(Vector2 position)
    {
        var bounds = CalculateMovementBounds();
        return position.x >= bounds.min.x && position.x <= bounds.max.x &&
               position.y >= bounds.min.y && position.y <= bounds.max.y;
    }
}