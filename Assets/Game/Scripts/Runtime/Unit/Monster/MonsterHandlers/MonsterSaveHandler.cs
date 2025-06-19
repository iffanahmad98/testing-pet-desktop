using UnityEngine;

public class MonsterSaveHandler
{
    private MonsterController _controller;
    private MonsterStatsHandler _statsHandler;
    
    public MonsterSaveHandler(MonsterController controller, MonsterStatsHandler statsHandler)
    {
        _controller = controller;
        _statsHandler = statsHandler;
    }
    
    public void SaveData()
    {
        var data = new MonsterSaveData
        {
            monsterId = _controller.monsterID,
            lastHunger = _statsHandler.CurrentHunger,
            lastHappiness = _statsHandler.CurrentHappiness,
            evolutionLevel = _controller.evolutionLevel,
            
            // Sick status data
            isSick = _statsHandler.IsSick,
            lastLowHungerTime = _statsHandler.LowHungerTime,
            
            // Evolution data
            timeSinceCreation = _controller.GetEvolutionTimeSinceCreation(),
            foodConsumed = _controller.GetEvolutionFoodConsumed(),
            interactionCount = _controller.GetEvolutionInteractionCount()
        };
        
        SaveSystem.SaveMon(data);
        
        // Save current evolution level
        PlayerPrefs.SetInt($"{_controller.monsterID}_evolutionLevel", _controller.evolutionLevel);
        
        Debug.Log($"[Save] Saved evolution level {_controller.evolutionLevel} for {_controller.monsterID}");
    }
    
    public void LoadData()
    {
        if (SaveSystem.LoadMon(_controller.monsterID, out var data))
        {
            // Initialize stats handler with loaded data
            _statsHandler.Initialize(data.lastHunger, data.lastHappiness, data.isSick, data.lastLowHungerTime);
            
            // Load evolution level (default to 1 if not found)
            int savedLevel = PlayerPrefs.GetInt($"{_controller.monsterID}_evolutionLevel", 1);
            _controller.evolutionLevel = savedLevel;
            
            Debug.Log($"[Load] Loaded evolution level {savedLevel} for {_controller.monsterID}");
            
            // Load evolution data
            _controller.LoadEvolutionData(data.timeSinceCreation, data.foodConsumed, data.interactionCount);
        }
        else
        {
            InitNewMonster();
        }
        
        ApplyMonsterDataStats();
        
        // Apply visuals after loading
        _controller.UpdateVisuals();
    }
    
    private void InitNewMonster()
    {
        var monsterData = _controller.MonsterData;
        float baseHunger = monsterData?.baseHunger ?? 50f;
        float baseHappiness = monsterData?.baseHappiness ?? 0f;
        
        // Initialize stats handler with base values
        _statsHandler.Initialize(baseHunger, baseHappiness, false, 0f);
        
        Debug.Log($"[Monster] Initializing new monster {_controller.monsterID} with base values: Hunger={baseHunger}, Happiness={baseHappiness}");
        
        // Initialize other controller data
        _controller.isFinalForm = false;
        _controller.evolutionLevel = 0;
        
        if (monsterData != null)
        {
            monsterData.isEvolved = false;
            monsterData.isFinalEvol = false;
            monsterData.evolutionLevel = 0;
        }
    }
    
    private void ApplyMonsterDataStats()
    {
        var monsterData = _controller.MonsterData;
        if (monsterData == null) return;

        _controller.MonsterData.moveSpd = monsterData.moveSpd;
        _controller.MonsterData.hungerDepleteRate = monsterData.hungerDepleteRate;
        _controller.MonsterData.poopRate = monsterData.poopRate;
        _controller.MonsterData.pokeHappinessValue = monsterData.pokeHappinessValue;
        _controller.MonsterData.areaHappinessRate = monsterData.areaHappinessRate;
    }
}