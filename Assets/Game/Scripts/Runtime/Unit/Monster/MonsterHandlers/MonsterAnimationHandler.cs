using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using System.Collections;

public class MonsterAnimationHandler
{
    private MonsterController _controller;
    private SkeletonGraphic _skeletonGraphic;
    private static readonly Dictionary<MonsterState, string[]> StateAnimationMap = new()
    {
        [MonsterState.Idle] = new[] { "idle" },
        [MonsterState.Walking] = new[] { "walking" },
        [MonsterState.Running] = new[] { "running" },
        [MonsterState.Flying] = new[] { "flying" },
        [MonsterState.Flapping] = new[] { "flapping" },
        [MonsterState.Jumping] = new[] { "jumping" },
        [MonsterState.Itching] = new[] { "itching" },
        [MonsterState.Eating] = new[] { "eating" }
    };

    public MonsterAnimationHandler(MonsterController controller, SkeletonGraphic skeletonGraphic)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
    }
    
    public void PlayStateAnimation(MonsterState state)
    {
        // Start delayed animation playback to handle initialization timing
        _controller.StartCoroutine(TryPlayStateAnimation(state));
    }

    private IEnumerator TryPlayStateAnimation(MonsterState state)
    {
        // Wait for skeleton initialization - multiple strategies
        int maxRetries = 5;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            // Check if skeleton components are ready
            if (_skeletonGraphic != null && _skeletonGraphic.skeletonDataAsset != null)
            {
                // Try to initialize if AnimationState is null
                if (_skeletonGraphic.AnimationState == null)
                {
                    _skeletonGraphic.Initialize(true);
                }
                
                // If we have AnimationState, we're ready
                if (_skeletonGraphic.AnimationState != null)
                {
                    break;
                }
            }
            
            retryCount++;

            // Wait with increasing delay
            float waitTime = retryCount * 0.1f; // 0.1s, 0.2s, 0.3s, etc.
            yield return new WaitForSeconds(waitTime);
        }
        
        // Now execute the original PlayStateAnimation logic
        InitializeAnimation(state);
    }

    private void InitializeAnimation(MonsterState state)
    {
        // Original PlayStateAnimation logic goes here
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
        {
            Debug.LogError($"[Animation] ❌ No skeleton components for {_controller.monsterID} after waiting");
            return;
        }

        if (_skeletonGraphic.AnimationState == null)
        {
            _skeletonGraphic.Initialize(true);
            
            if (_skeletonGraphic.AnimationState == null)
            {
                Debug.LogError($"[Animation] ❌ Failed to initialize AnimationState for {_controller.monsterID}");
                return;
            }
        }
    }

    public string GetAvailableAnimation(MonsterState state)
    {
        // Get evolution-specific animations if available
        string[] preferredAnimations = GetEvolutionSpecificAnimations(state);
        
        // Fallback to default animations if no evolution-specific ones
        if (preferredAnimations == null || preferredAnimations.Length == 0)
        {
            preferredAnimations = GetDefaultAnimations(state);
        }

        // Check if any of the preferred animations exist
        foreach (string animName in preferredAnimations)
        {
            if (HasAnimation(animName))
            {
                return animName;
            }
        }

        // Fallback to idle if nothing else works
        Debug.LogWarning($"[Animation] No valid animations found for {state} in {_controller.monsterID}, using idle");
        return "idle";
    }

    private string[] GetEvolutionSpecificAnimations(MonsterState state)
    {
        if (_controller?.MonsterData?.evolutionAnimationSets == null) return GetDefaultAnimations(state);
    
        int currentEvolutionLevel = _controller.evolutionLevel;
        var animationSet = System.Array.Find(_controller.MonsterData.evolutionAnimationSets,
            set => set.evolutionLevel == currentEvolutionLevel);
            
        if (animationSet?.availableAnimations == null || animationSet.availableAnimations.Length == 0)
        {
            for (int level = currentEvolutionLevel - 1; level >= 0; level--)
            {
                animationSet = System.Array.Find(_controller.MonsterData.evolutionAnimationSets,
                    set => set.evolutionLevel == level);
                if (animationSet?.availableAnimations != null && animationSet.availableAnimations.Length > 0)
                    break;
            }
        }
    
        if (animationSet?.availableAnimations == null || animationSet.availableAnimations.Length == 0)
        {
            animationSet = System.Array.Find(_controller.MonsterData.evolutionAnimationSets, 
                set => set.availableAnimations != null && set.availableAnimations.Length > 0);
        }
    
        if (animationSet?.availableAnimations == null)
        {
            Debug.LogWarning($"[Animation] No valid animation sets for {_controller.monsterID} evolution level {currentEvolutionLevel}, using defaults");
            return GetDefaultAnimations(state);
        }
    
        return FilterAnimationsByState(state, animationSet.availableAnimations);
    }

    private string[] FilterAnimationsByState(MonsterState state, string[] availableAnimations)
    {
        string[] patterns = GetDefaultAnimations(state);
        
        List<string> matchingAnimations = new List<string>();
        
        foreach (string pattern in patterns)
        {
            foreach (string animation in availableAnimations)
            {
                if (animation.ToLower().Contains(pattern.ToLower()))
                {
                    matchingAnimations.Add(animation);
                }
            }
        }
        
        // If no matches found, return all available animations as fallback
        return matchingAnimations.Count > 0 ? matchingAnimations.ToArray() : availableAnimations;
    }

    private bool HasAnimation(string animationName)
    {
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
        {
            Debug.LogWarning($"[Animation] Cannot check animation '{animationName}' - skeleton components missing");
            return false;
        }
        
        // Force initialize if needed
        if (_skeletonGraphic.AnimationState == null)
        {
            _skeletonGraphic.Initialize(true);
        }
            
        var skeletonData = _skeletonGraphic.skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData == null) 
        {
            Debug.LogWarning($"[Animation] Cannot get skeleton data to check animation '{animationName}'");
            return false;
        }
        
        var animation = skeletonData.FindAnimation(animationName);
        bool found = animation != null;
        
        return found;
    }
    
    public bool HasValidAnimationForState(MonsterState state)
    {
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
        {
            return false;
        }

        // Check if monster has ANY of the default animations for this state
        string[] defaultAnimations = GetDefaultAnimations(state);
        
        foreach (string animName in defaultAnimations)
        {
            if (HasAnimation(animName))
            {
                return true; // Found at least one valid animation
            }
        }

        return false;
    }

    // Your existing interface - now powered by the dictionary
    private string[] GetDefaultAnimations(MonsterState state)
    {
        return StateAnimationMap.TryGetValue(state, out var animations) 
            ? animations 
            : new[] { "idle" };
    }


    public float GetAnimationDuration(string animationName)
    {
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
            return 1f;
            
        var skeletonData = _skeletonGraphic.skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData == null) return 1f;
        
        var animation = skeletonData.FindAnimation(animationName);
        if (animation == null) return 1f;
        
        return animation.Duration;
    }
}
