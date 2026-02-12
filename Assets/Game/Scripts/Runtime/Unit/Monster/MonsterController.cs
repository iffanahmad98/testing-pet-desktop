using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Spine.Unity;
using System.Collections;

public class MonsterController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    ITargetable
{
    [Header("NPC Settings")] public bool isNPC = false; // Flag to identify NPC monsters
    private NPCPetCaretakerHandler _npcHandler;
    public NPCPetCaretakerHandler NPC => _npcHandler;

    [Header("Monster Stats")]
    // These fields are just for display and debugging purposes
    public float currentHappiness;

    public float currentHunger;
    public float currentHealth;
    public int currentGameAreaIndex;

    [Header("Evolution Progress (Debug View)")]
    [Space(5)]
    [Header("Current Progress")]
    [SerializeField] private float _currentTimeSinceCreation;
    [SerializeField] private int _currentNutritionConsumed;
    [SerializeField] private int _currentInteractionCount;
    [SerializeField] private float _currentHappiness;
    [SerializeField] private float _currentHunger;

    [Space(5)]
    [Header("Target Requirements")]
    [SerializeField] private float _targetTimeAlive;
    [SerializeField] private int _targetNutrition;
    [SerializeField] private int _targetInteractions;
    [SerializeField] private float _targetHappiness;
    [SerializeField] private float _targetHunger;

    [Space(5)]
    [Header("Overall Progress")]
    [SerializeField] private float _evolutionProgressPercent;

    [Space(5)]
    [Header("Eating Components")]
    [SerializeField] private Transform eatingPos;

    [Space(5)]
    #region Fields & Properties

    // Monster identification & basic data
    public string monsterID;
    public int evolutionLevel;
    [SerializeField] private MonsterDataSO monsterData;
    public MonsterDataSO MonsterData => monsterData;
    public Transform EatingPos => eatingPos;
    public string timeCreated { get; private set; }
    public string lastPokedTime { get; private set; }

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
    public MonsterInteractionHandler InteractionHandler => _interactionHandler;
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

    public event Action<PointerEventData> OnClicked;

    // Unity components
    private SkeletonGraphic _monsterSpineGraphic;
    public SkeletonGraphic _beforeMonsterSpineGraphic;
    public SkeletonGraphic _afterMonsterSpineGraphic;
    private RectTransform _rectTransform;
    private MonsterManager _monsterManager;
    public MonsterManager MonsterManager => _monsterManager;

    // NPC reservation system to prevent multiple NPCs targeting the same monster for feeding
    public MonsterController ReservedByNPC { get; private set; }

    public bool IsTargetable => gameObject.activeInHierarchy && !isNPC && ReservedByNPC == null;
    public Vector2 Position => _rectTransform.anchoredPosition;

    // Movement related
    private Vector2 _targetPosition;
    private bool _isHovered;
    private Vector2 _lastSortPosition;
    private float _depthSortThreshold = 20f;
    private float _lastTargetChangeTime = 0f;
    private const float TARGET_CHANGE_COOLDOWN = 3f;
    private bool _movementFrozenByTutorial = false;
    private bool _interactionsDisabledByTutorial = false;

    public bool IsMovementFrozenByTutorial => _movementFrozenByTutorial;
    public bool InteractionsDisabledByTutorial => _interactionsDisabledByTutorial;

    #endregion

    #region Initialization

    private void Awake()
    {
        InitializeComponents();
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
       // Debug.Log ("Initialize Monster 0");
        if (isNPC)
        {
            _stateMachine = GetComponent<MonsterStateMachine>();
            _rectTransform = GetComponent<RectTransform>();
            _monsterSpineGraphic = GetComponentInChildren<SkeletonGraphic>();
            _monsterSpineGraphic?.Initialize(true);

            _boundHandler = new MonsterBoundsHandler(_monsterManager, _rectTransform);
            _separationBehavior = new MonsterSeparationHandler(this, _rectTransform);
            _movementHandler = new MonsterMovementHandler(this, _rectTransform, _monsterSpineGraphic);
            _visualHandler = new MonsterVisualHandler(this, _monsterSpineGraphic);

            var npcIndex = gameObject.name == "NPCMonsterData 1_100" ? 1 : 0;

            Debug.Log($"NPC DEBUG {npcIndex}");
            _npcHandler = new NPCPetCaretakerHandler(this, npcIndex);
            _npcHandler.Initialize();

            UI.Initialize(_statsHandler, this);
            return;
        }

        UpdateEatingOffset(evolutionLevel);

        CreateHandlers();
        StartCoroutine(FinalizeInitialization());
    }

