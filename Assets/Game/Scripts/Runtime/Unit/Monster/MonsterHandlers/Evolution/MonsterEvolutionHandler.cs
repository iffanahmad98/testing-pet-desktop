using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using Spine.Unity;
using CartoonFX;
using System.Collections.Generic;
using DG.Tweening;

[Serializable]
public class MonsterEvolutionHandler
{
    private MonsterController _controller;
    private SkeletonGraphic _skeletonGraphic;
    private SkeletonGraphic _skeletonGraphicBefore;
    private SkeletonGraphic _skeletonGraphicAfter;
    public bool IsEvolving => _isEvolving;
    private bool _isEvolving = false;
    private int _evolutionLevel = 1; // Current evolution level
    private int _targetLevel = 2; // Target evolution level for next evolution

    // Evolution tracking
    private float _timeSinceCreation;
    private string _timeCreated;
    private int _nutritionConsumed;
    private int _interactionCount;

    public bool CanEvolve => _controller?.MonsterData != null && _controller.MonsterData.canEvolve;
    public float TimeSinceCreation => _timeSinceCreation;
    public string TimeCreated => _timeCreated;
    public int NutritionConsumed => _nutritionConsumed;
    public int InteractionCount => _interactionCount;

    public static event Action<MonsterController> OnMonsterEvolved;

