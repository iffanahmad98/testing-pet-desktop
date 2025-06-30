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
    public bool IsEvolving => _isEvolving;
    private bool _isEvolving = false;
    private int _evolutionLevel = 1; // Current evolution level
    private int _targetLevel = 2; // Target evolution level for next evolution

    // Evolution tracking
    private float _timeSinceCreation;
    private string _timeCreated;
    private int _foodConsumed;
    private int _interactionCount;

    public bool CanEvolve => _controller?.MonsterData != null && _controller.MonsterData.canEvolve;
    public float TimeSinceCreation => _timeSinceCreation;
    public string TimeCreated => _timeCreated;
    public int FoodConsumed => _foodConsumed;
    public int InteractionCount => _interactionCount;

    public MonsterEvolutionHandler(MonsterController controller, SkeletonGraphic skeletonGraphic)
    {
        _controller = controller;
        _skeletonGraphic = skeletonGraphic;
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
        _foodConsumed = foodConsumed;
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
        _foodConsumed = 0;
        _interactionCount = 0;
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
        Debug.Log($"[Evolution] {_controller.MonsterData.monsterName} evolution started.");

        var originalPos = _skeletonGraphic.rectTransform.anchoredPosition;
        var originalScale = _skeletonGraphic.rectTransform.localScale.x;
        var originalParent = _controller.transform.parent;
        var monsterTransform = _controller.transform;
        var areaTransform = _controller.MonsterManager.gameArea.transform;

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
        var evolutionParticle = _controller.UI.evolutionVFX;
        var whiteFlashMaterial = _controller.UI.evolutionMaterial;

        // turn other monsters invisible
        int _remaining = 0;
        foreach (var monster in _controller.MonsterManager.activeMonsters)
        {
            _remaining = _controller.MonsterManager.activeMonsters.Count - 1; // -1 for self
            if (monster != _controller)
            {
                monster.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0f, 0.25f).SetEase(Ease.InOutSine);
                _remaining--;
                yield return new WaitForSeconds(0.1f); // stagger the fade 
            }
        }

        //turn coin, poop, and food collection invisible
        foreach (var coin in _controller.MonsterManager._activeCoins)
        {
            coin.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).SetEase(Ease.InOutSine);
        }
        foreach (var poop in _controller.MonsterManager._activePoops)
        {
            poop.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).SetEase(Ease.InOutSine);
        }
        foreach (var food in _controller.MonsterManager.activeFoods)
        {
            food.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).SetEase(Ease.InOutSine);
        }

        bool sequenceDone = false;
        MonsterEvolutionSequenceHelper.PlayEvolutionUISequence(
            evolveCam,
            spineGraphic,
            evolutionParticle,
            whiteFlashMaterial,
            nextSkeleton,
            () =>
            {
                _controller.evolutionLevel = _targetLevel;
                ResetMonsterData();
                ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.monsterName} evolved to level {_targetLevel}!", 3f);
                sequenceDone = true;
            },
            monsterPos
        );
        yield return new WaitUntil(() => sequenceDone);

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

        //turn coin, poop, and food collection visible
        foreach (var coin in _controller.MonsterManager._activeCoins)
        {
            coin.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        }
        foreach (var poop in _controller.MonsterManager._activePoops)
        {
            poop.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        }
        foreach (var food in _controller.MonsterManager.activeFoods)
        {
            food.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        }

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
    public void ResetMonsterData()
    {
        // Refresh Monster ID (already done during evolution)
        UpdateMonsterID(_targetLevel);

        // Reset tracking
        ResetEvolutionTracking();

        // Reload max HP and max Hunger for the new evolution level
        float newMaxHP = _controller.MonsterData.GetMaxHealth(_targetLevel);
        float newMaxHunger = _controller.MonsterData.GetMaxHunger(_targetLevel);

        // Reset and clamp stats
        _controller.StatsHandler.Initialize(
            initialHealth: newMaxHP,
            initialHunger: newMaxHunger,
            initialHappiness: _controller.MonsterData.baseHappiness,
            maxHP: newMaxHP
        );

        // Save the refreshed data
        _controller.SaveMonData();
    }

}
