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

    private SkeletonGraphic _skeletonGraphic;
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
        _skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
        _animationHandler = new MonsterAnimationHandler(_controller, _skeletonGraphic);
        _behaviorHandler = new MonsterBehaviorHandler(_controller);

        ChangeState(MonsterState.Idle);
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;

        if (_controller?.EvolutionHandler?.IsEvolving == true) return;

        // Reduce movement intensity when happiness or hunger is low (but don't force 100% idle)
        // The actual state selection logic is handled in MonsterBehaviorHandler

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

        // Force Idle if movement states have been running for more than 10 seconds
        if ((_currentState == MonsterState.Walking ||
             _currentState == MonsterState.Running ||
             _currentState == MonsterState.Flying) &&
            _stateTimer >= 10f)
        {
            ChangeState(MonsterState.Idle);
            return;
        }

        // Force movement if Idle has been running for more than 10 seconds
        if (_currentState == MonsterState.Idle && _stateTimer >= 10f)
        {
            if (_behaviorHandler != null)
            {
                // Force movement state based on position
                bool isInAir = IsMonsterInAir(GetComponent<RectTransform>().anchoredPosition);
                MonsterState forcedMovement = isInAir ? MonsterState.Flying : MonsterState.Walking;
                ChangeState(forcedMovement);
                return;
            }
        }

        if (ShouldContinueCurrentMovement())
        {
            return;
        }

        if (_stateTimer >= _currentStateDuration && _currentState != MonsterState.Eating)
        {
            if (_behaviorHandler == null)
            {
                // Debug.LogError("[AI] _behaviorHandler is NULL", this);
                _currentStateDuration += 0.5f; // cegah loop ketat
                return;
            }

            MonsterState nextState;
            try
            {
                nextState = _behaviorHandler.SelectNextState(_currentState);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex, this);   // log stacktrace yang asli
                _currentStateDuration += 0.5f;
                return;
            }

            if (IsValidStateTransition(_currentState, nextState))
                ChangeState(nextState);
            else
                _currentStateDuration += 0.5f;
        }
    }

    public void ChangeState(MonsterState newState)
    {
        if (_controller == null) return;

        // ✅ Amanin null di sini
        if (!(_animationHandler?.HasValidAnimationForState(newState) ?? false))
            newState = MonsterState.Idle;

        // PREVENT CONSECUTIVE IDLE: If trying to go from Idle to Idle, force movement instead
        // BUT: Allow it if this is the first state change (previous == current, meaning just spawned)
        // When stats are low, the BehaviorHandler will naturally select Idle more often
        if (_currentState == MonsterState.Idle && newState == MonsterState.Idle && _previousState != _currentState)
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                bool isInAir = IsMonsterInAir(rectTransform.anchoredPosition);
                newState = isInAir ? MonsterState.Flying : MonsterState.Walking;
                // Debug.Log($"[{_controller.gameObject.name}] Prevented Idle→Idle! Forcing {newState}");
            }
        }

        _previousState = _currentState;
        _currentState  = newState;
        _stateTimer    = 0f;

        // Log state transition
        Debug.Log($"[{_controller.gameObject.name}] State changed: {_previousState} → {_currentState} (duration will be set)");

        _animationHandler?.PlayStateAnimation(newState); // aman kalau null
        OnStateChanged?.Invoke(_currentState);

        _currentStateDuration = (_behaviorHandler != null)
            ? _behaviorHandler.GetStateDuration(newState, _animationHandler)
            : 1f; // fallback durasi default
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

        // Check if using horizontal-only movement (small game area height)
        var monsterManager = _controller.MonsterManager;
        bool isHorizontalOnly = false;
        if (monsterManager != null)
        {
            var gameAreaRect = monsterManager.gameAreaRT;
            if (gameAreaRect != null)
            {
                float currentHeight = gameAreaRect.sizeDelta.y;
                float maxHeight = monsterManager.GetMaxGameAreaHeight();
                isHorizontalOnly = currentHeight <= maxHeight / 2f;
            }
        }

        // For horizontal-only movement, only check X distance
        float distanceToTarget = isHorizontalOnly
            ? Mathf.Abs(currentPos.x - targetPos.x)
            : Vector2.Distance(currentPos, targetPos);

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