    private void CreateHandlers()
    {
        // 1. Create core handlers first (no dependencies)
        _statsHandler = new MonsterStatsHandler(this);

        // 2. Create handlers that depend on core handlers
        _coroutineHandler = new MonsterCoroutineHandler(this);
        _saveHandler = new MonsterSaveHandler(this);
        _evolutionHandler = new MonsterEvolutionHandler(this, _monsterSpineGraphic, _beforeMonsterSpineGraphic,
            _afterMonsterSpineGraphic);

        // 3. Create handlers that depend on the state machine
        _visualHandler = new MonsterVisualHandler(this, _monsterSpineGraphic);
        _interactionHandler = new MonsterInteractionHandler(this, _stateMachine);
        _boundHandler = new MonsterBoundsHandler(_monsterManager, _rectTransform);
        _movementHandler = new MonsterMovementHandler(this, _rectTransform, _monsterSpineGraphic);
        _separationBehavior = new MonsterSeparationHandler(this, _rectTransform);
        _consumableHandler = new MonsterConsumableHandler(this, _rectTransform);

        UI.Initialize(_statsHandler, this);
        // Debug.Log ("Initialize Monster 1");
    }

    private IEnumerator FinalizeInitialization()
    {
        yield return new WaitUntil(() => SaveSystem.IsLoadFinished);

        // Add this: Load saved data before starting coroutines
        if (MonsterData != null)
        {
            SubscribeToEvents();
            LoadMonData(); // This will call _saveHandler?.LoadData()
            _coroutineHandler?.StartAllCoroutines();
        }

        // _initState = InitializationState.FullyInitialized;

        currentGameAreaIndex = _monsterManager.currentGameAreaIndex;
        yield break;
    }

    #endregion

    #region Lifecycle Methods

    private void Update()
    {
        // ADD: Skip all updates during evolution except evolution tracking
        if (_evolutionHandler?.IsEvolving == true)
        {
            _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
            return; // Skip movement, interactions, etc.
        }

        if (!isNPC)
        {
            UI.UpdateHungerDisplay(StatsHandler.CurrentHunger, _isHovered);
            UI.UpdateHappinessDisplay(StatsHandler.CurrentHappiness, _isHovered);
            UI.UpdateHealthDisplay(StatsHandler.CurrentHP);
            // Update display fields (these are just for display/debugging)
            currentHappiness = StatsHandler.CurrentHappiness;
            currentHunger = StatsHandler.CurrentHunger;
            currentHealth = StatsHandler.CurrentHP;

            if (_isHovered)
            {
                UI.UpdateEmojiVisibility(IsSick);
            }

            _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
            _interactionHandler?.UpdateTimers(Time.deltaTime);

            // Update evolution progress display fields
            UpdateEvolutionProgressDisplay();
            _interactionHandler?.UpdateOutsideInteraction();
        }

        HandleMovement();
    }

    public void SetMovementFrozenByTutorial(bool frozen)
    {
        _movementFrozenByTutorial = frozen;

        if (frozen)
        {
            // Paksa ke Idle supaya animasi jalan/terbang berhenti.
            _stateMachine?.ChangeState(MonsterState.Idle);
            SetFallingStarsState(false);
        }
    }

    public void SetInteractionsDisabledByTutorial(bool disabled)
    {
        _interactionsDisabledByTutorial = disabled;
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        _coroutineHandler?.StartAllCoroutines();
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
        if (_movementFrozenByTutorial)
        {
            // Saat dibekukan oleh tutorial, jangan proses movement sama sekali.
            SetFallingStarsState(false);
            return;
        }

        // Early return for NPCs - they should use simplified movement
        if (isNPC)
        {
            HandleNPCMovement();
            return;
        }

        // Early return if eating
        bool isEating = _consumableHandler?.IsCurrentlyConsuming ?? false;
        if (isEating) return;

        // Check movement state early
        bool isMovementState = _stateMachine?.CurrentState == MonsterState.Walking ||
                               _stateMachine?.CurrentState == MonsterState.Running ||
                               _stateMachine?.CurrentState == MonsterState.Flying;

        SetFallingStarsState(isMovementState);

        if (!isMovementState) return;

        // Bounds and separation logic
        bool useRelaxedBounds = _boundHandler?.IsMovementAreaTooSmall() ?? false;

        // Handle separation
        HandleSeparationLogic(useRelaxedBounds);

        // Handle consumable logic
        HandleConsumableLogic();

        // Update movement
        _movementHandler?.UpdateMovement(ref _targetPosition, monsterData);

        // Handle bounds checking
        HandleBoundsChecking(useRelaxedBounds);

        // Handle target reaching
        HandleTargetReaching(useRelaxedBounds);

        // Handle depth sorting
        HandleDepthSorting();
    }

