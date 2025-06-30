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
            instanceId = _controller.monsterID,
            monsterId = _controller.MonsterData.id,
            currentHunger = _controller.StatsHandler.CurrentHunger,
            currentHappiness = _controller.StatsHandler.CurrentHappiness,
            currentHealth = _controller.StatsHandler.CurrentHP,
            currentEvolutionLevel = _controller.evolutionLevel,

            // Evolution data
            timeCreated = _controller.GetEvolutionTimeCreated(),
            totalTimeSinceCreation = _controller.GetEvolutionTimeSinceCreation(),
            nutritionCount = _controller.GetEvolutionFoodConsumed(),
            currentInteraction = _controller.GetEvolutionInteractionCount()
        };

        SaveSystem.SaveMon(data);

        // Save current evolution level
        PlayerPrefs.SetInt($"{_controller.monsterID}_evolutionLevel", _controller.evolutionLevel);
    }

    public void LoadData(float maxHP)
    {
        if (SaveSystem.LoadMon(_controller.monsterID, out var data))
        {
            // Initialize stats handler with loaded data
            _controller.StatsHandler.Initialize(data.currentHealth, data.currentHunger, data.currentHappiness, maxHP);

            // Load evolution level (default to 1 if not found)
            int savedLevel = PlayerPrefs.GetInt($"{_controller.monsterID}_evolutionLevel", 1);
            _controller.evolutionLevel = savedLevel;

            // Load evolution data
            _controller.LoadEvolutionData(data.totalTimeSinceCreation, data.timeCreated, data.nutritionCount, data.currentInteraction);
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