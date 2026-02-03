using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterInteractionHandler
{
    private MonsterController _controller;
    private CursorManager _cursorManager;
    private float _pokeCooldownTimer = 0f;
    private bool _hasBeenInteractedWith = false; 
    private bool _pendingEvolutionCheck = false;
    private bool _isNPC = false;

    public MonsterInteractionHandler(MonsterController controller, MonsterStateMachine stateMachine)
    {
        _controller = controller;
        // (Ada bug send to plains kalau ini di aktifkan pas buka telur) _controller.StateMachine.OnStateChanged += OnStateChanged;
        _cursorManager = ServiceLocator.Get<CursorManager>();
        _isNPC = controller.isNPC;
    }
    
    private void OnStateChanged(MonsterState newState)
    {
        if (_pendingEvolutionCheck)
        {
            bool wasPokeState = 
                _controller.StateMachine.PreviousState == MonsterState.Jumping ||
                _controller.StateMachine.PreviousState == MonsterState.Itching ||
                _controller.StateMachine.PreviousState == MonsterState.Flapping;

            bool isNowNormalState = newState == MonsterState.Idle;
                
            if (wasPokeState && isNowNormalState)
            {
                _pendingEvolutionCheck = false;
                _controller.StartCoroutine(DelayedEvolutionTrigger());
            }
        }
    }
    
    public void HandlePoke()
    {
        if (_pokeCooldownTimer > 0f) return;
        if (_controller?.MonsterData == null) return;

        _controller.IncreaseHappiness(_controller.MonsterData.pokeHappinessValue);
        _controller.DropCoin(CoinType.Gold);
        _pendingEvolutionCheck = true;

        MonsterState pokeState = GetRandomPokeState();
        _controller.StateMachine?.ChangeState(pokeState);
        _controller.SetLastTimePokedTimer (DateTime.UtcNow.ToString("o"));

        if (_hasBeenInteractedWith)
        {
            _pokeCooldownTimer = 60f;
            _pokeCooldownTimer = 3f; //temp cooldown for demo purposes
        }
        else
        {
            _hasBeenInteractedWith = true;
            _pokeCooldownTimer = 60f;
            _pokeCooldownTimer = 3f; //temp cooldown for demo purposes
        }
    }
    
    private void HandleMonsterInfo()
    {
        _controller.UI.ShowMonsterInfo();
    }

    private MonsterState GetRandomPokeState()
    {
        List<MonsterState> availableStates = new List<MonsterState>();
        MonsterState[] potentialStates = { MonsterState.Jumping, MonsterState.Itching, MonsterState.Flapping };

        foreach (var state in potentialStates)
        {
            if (_controller.StateMachine.AnimationHandler != null && _controller.StateMachine.AnimationHandler.HasValidAnimationForState(state))
            {
                availableStates.Add(state);
            }
        }

        if (availableStates.Count == 0)
        {
            if (_controller.StateMachine.AnimationHandler != null && _controller.StateMachine.AnimationHandler.HasValidAnimationForState(MonsterState.Jumping))
                return MonsterState.Jumping;

            return MonsterState.Idle;
        }

        int randomIndex = UnityEngine.Random.Range(0, availableStates.Count);
        MonsterState selectedState = availableStates[randomIndex];

        if (!_controller.StateMachine.AnimationHandler.HasValidAnimationForState(selectedState))
        {
            Debug.LogWarning($"[Interaction] Selected state {selectedState} failed re-validation, falling back to Idle");
            return MonsterState.Idle;
        }

        return selectedState;
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
    
    public void UpdateOutsideInteraction()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
           _controller.UI.HideMonsterInfo();
        }
    }

    
    private IEnumerator DelayedEvolutionTrigger()
    {
        yield return new WaitForSeconds(0.5f);
        _controller.CheckEvolveAfterInteraction();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (_controller.EvolutionHandler.IsEvolving) return;

        if (!_isNPC)
        {
            _controller.SetHovered(true);
            _cursorManager?.Set(CursorType.Monster, Vector2.zero);
        }
        else
        {
            return;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (_controller.EvolutionHandler.IsEvolving) return;

        if (!_isNPC)
        {
            _controller.SetHovered(false);
            _cursorManager?.Reset();
        }
        else
        {
            return;
        }
    }
    
    public void OnPointerClick(PointerEventData e)
    {
        if (_controller.EvolutionHandler.IsEvolving) return;

        if (!_isNPC)
        {
            switch (e.button)
            {
                case PointerEventData.InputButton.Left:
                    if (_pokeCooldownTimer <= 0f)
                    {
                        HandlePoke();
                    }
                    else
                    {
                        Debug.Log("⏱️ Poke cooldown active, cannot poke now!");
                    }
                    break;
                case PointerEventData.InputButton.Right:
                    HandleMonsterInfo();
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (e.button)
            {
                case PointerEventData.InputButton.Left:
                    Debug.Log("NPC Monster clicked - no interaction available");
                    break;
                case PointerEventData.InputButton.Right:
                    Debug.Log("NPC Monster right-clicked - no interaction available");
                    break;
            }
        }
    }
}
