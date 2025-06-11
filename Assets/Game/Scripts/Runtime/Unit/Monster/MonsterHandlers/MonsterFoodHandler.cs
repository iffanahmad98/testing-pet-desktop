using UnityEngine;
using System.Collections;

public class MonsterFoodHandler
{
    private MonsterController _controller;
    private GameManager _gameManager;
    private RectTransform _rectTransform;
    private float _foodDetectionRangeSqr;
    private float _eatDistanceSqr;
    private float _cachedFoodDistanceSqr = float.MaxValue;
    private float _lastEatingTime = 0f;
    private const float EATING_COOLDOWN = 3f;
    
    // FIXED: Track eating coroutine AND eating state internally
    private Coroutine _eatingCoroutine;
    private bool _isInternallyEating = false; // Internal state to prevent race conditions
    
    public FoodController NearestFood { get; private set; }
    public bool IsNearFood { get; private set; }
    
    // FIXED: Single source of truth - use cached reference
    private MonsterStateMachine _stateMachine;
    
    public MonsterFoodHandler(MonsterController controller, GameManager gameManager, RectTransform rectTransform)
    {
        _controller = controller;
        _gameManager = gameManager;
        _rectTransform = rectTransform;
        _stateMachine = controller.GetComponent<MonsterStateMachine>(); // Cache once
    }
    
    public void Initialize(MonsterDataSO data)
    {
        _foodDetectionRangeSqr = data.foodDetectionRange * data.foodDetectionRange;
        _eatDistanceSqr = data.eatDistance * data.eatDistance;
    }
    
    public void FindNearestFood()
    {
        if (!_controller.IsLoaded || _isInternallyEating) return;
        
        if (Time.time - _lastEatingTime < EATING_COOLDOWN)
        {
            return;
        }

        // FIXED: Clear previous food claim before finding new one
        if (NearestFood != null)
        {
            NearestFood.ReleaseClaim(_controller);
        }

        NearestFood = null;
        float closestSqr = float.MaxValue;
        Vector2 pos = _rectTransform.anchoredPosition;

        foreach (FoodController food in _gameManager.activeFoods)
        {
            if (food == null) continue;

            RectTransform foodRt = food.GetComponent<RectTransform>();
            Vector2 foodPos = foodRt.anchoredPosition;
            float sqrDist = (foodPos - pos).sqrMagnitude;

            if (sqrDist < _foodDetectionRangeSqr && sqrDist < closestSqr)
            {
                if (food.TryClaim(_controller))
                {
                    closestSqr = sqrDist;
                    NearestFood = food;
                }
            }
        }

        _cachedFoodDistanceSqr = NearestFood != null ? closestSqr : float.MaxValue;
        IsNearFood = _cachedFoodDistanceSqr < _eatDistanceSqr;
    }
    
    public void HandleFoodLogic(ref Vector2 targetPosition)
    {
        if (NearestFood == null || _isInternallyEating) return;
        
        // FIXED: Use cached state machine reference
        if (_stateMachine?.CurrentState == MonsterState.Eating)
        {
            return;
        }

        // Validate food is still valid
        if (!IsValidFood(NearestFood))
        {
            ClearFood();
            return;
        }

        Vector2 currentPos = _rectTransform.anchoredPosition;
        Vector2 foodPos = GetFoodPosition(NearestFood);
        float currentDistanceSqr = (foodPos - currentPos).sqrMagnitude;
        
        if (currentDistanceSqr < _eatDistanceSqr)
        {
            StartEatingSequence();
        }
        else
        {
            targetPosition = foodPos;
        }
    }
    
    // IMPROVED: Atomic eating start
    private void StartEatingSequence()
    {
        if (!ValidateEatingConditions()) return;
        if (_isInternallyEating || _eatingCoroutine != null) return;

        // Lock the eating state first
        _isInternallyEating = true;
        
        // Then trigger state machine
        _controller.TriggerEating();
        
        // Finally start coroutine
        _eatingCoroutine = _controller.StartCoroutine(ConsumeAfterEating());
    }
    
    // FIXED: Improved force reset with better coordination
    public void ForceResetEating()
    {
        Debug.Log($"[FoodHandler] Force reset eating for {_controller.name}");
        
        // Stop coroutine first
        if (_eatingCoroutine != null)
        {
            _controller.StopCoroutine(_eatingCoroutine);
            _eatingCoroutine = null;
        }
        
        // Reset internal state
        _isInternallyEating = false;
        
        // Clear food reference and release claim
        ClearFood();
        
        // Set cooldown to prevent immediate re-eating
        _lastEatingTime = Time.time;
        
        // Let controller handle state reset
        _controller.SetRandomTarget();
    }
    
