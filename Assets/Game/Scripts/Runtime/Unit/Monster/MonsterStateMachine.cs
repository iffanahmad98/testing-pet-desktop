using UnityEngine;
using Spine.Unity;
using System;

public class MonsterStateMachine : MonoBehaviour
{
    [SerializeField] private MonsterState _currentState = MonsterState.Idle;
    private MonsterState _previousState = MonsterState.Idle;
    private float _stateTimer;
    private float _currentStateDuration;
    private const float _defaultEatingStateDuration = 2f;
    
    private MonsterController _controller;
    private MonsterAnimationHandler _animationHandler;
    public MonsterAnimationHandler AnimationHandler => _animationHandler;
    private MonsterBehaviorHandler _behaviorHandler;
    public MonsterBehaviorHandler BehaviorHandler => _behaviorHandler;

    public MonsterState CurrentState => _currentState;
    public MonsterState PreviousState => _previousState;
    public event Action<MonsterState> OnStateChanged;
    private void Start()
    {
        _controller = GetComponent<MonsterController>();
        var skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();

        _animationHandler = new MonsterAnimationHandler(_controller, skeletonGraphic);
        _behaviorHandler = new MonsterBehaviorHandler(_controller);

        ChangeState(MonsterState.Idle);
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;

        // Prevent state changes during evolution
        if (_controller != null && _controller.IsEvolving)
        {
            return; // Don't process state changes while evolving
        }

        // Better coordination with food handler
        if (_currentState == MonsterState.Eating)
        {
            var foodHandler = _controller.FoodHandler;

            // Only timeout if food handler confirms eating should end
            if (_stateTimer > _defaultEatingStateDuration * 4f) // Even more lenient
            {
                bool isStillEating = foodHandler?.IsCurrentlyEating ?? false;
                if (!isStillEating)
                {
                    ChangeState(MonsterState.Idle);
                    return;
                }
                else
                {
                    // Continue waiting
                }
            }
        }

        // ADD: Check if monster should continue current movement state
        if (ShouldContinueCurrentMovement())
        {
            return; // Don't change state while traveling to target
        }

        // Normal state transitions - exclude eating from auto-transitions
        if (_stateTimer >= _currentStateDuration && _currentState != MonsterState.Eating)
        {
            var nextState = _behaviorHandler.SelectNextState(_currentState);
            
            // ADD: Validate state transition based on position
            if (IsValidStateTransition(_currentState, nextState))
            {
                ChangeState(nextState);
            }
            else
            {
                // Extend current state duration and try again later
                _currentStateDuration += 1f;
            }
        }
    }

    public void ChangeState(MonsterState newState)
    {
        // Prevent state changes during evolution
        if (_controller != null && _controller.IsEvolving)
        {
            return;
        }

        // Validate state has animations before changing
        if (!_animationHandler.HasValidAnimationForState(newState))
        {
            newState = MonsterState.Idle; // Fallback to idle
        }
        
        _previousState = _currentState;
        _currentState = newState;
        _stateTimer = 0f;

        _animationHandler.PlayStateAnimation(newState);
        OnStateChanged?.Invoke(_currentState);
        _currentStateDuration = _behaviorHandler.GetStateDuration(newState, _animationHandler);
    }

    public float GetCurrentStateDuration() => _currentStateDuration;
    public string GetAvailableAnimation(MonsterState state) => _animationHandler?.GetAvailableAnimation(state) ?? "idle";

    // ADD: Method to check if monster should continue moving to target
    private bool ShouldContinueCurrentMovement()
    {
        // Only apply to movement states
        if (_currentState != MonsterState.Walking && 
            _currentState != MonsterState.Running && 
            _currentState != MonsterState.Flying &&
            _currentState != MonsterState.Flapping)
        {
            return false;
        }

        // Get current position and target from controller
        var rectTransform = _controller.GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 currentPos = rectTransform.anchoredPosition;
        Vector2 targetPos = _controller.GetTargetPosition(); // You'll need to expose this from MonsterController
        
        // Check if still traveling to target (within reasonable distance)
        float distanceToTarget = Vector2.Distance(currentPos, targetPos);
        
        // If still far from target, continue current state
        if (distanceToTarget > 30f) // Adjust threshold as needed
        {
            return true;
        }

        return false;
    }

    // ADD: Method to validate state transitions based on position
    private bool IsValidStateTransition(MonsterState fromState, MonsterState toState)
    {
        // Get current position
        var rectTransform = _controller.GetComponent<RectTransform>();
        if (rectTransform == null) return true; // Allow transition if can't check

        Vector2 currentPos = rectTransform.anchoredPosition;
        
        // Check if monster is in air (you can get this from bounds handler)
        bool isInAir = IsMonsterInAir(currentPos);
        
        // Flying to ground state transitions - only allow if monster is actually on ground
        if ((fromState == MonsterState.Flying || fromState == MonsterState.Flapping) &&
            (toState == MonsterState.Walking || toState == MonsterState.Running || toState == MonsterState.Idle))
        {
            return !isInAir; // Only transition to ground states if not in air
        }
        
        // Ground to flying transitions - always allowed
        if ((fromState == MonsterState.Walking || fromState == MonsterState.Running || fromState == MonsterState.Idle) &&
            (toState == MonsterState.Flying || toState == MonsterState.Flapping))
        {
            return true;
        }
        
        // Non-movement state transitions while flying - keep in air
        if (isInAir && (toState == MonsterState.Idle || toState == MonsterState.Jumping || toState == MonsterState.Itching))
        {
            return true; // Allow these but they'll use air bounds
        }
        
        return true; // Allow other transitions
    }

        // ADD: Helper method to check if monster is in air
    private bool IsMonsterInAir(Vector2 position)
    {
        // You can access the bounds handler through the controller
        var boundsHandler = _controller.BoundHandler; // You'll need to expose this
        if (boundsHandler != null)
        {
            var groundBounds = boundsHandler.CalculateGroundBounds(); // You'll need to make this public
            return position.y > groundBounds.max.y + 50f; // 50f buffer zone
        }
        
        // Fallback: simple Y threshold
        return position.y > -200f; // Adjust based on your game area
    }
}