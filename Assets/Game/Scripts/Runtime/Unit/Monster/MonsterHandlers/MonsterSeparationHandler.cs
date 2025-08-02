using UnityEngine;
using System.Collections.Generic;

public class MonsterSeparationHandler
{
    private MonsterController _controller;
    private RectTransform _rectTransform;
    
    [Header("Separation Settings")]
    public float separationRadius = 100f;  // Increased radius
    public float separationForce = 100f;   // Much stronger force
    public float maxSeparationSpeed = 100f; // Limit separation movement speed
    public LayerMask monsterLayer = -1;
    
    private Vector2 _lastSeparationDirection = Vector2.zero;
    private const float DIRECTION_SMOOTHING = 0.3f;
    
    public MonsterSeparationHandler(MonsterController controller, RectTransform rectTransform)
    {
        _controller = controller;
        _rectTransform = rectTransform;
    }
    
    public Vector2 CalculateSeparationForce()
    {
        Vector2 separationVector = Vector2.zero;
        int count = 0;
        Vector2 currentPosition = _rectTransform.anchoredPosition;
        
        foreach (var otherMonster in _controller.MonsterManager.activeMonsters)
        {
            if (otherMonster == _controller || otherMonster == null) continue;
            
            var otherTransform = otherMonster.GetComponent<RectTransform>();
            if (otherTransform == null) continue;
            
            Vector2 otherPosition = otherTransform.anchoredPosition;
            float distance = Vector2.Distance(currentPosition, otherPosition);
            
            if (distance > 0 && distance < separationRadius)
            {
                Vector2 diff = currentPosition - otherPosition;
                diff.Normalize();
                
                // Stronger repulsion for closer monsters
                float strength = (separationRadius - distance) / separationRadius;
                diff *= strength * separationForce;
                
                separationVector += diff;
                count++;
            }
        }
        
        if (count > 0)
        {
            separationVector /= count;
            
            // Don't completely disable separation in small areas
            var gameAreaSize = _controller.MonsterManager.gameAreaRT.sizeDelta;
            float areaFactor = Mathf.Clamp01(gameAreaSize.x * gameAreaSize.y / (500f * 500f));
            areaFactor = Mathf.Max(areaFactor, 0.3f); // Minimum 30% separation force
            separationVector *= areaFactor;
            
            return separationVector;
        }
        
        return Vector2.zero;
    }
    
    public Vector2 ApplySeparationToTarget(Vector2 originalTarget)
    {
        Vector2 separation = CalculateSeparationForce();
        
        // Smooth direction changes to prevent rapid flipping
        if (separation != Vector2.zero)
        {
            separation = Vector2.Lerp(_lastSeparationDirection, separation, DIRECTION_SMOOTHING);
            _lastSeparationDirection = separation;
        }
        
        // When height is constrained, INCREASE horizontal separation instead
        var gameAreaSize = _controller.MonsterManager.gameAreaRT.sizeDelta;
        if (gameAreaSize.y < 300f) {
            // Boost horizontal movement when vertical space is limited
            float boostFactor = Mathf.Lerp(2.0f, 1.0f, gameAreaSize.y / 300f);
            separation.x *= boostFactor;
        }
        
        return originalTarget + separation;
    }
}