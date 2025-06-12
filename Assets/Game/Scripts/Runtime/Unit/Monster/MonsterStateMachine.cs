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

        // Normal state transitions - exclude eating from auto-transitions
        if (_stateTimer >= _currentStateDuration && _currentState != MonsterState.Eating)
        {
            var nextState = _behaviorHandler.SelectNextState(_currentState);
            ChangeState(nextState);
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
}