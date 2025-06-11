using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Spine.Unity;
using System.Collections;

public class MonsterController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Add these delegate fields at the top of the class
    private System.Action<float> _hungerChangedHandler;
    private System.Action<float> _happinessChangedHandler;
    private System.Action<bool> _sickChangedHandler;
    private System.Action<bool> _hoverChangedHandler;
    
    private MonsterDataSO monsterData;
    public string monsterID;
    public MonsterDataSO MonsterData => monsterData;
    [HideInInspector] public bool isFinalForm;
    [HideInInspector] public int evolutionLevel;


    public float currentHunger => _statsHandler?.CurrentHunger ?? 100f;
    public float currentHappiness => _statsHandler?.CurrentHappiness ?? 100f;
    public bool IsSick => _statsHandler?.IsSick ?? false;
    public bool isHovered => _isHovered;
    public bool IsLoaded => _isLoaded;
    public FoodController nearestFood => _foodHandler?.NearestFood;
    public event Action<float> OnHungerChanged
    {
        add => _statsHandler.OnHungerChanged += value;
        remove => _statsHandler.OnHungerChanged -= value;
    }
    public event Action<bool> OnSickChanged
    {
        add => _statsHandler.OnSickChanged += value;
        remove => _statsHandler.OnSickChanged -= value;
    }
    public event Action<float> OnHappinessChanged
    {
        add => _statsHandler.OnHappinessChanged += value;
        remove => _statsHandler.OnHappinessChanged -= value;
    }
    public event Action<bool> OnHoverChanged;

    private MonsterSaveHandler _saveHandler;
    private MonsterVisualHandler _visualHandler;
    private MonsterFoodHandler _foodHandler;
    private MonsterInteractionHandler _interactionHandler;
    private MonsterBoundsHandler _movementBounds;
    private MonsterEvolutionHandler _evolutionHandler;
    private MonsterSeparationHandler _separationBehavior;
    
    public MonsterUIHandler UI = new MonsterUIHandler();
    public MonsterFoodHandler FoodHandler => _foodHandler; 

    private SkeletonGraphic _monsterSpineGraphic;
    private RectTransform _rectTransform;
    private GameManager _gameManager;
    private MonsterStateMachine _stateMachine;
    private MonsterMovementHandler _movementHandler;
    private MonsterStatsHandler _statsHandler;
    private MonsterCoroutineHandler _coroutineHandler; 

    private Vector2 _targetPosition;
    private bool _isLoaded = false;
    private bool _isHovered;
    private Vector2 _lastSortPosition;
    private float _depthSortThreshold = 20f;

    private void Awake()
    {
        InitializeID();
        InitializeComponents();
        InitializeModules();
    }

    private void Start()
    {
        InitializeStateMachine();
        InitializeValues();
        SetRandomTarget();

        if (monsterData != null)
        {
            _visualHandler?.ApplyMonsterVisuals();
        }
        
        // Wait for state machine to initialize, then connect animation handler
        StartCoroutine(ConnectAnimationHandler());
    }

    private IEnumerator ConnectAnimationHandler()
    {
        // Wait one frame to ensure StateMachine.Start() has completed
        yield return null;
        
        // Now connect the animation handler to interaction handler
        if (_interactionHandler != null && _stateMachine?.AnimationHandler != null)
        {
            _interactionHandler.SetAnimationHandler(_stateMachine.AnimationHandler);
            Debug.Log($"[MonsterController] Connected animation handler to interaction handler for {monsterID}");
        }
        else
        {
            Debug.LogError($"[MonsterController] Failed to connect animation handler - interactionHandler null: {_interactionHandler == null}, stateMachine null: {_stateMachine == null}, animHandler null: {_stateMachine?.AnimationHandler == null}");
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        _coroutineHandler?.StartAllCoroutines(); // CHANGED: Use handler
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        _coroutineHandler?.StopAllCoroutines(); // CHANGED: Use handler
    }

    private void Update()
    {
        if (!_isLoaded) return;

        _interactionHandler?.UpdateTimers(Time.deltaTime);
        _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
        HandleMovement();
    }

    private void InitializeModules()
    {
        _statsHandler = new MonsterStatsHandler(this);
        _coroutineHandler = new MonsterCoroutineHandler(this, _statsHandler);
        _saveHandler = new MonsterSaveHandler(this, _statsHandler);
        _visualHandler = new MonsterVisualHandler(this, _monsterSpineGraphic);
        _interactionHandler = new MonsterInteractionHandler(this, _stateMachine);
        _movementHandler = new MonsterMovementHandler(_rectTransform, _stateMachine, this, _gameManager, _monsterSpineGraphic);
        _evolutionHandler = new MonsterEvolutionHandler(this);

        UI.Init();
        _evolutionHandler.InitUIParticles(UI);
    }

    private void InitializeID()
    {
        if (string.IsNullOrEmpty(monsterID))
        {
            monsterID = "temp_" + System.Guid.NewGuid().ToString("N")[..8];
        }
    }

    private void InitializeComponents()
    {
        _rectTransform = GetComponent<RectTransform>();
        _monsterSpineGraphic = GetComponentInChildren<SkeletonGraphic>();
    }

    private void InitializeStateMachine()
    {
        _stateMachine = GetComponent<MonsterStateMachine>(); 
        if (_stateMachine != null)
        {
            _interactionHandler = new MonsterInteractionHandler(this, _stateMachine);
            Debug.Log($"[MonsterController] Created interaction handler for {monsterID}");
        }
        else
        {
            Debug.LogError($"[MonsterController] Failed to get StateMachine component for {monsterID}");
        }
    }

    private void InitializeValues()
    {
        _gameManager = ServiceLocator.Get<GameManager>();
        _gameManager?.RegisterActiveMonster(this);

        if (_gameManager != null && _gameManager.isActiveAndEnabled)
            _isLoaded = true;

        _foodHandler = new MonsterFoodHandler(this, _gameManager, _rectTransform);
        _foodHandler.Initialize(monsterData);

        _movementBounds = new MonsterBoundsHandler(_rectTransform, _gameManager);
        _movementHandler = new MonsterMovementHandler(_rectTransform, _stateMachine, this, _gameManager, _monsterSpineGraphic);
        _separationBehavior = new MonsterSeparationHandler(this, _gameManager, _rectTransform); // Keep only this one
    }

    private void SubscribeToEvents()
    {
        _hungerChangedHandler = (hunger) => UI.UpdateHungerDisplay(hunger, _isHovered);
        _happinessChangedHandler = (happiness) => UI.UpdateHappinessDisplay(happiness, _isHovered);
        _sickChangedHandler = (isSick) =>
        {
            UI.UpdateSickStatusDisplay(_statsHandler?.IsSick == true, _isHovered);
        };
        _hoverChangedHandler = (hovered) =>
        {
            UI.UpdateHungerDisplay(currentHunger, hovered);
            UI.UpdateHappinessDisplay(currentHappiness, hovered);
        };

        OnHungerChanged += _hungerChangedHandler;
        OnHappinessChanged += _happinessChangedHandler;
        OnSickChanged += _sickChangedHandler;
        OnHoverChanged += _hoverChangedHandler;
    }

    private void UnsubscribeFromEvents()
    {
        OnHungerChanged -= _hungerChangedHandler;
        OnHappinessChanged -= _happinessChangedHandler;
        OnSickChanged -= _sickChangedHandler;
        OnHoverChanged -= _hoverChangedHandler;
    }

    private void HandleMovement()
    {
        if (monsterData == null) return;

        // Apply separation force regardless of state
        Vector2 separationForce = _separationBehavior.CalculateSeparationForce();
        if (separationForce.magnitude > 0.1f)
        {
            Vector2 _currentPos = _rectTransform.anchoredPosition;
            Vector2 _newPos = _currentPos + separationForce * Time.deltaTime;

            // Ensure new position is within bounds
            var bounds = _movementBounds.CalculateMovementBounds();
            _newPos.x = Mathf.Clamp(_newPos.x, bounds.min.x, bounds.max.x);
            _newPos.y = Mathf.Clamp(_newPos.y, bounds.min.y, bounds.max.y);

            _rectTransform.anchoredPosition = _newPos;
        }

        // Unified eating state check - use only one method
        bool isEating = _stateMachine?.CurrentState == MonsterState.Eating;

        if (isEating)
        {
            return;
        }

        bool isMovementState = _stateMachine?.CurrentState == MonsterState.Walking ||
                              _stateMachine?.CurrentState == MonsterState.Running ||
                              _stateMachine?.CurrentState == MonsterState.Flying;

        // Only find food once per frame for movement states
        if (isMovementState)
        {
            if (_foodHandler?.NearestFood == null)
            {
                _foodHandler?.FindNearestFood();
            }

            // Handle food logic without calling FindNearestFood again
            if (_foodHandler?.NearestFood != null)
            {
                _foodHandler?.HandleFoodLogic(ref _targetPosition);
            }

            // Apply separation behavior to target position
            _targetPosition = _separationBehavior.ApplySeparationToTarget(_targetPosition);
        }

        _movementHandler.UpdateMovement(ref _targetPosition, monsterData);

        // Add bounds checking after movement
        Vector2 currentPos = _rectTransform.anchoredPosition;
        if (!_movementBounds.IsWithinBounds(currentPos))
        {
            // Clamp position to bounds and set new random target
            var bounds = _movementBounds.CalculateMovementBounds();
            Vector2 clampedPos = new Vector2(
                Mathf.Clamp(currentPos.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(currentPos.y, bounds.min.y, bounds.max.y)
            );
            _rectTransform.anchoredPosition = clampedPos;
            SetRandomTarget();
        }

        // Check if monster moved significantly and request immediate depth sort
        Vector2 newPosition = _rectTransform.anchoredPosition;
        if (Vector2.Distance(newPosition, _lastSortPosition) >= _depthSortThreshold)
        {
            _lastSortPosition = newPosition;
            ServiceLocator.Get<GameManager>().SortMonstersByDepth();
        }

        bool isPursuingFood = _foodHandler?.NearestFood != null;
        float distanceToTarget = Vector2.Distance(_rectTransform.anchoredPosition, _targetPosition);
        if (distanceToTarget < 10f && !isPursuingFood && isMovementState)
        {
            SetRandomTarget();
        }
    }

    public void TriggerEating()
    {
        _stateMachine?.ForceState(MonsterState.Eating);
    }

    public void SetRandomTarget()
    {
        _targetPosition = _movementBounds?.GetRandomTarget() ?? Vector2.zero;
    }

    public void SetHovered(bool value)
    {
        if (_isHovered == value) return;
        _isHovered = value;
        OnHoverChanged?.Invoke(_isHovered);
    }

    public void DropCoinAfterPoke() => DropCoin(CoinType.Silver);

    public void UpdateVisuals() => _visualHandler?.UpdateMonsterVisuals();

    public void Poop(PoopType type = PoopType.Normal) => ServiceLocator.Get<GameManager>().SpawnPoopAt(_rectTransform.anchoredPosition, type);
    public void DropCoin(CoinType type) => ServiceLocator.Get<GameManager>().SpawnCoinAt(_rectTransform.anchoredPosition, type);

    public void GiveMedicine()
    {
        if (_statsHandler?.IsSick == true)
        {
            TreatSickness();
            // Could consume medicine item, cost coins, etc.
        }
    }

    public void OnPointerEnter(PointerEventData e) => _interactionHandler?.OnPointerEnter(e);
    public void OnPointerExit(PointerEventData e) => _interactionHandler?.OnPointerExit(e);
    public void OnPointerClick(PointerEventData eventData)
    {
        _interactionHandler?.OnPointerClick(eventData);
        _evolutionHandler?.OnInteraction();
    }

    public void SaveMonData() => _saveHandler?.SaveData();
    public void LoadMonData() => _saveHandler?.LoadData();
    public void SetMonsterData(MonsterDataSO newMonsterData)
    {
        if (newMonsterData == null) return;

        monsterData = newMonsterData;

        if (monsterID.StartsWith("temp_") || string.IsNullOrEmpty(monsterID))
        {
            monsterID = $"{monsterData.id}_Lv{evolutionLevel}_{System.Guid.NewGuid().ToString("N")[..8]}";
            gameObject.name = $"{monsterData.monsterName}_{monsterID}";
        }

        // Initialize food handler with new data
        if (_foodHandler != null)
        {
            _foodHandler.Initialize(monsterData);
        }

        if (_evolutionHandler != null)
        {
            _evolutionHandler.InitializeWithMonsterData();
        }

        // Apply visuals after all data is ready
        if (_visualHandler != null)
        {
            _visualHandler.ApplyMonsterVisuals();
        }

        // Load data last to ensure all handlers are ready
        _saveHandler?.LoadData();
    }

    public void ForceResetEating()
    {
        Debug.Log($"[MonsterController] Coordinated eating reset for {name}");
        
        // FIXED: Coordinate between food handler and state machine
        _foodHandler?.ForceResetEating();
        
        // Wait a frame to let food handler clean up, then reset state
        StartCoroutine(DelayedStateReset());
    }

    private IEnumerator DelayedStateReset()
    {
        yield return null; // Wait one frame
        
        if (_stateMachine?.CurrentState == MonsterState.Eating)
        {
            _stateMachine.ForceState(MonsterState.Idle);
        }
    }

    // Add public methods to access evolution handler
    public float GetEvolutionProgress() => _evolutionHandler?.GetEvolutionProgress() ?? 0f;

    // Add these getter methods for save handler to access evolution data
    public float GetEvolutionTimeSinceCreation()
    {
        float time = _evolutionHandler?.TimeSinceCreation ?? 0f;
        return time;
    }

    public int GetEvolutionFoodConsumed()
    {
        int food = _evolutionHandler?.FoodConsumed ?? 0;
        return food;
    }

    public int GetEvolutionInteractionCount()
    {
        int interactions = _evolutionHandler?.InteractionCount ?? 0;
        return interactions;
    }

    public void LoadEvolutionData(float timeSinceCreation, int foodConsumed, int interactionCount)
    {
        _evolutionHandler?.LoadEvolutionData(timeSinceCreation, foodConsumed, interactionCount);
    }

    public void SetHunger(float value) => _statsHandler?.SetHunger(value);
    public void SetHappiness(float value) => _statsHandler?.SetHappiness(value);
    public void SetSick(bool value) => _statsHandler?.SetSick(value);
    public float GetLowHungerTime() => _statsHandler?.LowHungerTime ?? 0f;
    public void SetLowHungerTime(float value) => _statsHandler?.SetLowHungerTime(value);
    public void IncreaseHappiness(float amount) => _statsHandler?.IncreaseHappiness(amount);
    public void TreatSickness() => _statsHandler?.TreatSickness();
    public void TriggerFoodConsumption() => _evolutionHandler?.OnFoodConsumed();
}