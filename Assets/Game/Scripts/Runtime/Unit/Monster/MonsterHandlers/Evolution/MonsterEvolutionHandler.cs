using UnityEngine;
using System;
using System.Linq;
using System.Collections;

[Serializable]
public class MonsterEvolutionHandler
{
    private MonsterController _controller;
    public bool IsEvolving => _isEvolving;
    private bool _isEvolving = false;
    private int _evolutionLevel = 1; // Current evolution level
    private int _targetLevel = 2; // Target evolution level for next evolution

    // Evolution tracking
    private float _timeSinceCreation;
    private int _foodConsumed;
    private int _interactionCount;

    public bool CanEvolve => _controller?.MonsterData != null && _controller.MonsterData.canEvolve && !_controller.MonsterData.isFinalEvol;
    public float TimeSinceCreation => _timeSinceCreation;
    public int FoodConsumed => _foodConsumed;
    public int InteractionCount => _interactionCount;

    public MonsterEvolutionHandler(MonsterController controller)
    {
        _controller = controller;
    }

    private EvolutionRequirement[] GetAvailableEvolutions()
    {
        if (_controller?.MonsterData?.evolutionRequirements == null)
            return new EvolutionRequirement[0];

        return _controller.MonsterData.evolutionRequirements
            .Where(req => req.targetEvolutionLevel == _controller.evolutionLevel + 1)
            .ToArray();
    }

    public void LoadEvolutionData(float timeSinceCreation, int foodConsumed, int interactionCount)
    {
        _timeSinceCreation = timeSinceCreation;
        _foodConsumed = foodConsumed;
        _interactionCount = interactionCount;
    }

    public void UpdateEvolutionTracking(float deltaTime)
    {
        if (!CanEvolve) return;
        _timeSinceCreation += deltaTime;
    }

    public void OnFoodConsumed()
    {
        _foodConsumed++;
        CheckEvolutionConditions();
    }

    public void OnInteraction()
    {
        _interactionCount++;
        CheckEvolutionConditions();
    }

    
    private IEnumerator WaitForIdle()
    {
        // Wait until the monster is idle
        while (_controller.StateMachine.CurrentState != MonsterState.Idle)
            yield return new WaitForSeconds(2.1f);
        CheckEvolutionConditions();
    }

    private void CheckEvolutionConditions()
    {
        if (!CanEvolve || _isEvolving) return;

        var nextEvolution = GetNextEvolutionRequirement();
        if (nextEvolution == null) return;

        if (MeetsEvolutionRequirements(nextEvolution))
        {
            _controller.StateMachine?.ChangeState(MonsterState.Idle);
            if (_controller.StateMachine?.CurrentState == MonsterState.Idle)
            {
                TriggerEvolution();
            }
        }
    }

    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        return GetAvailableEvolutions().FirstOrDefault();
    }

    private bool MeetsEvolutionRequirements(EvolutionRequirement req)
    {
        return _timeSinceCreation >= req.minTimeAlive &&
               _foodConsumed >= req.minFoodConsumed &&
               _interactionCount >= req.minInteractions &&
               _controller.StatsHandler.CurrentHappiness >= req.minCurrentHappiness &&
               _controller.StatsHandler.CurrentHunger >= req.minCurrentHunger &&
               (req.customCondition?.Invoke(_controller) ?? true);
    }

    public void TriggerEvolution()
    {
        _isEvolving = true;
        _evolutionLevel = _controller.evolutionLevel;
        _targetLevel = _evolutionLevel + 1;
        _controller.StartCoroutine(EvolutionSequence());
    }

    private IEnumerator EvolutionSequence()
    {
        Debug.Log($"[Evolution] { _controller.MonsterData.monsterName } evolution started.");

        // Simulate a short delay for effect (optional, can be removed)
        yield return new WaitForSeconds(0.5f);

        // Update evolution level and monster ID
        _controller.evolutionLevel = _targetLevel;
        UpdateMonsterID(_targetLevel);

        // Update monster visuals (e.g., change spine/appearance)
        _controller.UpdateVisuals();

        // Show evolution message
        ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.monsterName} evolved to level {_targetLevel}!", 3f);

        // Save data BEFORE resetting counters
        _controller.SaveMonData();

        // Reset counters for next evolution
        _foodConsumed = 0;
        _interactionCount = 0;

        Debug.Log($"[Evolution] { _controller.MonsterData.monsterName } evolved to level { _targetLevel }!");

        // Optional: short idle period after evolution
        yield return new WaitForSeconds(0.5f);

        _isEvolving = false;
    }

    private void UpdateMonsterID(int newLevel)
    {
        var oldID = _controller.monsterID;
        var parts = _controller.monsterID.Split('_');
        if (parts.Length >= 3)
        {
            // Update the ID in the controller
            _controller.monsterID = $"{parts[0]}_Stage{newLevel}_{parts[2]}";

            // Update the MonsterManager references
            var monsterManager = ServiceLocator.Get<MonsterManager>();
            if (monsterManager != null)
            {
                // Update saved IDs list in SaveSystem
                var savedIDs = SaveSystem.LoadSavedMonIDs();
                if (savedIDs.Contains(oldID))
                {
                    savedIDs.Remove(oldID);
                    savedIDs.Add(_controller.monsterID);
                    SaveSystem.SaveMonIDs(savedIDs);
                    monsterManager.RemoveSavedMonsterID(oldID);
                    monsterManager.AddSavedMonsterID(_controller.monsterID);
                }
            }

            // Remove the old save data
            SaveSystem.DeleteMon(oldID);
        }
    }

    public float GetEvolutionProgress()
    {
        var req = GetNextEvolutionRequirement();
        if (req == null) return 1f;

        float timeProgress = req.minTimeAlive > 0 ? _timeSinceCreation / req.minTimeAlive : 1f;
        float foodProgress = req.minFoodConsumed > 0 ? (float)_foodConsumed / req.minFoodConsumed : 1f;
        float interactionProgress = req.minInteractions > 0 ? (float)_interactionCount / req.minInteractions : 1f;
        float happinessProgress = req.minCurrentHappiness > 0 ? _controller.StatsHandler.CurrentHappiness / req.minCurrentHappiness : 1f;
        float hungerProgress = req.minCurrentHunger > 0 ? _controller.StatsHandler.CurrentHunger / req.minCurrentHunger : 1f;

        return Mathf.Clamp01((timeProgress + foodProgress + interactionProgress + happinessProgress + hungerProgress) / 5f);
    }
}
