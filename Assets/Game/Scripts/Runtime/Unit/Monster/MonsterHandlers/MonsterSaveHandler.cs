using UnityEngine;

public class MonsterSaveHandler
{
    private MonsterController _controller;
    public MonsterSaveHandler(MonsterController controller)
    {
        _controller = controller;
    }
    
    public void SaveData()
    {
        var data = new MonsterSaveData
        {
            monsterId = _controller.monsterID,
            lastHunger = _controller.StatsHandler.CurrentHunger,
            lastHappiness = _controller.StatsHandler.CurrentHappiness,
            evolutionLevel = _controller.evolutionLevel,
            
            // Sick status data
            isSick = _controller.StatsHandler.IsSick,
            lastLowHungerTime = _controller.StatsHandler.LowHungerTime,

            // Evolution data
            timeSinceCreation = _controller.GetEvolutionTimeSinceCreation(),
            nutritionCount = _controller.GetEvolutionFoodConsumed(),
            interactionCount = _controller.GetEvolutionInteractionCount()
        };
        
        SaveSystem.SaveMon(data);
        
        // Save current evolution level
        PlayerPrefs.SetInt($"{_controller.monsterID}_evolutionLevel", _controller.evolutionLevel);    
    }
    
    public void LoadData()
    {
        if (SaveSystem.LoadMon(_controller.monsterID, out var data))
        {
            // Initialize stats handler with loaded data
            _controller.StatsHandler.Initialize(data.lastHunger, data.lastHappiness, data.isSick, data.lastLowHungerTime);

            // Load evolution level (default to 1 if not found)
            int savedLevel = PlayerPrefs.GetInt($"{_controller.monsterID}_evolutionLevel", 1);
            _controller.evolutionLevel = savedLevel;

            // Load evolution data
            _controller.LoadEvolutionData(data.timeSinceCreation, data.nutritionCount, data.interactionCount);
        }
        else
        {
            Debug.LogError($"No saved data found for monster ID: {_controller.monsterID}");
        }
        
        ApplyMonsterDataStats();
        // Apply visuals after loading
        _controller.UpdateVisuals();
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