    private void HandleNPCMovement()
    {
        // Simplified movement for NPCs
        if (_movementHandler != null && _npcHandler.OnMoveAction)
        {
            // Remove the automatic random target setting when not on action
            // The NPCPetCaretakerHandler will handle setting the target position
            _movementHandler.UpdateMovement(ref _targetPosition, monsterData);
        }

        // ADD: Handle separation for NPCs (simplified version)
        if (_separationBehavior != null)
        {
            Vector2 separationForce = _separationBehavior.CalculateSeparationForce();
            if (separationForce.magnitude > 0.1f)
            {
                Vector2 currentPos = _rectTransform.anchoredPosition;
                // Vector2 newPos = currentPos + separationForce * Time.deltaTime * 0.1f; // Reduced force for NPCs (Latest)
                Vector2 newPos = currentPos + separationForce * Time.deltaTime * 0.1f; // Reduced force for NPCs

                // Apply basic bounds
                var gameAreaSize = _monsterManager.gameAreaRT.sizeDelta;
                float padding = 20f;
                newPos.x = Mathf.Clamp(newPos.x, -gameAreaSize.x / 2 + padding, gameAreaSize.x / 2 - padding);
                newPos.y = Mathf.Clamp(newPos.y, -gameAreaSize.y / 2 + padding, gameAreaSize.y / 2 - padding);

                _rectTransform.anchoredPosition = newPos;
            }
        }

        // Only apply bounds if NPC is not actively pursuing a target
        if (!_npcHandler.OnAction)
        {
            // Basic bounds for NPCs
            Vector2 currentPos = _rectTransform.anchoredPosition;
            var gameAreaSize = _monsterManager.gameAreaRT.sizeDelta;
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
            }
        }

