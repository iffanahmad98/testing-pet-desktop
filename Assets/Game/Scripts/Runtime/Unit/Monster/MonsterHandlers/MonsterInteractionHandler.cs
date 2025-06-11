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
    }
    
    // NEW: Set animation handler reference
    public void SetAnimationHandler(MonsterAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
    }
    
    public void UpdateTimers(float deltaTime)
    {
        if (_pokeCooldownTimer > 0f)
            _pokeCooldownTimer -= deltaTime;
            
        // Check if we need to drop silver coin after poke animation
        CheckForPendingSilverCoinDrop();
    }
    
    public void HandlePoke()
    {
        if (_pokeCooldownTimer > 0f) return;

        _pokeCooldownTimer = _controller.MonsterData.pokeCooldownDuration;
        _controller.IncreaseHappiness(_controller.MonsterData.pokeHappinessValue);
        
        // Mark that we should drop a silver coin after animation
        _pendingSilverCoinDrop = true;

        // NEW: Random between 3 poke states - Jumping, Itching, and Flapping
        MonsterState pokeState = GetRandomPokeState();
        _stateMachine?.ForceState(pokeState);
    }
    
    // NEW: Get random poke state with animation validation
    private MonsterState GetRandomPokeState()
    {
        List<MonsterState> availableStates = new List<MonsterState>();
        
        // Check each potential poke state and add if animation exists
        MonsterState[] potentialStates = { MonsterState.Jumping, MonsterState.Itching, MonsterState.Flapping }; // CHANGED: Flapping instead of Flying
        
        foreach (var state in potentialStates)
        {
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(state))
            {
                availableStates.Add(state);
            }
        }
        
        // If no special animations available, fallback to basic states
        if (availableStates.Count == 0)
        {
            // Try Jumping first (most common)
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(MonsterState.Jumping))
                return MonsterState.Jumping;
                
            // Then Itching
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(MonsterState.Itching))
                return MonsterState.Itching;
                
            // Ultimate fallback to Idle
            return MonsterState.Idle;
        }
        
        // Return random from available states
        int randomIndex = Random.Range(0, availableStates.Count);
        return availableStates[randomIndex];
    }
    
    private void CheckForPendingSilverCoinDrop()
    {
        if (_pendingSilverCoinDrop && 
            _stateMachine.CurrentState != MonsterState.Jumping && 
            _stateMachine.CurrentState != MonsterState.Itching &&
            _stateMachine.CurrentState != MonsterState.Flapping) // CHANGED: Flapping instead of Flying
        {
            // Animation finished, drop the silver coin
            _controller.DropCoinAfterPoke();
            _pendingSilverCoinDrop = false;
        }
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
