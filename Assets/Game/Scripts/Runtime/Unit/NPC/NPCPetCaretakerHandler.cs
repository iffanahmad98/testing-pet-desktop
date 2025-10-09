using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class NPCPetCaretakerHandler
{
    private MonsterManager _monsterManager;
    private NPCManager _nPCManager;
    private MonsterController _monsterController;
    private List<MonsterController> _petMonsterList = new List<MonsterController>();
    private List<CoinController> _coinList = new List<CoinController>();
    private List<PoopController> _poopList = new List<PoopController>();
    private Vector2 NPCPosition;
    private CoinController TargetCoin;
    private PoopController TargetPoop;
    private MonsterController TargetPet;
    private bool PoopCollectorEnabled = true;
    private bool OnCollectingPoop = false;
    private bool CoinCollectorEnabled = true;
    private bool OnCollectingCoin = false;
    private bool PetInteractionEnabled = true;
    private bool OnPetInteraction = false;
    private bool PetFeederEnabled = true;
    private bool OnFeedingPet = false;
    private bool OnIdling = false;
    private bool OnMove = false; 
    private float minDistanceFromTarget = 100f;
    private int npcIndex = 0; // You may need to implement a way to get the NPC's index
    
    private enum ActionType { CoinCollection, PoopCollection, PetInteraction, PetFeeding, Idling }
    private ActionType _currentAction = ActionType.Idling;
    private bool _isPerformingAction = false;

    public bool OnAction => OnDoingAction();
    public bool OnMoveAction => OnMove;

    public NPCPetCaretakerHandler(MonsterController monsterController, int npcIdleStationIndex)
    {
        npcIndex = npcIdleStationIndex;
        _monsterController = monsterController;
        _monsterManager = ServiceLocator.Get<MonsterManager>();
        _nPCManager = ServiceLocator.Get<NPCManager>();

        if (_monsterManager == null || _monsterController == null)
        {
            Debug.LogError("NPCPetCaretakerHandler: MonsterManager or MonsterController is null.");
            return;
        }
    }

    public void Initialize()
    {
        InitList(_monsterManager);
        InitMode();
        InitState();
        _monsterController.StartCoroutine(ManageActions());
    }

    private IEnumerator ManageActions()
    {
        while (true)
        {
            if (!_isPerformingAction)
            {
                yield return StartNextAction();
            }
            yield return new WaitForSeconds(10f); // Check every 10 seconds
        }
    }

    // Update the StartNextAction method to use test sequence
    private IEnumerator StartNextAction()
    {
        _isPerformingAction = true;

        Debug.Log($"NPC ACTIOn {_currentAction}");
        
        switch (_currentAction)
        {
            case ActionType.CoinCollection when CoinCollectorEnabled:
                yield return DoCoinCollection();
                Debug.Log("NPC is collecting coins.");
                break;
            case ActionType.PoopCollection when PoopCollectorEnabled:
                yield return DoPoopCollection();
                Debug.Log("NPC is collecting poop.");
                break;
            case ActionType.PetInteraction when PetInteractionEnabled:
                yield return DoPetInteraction();
                Debug.Log("NPC is interacting with a pet.");
                break;
            case ActionType.PetFeeding when PetFeederEnabled:
                yield return DoPetFeeding();
                Debug.Log("NPC is feeding a pet.");
                break;
            case ActionType.Idling:
                Debug.Log("NPC is do idling.");
                yield return DoIdling();
                Debug.Log("NPC is idling.");
                _monsterController.StateMachine.ChangeState(MonsterState.Idle);
                break;
            default:
                Debug.LogWarning($"NPCPetCaretakerHandler: Action {_currentAction} is not enabled or not implemented.");
                _isPerformingAction = false;
                yield break;
        }
        _isPerformingAction = false;
        _currentAction = GetAction();
    }

    private ActionType GetAction()
    {
        // Priority 1: Check if any monster has hunger under 40%
        if (PetFeederEnabled)
        {
            foreach (var pet in _petMonsterList)
            {
                if (pet != null && pet.IsTargetable)
                {
                    // Assuming pets have a hunger property - adjust property name as needed
                    if (pet.StatsHandler.CurrentHunger < 100f * 0.4f)
                    {
                        return ActionType.PetFeeding;
                    }
                }
            }
        }

        // Priority 2: Check if any monster has happiness under 50%
        if (PetInteractionEnabled)
        {
            foreach (var pet in _petMonsterList)
            {
                if (pet != null && pet.IsTargetable)
                {
                    // Assuming pets have a happiness property - adjust property name as needed
                    if (pet.StatsHandler.CurrentHappiness < 100f * 0.5f)
                    {
                        return ActionType.PetInteraction;
                    }
                }
            }
        }

        // Priority 3: Collect coins if available
        if (CoinCollectorEnabled && _coinList.Any(coin => coin != null && coin.IsTargetable))
        {
            return ActionType.CoinCollection;
        }

        // Priority 4: Collect poop if available
        if (PoopCollectorEnabled && _poopList.Any(poop => poop != null && poop.IsTargetable))
        {
            return ActionType.PoopCollection;
        }

        // Default: Idle if nothing to do
        return ActionType.Idling;
    }

    public void InitMode(bool poopCollectorEnabled = true, bool coinCollectorEnabled = true, bool petInteractionEnabled = true, bool petFeederEnabled = true)
    {
        PoopCollectorEnabled = poopCollectorEnabled;
        CoinCollectorEnabled = coinCollectorEnabled;
        PetInteractionEnabled = petInteractionEnabled;
        PetFeederEnabled = petFeederEnabled;
    }

    public void InitState(bool onCollectingCoin = false, bool onCollectingPoop = false, bool onPetInteraction = false, bool onFeedingPet = false, bool onIdling = false)
    {
        OnCollectingCoin = onCollectingCoin;
        OnCollectingPoop = onCollectingPoop;
        OnPetInteraction = onPetInteraction;
        OnFeedingPet = onFeedingPet;
        OnIdling = onIdling;
    }

    public void InitList(MonsterManager manager)
    {
        _petMonsterList.Clear();
        _coinList.Clear();
        _poopList.Clear();
        _petMonsterList = manager.activeMonsters;
        _coinList = manager.activeCoins;
        _poopList = manager.activePoops;

        if (_petMonsterList.Count <= 0 || _coinList.Count <= 0 || _poopList.Count <= 0)
        {
            Debug.LogWarning($"NPCPetCaretakerHandler: No pets {_petMonsterList.Count}, coins {_coinList.Count}, or poops {_poopList.Count} found.");
            return;
        }
    }

    private IEnumerator MoveTo(RectTransform target)
    {
        float distanceThreshold = OnIdling ? 2f : minDistanceFromTarget;
        OnMove = true;

        // Keep moving until we reach the target
        while (OnAction)
        {
            // Check if target still exists
            if (target == null || !target.gameObject.activeInHierarchy) yield break;

            // Get target position and adjust for game area height constraints
            Vector2 targetPos = target.anchoredPosition;
            Vector2 adjustedTarget = GetAdjustedTargetPosition(targetPos);

            // Update target position every frame with adjusted position
            _monsterController.SetTargetPosition(adjustedTarget);

            // Check if we're close enough to the target
            NPCPosition = GetAnchoredPos(_monsterController.transform);

            // Calculate distance based on whether we allow diagonal movement
            float distance = ShouldAllowDiagonalMovement()
                ? Vector2.Distance(NPCPosition, adjustedTarget)
                : Mathf.Abs(NPCPosition.x - adjustedTarget.x); // Only check X distance for horizontal-only movement

            if (distance <= distanceThreshold)
            {
                OnNearTarget(target.gameObject);
                yield break;
            }
            yield return null;
        }
    }

    private Vector2 GetAdjustedTargetPosition(Vector2 targetPos)
    {
        // Check if we should only move horizontally
        if (!ShouldAllowDiagonalMovement())
        {
            // Keep current Y position, only move to target X
            Vector2 currentPos = GetAnchoredPos(_monsterController.transform);
            return new Vector2(targetPos.x, currentPos.y);
        }

        return targetPos;
    }

    private bool ShouldAllowDiagonalMovement()
    {
        var monsterManager = _monsterManager;
        if (monsterManager == null) return false;

        var gameAreaRect = monsterManager.gameAreaRT;
        if (gameAreaRect == null) return false;

        float currentHeight = gameAreaRect.sizeDelta.y;
        float maxHeight = monsterManager.GetMaxGameAreaHeight();

        return currentHeight > maxHeight / 2f;
    }

    private void OnNearTarget(GameObject target)
    {
        if (target.TryGetComponent<CoinController>(out var coin))
        {
            CollectCoin(coin);
            return;
        }
        if (target.TryGetComponent<PoopController>(out var poop))
        {
            CollectPoop(poop);
            return;
        }
        if (target.TryGetComponent<MonsterController>(out var pet))
        {
            _monsterController.StateMachine.ChangeState(MonsterState.Jumping);
            if (_currentAction == ActionType.PetInteraction)
            {
                Debug.Log($"NPC {_monsterController.monsterID} is POKING pet {pet.monsterID} at position {pet.Position}");
                pet.InteractionHandler.HandlePoke();
            }
            if (_currentAction == ActionType.PetFeeding)
            {
                Debug.Log($"NPC {_monsterController.monsterID} is FEEDING pet {pet.monsterID} at position {pet.Position}");

                var itemInventoryUI = ServiceLocator.Get<ItemInventoryUI>();
                var itemDatabase = itemInventoryUI.ItemDatabase;

                // Get all food items and sort by price
                var foodItems = itemDatabase.GetItemsByCategory(ItemType.Food);
                var cheapestFood = foodItems
                    .Where(i => i != null && i.price > 0)
                    .OrderBy(i => i.price)
                    .FirstOrDefault();

                if (cheapestFood == null)
                {
                    Debug.LogWarning("No valid food item found in the database.");
                    return;
                }

                // Attempt to buy the item
                bool bought = SaveSystem.TryBuyItem(cheapestFood);
                if (bought)
                {
                    // Reduce from owned items and spawn food
                    var inventoryFood = SaveSystem.PlayerConfig.ownedItems
                        .FirstOrDefault(i => i.itemID == cheapestFood.itemID);

                    if (inventoryFood != null && inventoryFood.amount > 0)
                    {
                        inventoryFood.amount -= 1;
                        _monsterManager.SpawnItem(cheapestFood, pet.GetComponent<RectTransform>().anchoredPosition);
                        Debug.Log($"NPC successfully fed pet {pet.monsterID} with {cheapestFood.itemID} (cost: {cheapestFood.price})");
                    }
                    else
                    {
                        Debug.LogWarning("Food was bought but not found in inventory.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Not enough coins to buy {cheapestFood.itemID} (cost: {cheapestFood.price})");
                }
            }
            return;
        }

        // Handle idle station - if it's not a coin, poop, or pet, it's likely an idle station
        if (_currentAction == ActionType.Idling)
        {
            // Just reached the idle station, no special action needed
            // The DoIdling method will handle the waiting
            
            OnMove = false;
            _monsterController.StateMachine.ChangeState(MonsterState.Idle);
            return;
        }
    }

    private void CollectCoin(CoinController coin)
    {
        coin.OnCollected();
        // Remove from active coins list
        if (_coinList.Contains(coin))
        {
            _coinList.Remove(coin);
        }
    }

    private void CollectPoop(PoopController poop)
    {
        poop.OnCollected();
        // Remove from active poops list
        if (_poopList.Contains(poop))
        {
            _poopList.Remove(poop);
        }
    }

    public IEnumerator DoCoinCollection()
    {
        while (CoinCollectorEnabled) // Add continuous loop
        {
            if (OnDoingAction())
            {
                yield return new WaitUntil(() => !OnDoingAction()); // Wait before checking again
                continue;
            }
            OnCollectingCoin = true;
            TargetCoin = FindNearestTarget(_coinList);
            if (TargetCoin != null)
            {
                // Reserve the coin so other NPCs won't target it
                TargetCoin.Reserve(_monsterController);
                Debug.Log($"NPC {_monsterController.monsterID} reserved coin at {TargetCoin.Position}");
                yield return _monsterController.StartCoroutine(MoveTo(TargetCoin.transform as RectTransform));
            }
            OnCollectingCoin = false;
            TargetCoin = null; // Reset target after collection
            // Wait before looking for next coin
            yield break;
        }
    }

    public IEnumerator DoPoopCollection()
    {
        while (PoopCollectorEnabled) // Add continuous loop
        {
            if (OnDoingAction())
            {
                yield return new WaitUntil(() => !OnDoingAction()); // Wait before checking again
                continue;
            }
            OnCollectingPoop = true;
            TargetPoop = FindNearestTarget(_poopList);
            if (TargetPoop != null)
            {
                // Reserve the poop so other NPCs won't target it
                TargetPoop.Reserve(_monsterController);
                Debug.Log($"NPC {_monsterController.monsterID} reserved poop at {TargetPoop.Position}");
                yield return _monsterController.StartCoroutine(MoveTo(TargetPoop.transform as RectTransform));
            }
            OnCollectingPoop = false;
            TargetPoop = null; // Reset target after collection
            // Wait before looking for next poop
            yield break;
        }
    }

    public IEnumerator DoPetInteraction()
    {
        while (PetInteractionEnabled) // Add continuous loop
        {
            if (OnDoingAction())
            {
                yield return new WaitUntil(() => !OnDoingAction()); // Wait before checking again
                continue;
            }
            OnPetInteraction = true;
            TargetPet = FindNearestTarget(_petMonsterList);
            if (TargetPet != null)
            {
                Debug.Log($"NPC {_monsterController.monsterID} targeting pet {TargetPet.monsterID} for INTERACTION at position {TargetPet.Position}");
                yield return _monsterController.StartCoroutine(MoveTo(TargetPet.transform as RectTransform));
            }
            else
            {
                Debug.Log($"NPC {_monsterController.monsterID} found no pet to interact with");
            }
            OnPetInteraction = false;
            TargetPet = null; // Reset target after interaction
            // Wait before looking for next pet
            yield break;
        }
    }

    public IEnumerator DoPetFeeding()
    {
        while (PetFeederEnabled) // Add continuous loop
        {
            if (OnDoingAction())
            {
                yield return new WaitUntil(() => !OnDoingAction()); // Wait before checking again
                continue;
            }
            OnFeedingPet = true;

            // Find a pet at moderate distance (not too close) for feeding
            TargetPet = FindModerateDistanceTarget(_petMonsterList);

            if (TargetPet != null)
            {
                Debug.Log($"NPC {_monsterController.monsterID} targeting pet {TargetPet.monsterID} for FEEDING at moderate distance");
                yield return _monsterController.StartCoroutine(MoveTo(TargetPet.transform as RectTransform));
            }
            else
            {
                Debug.Log($"NPC {_monsterController.monsterID} found no pet at moderate distance to feed");
            }

            OnFeedingPet = false;
            TargetPet = null; // Reset target after feeding
            // Wait before looking for next pet
            yield break;
        }
    }

    public IEnumerator DoIdling()
    {
        OnIdling = true;
        
        Debug.Log($"NPC NPCIdleFlower: TARGET {_nPCManager != null}");
        
        if (_nPCManager != null)
        {
            // Assuming each NPC has an index - you might need to add this property to MonsterController
            // For now, using a default index of 0
            GameObject idleStation = _nPCManager.GetIdleTarget(npcIndex);
            
            Debug.Log($"NPC NPCIdleFlower: TARGET {idleStation.gameObject.name}");
            
            if (idleStation != null)
            {
                RectTransform idleTransform = idleStation.GetComponent<RectTransform>();
                if (idleTransform != null)
                {
                    // Move to idle station
                    yield return _monsterController.StartCoroutine(MoveTo(idleTransform));
                    
                    // Wait for a random duration (between 5-15 seconds)
                    float idleDuration = Random.Range(5f, 15f);
                    Debug.Log($"NPC is idling for {idleDuration:F1} seconds");
                    _monsterController.StateMachine.ChangeState(MonsterState.Idle);
                    yield return new WaitForSeconds(idleDuration);
                }
            }
        }
        
        OnIdling = false;
    }

    public T FindNearestTarget<T>(IEnumerable<T> list) where T : MonoBehaviour, ITargetable
    {
        T nearest = null;
        float shortestDistance = float.MaxValue;
        Vector2 npcPos = GetAnchoredPos(_monsterController.transform);

        foreach (var t in list)
        {
            if (t == null || !t.IsTargetable) continue;
            float d = Vector2.Distance(npcPos, t.Position);
            if (d < shortestDistance)
            {
                shortestDistance = d;
                nearest = t;
            }
        }
        return nearest;
    }

    public MonsterController FindModerateDistanceTarget(List<MonsterController> list, float minDistance = 400f, float maxDistance = 1000f)
    {
        MonsterController bestTarget = null;
        float bestScore = float.MaxValue;
        Vector2 npcPos = GetAnchoredPos(_monsterController.transform);

        foreach (var pet in list)
        {
            if (pet == null || !pet.IsTargetable) continue;

            // Skip this NPC itself
            if (pet == _monsterController) continue;

            float distance = Vector2.Distance(npcPos, pet.Position);

            // Prefer targets within the moderate distance range
            if (distance >= minDistance && distance <= maxDistance)
            {
                // Score is how close to the middle of the range (closer to middle = better)
                float midPoint = (minDistance + maxDistance) / 2f;
                float score = Mathf.Abs(distance - midPoint);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = pet;
                }
            }
        }

        // If no target in moderate range, find any valid target with hunger < 40%
        if (bestTarget == null)
        {
            foreach (var pet in list)
            {
                if (pet == null || !pet.IsTargetable) continue;
                if (pet == _monsterController) continue;

                if (pet.StatsHandler.CurrentHunger < 100f * 0.4f)
                {
                    bestTarget = pet;
                    break;
                }
            }
        }

        return bestTarget;
    }

    private bool OnDoingAction()
    {
        // Handle other actions here
        if (OnCollectingCoin || OnCollectingPoop || OnPetInteraction || OnFeedingPet || OnIdling)
        {
            return true;
        }
        return false;
    }

    private Vector2 GetAnchoredPos(Transform target)
    {
        RectTransform rectTransform = target.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            // Use anchoredPosition for UI elements
            return rectTransform.anchoredPosition;
        }
        else
        {
            // Fallback to world position for non-UI objects
            return new Vector2(target.position.x, target.position.y);
        }
    }

}
