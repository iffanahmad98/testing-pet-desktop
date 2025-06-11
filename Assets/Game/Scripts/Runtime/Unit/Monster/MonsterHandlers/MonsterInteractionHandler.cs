using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MonsterInteractionHandler
{
    private MonsterController _controller;
    private MonsterStateMachine _stateMachine;
    private MonsterAnimationHandler _animationHandler;
    private float _pokeCooldownTimer;
    private bool _pendingSilverCoinDrop;
    
    public MonsterInteractionHandler(MonsterController controller, MonsterStateMachine stateMachine)
    {
        _controller = controller;
        _stateMachine = stateMachine;
        
        // Subscribe to state changes to detect when poke animations finish
        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged += OnStateChanged;
        }
    }
    
    // NEW: Handle state changes to detect animation completion
    private void OnStateChanged(MonsterState newState)
    {
        if (_pendingSilverCoinDrop)
        {
            // Check if we transitioned away from a poke state
            bool wasPokeState = 
                _stateMachine.PreviousState == MonsterState.Jumping ||
                _stateMachine.PreviousState == MonsterState.Itching ||
                _stateMachine.PreviousState == MonsterState.Flapping;
                
            bool isNowNormalState = 
                newState == MonsterState.Idle ||
                newState == MonsterState.Walking;
                
            if (wasPokeState && isNowNormalState)
            {
                // Poke animation completed, drop coin
                _controller.DropCoinAfterPoke();
                _pendingSilverCoinDrop = false;
            }
        }
    }
    
    // NEW: Set animation handler reference
    public void SetAnimationHandler(MonsterAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
        Debug.Log($"[InteractionHandler] Animation handler set for {_controller?.monsterID}, is null: {_animationHandler == null}");
    }
    
    public void UpdateTimers(float deltaTime)
    {
        if (_pokeCooldownTimer > 0f)
            _pokeCooldownTimer -= deltaTime;
            
        // Remove the old CheckForPendingSilverCoinDrop call since we use events now
    }
    
    public void HandlePoke()
    {
        Debug.Log($"[Interaction] HandlePoke called for {_controller?.monsterID}");
        
        if (_pokeCooldownTimer > 0f) 
        {
            Debug.Log($"[Interaction] Poke on cooldown: {_pokeCooldownTimer}s remaining");
            return;
        }

        if (_controller?.MonsterData == null)
        {
            Debug.LogError("[Interaction] Monster data is null!");
            return;
        }

        _pokeCooldownTimer = _controller.MonsterData.pokeCooldownDuration;
        _controller.IncreaseHappiness(_controller.MonsterData.pokeHappinessValue);
        
        // Mark that we should drop a silver coin after animation
        _pendingSilverCoinDrop = true;
        Debug.Log($"[Interaction] Set pending coin drop to true");

        // NEW: Random between 3 poke states - Jumping, Itching, and Flapping
        MonsterState pokeState = GetRandomPokeState();
        Debug.Log($"[Interaction] Selected poke state: {pokeState}");
        
        _stateMachine?.ForceState(pokeState);
    }

    private MonsterState GetRandomPokeState()
    {
        Debug.Log($"[Interaction] Getting random poke state, animationHandler is null: {_animationHandler == null}");
        
        List<MonsterState> availableStates = new List<MonsterState>();
        
        // Check each potential poke state and add if animation exists
        MonsterState[] potentialStates = { MonsterState.Jumping, MonsterState.Itching, MonsterState.Flapping };
        
        foreach (var state in potentialStates)
        {
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(state))
            {
                Debug.Log($"[Interaction] Adding available state: {state}");
                availableStates.Add(state);
            }
            else
            {
                Debug.Log($"[Interaction] State {state} not available - animHandler null: {_animationHandler == null}");
            }
        }
        
        // If no special animations available, fallback to basic states
        if (availableStates.Count == 0)
        {
            Debug.LogWarning("[Interaction] No poke animations available, using fallbacks");
            
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(MonsterState.Jumping))
                return MonsterState.Jumping;
                
            // Ultimate fallback to Idle
            Debug.LogWarning("[Interaction] No valid poke animations found, fallback to Idle");
            return MonsterState.Idle;
        }
        
        // Return random from available states
        int randomIndex = Random.Range(0, availableStates.Count);
        var selectedState = availableStates[randomIndex];
        Debug.Log($"[Interaction] Selected random state: {selectedState}");
        return selectedState;
    }
    
    public void OnPointerEnter(PointerEventData e)
    {
        _controller.SetHovered(true);
        var cursorManager = ServiceLocator.Get<CursorManager>();
        cursorManager?.Set(CursorType.Monster);
    }
    
    public void OnPointerExit(PointerEventData e)
    {
        _controller.SetHovered(false);
        var cursorManager = ServiceLocator.Get<CursorManager>();
        cursorManager?.Reset();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_controller.isHovered) HandlePoke();
    }
}
