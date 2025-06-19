using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MonsterInteractionHandler
{
    private MonsterController _controller;
    private MonsterStateMachine _stateMachine;
    private MonsterAnimationHandler _animationHandler;
    private float _pokeCooldownTimer = 0f;
    private bool _hasBeenInteractedWith = false; 
    private bool _pendingEvolutionCheck = false;
    
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
        if (_pendingEvolutionCheck)
        {
            bool wasPokeState = 
                _stateMachine.PreviousState == MonsterState.Jumping ||
                _stateMachine.PreviousState == MonsterState.Itching ||
                _stateMachine.PreviousState == MonsterState.Flapping;

            bool isNowNormalState = newState == MonsterState.Idle;
                
            if (wasPokeState && isNowNormalState)
            {
                _pendingEvolutionCheck = false;
                _controller.StartCoroutine(DelayedEvolutionTrigger());
            }
        }
    }

    public void SetAnimationHandler(MonsterAnimationHandler animationHandler)
    {
        _animationHandler = animationHandler;
    }
    
    public void HandlePoke()
    {
        if (_pokeCooldownTimer > 0f) return;
        
        if (_controller?.MonsterData == null) return;

        Debug.Log("✅ Poke executed successfully!");
        
        _controller.IncreaseHappiness(_controller.MonsterData.pokeHappinessValue);
        _controller.DropCoin(CoinType.Gold);
        _pendingEvolutionCheck = true; 
        
        MonsterState pokeState = GetRandomPokeState();
        _stateMachine?.ChangeState(pokeState);

        if (_hasBeenInteractedWith)
        {
            _pokeCooldownTimer = 60f;
        }
        else
        {
            _hasBeenInteractedWith = true;
            _pokeCooldownTimer = 60f;
        }
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

    private IEnumerator DelayedEvolutionTrigger()
    {
        yield return new WaitForSeconds(0.5f); 
        _controller.CheckEvolutionAfterInteraction();
    }

    public void UpdateTimers(float deltaTime)
    {
        if (_pokeCooldownTimer > 0f) 
        {
            _pokeCooldownTimer -= deltaTime;
            if (_pokeCooldownTimer <= 0f)
            {
                Debug.Log("⏱️ Cooldown finished - monster ready for interaction!");
            }
        }
    }
}
