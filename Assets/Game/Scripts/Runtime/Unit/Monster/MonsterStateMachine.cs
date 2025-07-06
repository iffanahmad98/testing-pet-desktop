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

        if (_controller?.EvolutionHandler?.IsEvolving == true) return;

        if (_currentState == MonsterState.Eating)
        {
            var foodHandler = _controller.ConsumableHandler;

            if (_stateTimer > _defaultEatingStateDuration * 4f)
            {
                bool isStillEating = foodHandler?.IsCurrentlyConsuming ?? false;
                if (!isStillEating)
                {
                    ChangeState(MonsterState.Idle);
                    return;
                }
            }
        }

        if (ShouldContinueCurrentMovement())
        {
            return;
        }

        if (_stateTimer >= _currentStateDuration && _currentState != MonsterState.Eating)
        {
            var nextState = _behaviorHandler.SelectNextState(_currentState);

            if (IsValidStateTransition(_currentState, nextState))
            {
                ChangeState(nextState);
            }
            else
            {
                _currentStateDuration += 1f;
            }
        }
    }

    public void ChangeState(MonsterState newState)
    {
        if (_controller == null) return;
        if (!_animationHandler.HasValidAnimationForState(newState)) newState = MonsterState.Idle;

        _previousState = _currentState;
        _currentState = newState;
        _stateTimer = 0f;

        _animationHandler.PlayStateAnimation(newState);
        OnStateChanged?.Invoke(_currentState);
        _currentStateDuration = _behaviorHandler.GetStateDuration(newState, _animationHandler);
    }

    public float GetCurrentStateDuration() => _currentStateDuration;
    public string GetAvailableAnimation(MonsterState state) => _animationHandler?.GetAvailableAnimation(state) ?? "idle";

    private bool ShouldContinueCurrentMovement()
    {
        if (_currentState != MonsterState.Walking &&
            _currentState != MonsterState.Running &&
            _currentState != MonsterState.Flying)
        {
            return false;
        }

        if (_currentState == MonsterState.Flying)
        {
            if (ShouldUseHoveringBehavior())
            {
                return UnityEngine.Random.value < 0.5f;
            }
        }

        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 currentPos = rectTransform.anchoredPosition;
        Vector2 targetPos = _controller.GetTargetPosition();
        float distanceToTarget = Vector2.Distance(currentPos, targetPos);

        return distanceToTarget > 30f;
    }

    private bool IsValidStateTransition(MonsterState fromState, MonsterState toState)
    {
        var rectTransform = _controller.GetComponent<RectTransform>();
        if (rectTransform == null) return true;

        Vector2 currentPos = rectTransform.anchoredPosition;
        bool isInAir = IsMonsterInAir(currentPos);

        if ((fromState == MonsterState.Flying || fromState == MonsterState.Flapping) &&
            (toState == MonsterState.Walking || toState == MonsterState.Running || toState == MonsterState.Idle))
        {
            return !isInAir;
        }

        if ((fromState == MonsterState.Walking || fromState == MonsterState.Running || fromState == MonsterState.Idle) &&
            (toState == MonsterState.Flying || toState == MonsterState.Flapping))
        {
            return true;
        }

        if (isInAir && (toState == MonsterState.Idle || toState == MonsterState.Jumping || toState == MonsterState.Itching))
        {
            return true;
        }

        return true;
    }

    private bool IsMonsterInAir(Vector2 position)
    {
        var boundsHandler = _controller.BoundHandler;
        if (boundsHandler != null)
        {
            var groundBounds = boundsHandler.CalculateGroundBounds();
            return position.y > groundBounds.max.y + 50f;
        }

        return position.y > -200f;
    }

    public bool ShouldUseHoveringBehavior()
    {
        if (_currentState != MonsterState.Flying) return false;

        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 currentPos = rectTransform.anchoredPosition;
        Vector2 targetPos = _controller.GetTargetPosition();

        return Vector2.Distance(currentPos, targetPos) < 10f;
    }
}