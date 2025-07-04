using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCPetCaretakerHandler
{
    private MonsterController _controller;
    private MonsterManager _monsterManager;
    private float _interactionCooldown = 15f;
    private float _interactionRadius = 150f;
    private float _coinDetectionRadius = 100f;
    
    // Stat thresholds for determining when pets need attention
    private float _hungerThreshold = 50f;
    private float _happinessThreshold = 50f;
    private float _healthThreshold = 50f;
    
    // Item references
    private List<ItemDataSO> _availableFoodItems = new List<ItemDataSO>();
    private List<ItemDataSO> _availableMedicineItems = new List<ItemDataSO>();
    
    // Coin collection
    private float _coinCollectDistance = 30f;
    private Coroutine _coinCollectionRoutine;
    private Coroutine _petInteractionRoutine;
    
    // Track if behaviors are active
    private bool _isInteractingWithPets = false;
    private bool _isCollectingCoin = false;
    
    public NPCPetCaretakerHandler(MonsterController controller)
    {
        _controller = controller;
        _monsterManager = ServiceLocator.Get<MonsterManager>();
        
        InitializeAvailableItems();
        
        // Start the main behavior routines
        // StartPetCaretakerBehavior();
        StartCoinCollectionBehavior();
    }
    
    private void InitializeAvailableItems()
    {
        // Try to get a reference to the item database
        var itemDatabase = SaveSystem.PlayerConfig.ownedItems;
        
        if (itemDatabase != null)
        {
            // Add all food items from database
            foreach (var item in itemDatabase)
            {
                if (item.type == ItemType.Food)
                {
                    _availableFoodItems.Add(FindItemById(item.itemID));
                }
                else if (item.type == ItemType.Medicine)
                {
                    _availableMedicineItems.Add(FindItemById(item.itemID));
                }
            }
        }
        
        // If no items found, log the error
        if (_availableFoodItems.Count == 0)
        {
            Debug.Log("No food items available.");
        }
    }
    
    private ItemDataSO FindItemById(string itemId)
    {
        var itemDatabase = ServiceLocator.Get<ItemInventoryUI>()?.ItemDatabase;
        if (itemDatabase != null)
        {
            return itemDatabase.GetItem(itemId);
        }
        return null;
    }
    
    // Start the main behavior coroutines
    public void StartPetCaretakerBehavior()
    {
        if (!_isInteractingWithPets)
        {
            _petInteractionRoutine = _controller.StartCoroutine(PetCaretakerRoutine());
        }
    }
    
    public void StartCoinCollectionBehavior()
    {
        if (!_isCollectingCoin)
        {
            _coinCollectionRoutine = _controller.StartCoroutine(CoinCollectionRoutine());
        }
    }
    
    // Stop the coroutines if needed
    public void StopAllBehaviors()
    {
        if (_petInteractionRoutine != null)
            _controller.StopCoroutine(_petInteractionRoutine);
            
        if (_coinCollectionRoutine != null)
            _controller.StopCoroutine(_coinCollectionRoutine);
            
        _isInteractingWithPets = false;
        _isCollectingCoin = false;
    }
    
    // Main pet caretaker routine
    private IEnumerator PetCaretakerRoutine()
    {
        _isInteractingWithPets = true;
        
        // Wait a random initial delay
        yield return new WaitForSeconds(3f);
        
        while (_isInteractingWithPets)
        {
            // Find nearby pet monsters
            var nearbyPets = FindNearbyPetMonsters();

            if (nearbyPets.Count > 0)
            {
                // Find pets with needs
                var hungryPets = FindHungryPets(nearbyPets);
                var unhappyPets = FindUnhappyPets(nearbyPets);
                // var sickPets = FindSickPets(nearbyPets);

                MonsterController targetPet = null;

                // First priority: Sick pets that need medicine
                // if (sickPets.Count > 0 && _availableMedicineItems.Count > 0)
                // {
                //     targetPet = sickPets[Random.Range(0, sickPets.Count)];
                //     yield return _controller.StartCoroutine(DropMedicineNearPetRoutine(targetPet));
                // }
                // Second priority: Very hungry pets
                if (hungryPets.Count > 0 && _availableFoodItems.Count > 0)
                {
                    targetPet = hungryPets[Random.Range(0, hungryPets.Count)];
                    yield return _controller.StartCoroutine(DropFoodNearPetRoutine(targetPet));
                }
                // Third priority: Unhappy pets
                else if (unhappyPets.Count > 0)
                {
                    targetPet = unhappyPets[Random.Range(0, unhappyPets.Count)];
                    yield return _controller.StartCoroutine(InteractWithPetRoutine(targetPet));
                }
                // Fallback: Random pet with random interaction
                else if (nearbyPets.Count > 0)
                {
                    targetPet = nearbyPets[Random.Range(0, nearbyPets.Count)];

                    // Decide what interaction to perform
                    float interactionRoll = Random.value;

                    if (interactionRoll < 0.4f && _availableFoodItems.Count > 0)
                    {
                        yield return _controller.StartCoroutine(DropFoodNearPetRoutine(targetPet));
                    }
                    else
                    {
                        yield return _controller.StartCoroutine(InteractWithPetRoutine(targetPet));
                    }
                }
            }

            // Wait before next interaction with some randomness
            yield return new WaitForSeconds(_interactionCooldown + Random.Range(-2f, 3f));
        }
    }
    
    // Coin collection routine - runs independently of pet interactions
    private IEnumerator CoinCollectionRoutine()
    {
        _isCollectingCoin = true;
        
        while (_isCollectingCoin)
        {
            GameObject targetCoin = FindNearestCoin();
            
            if (targetCoin != null)
            {
                // If we found a coin, collect it
                yield return _controller.StartCoroutine(MoveToAndCollectCoin(targetCoin));
            }
            
            // Wait before checking for coins again
            yield return new WaitForSeconds(5f);
        }
    }
    
    // Sub-routines for specific behaviors
    private IEnumerator DropFoodNearPetRoutine(MonsterController pet)
    {
        if (_availableFoodItems.Count == 0) yield break;
        
        // Choose a random food item
        var foodItem = _availableFoodItems[Random.Range(0, _availableFoodItems.Count)];
        
        // Find a position near the pet
        Vector2 petPosition = pet.GetComponent<RectTransform>().anchoredPosition;
        Vector2 dropOffset = Random.insideUnitCircle * 60f;
        Vector2 dropPosition = petPosition + dropOffset;
        
        // Walk towards the position (optional)
        yield return _controller.StartCoroutine(MoveToPosition(dropPosition));
        
        // Spawn the food
        _monsterManager.SpawnItem(foodItem, dropPosition);
        
        // Play a special animation on the NPC
        _controller.StateMachine?.ChangeState(MonsterState.Jumping);
        
        // Wait for animation to complete
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator DropMedicineNearPetRoutine(MonsterController pet)
    {
        if (_availableMedicineItems.Count == 0) yield break;
        
        // Choose a medicine item
        var medicineItem = _availableMedicineItems[0]; // Usually just one type
        
        // Find a position near the pet
        Vector2 petPosition = pet.GetComponent<RectTransform>().anchoredPosition;
        Vector2 dropOffset = Random.insideUnitCircle * 60f;
        Vector2 dropPosition = petPosition + dropOffset;
        
        // Walk towards the position (optional)
        yield return _controller.StartCoroutine(MoveToPosition(dropPosition));
        
        // Spawn the medicine
        _monsterManager.SpawnItem(medicineItem, dropPosition);
        
        // Play a special animation on the NPC
        _controller.StateMachine?.ChangeState(MonsterState.Jumping);
        
        // Wait for animation to complete
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator InteractWithPetRoutine(MonsterController pet)
    {
        if (pet == null || pet.StatsHandler == null) yield break;
        
        // Move close to the pet first
        Vector2 petPosition = pet.GetComponent<RectTransform>().anchoredPosition;
        yield return _controller.StartCoroutine(MoveToPosition(petPosition));
        
        // Simulate a "poke" interaction
        pet.IncreaseHappiness(5f);
        
        // Check for evolution
        pet.CheckEvolutionAfterInteraction();
        
        // Play interaction animation on NPC
        _controller.StateMachine?.ChangeState(MonsterState.Jumping);
        
        // Wait for animation to complete
        yield return new WaitForSeconds(0.5f);
    }
    
    private GameObject FindNearestCoin()
    {
        if (_monsterManager == null || _monsterManager.activeCoins.Count == 0)
            return null;

        Debug.Log("Finding nearest coin...");
            
        Vector2 myPosition = _controller.GetComponent<RectTransform>().anchoredPosition;
        float closestDistance = float.MaxValue;
        GameObject closestCoin = null;
        
        foreach (var coin in _monsterManager.activeCoins)
        {
            if (coin == null || !coin.activeInHierarchy)
                continue;
                
            Vector2 coinPosition = coin.GetComponent<RectTransform>().anchoredPosition;
            float distance = Vector2.Distance(myPosition, coinPosition);
            
            if (distance <= _coinDetectionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestCoin = coin;
            }
        }
        
        return closestCoin;
    }
    
    private IEnumerator MoveToAndCollectCoin(GameObject coin)
    {
        if (coin == null || !coin.activeInHierarchy)
            yield break;
            
        // Get coin position
        Vector2 coinPosition = coin.GetComponent<RectTransform>().anchoredPosition;
        
        // Move to the coin
        yield return _controller.StartCoroutine(MoveToPosition(coinPosition, _coinCollectDistance));
        
        // If coin still exists, collect it
        if (coin != null && coin.activeInHierarchy)
        {
            // Trigger coin collection
            var coinCtrl = coin.GetComponent<CoinController>();
            if (coinCtrl != null)
            {
                // Add to player's coins
                int coinValue = (int)coinCtrl.Type;
                SaveSystem.PlayerConfig.coins += coinValue;
                
                // Return coin to pool
                _monsterManager.DespawnToPool(coin);
                
                // Play collection animation
                _controller.StateMachine?.ChangeState(MonsterState.Jumping);
                
                // Wait for animation to complete
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    // Helper movement coroutine
    private IEnumerator MoveToPosition(Vector2 targetPosition, float stopDistance = 20f) // Reduced distance
    {
        // Set walking state BEFORE moving
        _controller.StateMachine?.ChangeState(MonsterState.Walking);
    
        Vector2 myPosition = _controller.GetComponent<RectTransform>().anchoredPosition;
        float distance = Vector2.Distance(myPosition, targetPosition);
        
        while (distance > stopDistance)
        {
            // Calculate direction and distance
            myPosition = _controller.GetComponent<RectTransform>().anchoredPosition;
            Vector2 direction = (targetPosition - myPosition).normalized;
            distance = Vector2.Distance(myPosition, targetPosition);
            
            // Move NPC toward the target (increased speed)
            float moveSpeed = 100f * Time.deltaTime;
            Vector2 newPosition = myPosition + direction * moveSpeed;
            
            // Update position
            _controller.GetComponent<RectTransform>().anchoredPosition = newPosition;
            
            // Face the right direction
            if (direction.x > 0)
            {
                _controller.transform.localScale = new Vector3(Mathf.Abs(_controller.transform.localScale.x), 
                                             _controller.transform.localScale.y, 
                                             _controller.transform.localScale.z);
            }
            else
            {
                _controller.transform.localScale = new Vector3(-Mathf.Abs(_controller.transform.localScale.x), 
                                             _controller.transform.localScale.y, 
                                             _controller.transform.localScale.z);
            }
            
            yield return null;
        }
        
        // Return to idle after reaching destination (unless another action follows)
        _controller.StateMachine?.ChangeState(MonsterState.Idle);
    }
    
    // These methods stay the same
    private List<MonsterController> FindHungryPets(List<MonsterController> pets)
    {
        // Same as before
        List<MonsterController> hungryPets = new List<MonsterController>();
        
        foreach (var pet in pets)
        {
            if (pet.StatsHandler != null && pet.StatsHandler.CurrentHunger <= _hungerThreshold)
            {
                hungryPets.Add(pet);
            }
        }
        
        // Sort by hunger level (lowest first)
        hungryPets.Sort((a, b) => 
            a.StatsHandler.CurrentHunger.CompareTo(b.StatsHandler.CurrentHunger));
            
        return hungryPets;
    }
    
    private List<MonsterController> FindUnhappyPets(List<MonsterController> pets)
    {
        // Same as before
        List<MonsterController> unhappyPets = new List<MonsterController>();
        
        foreach (var pet in pets)
        {
            if (pet.StatsHandler != null && pet.StatsHandler.CurrentHappiness <= _happinessThreshold)
            {
                unhappyPets.Add(pet);
            }
        }
        
        // Sort by happiness level (lowest first)
        unhappyPets.Sort((a, b) => 
            a.StatsHandler.CurrentHappiness.CompareTo(b.StatsHandler.CurrentHappiness));
            
        return unhappyPets;
    }
    
    private List<MonsterController> FindSickPets(List<MonsterController> pets)
    {
        // Same as before
        List<MonsterController> sickPets = new List<MonsterController>();
        
        foreach (var pet in pets)
        {
            if (pet.StatsHandler != null && pet.StatsHandler.IsSick)
            {
                sickPets.Add(pet);
            }
        }
        
        return sickPets;
    }
    
    private List<MonsterController> FindNearbyPetMonsters()
    {
        // Same as before
        List<MonsterController> nearbyPets = new List<MonsterController>();
        
        if (_monsterManager == null || _monsterManager.activeMonsters.Count == 0)
            return nearbyPets;
            
        Vector2 myPosition = _controller.GetComponent<RectTransform>().anchoredPosition;
        
        foreach (var monster in _monsterManager.activeMonsters)
        {
            // Skip self and other NPCs
            if (monster == _controller || monster.isNPC)
                continue;
                
            Vector2 petPosition = monster.GetComponent<RectTransform>().anchoredPosition;
            float distance = Vector2.Distance(myPosition, petPosition);
            
            if (distance <= _interactionRadius)
            {
                nearbyPets.Add(monster);
            }
        }
        
        return nearbyPets;
    }
}
