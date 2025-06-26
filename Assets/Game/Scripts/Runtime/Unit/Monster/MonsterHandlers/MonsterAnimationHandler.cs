using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MonsterAnimationHandler
{
    private MonsterController _controller;
    private SkeletonGraphic _skeletonGraphic;
    private SkeletonData _cachedSkeletonData;
    private Dictionary<string, Spine.Animation> _animationCache = new Dictionary<string, Spine.Animation>();
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

    private void SetupAnimationBlend()
    {
        if (_skeletonGraphic?.AnimationState?.Data == null) return;

        var mixData = _skeletonGraphic.AnimationState.Data;
        mixData.DefaultMix = 0.2f; // fallback

        // Get config from controller
        var config = _controller?.MonsterData?.evolutionBehaviors.FirstOrDefault(
            behavior => behavior.evolutionLevel == _controller.evolutionLevel)?.behaviorConfig;
        if (config != null && config.transitions != null)
        {
            foreach (var transition in config.transitions)
            {
                // Get animation names for states
                string[] fromAnims = GetDefaultAnimations(transition.fromState);
                string[] toAnims = GetDefaultAnimations(transition.toState);
                foreach (var from in fromAnims)
                {
                    foreach (var to in toAnims)
                    {
                        mixData.SetMix(from.ToLowerInvariant(), to.ToLowerInvariant(), transition.blendDuration);
                    }
                }
            }
        }
    }

    public MonsterAnimationHandler(MonsterController controller, SkeletonGraphic skeletonGraphic)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
        SetupAnimationBlend();
    }
    
    public void PlayStateAnimation(MonsterState state)
    {
        if (_skeletonGraphic != null && _skeletonGraphic.skeletonDataAsset != null && _skeletonGraphic.AnimationState != null)
        {
            string animName = GetAvailableAnimation(state);
            bool loop = state != MonsterState.Itching && state != MonsterState.Jumping;
            _skeletonGraphic.AnimationState.SetAnimation(0, animName, loop);
        }
        else
        {
            _controller.StartCoroutine(TryPlayStateAnimation(state));
        }
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
        
        List<string> matchingAnimations = new List<string>(availableAnimations.Length); 
        
        foreach (string pattern in patterns)
        {
            foreach (string animation in availableAnimations)
            {
                // Replace ToLower() calls with case-insensitive comparison
                if (animation.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matchingAnimations.Add(animation);
                }
            }
        }
        
        // Avoid allocation if empty
        return matchingAnimations.Count > 0 ? matchingAnimations.ToArray() : availableAnimations;
    }

    private Spine.Animation GetAnimation(string animationName)
    {
        // Use cached animation if available
        if (_animationCache.TryGetValue(animationName, out var cachedAnim))
            return cachedAnim;
            
        // Otherwise look it up and cache the result
        var skeletonData = GetSkeletonData();
        if (skeletonData == null) 
            return null;
            
        var animation = skeletonData.FindAnimation(animationName);
        
        // Cache if found
        if (animation != null)
            _animationCache[animationName] = animation;
            
        return animation;
    }

    private bool HasAnimation(string animationName)
    {
        if (_skeletonGraphic == null || _skeletonGraphic.skeletonDataAsset == null)
        {
            return false;
        }
        
        if (_skeletonGraphic.AnimationState == null)
        {
            _skeletonGraphic.Initialize(true);
        }
        
        return GetAnimation(animationName) != null;
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

    private string[] GetDefaultAnimations(MonsterState state)
    {
        var key = state.ToString().ToLowerInvariant();
        return StateAnimationMap.TryGetValue(state, out var animations)
            ? animations
            : new[] { "idle" };
    }

    public float GetAnimationDuration(string animationName)
    {
        var animation = GetAnimation(animationName);
        return animation.Duration;
    }

    private SkeletonData GetSkeletonData()
    {
        if (_cachedSkeletonData == null && _skeletonGraphic?.skeletonDataAsset != null)
        {
            _cachedSkeletonData = _skeletonGraphic.skeletonDataAsset.GetSkeletonData(false);
        }
        return _cachedSkeletonData;
    }

    // Call this when the controller is destroyed
    public void Dispose()
    {
        _cachedSkeletonData = null;
        _animationCache.Clear();
    }

    // Call this when skeleton data is updated
    public void InvalidateCache()
    {
        _cachedSkeletonData = null;
        _animationCache.Clear();
    }
}
