using UnityEngine;
using System.Collections.Generic;

public class MonsterSeparationBehavior
{
    private MonsterController _controller;
    private GameManager _gameManager;
    private RectTransform _rectTransform;
    
    [Header("Separation Settings")]
    public float separationRadius = 150f;  // Increased radius
    public float separationForce = 200f;   // Much stronger force
    public float maxSeparationSpeed = 100f; // Limit separation movement speed
    public LayerMask monsterLayer = -1;
    
    public MonsterSeparationBehavior(MonsterController controller, GameManager gameManager, RectTransform rectTransform)
    {
        _controller = controller;
        _gameManager = gameManager;
        _rectTransform = rectTransform;
    }
    
    public Vector2 CalculateSeparationForce()
    {
        Vector2 separationVector = Vector2.zero;
        int count = 0;
        Vector2 currentPosition = _rectTransform.anchoredPosition;
        
        foreach (var otherMonster in _gameManager.activeMonsters)
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
                
                // FIXED: Stronger repulsion for closer monsters
                float strength = (separationRadius - distance) / separationRadius;
                diff *= strength * separationForce;
                
                separationVector += diff;
                count++;
            }
        }
        
        if (count > 0)
        {
            separationVector /= count;
            // Don't normalize here - we want to preserve the accumulated force
            return separationVector;
        }
        
        return Vector2.zero;
    }
    
    public Vector2 ApplySeparationToTarget(Vector2 originalTarget)
    {
        Vector2 separation = CalculateSeparationForce();
        return originalTarget + separation;
    }
}