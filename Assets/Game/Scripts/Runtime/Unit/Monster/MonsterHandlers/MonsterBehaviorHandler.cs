using UnityEngine;
using System.Collections.Generic;

public class MonsterBehaviorHandler
{
    private MonsterController _controller;
    private List<StateTransition> _transitions = new List<StateTransition>();
    private MonsterBehaviorConfigSO _cachedBehaviorConfig;
    private int _lastEvolutionLevel = -1;
    
    public MonsterBehaviorHandler(MonsterController controller)
    {
        _controller = controller;
    }
    
    public MonsterState SelectNextState(MonsterState currentState)
    {
        // Fast path for eating state
        if (currentState == MonsterState.Eating)
        {
            return Random.value < 0.5f ? MonsterState.Idle : MonsterState.Walking;
        }

        var possibleTransitions = GetValidTransitions(currentState);
        
        if (possibleTransitions.Count == 0) 
        {
            return GetSimpleDefaultNextState(currentState);
        }

        // Optimize weighted random selection
        float totalWeight = 0f;
        for (int i = 0; i < possibleTransitions.Count; i++)
        {
            totalWeight += possibleTransitions[i].probability;
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < possibleTransitions.Count; i++)
        {
            currentWeight += possibleTransitions[i].probability;
            if (randomValue <= currentWeight)
            {
                return possibleTransitions[i].toState;
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
        
        MonsterBehaviorConfigSO currentBehaviorConfig = GetCurrentBehaviorConfig();
        if (currentBehaviorConfig == null || currentBehaviorConfig.transitions == null)
            return _transitions;

        // Consider getting hunger and happiness once to avoid property access in loop
        float currentHunger = _controller.currentHunger;
        float currentHappiness = _controller.currentHappiness;
        bool hasFoodNearby = _controller.nearestFood != null;

        for (int i = 0; i < currentBehaviorConfig.transitions.Length; i++)
        {
            var transition = currentBehaviorConfig.transitions[i];
            if (transition.fromState != currentState) continue;
            if (transition.requiresFood && !hasFoodNearby) continue;
            if (currentHunger < transition.hungerThreshold) continue;
            if (currentHappiness < transition.happinessThreshold) continue;

            _transitions.Add(transition);
        }

        return _transitions;
    }
    
    private MonsterBehaviorConfigSO GetCurrentBehaviorConfig()
    {
        // Cache behavior config based on evolution level
        int currentEvolutionLevel = _controller.evolutionLevel;
        
        if (_cachedBehaviorConfig != null && _lastEvolutionLevel == currentEvolutionLevel)
        {
            return _cachedBehaviorConfig;
        }
        
        _lastEvolutionLevel = currentEvolutionLevel;
        _cachedBehaviorConfig = null;
        
        if (_controller?.MonsterData?.evolutionBehaviors != null)
        {
            var evolutionBehavior = System.Array.Find(_controller.MonsterData.evolutionBehaviors,
                config => config.evolutionLevel == currentEvolutionLevel);
                
            if (evolutionBehavior?.behaviorConfig != null)
            {
                _cachedBehaviorConfig = evolutionBehavior.behaviorConfig;
            }
        }
        
        return _cachedBehaviorConfig;
    }
    
    public float GetStateDuration(MonsterState state, MonsterAnimationHandler animationHandler)
    {
        // Get base animation duration for potential cycle calculations
        float baseAnimDuration = TryGetSpineDuration(state, animationHandler);
        bool useExactDuration = baseAnimDuration > 0;
        
        // Cache behavior config to avoid multiple calls
        MonsterBehaviorConfigSO behaviorConfig = GetCurrentBehaviorConfig();
        
        // Fast branching based on state type
        switch (state)
        {
            // GROUP 1: Non-movement states - use exact animation duration
            case MonsterState.Idle:
                return useExactDuration ? baseAnimDuration : 
                    GetRandomDuration(behaviorConfig?.minIdleDuration, behaviorConfig?.maxIdleDuration, 2f, 4f);
                    
            case MonsterState.Jumping:
                return useExactDuration ? baseAnimDuration : 
                    (behaviorConfig?.jumpDuration > 0 ? behaviorConfig.jumpDuration : 1f);
                    
            case MonsterState.Itching:
                return useExactDuration ? baseAnimDuration : 
                    GetRandomDuration(behaviorConfig?.minIdleDuration, behaviorConfig?.maxIdleDuration, 2f, 4f);
                    
            case MonsterState.Flapping:
                return useExactDuration ? baseAnimDuration : 
                    (behaviorConfig?.jumpDuration > 0 ? behaviorConfig.jumpDuration : 1.5f);
                    
            case MonsterState.Eating:
                return useExactDuration ? baseAnimDuration : 2f;
                
            // GROUP 2: Movement states - use random durations rounded to animation cycles
            case MonsterState.Walking:
            case MonsterState.Running:
            case MonsterState.Flying:
                float randomDuration;
                
                if (state == MonsterState.Walking)
                    randomDuration = GetRandomDuration(behaviorConfig?.minWalkDuration, behaviorConfig?.maxWalkDuration, 3f, 5f);
                else if (state == MonsterState.Running)
                    randomDuration = GetRandomDuration(behaviorConfig?.minRunDuration, behaviorConfig?.maxRunDuration, 3f, 5f);
                else // Flying
                    randomDuration = GetRandomDuration(behaviorConfig?.minFlyDuration, behaviorConfig?.maxFlyDuration, 3f, 5f);
                    
                // Round to complete animation cycles if we have a valid base duration
                if (baseAnimDuration >= 0.5f)
                {
                    int cycles = Mathf.Max(1, Mathf.RoundToInt(randomDuration / baseAnimDuration));
                    return cycles * baseAnimDuration;
                }
                
                return randomDuration;
                
            default:
                return 3f; // Default fallback
        }
    }

    private float TryGetSpineDuration(MonsterState state, MonsterAnimationHandler animationHandler)
    {
        if (animationHandler == null) return 0f;
        
        // Get the specific animation name for this state
        string animationName = animationHandler.GetAvailableAnimation(state);
        if (string.IsNullOrEmpty(animationName)) return 0f;
        
        // Get the actual duration
        float duration = animationHandler.GetAnimationDuration(animationName);
        
        // Validate the duration is reasonable (between 0.5s and 10s)
        if (duration >= 0.5f && duration <= 10f)
        {
            return duration;
        }
        
        return 0f; // Return 0 to indicate we should use fallbacks
    }

    private float GetRandomDuration(float? configMin, float? configMax, float defaultMin, float defaultMax)
    {
        float min = configMin.GetValueOrDefault(defaultMin);
        float max = configMax.GetValueOrDefault(defaultMax);
        return Random.Range(min, max);
    }

    private bool IsMonsterCurrentlyInAir()
    {
        var rectTransform = _controller?.GetComponent<RectTransform>();
        if (rectTransform == null) return false;
        
        return rectTransform.anchoredPosition.y > -200f;
    }
}