    private void ClearFood()
    {
        if (NearestFood != null)
        {
            Debug.Log($"[FoodHandler] Clearing food: {NearestFood.name}");
            NearestFood.ReleaseClaim(_controller); // ADDED: Release claim
            NearestFood = null;
        }
    }
    
    private bool IsValidFood(FoodController food)
    {
        return food != null && food.gameObject.activeInHierarchy;
    }
    
    private Vector2 GetFoodPosition(FoodController food)
    {
        var foodRT = food.GetComponent<RectTransform>();
        return foodRT != null ? foodRT.anchoredPosition : Vector2.zero;
    }
    
    private IEnumerator ConsumeAfterEating()
    {
        Debug.Log($"[FoodHandler] Starting eating coroutine for {_controller.name}");
        
        float eatingDuration = _stateMachine?.GetCurrentStateDuration() ?? 2f;
        Debug.Log($"[FoodHandler] Eating duration: {eatingDuration}");
        yield return new WaitForSeconds(eatingDuration);
        
        // FIXED: Check internal state first, then external state
        if (!_isInternallyEating)
        {
            Debug.LogWarning($"[FoodHandler] Eating was cancelled internally for {_controller.name}");
            ClearFood(); // ADDED: Properly release claim
            yield break;
        }
        
        if (_stateMachine?.CurrentState != MonsterState.Eating)
        {
            Debug.LogWarning($"[FoodHandler] Eating was cancelled externally for {_controller.name}");
            _isInternallyEating = false;
            ClearFood(); // ADDED: Properly release claim
            yield break;
        }
        
        // FIXED: Use ValidateEatingConditions for complete validation
        if (NearestFood != null && ValidateEatingConditions())
        {
            Debug.Log($"[FoodHandler] Consuming food: {NearestFood.name} with nutrition: {NearestFood.nutritionValue}");
            
            bool feedSuccess = TryFeedMonster(NearestFood.nutritionValue);
            
            if (feedSuccess)
            {
                // Remove and despawn food
                _gameManager.activeFoods.Remove(NearestFood);
                _gameManager.DespawnToPool(NearestFood.gameObject);
                Debug.Log($"[FoodHandler] Successfully consumed and despawned food");
                
                _controller.TriggerFoodConsumption();
            }
            else
            {
                Debug.LogWarning($"[FoodHandler] Failed to feed monster - monster might be sick");
            }
            
            // FIXED: Always clear food properly
            ClearFood();
        }
        else
        {
            Debug.LogWarning($"[FoodHandler] Food became invalid during eating");
            ClearFood(); // ADDED: Clean up invalid food
        }
        
        CompleteEatingSequence();
    }
    
    // FIXED: Atomic completion of eating sequence
    private void CompleteEatingSequence()
    {
        _lastEatingTime = Time.time;
        _eatingCoroutine = null;
        _isInternallyEating = false;
        
        Debug.Log($"[FoodHandler] Finished eating, returning to idle");
        
        // Let state machine handle transition
        if (_stateMachine?.CurrentState == MonsterState.Eating)
        {
            _stateMachine.ForceState(MonsterState.Idle);
        }
        
        _controller.SetRandomTarget();
    }
    
    // FIXED: Direct stat modification without circular dependencies
    private bool TryFeedMonster(float nutritionValue)
    {
        Debug.Log($"[FoodHandler] Attempting to feed {_controller.name} with {nutritionValue} nutrition");
        
        // Check if monster is sick
        if (_controller.IsSick)
        {
            Debug.LogWarning($"[FoodHandler] Monster {_controller.name} is too sick to eat regular food!");
            return false;
        }
        
        // FIXED: Use controller's stat methods directly (no circular calls)
        float newHunger = Mathf.Clamp(_controller.currentHunger + nutritionValue, 0f, 100f);
        float newHappiness = Mathf.Clamp(_controller.currentHappiness + (nutritionValue * 0.5f), 0f, 100f);
        
        _controller.SetHunger(newHunger);
        _controller.SetHappiness(newHappiness);
        
        Debug.Log($"[FoodHandler] Successfully fed {nutritionValue} to {_controller.name}");
        return true;
    }
    
    private bool CanEat()
    {
        return !_controller.IsSick && 
               Time.time - _lastEatingTime >= EATING_COOLDOWN &&
               NearestFood != null && 
               NearestFood.gameObject.activeInHierarchy &&
               !_isInternallyEating; // ADDED: Check internal state
    }
    
    // FIXED: Public getter using internal state
    public bool IsCurrentlyEating => _isInternallyEating || (_stateMachine?.CurrentState == MonsterState.Eating);
    
    // IMPROVED: Better validation chain
    private bool ValidateEatingConditions()
    {
        return NearestFood != null && 
               IsValidFood(NearestFood) && 
               NearestFood.IsClaimedBy(_controller) && 
               !_controller.IsSick &&
               Time.time - _lastEatingTime >= EATING_COOLDOWN;
    }
}
