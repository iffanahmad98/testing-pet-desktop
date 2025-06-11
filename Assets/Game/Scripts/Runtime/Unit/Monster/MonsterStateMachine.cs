using UnityEngine;
using Spine.Unity;

public class MonsterStateMachine : MonoBehaviour
{
    private MonsterState _currentState = MonsterState.Idle;
    private MonsterState _previousState = MonsterState.Idle;
    private float _stateTimer;
    private float _currentStateDuration;
    private const float _defaultEatingStateDuration = 2f;
    
    private MonsterController _controller;
    private MonsterAnimationHandler _animationHandler;
    private MonsterBehaviorHandler _behaviorHandler;

    public MonsterState CurrentState => _currentState;
    public MonsterState PreviousState => _previousState;
    public event System.Action<MonsterState> OnStateChanged;

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

        if (_currentState == MonsterState.Eating && _stateTimer > _defaultEatingStateDuration)
        {
            _controller?.ForceResetEating();
            return;
        }

        if (_stateTimer >= _currentStateDuration)
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