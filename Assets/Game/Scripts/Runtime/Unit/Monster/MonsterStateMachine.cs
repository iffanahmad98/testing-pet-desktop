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
    private MonsterBehaviorHandler _behaviorHandler;

    public MonsterState CurrentState => _currentState;
    public MonsterState PreviousState => _previousState;
    public event Action<MonsterState> OnStateChanged;
    public MonsterAnimationHandler AnimationHandler => _animationHandler; 

    private void Start()
    {
        _controller = GetComponent<MonsterController>();
        var skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
        
        _animationHandler = new MonsterAnimationHandler(_controller, skeletonGraphic);
        _behaviorHandler = new MonsterBehaviorHandler(_controller);
        
        // DEBUG: Log available animations right after creating the handler
        if (_animationHandler != null)
        {
            _animationHandler.LogAvailableAnimations();
        }
        
        ChangeState(MonsterState.Idle);
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;

        // FIXED: Better coordination with food handler
        if (_currentState == MonsterState.Eating)
        {
            var foodHandler = _controller.FoodHandler;
            
            // Only timeout if food handler confirms eating should end
            if (_stateTimer > _defaultEatingStateDuration * 4f) // Even more lenient
            {
                bool isStillEating = foodHandler?.IsCurrentlyEating ?? false;
                if (!isStillEating)
                {
                    Debug.LogWarning($"[StateMachine] Eating confirmed complete for {gameObject.name} after {_stateTimer}s");
                    ForceState(MonsterState.Idle);
                    return;
                }
                else
                {
                    Debug.Log($"[StateMachine] Waiting for food handler to complete eating for {gameObject.name}");
                }
            }
        }

        // Normal state transitions - exclude eating from auto-transitions
        if (_stateTimer >= _currentStateDuration && _currentState != MonsterState.Eating)
        {
            var nextState = _behaviorHandler.SelectNextState(_currentState);
            ChangeState(nextState);
        }
    }

    private void ChangeState(MonsterState newState)
    {
        // Validate state has animations before changing
        if (!_animationHandler.HasValidAnimationForState(newState))
        {
            Debug.LogWarning($"Skipping transition to {newState} - no valid animations found");
            newState = MonsterState.Idle; // Fallback to idle
        }
        
        _previousState = _currentState;
        _currentState = newState;
        _stateTimer = 0f;

        _animationHandler.PlayStateAnimation(newState);
        OnStateChanged?.Invoke(_currentState);
        _currentStateDuration = _behaviorHandler.GetStateDuration(newState, _animationHandler);
    }

    public void ForceState(MonsterState newState)
    {
        Debug.Log($"[StateMachine] Force changing state from {_currentState} to {newState} for {gameObject.name}");
        
        // CHANGED: Ensure timer is reset when forcing state
        _stateTimer = 0f;
        
        ChangeState(newState);
    }

    public float GetCurrentStateDuration()
    {
        return _currentStateDuration;
    }
    
    // Public method for other classes to get animations
    public string GetAvailableAnimation(MonsterState state)
    {
        return _animationHandler?.GetAvailableAnimation(state) ?? "idle";
    }
}