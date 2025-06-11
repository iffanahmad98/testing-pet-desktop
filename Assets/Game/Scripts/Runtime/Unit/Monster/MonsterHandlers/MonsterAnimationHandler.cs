using UnityEngine;
using Spine.Unity;
using System.Collections.Generic;
using System.Collections;

public class MonsterAnimationHandler
{
    private MonsterController _controller;
    private SkeletonGraphic _skeletonGraphic;
    
    public MonsterAnimationHandler(MonsterController controller, SkeletonGraphic skeletonGraphic)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
    }
    
    public void PlayStateAnimation(MonsterState state)
    {
        if (_skeletonGraphic == null) return;
        
        // Ensure skeleton data is available before trying to use AnimationState
        if (_skeletonGraphic.skeletonDataAsset == null) return;
        
        if (_skeletonGraphic.AnimationState == null)
        {
            _skeletonGraphic.Initialize(true);
            
            // Wait for initialization to complete
            _controller.StartCoroutine(PlayAnimationAfterInitialization(state));
            return;
        }

        // Get animation name and play it
        string animationName = GetAvailableAnimation(state);
        if (!string.IsNullOrEmpty(animationName) && HasAnimation(animationName))
        {
            _skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
        }
    }
    
    private IEnumerator PlayAnimationAfterInitialization(MonsterState state)
    {
        // Wait one frame for initialization to complete
        yield return null;
        
        if (_skeletonGraphic.AnimationState != null)
        {
            string animationName = GetAvailableAnimation(state);
            if (!string.IsNullOrEmpty(animationName) && HasAnimation(animationName))
            {
                _skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
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
        return "idle";
    }

    private string[] GetEvolutionSpecificAnimations(MonsterState state)
    {
        if (_controller?.MonsterData?.evolutionAnimationSets == null)
            return GetDefaultAnimations(state);
    
        int currentEvolutionLevel = _controller.evolutionLevel;
    
        // Try current evolution level first
        var animationSet = System.Array.Find(_controller.MonsterData.evolutionAnimationSets, 
            set => set.evolutionLevel == currentEvolutionLevel);
    
        // Fallback strategy: Try lower evolution levels in descending order
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
    
        // If still no valid set found, use first available set
        if (animationSet?.availableAnimations == null || animationSet.availableAnimations.Length == 0)
        {
            animationSet = System.Array.Find(_controller.MonsterData.evolutionAnimationSets, 
                set => set.availableAnimations != null && set.availableAnimations.Length > 0);
        }
    
        // Complete fallback to default patterns
        if (animationSet?.availableAnimations == null)
        {
            Debug.LogWarning($"[Animation] No valid animation sets for {_controller.monsterID} evolution level {currentEvolutionLevel}, using defaults");
            return GetDefaultAnimations(state);
        }
    
        return FilterAnimationsByState(state, animationSet.availableAnimations);
    }

    private string[] FilterAnimationsByState(MonsterState state, string[] availableAnimations)
    {
        string[] patterns = GetStatePatterns(state);
        
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

    private string[] GetStatePatterns(MonsterState state)
    {
        return state switch
        {
            MonsterState.Idle => new[] { "idle", "rest", "stand", "sleep", "roar", "breathe" },
            MonsterState.Walking => new[] { "walk", "walking", "move" },
            MonsterState.Running => new[] { "run", "running", "sprint", "hunt" },
            MonsterState.Flying => new[] { "fly", "flying", "hover", "float" },
            MonsterState.Flapping => new[] { "flap", "flapping", "wing", "wings", "wingbeat" }, // NEW: Separate Flapping patterns
            MonsterState.Jumping => new[] { "jump", "jumping", "leap", "pounce" },
            MonsterState.Itching => new[] { "itch", "itching", "scratch" },
            MonsterState.Eating => new[] { "eat", "eating", "feed", "consume" },
            _ => new[] { "idle" }
        };
    }

    private string[] GetDefaultAnimations(MonsterState state)
    {
        return state switch
        {
            MonsterState.Idle => new[] { "idle" },
            MonsterState.Walking => new[] { "walking", "walk" },
            MonsterState.Running => new[] { "running", "run", "walking", "walk" },
            MonsterState.Flying => new[] { "flying", "fly", "running", "run", "walking", "walk" },
            MonsterState.Flapping => new[] { "flapping", "flap", "jumping", "jump" }, // NEW: Separate Flapping fallbacks
            MonsterState.Jumping => new[] { "jumping", "jump" },
            MonsterState.Itching => new[] { "itching", "itch" },
            MonsterState.Eating => new[] { "eating", "eat" },
            _ => new[] { "idle" }
        };
    }

    public bool HasAnimation(string animationName)
    {
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
            return false;
            
        var skeletonData = _skeletonGraphic.skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData == null) return false;
        
        var animation = skeletonData.FindAnimation(animationName);
        return animation != null;
    }
    
    public bool HasValidAnimationForState(MonsterState state)
    {
        string[] animations = GetEvolutionSpecificAnimations(state);
        
        foreach (string animName in animations)
        {
            if (HasAnimation(animName))
            {
                return true;
            }
        }
        
        // If no specific animations, check if idle exists as absolute fallback
        return state == MonsterState.Idle ? HasAnimation("idle") : false;
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
