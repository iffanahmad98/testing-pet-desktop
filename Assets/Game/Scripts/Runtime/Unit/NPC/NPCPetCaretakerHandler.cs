using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCPetCaretakerHandler
{
    private MonsterController _controller;
    private MonsterManager _monsterManager;
    private float _interactionCooldown = 15f; // Seconds between interactions
    private float _nextInteractionTime = 0f;
    private float _interactionRadius = 150f; // Detection radius for pets
    
    // Configurable behavior chances
    private float _foodDropChance = 0.4f;
    private float _pokeChance = 0.6f;
    
    // Item references
    private List<ItemDataSO> _availableFoodItems;
    
    public NPCPetCaretakerHandler(MonsterController controller)
    {
        _controller = controller;
        _monsterManager = ServiceLocator.Get<MonsterManager>();
        
        // Initialize with random first interaction time
        _nextInteractionTime = Time.time + Random.Range(5f, _interactionCooldown);
        
        // Get available food items
        // _availableFoodItems = ServiceLocator.Get<InventoryManager>()?.GetAllFoodItems() ?? new List<ItemDataSO>();
        
        // Fallback if no food items found
        if (_availableFoodItems.Count == 0)
        {
            Debug.LogWarning("NPC has no food items to drop!");
        }
    }
    
    public void Update(float deltaTime)
    {
        if (Time.time < _nextInteractionTime) return;
        
        // Find nearby pet monsters
        var nearbyPets = FindNearbyPetMonsters();
        if (nearbyPets.Count == 0) return;
        
        // Choose a random nearby pet
        var targetPet = nearbyPets[Random.Range(0, nearbyPets.Count)];
        
        // Decide what interaction to perform
        float interactionRoll = Random.value;
        
        if (interactionRoll < _foodDropChance && _availableFoodItems.Count > 0)
        {
            DropFoodNearPet(targetPet);
        }
        else if (interactionRoll < (_foodDropChance + _pokeChance))
        {
            InteractWithPet(targetPet);
        }
        
        // Set next interaction time
        _nextInteractionTime = Time.time + _interactionCooldown + Random.Range(-2f, 3f);
    }
    
    private List<MonsterController> FindNearbyPetMonsters()
    {
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
    
    private void DropFoodNearPet(MonsterController pet)
    {
        if (_availableFoodItems.Count == 0) return;
        
        // Choose a random food item
        var foodItem = _availableFoodItems[Random.Range(0, _availableFoodItems.Count)];
        
        // Find a position near the pet
        Vector2 petPosition = pet.GetComponent<RectTransform>().anchoredPosition;
        Vector2 dropOffset = Random.insideUnitCircle * 60f;
        Vector2 dropPosition = petPosition + dropOffset;
        
        // Spawn the food
        _monsterManager.SpawnItem(foodItem, dropPosition);
        
        // Optional: Play a special animation on the NPC
        _controller.StateMachine?.ChangeState(MonsterState.Jumping);
    }
    
    private void InteractWithPet(MonsterController pet)
    {
        // Simulate a "poke" interaction
        if (pet.StatsHandler != null)
        {
            // Increase pet's happiness
            pet.IncreaseHappiness(5f);
            
            // Check for evolution
            pet.CheckEvolutionAfterInteraction();
            
            // Optional: Play interaction animation on NPC
            _controller.StateMachine?.ChangeState(MonsterState.Jumping);
        }
    }
}
