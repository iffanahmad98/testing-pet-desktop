using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Spine.Unity;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class MonsterController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("NPC Settings")]
    public bool isNPC = false;  // Flag to identify NPC monsters
    private NPCPetCaretakerHandler _npcHandler;
    private const float TARGET_CHANGE_COOLDOWN = 3f;

    #region Enums & Constants
    private enum InitializationState
    {
        NotStarted,
        ComponentsReady,
        HandlersCreated,
        DataLoaded,
        FullyInitialized
    }
    #endregion

    public float currentHappiness;
    public float currentHunger;
    public float currentHealth;  

    #region Fields & Properties
    // Initialization state tracking
    private InitializationState _initState = InitializationState.NotStarted;
    private bool _isInitializing = false;

    // Monster identification & basic data
    public string monsterID;
    public int evolutionLevel;
    [SerializeField] private MonsterDataSO monsterData;
    public MonsterDataSO MonsterData => monsterData;
    public string timeCreated { get; private set; }

    // Event handlers
    private Action<float> _hungerChangedHandler;
    private Action<float> _happinessChangedHandler;
    private Action<bool> _sickChangedHandler;
    private Action<float> _healthChangedHandler;
    private Action<bool> _hoverChangedHandler;

    // Stats & state properties
    public bool IsSick => _statsHandler?.IsSick ?? false;

    // Core events
    public event Action<float> OnHungerChanged;
    public event Action<bool> OnSickChanged;
    public event Action<float> OnHappinessChanged;
    public event Action<float> OnHealthChanged;
    public event Action<bool> OnHoverChanged;

    // Handler instances
    private MonsterSaveHandler _saveHandler;
    public MonsterSaveHandler SaveHandler => _saveHandler;
    private MonsterVisualHandler _visualHandler;
    private MonsterConsumableHandler _consumableHandler;
    public MonsterConsumableHandler ConsumableHandler => _consumableHandler;
    private MonsterInteractionHandler _interactionHandler;
    private MonsterBoundsHandler _boundHandler;
    public MonsterBoundsHandler BoundHandler => _boundHandler;
    private MonsterEvolutionHandler _evolutionHandler;
    public MonsterEvolutionHandler EvolutionHandler => _evolutionHandler;
    private MonsterSeparationHandler _separationBehavior;
    public MonsterSeparationHandler SeparationBehavior => _separationBehavior;
    private MonsterStateMachine _stateMachine;
    public MonsterStateMachine StateMachine => _stateMachine;
    private MonsterMovementHandler _movementHandler;
    private MonsterStatsHandler _statsHandler;
    public MonsterStatsHandler StatsHandler => _statsHandler;
    private MonsterCoroutineHandler _coroutineHandler;

    public MonsterUIHandler UI = new MonsterUIHandler();

    // Unity components
    private SkeletonGraphic _monsterSpineGraphic;
    private RectTransform _rectTransform;
    private MonsterManager _monsterManager;
    public MonsterManager MonsterManager => _monsterManager;

    // Movement related
    private Vector2 _targetPosition;
    private bool _isHovered;
    private Vector2 _lastSortPosition;
    private float _depthSortThreshold = 20f;
    private float _lastTargetChangeTime = 0f;

    // Safe property access with initialization checks
    public bool IsFullyInitialized => _initState == InitializationState.FullyInitialized;
    #endregion

    #region Initialization
    private void Awake()
    {
        if (_isInitializing) return;
        _isInitializing = true;

        InitializeID();
        InitializeComponents();
        _initState = InitializationState.ComponentsReady;
    }

    private void InitializeID()
    {
        if (string.IsNullOrEmpty(monsterID))
        {
            monsterID = $"{Guid.NewGuid().ToString("N")[..8]}";
        }
        gameObject.name = $"Monster_{monsterID}";
    }

    private void InitializeComponents()
    {
        _monsterManager = ServiceLocator.Get<MonsterManager>();
        _rectTransform = GetComponent<RectTransform>();
        _stateMachine = GetComponent<MonsterStateMachine>();
        _monsterSpineGraphic = GetComponentInChildren<SkeletonGraphic>();
        _monsterSpineGraphic?.Initialize(true);

        if (_monsterSpineGraphic == null)
        {
            Debug.LogError($"[MonsterController] No SkeletonGraphic found for {gameObject.name}!");
        }
        if (_rectTransform == null)
        {
            Debug.LogError($"[MonsterController] No RectTransform found on {gameObject.name}");
        }
        if (_stateMachine == null)
        {
            Debug.LogError($"[MonsterController] No MonsterStateMachine found on {gameObject.name}");
        }
        if (_monsterManager == null)
        {
            Debug.LogError($"[MonsterController] No MonsterManager found in ServiceLocator!");
        }
    }

    private void Start()
    {
        if (_initState != InitializationState.ComponentsReady) return;

        // Create handlers in dependency order
        CreateHandlersInOrder();
        _initState = InitializationState.HandlersCreated;

        // Wait for external dependencies, then continue
        StartCoroutine(ContinueInitializationWhenReady());

        // If this is an NPC, initialize the caretaker handler
        if (isNPC)
        {
            _npcHandler = new NPCPetCaretakerHandler(this);
        }
    }

    private void CreateHandlersInOrder()
    {
        // 1. Create core handlers first (no dependencies)
        _statsHandler = new MonsterStatsHandler(this);

        // 2. Create handlers that depend on core handlers
        _coroutineHandler = new MonsterCoroutineHandler(this);
        _saveHandler = new MonsterSaveHandler(this);
        _evolutionHandler = new MonsterEvolutionHandler(this, _monsterSpineGraphic);

        // 3. Create handlers that depend on the state machine
        _visualHandler = new MonsterVisualHandler(this, _monsterSpineGraphic);
        _interactionHandler = new MonsterInteractionHandler(this, _stateMachine);
        _boundHandler = new MonsterBoundsHandler(_monsterManager, _rectTransform);
        _movementHandler = new MonsterMovementHandler(this, _rectTransform, _monsterSpineGraphic);
        _separationBehavior = new MonsterSeparationHandler(this, _rectTransform);
        _consumableHandler = new MonsterConsumableHandler(this, _rectTransform);

        UI.Initialize(_statsHandler, this);
    }

    private IEnumerator ContinueInitializationWhenReady()
    {
        yield return StartCoroutine(FinalizeInitialization());
    }

    private IEnumerator FinalizeInitialization()
    {
        // Add this: Load saved data before starting coroutines
        if (!isNPC && MonsterData != null)
        {
            SubscribeToEvents();
            LoadMonData(); // This will call _saveHandler?.LoadData()
            _coroutineHandler?.StartAllCoroutines();
        }
        
        _initState = InitializationState.FullyInitialized;
        yield break;
    }
    #endregion

    #region Lifecycle Methods
    private void Update()
    {
        // Skip stat-based updates for NPCs
        if (!isNPC)
        {
            // KEEP: Only UI updates
            UI.UpdateHungerDisplay(StatsHandler.CurrentHunger, _isHovered);
            UI.UpdateHappinessDisplay(StatsHandler.CurrentHappiness, _isHovered);
            UI.UpdateHealthDisplay(StatsHandler.CurrentHP);

            // Update display fields (these are just for display/debugging)
            currentHappiness = StatsHandler.CurrentHappiness;
            currentHealth = StatsHandler.CurrentHP;
            currentHunger = StatsHandler.CurrentHunger;
        }

        // ADD: Skip all updates during evolution except evolution tracking
        if (_evolutionHandler?.IsEvolving == true)
        {
            _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
            return; // Skip movement, interactions, etc.
        }

        _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
        _interactionHandler?.UpdateTimers(Time.deltaTime);
        _interactionHandler?.UpdateOutsideInteraction();

        HandleMovement();

        // Update emoji visibility if hovering
        if (_isHovered)
        {
            if (isNPC) return;
            UI.UpdateEmojiVisibility(IsSick);
        }
    }
    private void OnEnable()
    {
        if (IsFullyInitialized)
        {
            SubscribeToEvents();
            _coroutineHandler?.StartAllCoroutines();
        }
    }
    private void OnDisable()
    {
        UnsubscribeFromEvents();
        _coroutineHandler?.StopAllCoroutines();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        _coroutineHandler?.StopAllCoroutines();
    }
    #endregion

    #region Event Handling
    private void SubscribeToEvents()
    {
        if (_statsHandler == null) return;

        _hungerChangedHandler = (hunger) =>
        {
            UI.UpdateHungerDisplay(hunger, _isHovered);
            OnHungerChanged?.Invoke(hunger); // Forward to external subscribers
        };

        _happinessChangedHandler = (happiness) =>
        {
            UI.UpdateHappinessDisplay(happiness, _isHovered);
            OnHappinessChanged?.Invoke(happiness); // Forward to external subscribers
        };

        _sickChangedHandler = (isSick) =>
        {
            UI.UpdateSickStatusDisplay(_statsHandler?.IsSick == true, _isHovered);
            OnSickChanged?.Invoke(isSick); // Forward to external subscribers
        };

        _healthChangedHandler = (health) =>
        {
            UI.UpdateHealthDisplay(health);
            OnHealthChanged?.Invoke(health); // Forward to external subscribers
        };

        _hoverChangedHandler = (hovered) =>
        {
            UI.UpdateHungerDisplay(StatsHandler.CurrentHunger, hovered);
            UI.UpdateHappinessDisplay(StatsHandler.CurrentHappiness, hovered);
            UI.UpdateHealthDisplay(StatsHandler.CurrentHP);
            UI.UpdateSickStatusDisplay(StatsHandler.IsSick, hovered);
        };

        _statsHandler.OnHungerChanged += _hungerChangedHandler;
        _statsHandler.OnHappinessChanged += _happinessChangedHandler;
        _statsHandler.OnSickChanged += _sickChangedHandler;
        _statsHandler.OnHealthChanged += _healthChangedHandler;

        OnHoverChanged += _hoverChangedHandler;
    }

    private void UnsubscribeFromEvents()
    {
        if (_statsHandler != null)
        {
            if (_hungerChangedHandler != null)
                _statsHandler.OnHungerChanged -= _hungerChangedHandler;

            if (_happinessChangedHandler != null)
                _statsHandler.OnHappinessChanged -= _happinessChangedHandler;

            if (_sickChangedHandler != null)
                _statsHandler.OnSickChanged -= _sickChangedHandler;

            if (_healthChangedHandler != null)
                _statsHandler.OnHealthChanged -= _healthChangedHandler;
        }

        if (_hoverChangedHandler != null)
            OnHoverChanged -= _hoverChangedHandler;
    }
    #endregion

    #region Movement & Positioning
    private void HandleMovement()
    {
        if (monsterData == null) return;
        if (_evolutionHandler != null && _evolutionHandler.IsEvolving) return;
        // if (isNPC) return; 

        // NEW: Check if we should use relaxed bounds for very small areas, Only update movement every 3rd frame for tiny areas
        bool useRelaxedBounds = _boundHandler?.IsMovementAreaTooSmall() ?? false;
        Vector2 separationForce = _separationBehavior.CalculateSeparationForce();
        // NEW: For very small areas, drastically reduce movement updates
        // NEW: Reduce separation force for small areas
        if (useRelaxedBounds)
        {
            if (Time.frameCount % 3 != 0) return;
            separationForce *= 0.1f; // Much weaker separation
        }

        if (separationForce.magnitude > 0.1f)
        {
            Vector2 _pos = _rectTransform.anchoredPosition;
            Vector2 newPos = _pos + separationForce * Time.deltaTime;

            if (!useRelaxedBounds)
            {
                MonsterState _state = _stateMachine?.CurrentState ?? MonsterState.Idle;
                var bounds = _boundHandler.CalculateBoundsForState(_state);
                newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
                newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
            }
            else
            {
                // For small areas, only prevent moving completely outside game area
                var gameAreaSize = _monsterManager.gameArea.sizeDelta;
                float padding = 20f;
                newPos.x = Mathf.Clamp(newPos.x, -gameAreaSize.x / 2 + padding, gameAreaSize.x / 2 - padding);
                newPos.y = Mathf.Clamp(newPos.y, -gameAreaSize.y / 2 + padding, gameAreaSize.y / 2 - padding);
            }

            _rectTransform.anchoredPosition = newPos;
        }

        bool isEating = _consumableHandler?.IsCurrentlyConsuming ?? false;
        if (isEating) return;

        bool isMovementState = _stateMachine?.CurrentState == MonsterState.Walking ||
                              _stateMachine?.CurrentState == MonsterState.Running ||
                              _stateMachine?.CurrentState == MonsterState.Flying;

        if (isMovementState)
        {
            bool isGroundMovement = _stateMachine?.CurrentState != MonsterState.Flying;

            if (isGroundMovement && _consumableHandler?.NearestConsumable == null)
            {
                _consumableHandler?.FindNearestConsumable();
            }

            // Handle food logic only for ground movement
            if (isGroundMovement && _consumableHandler?.NearestConsumable != null)
            {
                _consumableHandler?.HandleConsumableLogic(ref _targetPosition);
            }

            _targetPosition = _separationBehavior.ApplySeparationToTarget(_targetPosition);
        }

        _movementHandler.UpdateMovement(ref _targetPosition, monsterData);

        // Enhanced bounds checking - only for normal sized areas
        Vector2 currentPos = _rectTransform.anchoredPosition;
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;

        if (!useRelaxedBounds && !_boundHandler.IsWithinBoundsForState(currentPos, currentState))
        {
            // Clamp position
            var bounds = _boundHandler.CalculateBoundsForState(currentState);
            Vector2 clampedPos = new Vector2(
                Mathf.Clamp(currentPos.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(currentPos.y, bounds.min.y, bounds.max.y)
            );
            _rectTransform.anchoredPosition = clampedPos;

            // NEW: For very small movement areas, don't set new targets at all
            bool areaIsTiny = (bounds.max.y - bounds.min.y) < 50f; // Less than 50 pixels height
            if (!areaIsTiny && Time.time - _lastTargetChangeTime > TARGET_CHANGE_COOLDOWN)
            {
                SetRandomTargetForCurrentState();
                _lastTargetChangeTime = Time.time;
            }
        }
        else if (useRelaxedBounds)
        {
            // For small areas, only check if completely outside game area
            var gameAreaSize = _monsterManager.gameArea.sizeDelta;
            float padding = 20f;
            bool outsideGameArea = currentPos.x < -gameAreaSize.x / 2 + padding ||
                                  currentPos.x > gameAreaSize.x / 2 - padding ||
                                  currentPos.y < -gameAreaSize.y / 2 + padding ||
                                  currentPos.y > gameAreaSize.y / 2 - padding;

            if (outsideGameArea)
            {
                Vector2 clampedPos = new Vector2(
                    Mathf.Clamp(currentPos.x, -gameAreaSize.x / 2 + padding, gameAreaSize.x / 2 - padding),
                    Mathf.Clamp(currentPos.y, -gameAreaSize.y / 2 + padding, gameAreaSize.y / 2 - padding)
                );
                _rectTransform.anchoredPosition = clampedPos;

                // NEW: Much longer cooldown for relaxed bounds
                if (Time.time - _lastTargetChangeTime > TARGET_CHANGE_COOLDOWN * 3f)
                {
                    SetRandomTargetForCurrentState();
                    _lastTargetChangeTime = Time.time;
                }
            }
        }

        // Check if monster moved significantly and request immediate depth sort
        Vector2 newPosition = _rectTransform.anchoredPosition;
        if (Vector2.Distance(newPosition, _lastSortPosition) >= _depthSortThreshold)
        {
            _lastSortPosition = newPosition;
            ServiceLocator.Get<MonsterManager>().SortMonstersByDepth();
        }

        bool isPursuingFood = _consumableHandler?.NearestConsumable != null;
        float distanceToTarget = Vector2.Distance(_rectTransform.anchoredPosition, _targetPosition);
        if (distanceToTarget < 10f && !isPursuingFood && isMovementState)
        {
            // NEW: Longer cooldown for reaching targets too
            float targetReachCooldown = useRelaxedBounds ? TARGET_CHANGE_COOLDOWN * 2f : TARGET_CHANGE_COOLDOWN;
            if (Time.time - _lastTargetChangeTime > targetReachCooldown)
            {
                SetRandomTargetForCurrentState();
                _lastTargetChangeTime = Time.time;
            }
        }
    }

    private void SetRandomTargetForCurrentState()
    {
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;
        _targetPosition = _boundHandler?.GetRandomTargetForState(currentState) ?? Vector2.zero;
    }

    public void SetRandomTarget()
    {
        // Don't change target too frequently
        if (Time.time - _lastTargetChangeTime < TARGET_CHANGE_COOLDOWN)
            return;

        _targetPosition = BoundHandler.GetRandomTargetForState(StateMachine.CurrentState);
        _lastTargetChangeTime = Time.time;

        SetRandomTargetForCurrentState();
    }

    public Vector2 GetTargetPosition() => _targetPosition;
    
    public void SetTargetPosition(Vector2 position)
    {
        _targetPosition = position;
        _lastTargetChangeTime = Time.time; // Reset cooldown
    }

    private Vector2 GetRandomPositionAroundMonster()
    {
        Vector2 basePos = _rectTransform.anchoredPosition;

        // Create a safe drop position below the monster
        Vector2 dropPosition = new Vector2(
            basePos.x + UnityEngine.Random.Range(-50f, 50f),
            basePos.y - 60f // Drop below monster
        );

        // Ensure it stays within game bounds if possible
        if (_monsterManager?.gameArea != null)
        {
            var gameAreaSize = _monsterManager.gameArea.sizeDelta;
            float padding = 30f;

            dropPosition.x = Mathf.Clamp(dropPosition.x,
                -gameAreaSize.x / 2 + padding,
                gameAreaSize.x / 2 - padding);
            dropPosition.y = Mathf.Max(dropPosition.y,
                -gameAreaSize.y / 2 + padding);
        }

        return dropPosition;
    }
    #endregion

    #region Monster Data Management
    public void SetMonsterData(MonsterDataSO newMonsterData)
    {
        if (newMonsterData == null) return;

        monsterData = newMonsterData;
        evolutionLevel = evolutionLevel == 0 ? monsterData.evolutionLevel : evolutionLevel;

        // Update ID if needed (now uses correct evolution level)
        if (monsterID.StartsWith("temp_") || string.IsNullOrEmpty(monsterID))
        {
            monsterID = $"{monsterData.id}_Lv{evolutionLevel}_{System.Guid.NewGuid().ToString("N")[..8]}";
            gameObject.name = $"{monsterData.monsterName}_{monsterID}";
            timeCreated = DateTime.UtcNow.ToString("o"); // ISO 8601
        }

        // // Load data only after handlers are ready
        // if (_initState >= InitializationState.DataLoaded)
        // {
        //     _saveHandler?.LoadData(monsterData.GetMaxHealth(evolutionLevel));
        // }
    }

    public void SaveMonData() => _saveHandler?.SaveData();
    public void LoadMonData()
    {
        if (!isNPC)
        {
            // Load stats for pet monsters
            _saveHandler?.LoadData();
        }
        else
        {
            // NPCs just need basic initialization
        }
    }
    #endregion

    #region Evolution Functionality
    public Sprite GetEvolutionIcon(MonsterIconType iconType = MonsterIconType.Card)
    {
        if (monsterData == null)
        {
            Debug.LogWarning($"[MonsterController] {monsterID} has no monster data assigned.");
            return null;
        }
        return monsterData.GetEvolutionIcon(evolutionLevel, iconType);
    }
    public int GetCurrentSellPrice() => monsterData?.GetSellPrice(evolutionLevel) ?? 0;
    public void CheckEvolveAfterInteraction() => _evolutionHandler?.OnInteraction();
    public float GetEvolveProgress() => _evolutionHandler?.GetEvolutionProgress() ?? 0f;
    public float GetEvolveTimeSinceCreation() => _evolutionHandler?.TimeSinceCreation ?? 0f;
    public string GetEvolveTimeCreated() => _evolutionHandler?.TimeCreated ?? DateTime.UtcNow.ToString("o"); // ISO 8601 format
    public int GetEvolveNutritionConsumed() => _evolutionHandler?.NutritionConsumed ?? 0;
    public int GetEvolutionInteractionCount() => _evolutionHandler?.InteractionCount ?? 0;
    public void LoadEvolutionData(float timeSinceCreation, string timeCreated, int foodConsumed, int interactionCount)
    {
        _evolutionHandler?.LoadEvolutionData(timeSinceCreation, timeCreated, foodConsumed, interactionCount);
    }
    #endregion

    #region Stats Management
    public void SetHunger(float value) => _statsHandler?.SetHunger(value);
    public void SetHappiness(float value) => _statsHandler?.SetHappiness(value);
    public void IncreaseHappiness(float amount) => _statsHandler?.IncreaseHappiness(amount);
    public void GiveMedicine(float healingValue) => _statsHandler?.Heal(healingValue);

    #endregion

    #region Interaction Handling
    public void SetHovered(bool value)
    {
        if (_isHovered == value) return;
        if (EvolutionHandler.IsEvolving)
        {
            _isHovered = false;
            return;
        }
        else
        {
            _isHovered = value;
        }
        OnHoverChanged?.Invoke(_isHovered);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
        OnHoverChanged?.Invoke(_isHovered);
        UI.OnHoverEnter();
        _interactionHandler?.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
        OnHoverChanged?.Invoke(_isHovered);
        UI.OnHoverExit();
        _interactionHandler?.OnPointerExit(eventData);
    }

    public void OnPointerClick(PointerEventData eventData) => _interactionHandler?.OnPointerClick(eventData);
    #endregion

    #region Visual Effects
    public void UpdateVisuals() => _visualHandler?.ApplyMonsterVisuals();
    public void DropPoop(PoopType type = PoopType.Normal) => _visualHandler?.SpawnPoopWithAnimation(type);
    public void DropCoin(CoinType type)
    {
        Vector2 launchPosition = _visualHandler?.GetCoinLaunchPosition() ?? _rectTransform.anchoredPosition;
        Vector2 targetPosition = _visualHandler?.GetRandomPositionOutsideBounds() ?? GetRandomPositionAroundMonster();
        ServiceLocator.Get<MonsterManager>().SpawnCoinWithArc(launchPosition, targetPosition, type);
    }
    public Sprite GetMonsterIcon() => _visualHandler?.GetMonsterIcon();
    #endregion
}