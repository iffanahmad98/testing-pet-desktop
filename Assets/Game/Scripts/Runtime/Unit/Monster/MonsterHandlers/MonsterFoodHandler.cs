using UnityEngine;
using System.Collections;

public class MonsterFoodHandler
{
    private MonsterController _controller;
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


    public MonsterFoodHandler(MonsterController controller, RectTransform rectTransform)
    {
        _controller = controller;
        _rectTransform = rectTransform;
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

        // Clear previous food claim before finding new one
        if (NearestFood != null)
        {
            NearestFood.ReleaseClaim(_controller);
        }

        NearestFood = null;
        float closestSqr = float.MaxValue;
        Vector2 pos = _rectTransform.anchoredPosition;

        foreach (FoodController food in _controller.MonsterManager.activeFoods)
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
        if (_controller.StateMachine?.CurrentState == MonsterState.Eating)
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
        // Simplify validation - only check if already eating or food exists
        if (_isInternallyEating || _eatingCoroutine != null) return;
        if (NearestFood == null) return;
        
        // Lock eating state
        _isInternallyEating = true;
        
        // Trigger eating animation
        _controller.TriggerEating();
        
        // Start consumption process
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
        // Wait for eating animation to complete
        float eatingDuration = _controller.StateMachine?.GetCurrentStateDuration() ?? 2f;
        yield return new WaitForSeconds(eatingDuration);
        
        // Simplified - just check if food still exists
        if (NearestFood != null)
        {
            // Always allow feeding regardless of sick status
            float newHunger = Mathf.Clamp(_controller.currentHunger + NearestFood.nutritionValue, 0f, 100f);
            float newHappiness = Mathf.Clamp(_controller.currentHappiness + (NearestFood.nutritionValue * 0.5f), 0f, 100f);
            
            _controller.SetHunger(newHunger);
            _controller.SetHappiness(newHappiness);
            
            // Remove and despawn food
            _controller.MonsterManager.activeFoods.Remove(NearestFood);
            _controller.MonsterManager.DespawnToPool(NearestFood.gameObject);
            _controller.TriggerFoodConsumption();
        }
        
        // Always clean up food reference
        ClearFood();
        CompleteEatingSequence();
    }
    
    private void CompleteEatingSequence()
    {
        _lastEatingTime = Time.time;
        _eatingCoroutine = null;
        _isInternallyEating = false;
        
        // Let state machine handle transition
        if (_controller.StateMachine?.CurrentState == MonsterState.Eating)
        {
            _controller.StateMachine.ChangeState(MonsterState.Idle);
        }
        
        _controller.SetRandomTarget();
    }
    
    // Public getter using internal state
    public bool IsCurrentlyEating => _isInternallyEating || (_controller.StateMachine?.CurrentState == MonsterState.Eating);

    // Modify ValidateEatingConditions to be simpler
    private bool ValidateEatingConditions()
    {
        return NearestFood != null &&
               IsValidFood(NearestFood) &&
               NearestFood.IsClaimedBy(_controller) &&
               Time.time - _lastEatingTime >= EATING_COOLDOWN;
    }
}