        HandleDepthSorting();
    }

    private void HandleSeparationLogic(bool useRelaxedBounds)
    {
        if (_separationBehavior == null) return;

        Vector2 separationForce = _separationBehavior.CalculateSeparationForce();

        if (useRelaxedBounds)
        {
            if (Time.frameCount % 3 != 0) return;
            separationForce *= 0.1f;
        }

        if (separationForce.magnitude > 0.1f)
        {
            Vector2 currentPos = _rectTransform.anchoredPosition;
            Vector2 newPos = currentPos + separationForce * Time.deltaTime;

            // Apply bounds to separation movement
            if (!useRelaxedBounds && _boundHandler != null)
            {
                MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;
                var bounds = _boundHandler.CalculateBoundsForState(currentState);
                newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
                newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
            }
            else if (useRelaxedBounds)
            {
                var gameAreaSize = _monsterManager.gameAreaRT.sizeDelta;
                float padding = 20f;
                newPos.x = Mathf.Clamp(newPos.x, -gameAreaSize.x / 2 + padding, gameAreaSize.x / 2 - padding);
                newPos.y = Mathf.Clamp(newPos.y, -gameAreaSize.y / 2 + padding, gameAreaSize.y / 2 - padding);
            }

            _rectTransform.anchoredPosition = newPos;
        }

        // REMOVED: Don't constantly override target with separation
        // The separation force already pushes the monster position directly above
    }

    private void HandleConsumableLogic()
    {
        bool isGroundMovement = _stateMachine?.CurrentState != MonsterState.Flying;

        if (isGroundMovement && _consumableHandler != null)
        {
            if (_consumableHandler.NearestConsumable == null)
            {
                _consumableHandler.FindNearestConsumable();
            }

            if (_consumableHandler.NearestConsumable != null)
            {
                _consumableHandler.HandleConsumableLogic(ref _targetPosition);
            }
        }
    }

    private void HandleBoundsChecking(bool useRelaxedBounds)
    {
        Vector2 currentPos = _rectTransform.anchoredPosition;
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;

        if (!useRelaxedBounds && _boundHandler != null &&
            !_boundHandler.IsWithinBoundsForState(currentPos, currentState))
        {
            var bounds = _boundHandler.CalculateBoundsForState(currentState);
            Vector2 clampedPos = new Vector2(
                Mathf.Clamp(currentPos.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(currentPos.y, bounds.min.y, bounds.max.y)
            );
            _rectTransform.anchoredPosition = clampedPos;

            bool areaIsTiny = (bounds.max.y - bounds.min.y) < 50f;
            if (!areaIsTiny && Time.time - _lastTargetChangeTime > TARGET_CHANGE_COOLDOWN)
            {
                SetRandomTargetForCurrentState();
                _lastTargetChangeTime = Time.time;
            }
        }
        else if (useRelaxedBounds)
        {
            // Handle relaxed bounds logic...
            var gameAreaSize = _monsterManager.gameAreaRT.sizeDelta;
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

                if (Time.time - _lastTargetChangeTime > TARGET_CHANGE_COOLDOWN * 3f)
                {
                    SetRandomTargetForCurrentState();
                    _lastTargetChangeTime = Time.time;
                }
            }
        }
    }

    private void HandleTargetReaching(bool useRelaxedBounds)
    {
        bool isPursuingFood = _consumableHandler?.NearestConsumable != null;

        // Check if using horizontal-only movement (small game area height)
        bool isHorizontalOnly = false;
        var gameAreaRect = _monsterManager.gameAreaRT;
        if (gameAreaRect != null)
        {
            float currentHeight = gameAreaRect.sizeDelta.y;
            float maxHeight = _monsterManager.GetMaxGameAreaHeight();
            isHorizontalOnly = currentHeight <= maxHeight / 2f;
        }

        // For horizontal-only movement, only check X distance
        float distanceToTarget = isHorizontalOnly
            ? Mathf.Abs(_rectTransform.anchoredPosition.x - _targetPosition.x)
            : Vector2.Distance(_rectTransform.anchoredPosition, _targetPosition);

        if (distanceToTarget < 10f && !isPursuingFood)
        {
            float targetReachCooldown = useRelaxedBounds ? TARGET_CHANGE_COOLDOWN * 2f : TARGET_CHANGE_COOLDOWN;
            if (Time.time - _lastTargetChangeTime > targetReachCooldown)
            {
                SetRandomTargetForCurrentState();
                _lastTargetChangeTime = Time.time;
            }
        }
    }

    private void HandleDepthSorting()
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;
        if (Vector2.Distance(newPosition, _lastSortPosition) >= _depthSortThreshold)
        {
            _lastSortPosition = newPosition;
            _monsterManager?.SortMonstersByDepth();
        }
    }

    private void SetRandomTargetForCurrentState()
    {
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;
        Vector2 newTarget = _boundHandler?.GetRandomTargetForState(currentState) ?? Vector2.zero;

        // Validate that the new target is within bounds before setting it
        if (_boundHandler != null && !isNPC)
        {
            if (_boundHandler.IsWithinBoundsForState(newTarget, currentState))
            {
                _targetPosition = newTarget;
            }
            else
            {
                // If generated target is invalid, try once more
                newTarget = _boundHandler.GetRandomTargetForState(currentState);
                if (_boundHandler.IsWithinBoundsForState(newTarget, currentState))
                {
                    _targetPosition = newTarget;
                }
                // else keep current target
            }
        }
        else
        {
            _targetPosition = newTarget;
        }
    }

    public void UpdateEatingOffset(int evolutionLevel)
    {
        if (monsterData.eatingOffset.Length <= evolutionLevel)
            return;

        // Update Anchor of Eating Position
        var eatingOffset = monsterData.eatingOffset[evolutionLevel];

        if (eatingOffset != null)
            eatingPos.GetComponent<RectTransform>().anchoredPosition = new Vector2(eatingOffset.x, eatingOffset.y);
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
            gameObject.name = $"{monsterData.name}_{monsterID}";
            timeCreated = DateTime.UtcNow.ToString("o"); // ISO 8601
        }
    }

    public void SetLastTimePokedTimer(string timer)
    {
        lastPokedTime = timer;
    }

    public void LoadMonData() => _saveHandler?.LoadData();

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

    public string GetEvolveTimeCreated() =>
        _evolutionHandler?.TimeCreated ?? DateTime.UtcNow.ToString("o"); // ISO 8601 format

    public int GetEvolveNutritionConsumed() => _evolutionHandler?.NutritionConsumed ?? 0;
    public int GetEvolutionInteractionCount() => _evolutionHandler?.InteractionCount ?? 0;

    public void
        LoadEvolutionData(float timeSinceCreation, string timeCreated, int foodConsumed, int interactionCount) =>
        _evolutionHandler?.LoadEvolutionData(timeSinceCreation, timeCreated, foodConsumed, interactionCount);

    private void UpdateEvolutionProgressDisplay()
    {
        if (_evolutionHandler == null || monsterData == null || !monsterData.canEvolve) return;

        // Update current progress values
        _currentTimeSinceCreation = _evolutionHandler.TimeSinceCreation;
        _currentNutritionConsumed = _evolutionHandler.NutritionConsumed;
        _currentInteractionCount = _evolutionHandler.InteractionCount;
        _currentHappiness = _statsHandler?.CurrentHappiness ?? 0f;
        _currentHunger = _statsHandler?.CurrentHunger ?? 0f;

        // Get next evolution requirement
        var nextEvolutionReq = GetNextEvolutionRequirement();
        if (nextEvolutionReq != null)
        {
            _targetTimeAlive = nextEvolutionReq.minTimeAlive;
            _targetNutrition = nextEvolutionReq.minFoodConsumed;
            _targetInteractions = nextEvolutionReq.minInteractions;
            _targetHappiness = nextEvolutionReq.minCurrentHappiness;
            _targetHunger = nextEvolutionReq.minCurrentHunger;
        }
        else
        {
            // Max level reached
            _targetTimeAlive = 0;
            _targetNutrition = 0;
            _targetInteractions = 0;
            _targetHappiness = 0;
            _targetHunger = 0;
        }

        _evolutionProgressPercent = _evolutionHandler.GetEvolutionProgress() * 100f;
    }

    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        if (monsterData?.evolutionRequirements == null || monsterData.evolutionRequirements.Length == 0)
            return null;

        // Find the evolution requirement for the next level
        foreach (var req in monsterData.evolutionRequirements)
        {
            if (req.targetEvolutionLevel == evolutionLevel + 1)
            {
                return req;
            }
        }

        return null;
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
        if (EvolutionHandler?.IsEvolving == true)
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

        //ServiceLocator.Get<TooltipController>().HoverEnter(monsterData.tooltipData, transform.position);
        if (monsterData.tooltipData != null)
            TooltipManager.Instance.StartHover(monsterData.tooltipData.infoData);
        else
            Debug.LogWarning($"Tooltip data of this object: {gameObject.name} is Empty");

        _interactionHandler?.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
        OnHoverChanged?.Invoke(_isHovered);
        UI.OnHoverExit();

        //ServiceLocator.Get<TooltipController>().HoverExit(monsterData.tooltipData);
        TooltipManager.Instance.EndHover();

        _interactionHandler?.OnPointerExit(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(eventData);
        _interactionHandler?.OnPointerClick(eventData);
    }

    #endregion

    #region Visual Effects

    public void UpdateVisuals() => _visualHandler?.ApplyMonsterVisuals();
    public void DropPoop(PoopType type = PoopType.Normal) => _visualHandler?.SpawnPoopWithAnimation(type);
    public void DropCoin(CoinType type) => _visualHandler?.SpawnCoinWithAnimation(type, CoinMultiplier());
    public Sprite GetMonsterIcon() => _visualHandler?.GetMonsterIcon();

    public void SetFallingStarsState(bool state)
    {
        if (monsterData == null) return;

        if (monsterData.monType != MonsterType.Legend)
        {
            UI.fallingStarsVfx.StopEmission();
            return;
        }

        if (!UI.fallingStarsVfx.gameObject.activeInHierarchy)
            UI.fallingStarsVfx.gameObject.SetActive(true);

        Vector3 q = UI.fallingStarsVfx.rectTransform.localEulerAngles;

        if (_monsterSpineGraphic.transform.localScale.x < 0)
            q.z = 90f;
        else
            q.z = -90f;

        UI.fallingStarsVfx.rectTransform.localEulerAngles = q;

        if (state)
            UI.fallingStarsVfx.StartEmission();
        else
            UI.fallingStarsVfx.StopEmission();
    }

    #endregion

    #region NPC Reservation System

    /// <summary>
    /// Reserve this monster for a specific NPC (for feeding/interaction)
    /// </summary>
    public void ReserveForNPC(MonsterController npc)
    {
        ReservedByNPC = npc;
    }

    /// <summary>
    /// Release the reservation (e.g., if NPC changes target or completes feeding)
    /// </summary>
    public void ReleaseNPCReservation()
    {
        ReservedByNPC = null;
    }

    /// <summary>
    /// Add evolution time when time is accelerated (Time Keeper facility)
    /// </summary>
    public void AddEvolutionTime(float seconds)
    {
        if (_evolutionHandler != null)
        {
            _evolutionHandler.AddEvolutionTime(seconds);
            Debug.Log($"{monsterID}: Added {seconds}s evolution time");
        }
    }

    /// <summary>
    /// Generate coins based on time skipped (Time Keeper facility)
    /// </summary>
    public void GenerateCoinsFromTimeSkip(float totalSeconds)
    {
        if (MonsterData == null) return;

        // Calculate how many coins should be generated based on time skipped
        float goldCoinInterval = (float)System.TimeSpan.FromHours(MonsterData.goldCoinDropRateStage1).TotalSeconds;
        float silverCoinInterval = (float)System.TimeSpan.FromHours(MonsterData.platCoinDropRateStage1).TotalSeconds;

        int goldCoinsToGenerate = Mathf.FloorToInt(totalSeconds / goldCoinInterval);
        int silverCoinsToGenerate = Mathf.FloorToInt(totalSeconds / silverCoinInterval);

        Debug.Log($"{monsterID}: Generating {goldCoinsToGenerate} gold coins and {silverCoinsToGenerate} silver coins from time skip");

        // Get monster position
        Vector2 monsterPos = _rectTransform.anchoredPosition;

        // Generate the coins with arc animation
        for (int i = 0; i < goldCoinsToGenerate; i++)
        {
            // Spawn coin slightly offset to avoid stacking
            Vector2 startPos = monsterPos + new Vector2(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-50f, 50f));
            Vector2 targetPos = monsterPos + new Vector2(UnityEngine.Random.Range(-100f, 100f), -150f);
            _monsterManager.SpawnCoinWithArc(startPos, targetPos, CoinType.Gold);
        }

        for (int i = 0; i < silverCoinsToGenerate; i++)
        {
            // Spawn coin slightly offset to avoid stacking
            Vector2 startPos = monsterPos + new Vector2(UnityEngine.Random.Range(-50f, 50f), UnityEngine.Random.Range(-50f, 50f));
            Vector2 targetPos = monsterPos + new Vector2(UnityEngine.Random.Range(-100f, 100f), -150f);
            _monsterManager.SpawnCoinWithArc(startPos, targetPos, CoinType.Platinum);
        }
    }

    #endregion

    #region GetMonsterOutsidePlains
    // EggCrackAnimator - MonsterManager
    public void CreateHandlersForSavingData()
    {
        /*
        CreateHandlers();
        
        */
        CreateHandlers();
    }
    #endregion

    #region Utility

    private int CoinMultiplier()
    {
        if (string.IsNullOrEmpty(lastPokedTime))
            return CalculateCoinMultiplier(timeCreated);
        else
            return CalculateCoinMultiplier(lastPokedTime);
    }

    private int CalculateCoinMultiplier(string time)
    {
        if (DateTime.TryParse(time, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSaveTime))
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan difference = now - lastSaveTime;

            double hoursAway = difference.TotalHours;

            if (hoursAway < 1)
                return 1;

            float baseMultiplier = MathF.Min(monsterData.GetGoldCoinDropRate(evolutionLevel) * (int)hoursAway, monsterData.GetGoldCoinDropRate(evolutionLevel) * 48f);

            if (IsSick || currentHunger <= 35f)
            {
                baseMultiplier /= 6f;
            }

            if (baseMultiplier < 1f)
                baseMultiplier = 1f;

            Debug.Log($"Coin Multiplier is {baseMultiplier} with Different Time {hoursAway}");

            return (int)baseMultiplier;
        }

        return 1;
    }
    #endregion
}