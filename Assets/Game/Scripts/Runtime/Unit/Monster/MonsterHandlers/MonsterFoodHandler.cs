using UnityEngine;
using System.Collections;

public class MonsterFoodHandler
{
    private MonsterController _controller;
    private MonsterStateMachine _stateMachine;
    private MonsterManager _gameManager;
    private RectTransform _rectTransform;
    private float _foodDetectionRangeSqr;
    private float _eatDistanceSqr;
    private float _cachedFoodDistanceSqr = float.MaxValue;
    private float _lastEatingTime = 0f;
    private const float EATING_COOLDOWN = 3f;
    private Coroutine _eatingCoroutine;
    private bool _isInternallyEating = false; 
    
    public FoodController NearestFood { get; private set; }
    public bool IsNearFood { get; private set; }

    
    public MonsterFoodHandler(MonsterController controller, MonsterManager gameManager, RectTransform rectTransform)
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
    
    private void ClearFood()
    {
        if (NearestFood != null)
        {
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
        float eatingDuration = _stateMachine?.GetCurrentStateDuration() ?? 2f;
        yield return new WaitForSeconds(eatingDuration);
        
        // FIXED: Use ValidateEatingConditions for complete validation
        if (NearestFood != null && ValidateEatingConditions())
        {
            bool feedSuccess = TryFeedMonster(NearestFood.nutritionValue);
            
            if (feedSuccess)
            {
                // Remove and despawn food
                _gameManager.activeFoods.Remove(NearestFood);
                _gameManager.DespawnToPool(NearestFood.gameObject);
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
            ClearFood(); 
        }
        
        CompleteEatingSequence();
    }
    
    private void CompleteEatingSequence()
    {
        _lastEatingTime = Time.time;
        _eatingCoroutine = null;
        _isInternallyEating = false;
        
        // Let state machine handle transition
        if (_stateMachine?.CurrentState == MonsterState.Eating)
        {
            _stateMachine.ChangeState(MonsterState.Idle);
        }
        
        _controller.SetRandomTarget();
    }
    
    private bool TryFeedMonster(float nutritionValue)
    {
        // Check if monster is sick
        if (_controller.IsSick) return false;

        // FIXED: Use controller's stat methods directly
        float newHunger = Mathf.Clamp(_controller.currentHunger + nutritionValue, 0f, 100f);
        float newHappiness = Mathf.Clamp(_controller.currentHappiness + (nutritionValue * 0.5f), 0f, 100f);
        
        _controller.SetHunger(newHunger);
        _controller.SetHappiness(newHappiness);

        return true;
    }
    
    // FIXED: Public getter using internal state
    public bool IsCurrentlyEating => _isInternallyEating || (_stateMachine?.CurrentState == MonsterState.Eating);

    // Better validation chain
    private bool ValidateEatingConditions()
    {
        return NearestFood != null &&
               IsValidFood(NearestFood) &&
               NearestFood.IsClaimedBy(_controller) &&
               !_controller.IsSick &&
               Time.time - _lastEatingTime >= EATING_COOLDOWN;
    }
}