    public MonsterEvolutionHandler(MonsterController controller, SkeletonGraphic skeletonGraphic, SkeletonGraphic skeletonGraphicBefore, SkeletonGraphic skeletonGraphicAfter)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
        _skeletonGraphicBefore = skeletonGraphicBefore;
        _skeletonGraphicAfter = skeletonGraphicAfter;
    }

    private EvolutionRequirement[] GetAvailableEvolutions()
    {
        if (_controller?.MonsterData?.evolutionRequirements == null)
            return new EvolutionRequirement[0];

        return _controller.MonsterData.evolutionRequirements
            .Where(req => req.targetEvolutionLevel == _controller.evolutionLevel + 1)
            .ToArray();
    }

    public void LoadEvolutionData(float timeSinceCreation, string timeCreated, int foodConsumed, int interactionCount)
    {
        _timeCreated = timeCreated;
        _timeSinceCreation = timeSinceCreation;
        _nutritionConsumed = foodConsumed;
        _interactionCount = interactionCount;
    }

    public void UpdateEvolutionTracking(float deltaTime)
    {
        if (!CanEvolve) return;
        _timeSinceCreation += deltaTime;
    }
    public void ResetEvolutionTracking()
    {
        _timeCreated = DateTime.UtcNow.ToString("o"); // ISO 8601 format
        _timeSinceCreation = 0f;
        _nutritionConsumed = 0;
        _interactionCount = 0;
    }

    public void OnFoodConsumed()
    {
        _nutritionConsumed++;
        CheckEvolutionConditions();
    }

    public void OnInteraction()
    {
        _interactionCount++;
        CheckEvolutionConditions();
    }

    private void CheckEvolutionConditions()
    {
        // if (!CanEvolve || _isEvolving) return;

        var nextEvolution = GetNextEvolutionRequirement();
        if (nextEvolution == null) return;

        // if (MeetsEvolutionRequirements(nextEvolution))
        // {
            _controller.StateMachine?.ChangeState(MonsterState.Idle);
            if (_controller.StateMachine?.CurrentState == MonsterState.Idle)
            {
                TriggerEvolution();
            }
        // }
    }

    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        return GetAvailableEvolutions().FirstOrDefault();
    }

    private bool MeetsEvolutionRequirements(EvolutionRequirement req)
    {
        return _timeSinceCreation >= req.minTimeAlive &&
               _nutritionConsumed >= req.minFoodConsumed &&
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
        // Store old ID before any changes
        var oldID = _controller.monsterID;
        
        // CRITICAL: Update ID and save data IMMEDIATELY before visual effects
        _controller.evolutionLevel = _targetLevel;
        UpdateMonsterID(_targetLevel); // Reuse existing method - this updates the ID AND saves
        
        // Reset evolution tracking data
        ResetEvolutionTracking();
        
        // Save the complete evolved state immediately
        SaveEvolvedMonsterData();

        var originalPos = _skeletonGraphic.rectTransform.anchoredPosition;
        var originalScale = _skeletonGraphic.rectTransform.localScale.x;
        var originalParent = _controller.transform.parent;
        var monsterTransform = _controller.transform;
        var areaTransform = _controller.MonsterManager.gameAreaRT.transform;

        // Get the next evolution skeleton asset
        var monsterData = _controller.MonsterData;
        var monsterPos = _controller.GetComponent<RectTransform>().anchoredPosition;
        int index = _targetLevel - 1;
        SkeletonDataAsset nextSkeleton =
            (monsterData != null && monsterData.monsterSpine != null && index >= 0 && index < monsterData.monsterSpine.Length)
            ? monsterData.monsterSpine[index]
            : null;
        // Get references to required components
        var evolveCam = MainCanvas.MonsterCamera;
        var spineGraphic = _skeletonGraphic;
        var spineGraphicBefore = _skeletonGraphicBefore;
        var spineGraphicAfter = _skeletonGraphicAfter;
        var evolutionParticle = _controller.UI.evolutionVFX;
        var whiteFlashMaterial = _controller.UI.evolutionMaterial;

        
        // hide semua monster kecuali yang evolve
        int _remaining = 0;
        foreach (var monster in _controller.MonsterManager.activeMonsters)
        {
            if (monster != _controller)
            {
                // visual = child(0), root = monster root
                var visual = monster.transform.GetChild(0)?.gameObject;
                var root   = monster.gameObject;
                FadeUtils.FadeOutVisualThenHideRoot(visual, root, 0.25f);

                yield return new WaitForSeconds(0.1f); // stagger
            }
        }

        // hide coin, poop, food (anggap visual = object itu sendiri, root = object itu sendiri)
        // karena biasanya mereka 1 GO saja; jika pun punya child visual, sesuaikan seperti di monster
        foreach (var coin in _controller.MonsterManager.activeCoins)
            FadeUtils.FadeOutVisualThenHideRoot(coin.gameObject, coin.gameObject, 0.25f);

        foreach (var poop in _controller.MonsterManager.activePoops)
            FadeUtils.FadeOutVisualThenHideRoot(poop.gameObject, poop.gameObject, 0.25f);

        foreach (var food in _controller.MonsterManager.activeFoods)
            FadeUtils.FadeOutVisualThenHideRoot(food.gameObject, food.gameObject, 0.25f);

        spineGraphicBefore.skeletonDataAsset = spineGraphic.skeletonDataAsset;
        spineGraphicBefore.Initialize(true);
        spineGraphicAfter.skeletonDataAsset = nextSkeleton;
        spineGraphicAfter.Initialize(true);
        var evolutionSequence = MonsterEvolutionSequenceHelper.PlayEvolutionUISequence(
            evolveCam,
            spineGraphic,
            evolutionParticle, // This should now be the array instead of evolutionParticle[8]
            nextSkeleton,
            () =>
            {
                _controller.evolutionLevel = _targetLevel;
                ResetMonsterData();
                ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.name} evolved to level {_targetLevel}!", 3f);
                // DON'T set sequenceDone here - this is just the transformation moment
            }
        );

        // Wait for the ENTIRE sequence to complete (including zoom out and position restoration)
        yield return evolutionSequence.WaitForCompletion();
        
        // NOW it's safe to show other objects
        yield return new WaitForSeconds(0.5f); // Optional small delay for polish

        // turn other monsters visible
        foreach (var monster in _controller.MonsterManager.activeMonsters)
        {
            _remaining = _controller.MonsterManager.activeMonsters.Count - 1; // -1 for self
            if (monster != _controller)
            {
                monster.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
                _remaining--;
                yield return new WaitForSeconds(0.1f); // stagger the fade 
            }
        }

        foreach (var monster in _controller.MonsterManager.activeMonsters)
        {
            if (monster != _controller)
            {
                var visual = monster.transform.GetChild(0)?.gameObject;
                var root   = monster.gameObject;
                FadeUtils.ShowRootThenFadeInVisual(visual, root, 0.25f);
            }
        }

        foreach (var coin in _controller.MonsterManager.activeCoins)
            FadeUtils.ShowRootThenFadeInVisual(coin.gameObject, coin.gameObject, 0.25f);

        foreach (var poop in _controller.MonsterManager.activePoops)
            FadeUtils.ShowRootThenFadeInVisual(poop.gameObject, poop.gameObject, 0.25f);

        foreach (var food in _controller.MonsterManager.activeFoods)
            FadeUtils.ShowRootThenFadeInVisual(food.gameObject, food.gameObject, 0.25f);

        yield return new WaitForSeconds(1f);
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
                var savedIDs = SaveSystem.LoadMonIDs();
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

    private void SaveEvolvedMonsterData()
    {
        // Save complete evolved monster state
        var data = new MonsterSaveData
        {
            instanceId = _controller.monsterID, // Already updated by UpdateMonsterID
            monsterId = _controller.MonsterData.id,
            gameAreaId = _controller.MonsterManager.currentGameAreaIndex,
            currentHunger = _controller.StatsHandler.CurrentHunger,
            currentHappiness = _controller.StatsHandler.CurrentHappiness,
            currentHealth = _controller.StatsHandler.CurrentHP,
            currentEvolutionLevel = _controller.evolutionLevel,
            timeCreated = _timeCreated, // Reset evolution tracking data
            totalTimeSinceCreation = _timeSinceCreation,
            nutritionConsumed = _nutritionConsumed,
            currentInteraction = _interactionCount
        };
        
        SaveSystem.SaveMon(data);
    }

    public float GetEvolutionProgress()
    {
        var req = GetNextEvolutionRequirement();
        if (req == null) return 1f;

        float timeProgress = req.minTimeAlive > 0 ? _timeSinceCreation / req.minTimeAlive : 1f;
        float foodProgress = req.minFoodConsumed > 0 ? (float)_nutritionConsumed / req.minFoodConsumed : 1f;
        float interactionProgress = req.minInteractions > 0 ? (float)_interactionCount / req.minInteractions : 1f;
        float happinessProgress = req.minCurrentHappiness > 0 ? _controller.StatsHandler.CurrentHappiness / req.minCurrentHappiness : 1f;
        float hungerProgress = req.minCurrentHunger > 0 ? _controller.StatsHandler.CurrentHunger / req.minCurrentHunger : 1f;

        return Mathf.Clamp01((timeProgress + foodProgress + interactionProgress + happinessProgress + hungerProgress) / 5f);
    }
    
    private void ResetMonsterData()
    {
        ResetEvolutionTracking();
        // Get max health for new evolution level
        var maxHealth = _controller.MonsterData.GetMaxHealth(_targetLevel);
        _controller.StatsHandler.Initialize(_controller.StatsHandler.CurrentHP,
                                          _controller.StatsHandler.CurrentHunger,
                                          _controller.StatsHandler.CurrentHappiness,
                                          maxHealth);
    }

}
