using UnityEngine;
using System.Collections.Generic;

public class MonsterBehaviorHandler
{
    private MonsterController _controller;
    private List<StateTransition> _transitions = new List<StateTransition>();
    
    public MonsterBehaviorHandler(MonsterController controller)
    {
        _controller = controller;
    }
    
    public MonsterState SelectNextState(MonsterState currentState)
    {
        if (currentState == MonsterState.Eating)
        {
            return Random.Range(0, 2) == 0 ? MonsterState.Idle : MonsterState.Walking;
        }

        var possibleTransitions = GetValidTransitions(currentState);
        
        // CHANGED: Use simple default behavior when no config exists
        if (possibleTransitions.Count == 0) 
        {
            return GetSimpleDefaultNextState(currentState);
        }

        float totalWeight = 0f;
        foreach (var transition in possibleTransitions)
            totalWeight += transition.probability;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var transition in possibleTransitions)
        {
            currentWeight += transition.probability;
            if (randomValue <= currentWeight)
            {
                return transition.toState;
            }
        }
        
        return GetSimpleDefaultNextState(currentState);    }
    
    // Simple fallback - only Idle and Walking
    private MonsterState GetSimpleDefaultNextState(MonsterState currentState)
    {
        // Check if monster is currently in air
        bool isInAir = IsMonsterCurrentlyInAir();
        
        return currentState switch
        {
            MonsterState.Idle => isInAir ? 
                (Random.Range(0, 3) == 0 ? MonsterState.Flying : MonsterState.Idle) :
                (Random.Range(0, 2) == 0 ? MonsterState.Walking : MonsterState.Idle),
                
            MonsterState.Walking => Random.Range(0, 2) == 0 ? MonsterState.Idle : MonsterState.Walking,
            
            // Flying states: prefer to stay flying if in air, transition to ground states if on ground
            MonsterState.Flying => isInAir ? 
                (Random.Range(0, 2) == 0 ? MonsterState.Flying : MonsterState.Idle) :
                MonsterState.Walking,
                
            MonsterState.Flapping => isInAir ? MonsterState.Flying : MonsterState.Idle,
            
            // Special states return to appropriate states based on position
            MonsterState.Jumping => isInAir ? MonsterState.Flying : MonsterState.Idle,
            MonsterState.Itching => MonsterState.Idle,
            MonsterState.Running => MonsterState.Walking,
            
            _ => MonsterState.Idle // Ultimate fallback
        };
    }
    
    public List<StateTransition> GetValidTransitions(MonsterState currentState)
    {
        _transitions.Clear();
        
        var currentBehaviorConfig = GetCurrentBehaviorConfig();
        if (currentBehaviorConfig == null || currentBehaviorConfig.transitions == null)
            return _transitions;

        foreach (var transition in currentBehaviorConfig.transitions)
        {
            if (transition.fromState != currentState) continue;
            if (transition.requiresFood && _controller.nearestFood == null) continue;
            if (_controller.currentHunger < transition.hungerThreshold) continue;
            if (_controller.currentHappiness < transition.happinessThreshold) continue;

            _transitions.Add(transition);
        }

        return _transitions;
    }
    
    private MonsterBehaviorConfigSO GetCurrentBehaviorConfig()
    {
        if (_controller?.MonsterData?.evolutionBehaviors != null)
        {
            var evolutionBehavior = System.Array.Find(_controller.MonsterData.evolutionBehaviors,
                config => config.evolutionLevel == _controller.evolutionLevel);
                
            if (evolutionBehavior?.behaviorConfig != null)
            {
                return evolutionBehavior.behaviorConfig;
            }
        }
        
        return null; // No config found - use simple default behavior
    }
    
    public float GetStateDuration(MonsterState state, MonsterAnimationHandler animationHandler)
    {
        // Get the config internally - we know better than the caller
        var behaviorConfig = GetCurrentBehaviorConfig();
        
        return state switch
        {
            // Movement states: Keep random duration 3-5 seconds
            MonsterState.Walking => GetRandomDuration(
                behaviorConfig?.minWalkDuration, behaviorConfig?.maxWalkDuration, 3f, 5f),
            MonsterState.Running => GetRandomDuration(
                behaviorConfig?.minRunDuration, behaviorConfig?.maxRunDuration, 3f, 5f),
            MonsterState.Flying => GetRandomDuration(
                behaviorConfig?.minFlyDuration, behaviorConfig?.maxFlyDuration, 3f, 5f),
            
            // Non-movement states: Use animation duration from Spine (with safe fallbacks)
            MonsterState.Idle => Mathf.Max(animationHandler?.GetAnimationDuration("idle") ?? 2f, 2f),
            MonsterState.Jumping => Mathf.Max(animationHandler?.GetAnimationDuration(animationHandler.GetAvailableAnimation(MonsterState.Jumping)) ?? 1f, 0.8f), // Min 0.8s
            MonsterState.Itching => Mathf.Max(animationHandler?.GetAnimationDuration(animationHandler.GetAvailableAnimation(MonsterState.Itching)) ?? 1.5f, 1.0f), // Min 1.0s  
            MonsterState.Flapping => Mathf.Max(animationHandler?.GetAnimationDuration(animationHandler.GetAvailableAnimation(MonsterState.Flapping)) ?? 1.5f, 1.0f), // Min 1.0s
            MonsterState.Eating => Mathf.Max(animationHandler?.GetAnimationDuration(animationHandler.GetAvailableAnimation(MonsterState.Eating)) ?? 2f, 2f),
            
            _ => 3f // Default duration
        };
    }

    private float GetRandomDuration(float? configMin, float? configMax, float defaultMin, float defaultMax)
    {
        float min = configMin > 0 ? configMin.Value : defaultMin;
        float max = configMax > 0 ? configMax.Value : defaultMax;
        return Random.Range(min, max);
    }

    // Add helper method
    private bool IsMonsterCurrentlyInAir()
    {
        var rectTransform = _controller?.GetComponent<RectTransform>();
        if (rectTransform == null) return false;
        
        Vector2 currentPos = rectTransform.anchoredPosition;
        return currentPos.y > -200f; // Adjust threshold based on your game area
    }
}