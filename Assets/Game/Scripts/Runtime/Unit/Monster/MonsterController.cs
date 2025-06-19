using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Spine.Unity;
using System.Collections;

public class MonsterController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private enum InitializationState
    {
        NotStarted,
        ComponentsReady,
        HandlersCreated,
        DataLoaded,
        FullyInitialized
    }
    
    private InitializationState _initState = InitializationState.NotStarted;
    private bool _isInitializing = false;
    
    private Action<float> _hungerChangedHandler;
    private Action<float> _happinessChangedHandler;
    private Action<bool> _sickChangedHandler;
    private Action<bool> _hoverChangedHandler;

    private MonsterDataSO monsterData;
    public string monsterID;
    public MonsterDataSO MonsterData => monsterData;
    [HideInInspector] public bool isFinalForm;
    [HideInInspector] public int evolutionLevel;


    public float currentHunger => _statsHandler?.CurrentHunger ?? 50f;
    public float currentHappiness => _statsHandler?.CurrentHappiness ?? 0f;
    public bool IsSick => _statsHandler?.IsSick ?? false;
    public bool isHovered => _isHovered;
    public bool IsLoaded => _isLoaded;
    public bool IsEvolving => _evolutionHandler?.IsEvolving ?? false;
    public FoodController nearestFood => _foodHandler?.NearestFood;
    public event Action<float> OnHungerChanged;
    public event Action<bool> OnSickChanged;
    public event Action<float> OnHappinessChanged;
    public event Action<bool> OnHoverChanged;
    public event Action<MonsterController> OnMonsterFullyInitialized;

    private MonsterSaveHandler _saveHandler;
    private MonsterVisualHandler _visualHandler;
    private MonsterFoodHandler _foodHandler;
    private MonsterInteractionHandler _interactionHandler;
    private MonsterBoundsHandler _movementBounds;
    private MonsterEvolutionHandler _evolutionHandler;
    private MonsterSeparationHandler _separationBehavior;
    
    public MonsterUIHandler UI = new MonsterUIHandler();
    public MonsterFoodHandler FoodHandler => _foodHandler;
    public MonsterBoundsHandler MovementBounds => _movementBounds; 

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
    private float _lastTargetChangeTime = 0f;
    private const float TARGET_CHANGE_COOLDOWN = 1f;

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
            monsterID = $"temp_{System.Guid.NewGuid().ToString("N")[..8]}";
        }
        gameObject.name = $"Monster_{monsterID}";
    }

    private void InitializeComponents()
    {
        _rectTransform = GetComponent<RectTransform>();

        _monsterSpineGraphic = FindSkeletonGraphicInHierarchy();
        if (_monsterSpineGraphic != null)
        {
            // Force initialization if skeleton data exists but AnimationState doesn't
            if (_monsterSpineGraphic.skeletonDataAsset != null && _monsterSpineGraphic.AnimationState == null)
            {
                _monsterSpineGraphic.Initialize(true);
            }
        }
        else
        {
            Debug.LogError($"[MonsterController] No SkeletonGraphic found for {monsterID}!");
            LogChildComponents();
        }

        if (_rectTransform == null)
        {
            Debug.LogError($"[MonsterController] No RectTransform found on {gameObject.name}");
        }
    }

    private SkeletonGraphic FindSkeletonGraphicInHierarchy()
    {
        // Method 1: Standard GetComponentInChildren (includes nested children)
        var skeletonGraphic = GetComponentInChildren<SkeletonGraphic>(false);
        if (skeletonGraphic != null)
        {
            return skeletonGraphic;
        }

        // Method 2: Include inactive objects
        skeletonGraphic = GetComponentInChildren<SkeletonGraphic>(true);
        if (skeletonGraphic != null)
        {
            skeletonGraphic.gameObject.SetActive(true);
            return skeletonGraphic;
        }

        // Method 3: Manual recursive search
        return FindSkeletonGraphicRecursive(transform);
    }

    private SkeletonGraphic FindSkeletonGraphicRecursive(Transform parent)
    {
        // Check current transform
        var skeletonGraphic = parent.GetComponent<SkeletonGraphic>();
        if (skeletonGraphic != null)
        {
            return skeletonGraphic;
        }

        // Check all children recursively
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            skeletonGraphic = FindSkeletonGraphicRecursive(child);
            if (skeletonGraphic != null)
            {
                return skeletonGraphic;
            }
        }

        return null;
    }
    private void LogChildComponents()
    {
        LogHierarchyRecursive(transform, 0);
    }

    private void LogHierarchyRecursive(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        var components = parent.GetComponents<Component>();
        string componentList = string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name));

        Debug.Log($"{indent}- {parent.name}: [{componentList}] (active: {parent.gameObject.activeInHierarchy})");

        for (int i = 0; i < parent.childCount; i++)
        {
            LogHierarchyRecursive(parent.GetChild(i), depth + 1);
        }
    }

    // Phase 2: Handler creation (Start)
    private void Start()
    {
        if (_initState != InitializationState.ComponentsReady) return;
        
        // Create handlers in dependency order
        CreateHandlersInOrder();
        _initState = InitializationState.HandlersCreated;
        
        // Wait for external dependencies, then continue
        StartCoroutine(ContinueInitializationWhenReady());
        
        // ADD: Safety timeout in case initialization gets stuck
        StartCoroutine(InitializationTimeout());
    }
    
    private void CreateHandlersInOrder()
    {
        // 1. Create core handlers first (no dependencies)
        _statsHandler = new MonsterStatsHandler(this);
        
        // 2. Create handlers that depend on core handlers
        _coroutineHandler = new MonsterCoroutineHandler(this, _statsHandler);
        _saveHandler = new MonsterSaveHandler(this, _statsHandler);
        _evolutionHandler = new MonsterEvolutionHandler(this);
          // Create handlers that need components (but not external services)
        if (_monsterSpineGraphic != null)
        {
            _visualHandler = new MonsterVisualHandler(this, _monsterSpineGraphic);
        }
        else
        {
            Debug.LogWarning($"[MonsterController] Cannot create VisualHandler - no SkeletonGraphic found on {gameObject.name}");
        }
        
        // Don't create StateMachine-dependent handlers yet
    }
    private IEnumerator ContinueInitializationWhenReady()
    {
        // Wait for external dependencies
        yield return new WaitUntil(() => 
            ServiceLocator.Get<GameManager>() != null && 
            GetComponent<MonsterStateMachine>() != null);

        // Now create StateMachine-dependent handlers
        _stateMachine = GetComponent<MonsterStateMachine>();
        _gameManager = ServiceLocator.Get<GameManager>();

        // Wait one more frame for StateMachine to complete its Start()
        yield return null;

        CreateDependentHandlers();
        _initState = InitializationState.DataLoaded;

        // Final initialization phase
        yield return StartCoroutine(FinalizeInitialization());
    }
      private void CreateDependentHandlers()
    {
        // Create handlers that need StateMachine
        if (_stateMachine != null)
        {
            _interactionHandler = new MonsterInteractionHandler(this, _stateMachine);
        }
        else
        {
            Debug.LogError($"[MonsterController] {monsterID} cannot create interaction handler - StateMachine is null");
        }
        
        // Create handlers that need components
        if (_rectTransform != null && _gameManager != null && _monsterSpineGraphic != null)
        {
            _movementHandler = new MonsterMovementHandler(_rectTransform, _stateMachine, this, _gameManager, _monsterSpineGraphic);
            _movementBounds = new MonsterBoundsHandler(_rectTransform, _gameManager);
            _separationBehavior = new MonsterSeparationHandler(this, _gameManager, _rectTransform);
        }
        else
        {
            Debug.LogError($"[MonsterController] {monsterID} cannot create movement handlers - rectTransform: {_rectTransform != null}, gameManager: {_gameManager != null}, spineGraphic: {_monsterSpineGraphic != null}");
        }
        
        // Create handlers that need GameManager
        if (_gameManager != null && _rectTransform != null)
        {
            _foodHandler = new MonsterFoodHandler(this, _gameManager, _rectTransform);
        }
        
        // Connect cross-dependencies safely
        if (_stateMachine?.AnimationHandler != null && _interactionHandler != null)
        {
            _interactionHandler.SetAnimationHandler(_stateMachine.AnimationHandler);
        }
        else
        {
            Debug.LogError($"[MonsterController] {monsterID} failed to connect animation handler - StateMachine.AnimationHandler: {_stateMachine?.AnimationHandler != null}, InteractionHandler: {_interactionHandler != null}");
            
            // Add retry mechanism
            if (_interactionHandler != null)
            {
                StartCoroutine(RetryAnimationHandlerConnection());
            }
        }
    }
      private IEnumerator RetryAnimationHandlerConnection()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (_stateMachine?.AnimationHandler != null && _interactionHandler != null)
        {
            _interactionHandler.SetAnimationHandler(_stateMachine.AnimationHandler);
        }
        else
        {
            Debug.LogError($"[MonsterController] {monsterID} RETRY failed - animation handler still not available");
        }
    }
    
    private IEnumerator FinalizeInitialization()
    {        // Initialize UI
        UI.Init();
        
        // Only call if handler exists
        if (_evolutionHandler != null)
        {
            _evolutionHandler.InitUIParticles(UI);
        }
        
        // Set initial values
        SetRandomTarget();
        
        // Apply monster data if available
        if (monsterData != null)
        {
            _visualHandler?.ApplyMonsterVisuals();
            _foodHandler?.Initialize(monsterData);
            _evolutionHandler?.InitializeWithMonsterData();
        }
        
        // Subscribe to events only after everything is ready
        SubscribeToEvents();
        
        // Register with GameManager last
        _gameManager?.RegisterActiveMonster(this);
        
        // Start coroutines
        _coroutineHandler?.StartAllCoroutines();
        
        _initState = InitializationState.FullyInitialized;
        _isLoaded = true;
        _isInitializing = false;
        
        OnMonsterFullyInitialized?.Invoke(this);
        yield break;
    }
    
    // Safe property access with initialization checks
    public bool IsFullyInitialized => _initState == InitializationState.FullyInitialized;
    
    // Prevent operations before full initialization
    private void Update()
    {
        if (!IsFullyInitialized) return;
        
        // ADD: Skip all updates during evolution except evolution tracking
        if (IsEvolving)
        {
            _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
            return; // Skip movement, interactions, etc.
        }
        
        _interactionHandler?.UpdateTimers(Time.deltaTime);
        _evolutionHandler?.UpdateEvolutionTracking(Time.deltaTime);
        HandleMovement();
    }
    
    // Safe event subscription with null checks
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
        
        _hoverChangedHandler = (hovered) =>
        {
            UI.UpdateHungerDisplay(currentHunger, hovered);
            UI.UpdateHappinessDisplay(currentHappiness, hovered);
        };

        _statsHandler.OnHungerChanged += _hungerChangedHandler;
        _statsHandler.OnHappinessChanged += _happinessChangedHandler;
        _statsHandler.OnSickChanged += _sickChangedHandler;
        OnHoverChanged += _hoverChangedHandler;
    }
    
    // Safe monster data setting with proper sequencing
    public void SetMonsterData(MonsterDataSO newMonsterData)
    {
        if (newMonsterData == null) return;

        monsterData = newMonsterData;
        
        // CRITICAL: Initialize evolution level BEFORE updating ID
        InitializeEvolutionLevel();

        // Update ID if needed (now uses correct evolution level)
        if (monsterID.StartsWith("temp_") || string.IsNullOrEmpty(monsterID))
        {
            monsterID = $"{monsterData.id}_Lv{evolutionLevel}_{System.Guid.NewGuid().ToString("N")[..8]}";
            gameObject.name = $"{monsterData.monsterName}_{monsterID}";
        }

        // Only initialize handlers if they exist and we're ready
        if (_initState >= InitializationState.HandlersCreated)
        {
            _foodHandler?.Initialize(monsterData);
            _evolutionHandler?.InitializeWithMonsterData();
            _visualHandler?.ApplyMonsterVisuals();
        }

        // Load data only after handlers are ready
        if (_initState >= InitializationState.DataLoaded)
        {
            _saveHandler?.LoadData();
        }
    }

    // Add this property to safely get current evolution level
    public int CurrentEvolutionLevel => evolutionLevel;

    // Method to initialize evolution level from SO when monster is first created
    private void InitializeEvolutionLevel()
    {
        if (monsterData != null)
        {
            // For new monsters, always start at level 1 regardless of SO
            if (evolutionLevel <= 0)
            {
                evolutionLevel = 1; // Always start at 1 for new monsters
                Debug.Log($"[Evolution] Initialized new monster {monsterID} to level 1");
            }
        }
        else
        {
            if (evolutionLevel <= 0)
            {
                evolutionLevel = 1;
            }
        }
    }

    // Method to evolve the monster (increases the instance level)
    public void EvolveToNextLevel()
    {
        if (monsterData != null && evolutionLevel < 3) // Assuming max 3 levels
        {
            evolutionLevel++;
            
            // Update visuals for new evolution level
            _visualHandler?.UpdateMonsterVisuals();
            
            // Update ID to reflect new level
            string baseName = monsterData.monsterName;
            gameObject.name = $"{baseName}_Lv{evolutionLevel}_{monsterID}";
            
            // Save the new evolution level
            SaveMonData();
        }
    }

    // Method to get sell price based on current evolution level
    public int GetCurrentSellPrice()
    {
        return monsterData?.GetSellPrice(evolutionLevel) ?? 0;
    }

    private void OnEnable()
    {
        // Only subscribe if fully initialized to prevent race conditions
        if (IsFullyInitialized)
        {
            SubscribeToEvents();
            _coroutineHandler?.StartAllCoroutines();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents(); // ADD THIS LINE
        _coroutineHandler?.StopAllCoroutines();
    }

    // ADD THIS METHOD
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
        }
        
        if (_hoverChangedHandler != null)
            OnHoverChanged -= _hoverChangedHandler;
    }

    // ALSO ADD OnDestroy for cleanup
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        // Clean up any other resources
        _coroutineHandler?.StopAllCoroutines();
        // _gameManager?.RemoveSavedMonsterID(monsterID);
    }

    private void HandleMovement()
    {
        if (monsterData == null) return;
        
        // Prevent movement during evolution
        if (_evolutionHandler != null && _evolutionHandler.IsEvolving)
        {
            return;
        }

        // NEW: Check if we should use relaxed bounds for very small areas
        bool useRelaxedBounds = _movementBounds?.IsMovementAreaTooSmall() ?? false;
        
        // NEW: For very small areas, drastically reduce movement updates
        if (useRelaxedBounds)
        {
            // Only update movement every 3rd frame for tiny areas
            if (Time.frameCount % 3 != 0) return;
        }

        // Apply separation force regardless of state
        Vector2 separationForce = _separationBehavior.CalculateSeparationForce();

        // NEW: Reduce separation force for small areas
        if (useRelaxedBounds)
        {
            separationForce *= 0.1f; // Much weaker separation
        }

        if (separationForce.magnitude > 0.1f)
        {
            Vector2 _pos = _rectTransform.anchoredPosition;
            Vector2 newPos = _pos + separationForce * Time.deltaTime;

            if (!useRelaxedBounds)
            {
                MonsterState _state = _stateMachine?.CurrentState ?? MonsterState.Idle;
                var bounds = _movementBounds.CalculateBoundsForState(_state);
                newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
                newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
            }
            else
            {
                // For small areas, only prevent moving completely outside game area
                var gameAreaSize = _gameManager.gameArea.sizeDelta;
                float padding = 20f;
                newPos.x = Mathf.Clamp(newPos.x, -gameAreaSize.x / 2 + padding, gameAreaSize.x / 2 - padding);
                newPos.y = Mathf.Clamp(newPos.y, -gameAreaSize.y / 2 + padding, gameAreaSize.y / 2 - padding);
            }

            _rectTransform.anchoredPosition = newPos;
        }

        bool isEating = _stateMachine?.CurrentState == MonsterState.Eating;
        if (isEating) return;

        bool isMovementState = _stateMachine?.CurrentState == MonsterState.Walking ||
                              _stateMachine?.CurrentState == MonsterState.Running ||
                              _stateMachine?.CurrentState == MonsterState.Flying ||
                              _stateMachine?.CurrentState == MonsterState.Flapping;

        if (isMovementState)
        {
            bool isGroundMovement = _stateMachine?.CurrentState != MonsterState.Flying && 
                                   _stateMachine?.CurrentState != MonsterState.Flapping;
            
            if (isGroundMovement && _foodHandler?.NearestFood == null)
            {
                _foodHandler?.FindNearestFood();
            }

            // Handle food logic only for ground movement
            if (isGroundMovement && _foodHandler?.NearestFood != null)
            {
                _foodHandler?.HandleFoodLogic(ref _targetPosition);
            }

            _targetPosition = _separationBehavior.ApplySeparationToTarget(_targetPosition);
        }

        _movementHandler.UpdateMovement(ref _targetPosition, monsterData);

        // Enhanced bounds checking - only for normal sized areas
        Vector2 currentPos = _rectTransform.anchoredPosition;
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Idle;
        
        if (!useRelaxedBounds && !_movementBounds.IsWithinBoundsForState(currentPos, currentState))
        {
            // Clamp position
            var bounds = _movementBounds.CalculateBoundsForState(currentState);
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
            var gameAreaSize = _gameManager.gameArea.sizeDelta;
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
            ServiceLocator.Get<GameManager>().SortMonstersByDepth();
        }

        bool isPursuingFood = _foodHandler?.NearestFood != null;
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

    // Add new method for state-aware target setting
    private void SetRandomTargetForCurrentState()
    {
        MonsterState currentState = _stateMachine?.CurrentState ?? MonsterState.Walking;
        _targetPosition = _movementBounds?.GetRandomTargetForState(currentState) ?? Vector2.zero;
    }

    // Update existing SetRandomTarget to use current state
    public void SetRandomTarget()
    {
        SetRandomTargetForCurrentState();
    }

    public void SetHovered(bool value)
    {
        if (_isHovered == value) return;
        _isHovered = value;
        OnHoverChanged?.Invoke(_isHovered);
    }

    public void UpdateVisuals() => _visualHandler?.UpdateMonsterVisuals();

    public void DropPoop(PoopType type = PoopType.Normal) 
    {
        // Use visual handler to spawn poop with animation, just like coins
        _visualHandler?.SpawnPoopWithAnimation(type);
    }

    public void DropCoin(CoinType type) 
    {
        Vector2 launchPosition = _visualHandler?.GetCoinLaunchPosition() ?? _rectTransform.anchoredPosition;
        Vector2 targetPosition = _visualHandler?.GetRandomPositionOutsideBounds() ?? GetRandomPositionAroundMonster();
        
        // Create coin with arc animation
        ServiceLocator.Get<GameManager>().SpawnCoinWithArc(launchPosition, targetPosition, type);
    }

    // Update fallback method to also respect bounds
    private Vector2 GetRandomPositionAroundMonster()
    {
        Vector2 basePos = _rectTransform.anchoredPosition;
        
        // Create a safe drop position below the monster
        Vector2 dropPosition = new Vector2(
            basePos.x + UnityEngine.Random.Range(-50f, 50f),
            basePos.y - 60f // Drop below monster
        );
        
        // Ensure it stays within game bounds if possible
        if (_gameManager?.gameArea != null)
        {
            var gameAreaSize = _gameManager.gameArea.sizeDelta;
            float padding = 30f;
            
            dropPosition.x = Mathf.Clamp(dropPosition.x, 
                -gameAreaSize.x / 2 + padding, 
                gameAreaSize.x / 2 - padding);
            dropPosition.y = Mathf.Max(dropPosition.y, 
                -gameAreaSize.y / 2 + padding);
        }
        
        return dropPosition;
    }

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
        // Don't call _evolutionHandler?.OnInteraction() immediately
        // Let the interaction handler manage the timing
    }

    // ADD: Method for delayed evolution check
    public void CheckEvolutionAfterInteraction()
    {
        _evolutionHandler?.OnInteraction(); // This will now be called at the right time
    }

    public void SaveMonData() => _saveHandler?.SaveData();
    public void LoadMonData() => _saveHandler?.LoadData();

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
    public void TriggerEating() => _stateMachine?.ChangeState(MonsterState.Eating);
    public void TriggerFoodConsumption() => _evolutionHandler?.OnFoodConsumed();
    
    private IEnumerator InitializationTimeout()
    {
        yield return new WaitForSeconds(10f); // 10 second timeout
        
        if (_initState != InitializationState.FullyInitialized)
        {   
            _initState = InitializationState.FullyInitialized;
            _isLoaded = true;
            _isInitializing = false;
        }
    }

    public Vector2 GetTargetPosition() => _targetPosition;
}