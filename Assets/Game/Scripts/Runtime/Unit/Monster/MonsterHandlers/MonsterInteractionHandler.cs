using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MonsterInteractionHandler
{
    private MonsterController _controller;
    private MonsterStateMachine _stateMachine;
    private MonsterAnimationHandler _animationHandler;
    private float _pokeCooldownTimer;
    private bool _pendingSilverCoinDrop;
    private bool _pendingEvolutionCheck = false;  // ADD: Track pending evolution check
    
    public MonsterInteractionHandler(MonsterController controller, MonsterStateMachine stateMachine)
    {
        _controller = controller;
        _stateMachine = stateMachine;
        
        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged += OnStateChanged;
        }
    }
    
    private void OnStateChanged(MonsterState newState)
    {
        // Handle coin dropping
        if (_pendingSilverCoinDrop)
        {
            bool wasPokeState = 
                _stateMachine.PreviousState == MonsterState.Jumping ||
                _stateMachine.PreviousState == MonsterState.Itching ||
                _stateMachine.PreviousState == MonsterState.Flapping;
                
            bool isNowNormalState = 
                newState == MonsterState.Idle ||
                newState == MonsterState.Walking;
                
            if (wasPokeState && isNowNormalState)
            {
                _controller.DropCoin(CoinType.Silver);
                _pendingSilverCoinDrop = false;
                
                // ADD: Trigger evolution check after state returns to normal
                if (_pendingEvolutionCheck)
                {
                    _pendingEvolutionCheck = false;
                    // Delay slightly to ensure state is fully settled
                    _controller.StartCoroutine(DelayedEvolutionTrigger());
                }
            }
        }
    }

    public void SetAnimationHandler(MonsterAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
    }
    
    public void UpdateTimers(float deltaTime)
    {
        if (_pokeCooldownTimer > 0f)
            _pokeCooldownTimer -= deltaTime;
    }
    
    public void HandlePoke()
    {
        if (_pokeCooldownTimer > 0f) return;

        if (_controller?.MonsterData == null)
        {
            Debug.LogError("[Interaction] Monster data is null!");
            return;
        }

        _pokeCooldownTimer = _controller.MonsterData.pokeCooldownDuration;
        _controller.IncreaseHappiness(_controller.MonsterData.pokeHappinessValue);
        
        _pendingSilverCoinDrop = true;
        _pendingEvolutionCheck = true;  // ADD: Mark that we need to check evolution later
        
        MonsterState pokeState = GetRandomPokeState();
        _stateMachine?.ChangeState(pokeState);
    }

    private MonsterState GetRandomPokeState()
    {
        List<MonsterState> availableStates = new List<MonsterState>();
        MonsterState[] potentialStates = { MonsterState.Jumping, MonsterState.Itching, MonsterState.Flapping };
        
        foreach (var state in potentialStates)
        {
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(state))
            {
                availableStates.Add(state);
            }
        }
        
        if (availableStates.Count == 0)
        {
            if (_animationHandler != null && _animationHandler.HasValidAnimationForState(MonsterState.Jumping))
                return MonsterState.Jumping;
                
            return MonsterState.Idle;
        }

        int randomIndex = Random.Range(0, availableStates.Count);
        MonsterState selectedState = availableStates[randomIndex];
        
        // Double-check validation (in case initialization state changed)
        if (!_animationHandler.HasValidAnimationForState(selectedState))
        {
            Debug.LogWarning($"[Interaction] Selected state {selectedState} failed re-validation, falling back to Idle");
            return MonsterState.Idle;
        }
        
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

    // ADD: Delayed evolution trigger
    private IEnumerator DelayedEvolutionTrigger()
    {
        yield return new WaitForSeconds(0.5f); // Small delay to ensure state is settled
        
        // Trigger the evolution check through the controller
        _controller.CheckEvolutionAfterInteraction();
    }
}
