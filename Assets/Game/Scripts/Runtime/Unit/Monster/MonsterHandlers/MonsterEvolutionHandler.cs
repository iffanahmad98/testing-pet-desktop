using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

[Serializable]
public class MonsterEvolutionSaveData
{
    public string monsterId;
    public float timeSinceCreation;
    public int foodConsumed;
    public int interactionCount;
}

[Serializable]
public class MonsterEvolutionHandler
{
    private EvolutionRequirementsSO _evolutionConfig;
    private MonsterController _controller;

    // Evolution tracking
    private float _lastUpdateTime;
    private float _lastEvolutionTime = -1f;
    private float _timeSinceCreation;
    private int _foodConsumed;
    private int _interactionCount;

    public bool CanEvolve => _controller?.MonsterData != null && _controller.MonsterData.canEvolve && !_controller.MonsterData.isFinalEvol;
    public float TimeSinceCreation => _timeSinceCreation;
    public int FoodConsumed => _foodConsumed;
    public int InteractionCount => _interactionCount;

    private ParticleSystem _evolutionParticle;
    private RectTransform _monsterRectTransform;
    private CanvasGroup _evolutionParticleCanvasGroup;

    public MonsterEvolutionHandler(MonsterController controller)
    {
        _controller = controller;
    }

    private void InitializeEvolutionRequirements()
    {
        if (_controller == null)
        {
            Debug.LogWarning("[Evolution] Controller is null, cannot initialize evolution requirements");
            return;
        }

        if (_controller.MonsterData == null)
        {
            Debug.LogWarning($"[Evolution] MonsterData is null for {_controller.monsterID}, cannot initialize evolution requirements");
            return;
        }

        if (_controller.MonsterData.evolutionRequirements == null)
        {
            Debug.LogWarning($"[Evolution] No evolution requirements found for {_controller.MonsterData.monsterName} ({_controller.monsterID})");
            return;
        }
        
        _evolutionConfig = _controller.MonsterData.evolutionRequirements;
    }

    public void InitializeWithMonsterData()
    {
        if (_evolutionConfig == null)
        {
            InitializeEvolutionRequirements();
        }
    }

    public void InitUIParticles(MonsterUIHandler uiHandler)
    {
        _evolutionParticle = uiHandler.evolutionEffect;
        _evolutionParticleCanvasGroup = uiHandler.evolutionEffectCg;
    }

    private EvolutionRequirement[] GetAvailableEvolutions()
    {
        if (_evolutionConfig == null || _evolutionConfig.requirements == null)
        {
            return new EvolutionRequirement[0];
        }
        var available = _evolutionConfig.requirements
          .Where(req => req.targetEvolutionLevel == _controller.evolutionLevel + 1)
          .ToArray();

        return available;
    }

    public void LoadEvolutionData(float timeSinceCreation, int foodConsumed, int interactionCount)
    {
        _timeSinceCreation = timeSinceCreation;
        _foodConsumed = foodConsumed;
        _interactionCount = interactionCount;
    }

    public void UpdateEvolutionTracking(float deltaTime)
    {
        if (!CanEvolve || _controller?.MonsterData == null) return;

        _timeSinceCreation += deltaTime;
        if (Time.time - _lastUpdateTime >= 5f)
        {
            CheckEvolutionConditions();
            _lastUpdateTime = Time.time;
        }
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
    }    private void CheckEvolutionConditions()
    {
        if (!CanEvolve) return;

        var nextEvolution = GetNextEvolutionRequirement();
        if (nextEvolution == null) return;

        if (MeetsEvolutionRequirements(nextEvolution))
        {
            TriggerEvolution();
        }
    }    private EvolutionRequirement GetNextEvolutionRequirement()
    {
        int currentLevel = _controller.evolutionLevel;

        foreach (var requirement in GetAvailableEvolutions())
        {
            if (requirement.targetEvolutionLevel == currentLevel + 1)
            {
                return requirement;
            }
        }

        return null;
    }

    private bool MeetsEvolutionRequirements(EvolutionRequirement requirement)
    {
        bool timeCheck = _timeSinceCreation >= requirement.minTimeAlive;
        if (!timeCheck) return false;

        bool foodCheck = _foodConsumed >= requirement.minFoodConsumed;
        if (!foodCheck) return false;

        bool interactionCheck = _interactionCount >= requirement.minInteractions;
        if (!interactionCheck) return false;

        bool happinessCheck = _controller.currentHappiness >= requirement.minCurrentHappiness;
        if (!happinessCheck) return false;

        bool hungerCheck = _controller.currentHunger >= requirement.minCurrentHunger;
        if (!hungerCheck) return false;

        bool customCheck = requirement.customCondition?.Invoke(_controller) ?? true;

        return customCheck;
    }    public void TriggerEvolution()
    {
        if (!CanEvolve) return;
        
        var oldLevel = _controller.evolutionLevel;
        var newLevel = oldLevel + 1;

        StartSimpleEvolutionEffect(oldLevel, newLevel);
    }    private void StartSimpleEvolutionEffect(int oldLevel, int newLevel)
    {
        _controller.StartCoroutine(EvolutionSequence(oldLevel, newLevel));
    }    private IEnumerator EvolutionSequence(int oldLevel, int newLevel)
    {
        _monsterRectTransform = _controller.GetComponent<RectTransform>();

        yield return _controller.StartCoroutine(PreEvolutionEffects());

        yield return _controller.StartCoroutine(ShowEvolutionParticles());

        _controller.evolutionLevel = newLevel;
        UpdateMonsterID(newLevel);

        yield return _controller.StartCoroutine(PostEvolutionEffects());

        _controller.UpdateVisuals();
        OnEvolutionComplete(oldLevel, newLevel);
    }    private IEnumerator PreEvolutionEffects()
    {
        var originalScale = _monsterRectTransform.localScale;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        float scaleTime = 0.5f;
        float elapsed = 0f;
        Vector3 targetScale = originalScale * 1.2f;

        while (elapsed < scaleTime)
        {
            _monsterRectTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / scaleTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }    private IEnumerator ShowEvolutionParticles()
    {
        if (_evolutionParticleCanvasGroup == null) yield break;

        _evolutionParticle = _evolutionParticle.GetComponent<ParticleSystem>();
        if (_evolutionParticle != null)
        {
            _evolutionParticle.Play();
        }

        float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            _evolutionParticleCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            _evolutionParticleCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_evolutionParticle != null)
        {
            _evolutionParticle.Stop();
        }
    }    private IEnumerator PostEvolutionEffects()
    {
        var originalScale = _monsterRectTransform.localScale;
        var targetScale = Vector3.one;

        float scaleTime = 0.8f;
        float elapsed = 0f;

        while (elapsed < scaleTime)
        {
            float t = elapsed / scaleTime;
            float bounceT = Mathf.Sin(t * Mathf.PI * 2) * 0.1f + t;
            _monsterRectTransform.localScale = Vector3.Lerp(originalScale, targetScale, bounceT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _monsterRectTransform.localScale = targetScale;

        yield return new WaitForSeconds(0.2f);
    }    private void UpdateMonsterID(int newLevel)
    {
        var oldID = _controller.monsterID;
        var parts = _controller.monsterID.Split('_'); 
        if (parts.Length >= 3)
        {
            _controller.monsterID = $"{parts[0]}_Lv{newLevel}_{parts[2]}";

            var gameManager = ServiceLocator.Get<GameManager>();
            if (gameManager != null)
            {
                var savedIDs = SaveSystem.LoadSavedMonIDs();
                if (savedIDs.Contains(oldID))
                {
                    savedIDs.Remove(oldID);
                    savedIDs.Add(_controller.monsterID);
                    SaveSystem.SaveMonIDs(savedIDs);

                    gameManager.RemoveSavedMonsterID(oldID);
                    gameManager.AddSavedMonsterID(_controller.monsterID);
                }
            }

            SaveSystem.DeleteMon(oldID);
        }
        else
        {
            Debug.LogWarning($"[Evolution] Could not update monster ID format for {_controller.monsterID}");
        }
    }    private void OnEvolutionComplete(int oldLevel, int newLevel)
    {
        ServiceLocator.Get<UIManager>()?.ShowMessage($"{_controller.MonsterData.monsterName} evolved to level {newLevel}!", 3f);

        _lastEvolutionTime = Time.time;

        _foodConsumed = 0;
        _interactionCount = 0;

        _controller.SaveMonData();
    }

    // Get evolution progress for UI
    public float GetEvolutionProgress()
    {
        var nextRequirement = GetNextEvolutionRequirement();
        if (nextRequirement == null)
        {
            return 1f;
        }

        float progress = 0f;
        int conditions = 0;

        if (nextRequirement.minTimeAlive > 0)
        {
            progress += Mathf.Clamp01(_timeSinceCreation / nextRequirement.minTimeAlive);
            conditions++;
        }

        if (nextRequirement.minFoodConsumed > 0)
        {
            progress += Mathf.Clamp01((float)_foodConsumed / nextRequirement.minFoodConsumed);
            conditions++;
        }

        if (nextRequirement.minInteractions > 0)
        {
            progress += Mathf.Clamp01((float)_interactionCount / nextRequirement.minInteractions);
            conditions++;
        }

        if (nextRequirement.minCurrentHappiness > 0)
        {
            progress += Mathf.Clamp01(_controller.currentHappiness / nextRequirement.minCurrentHappiness);
            conditions++;
        }        if (nextRequirement.minCurrentHunger > 0)
        {
            progress += Mathf.Clamp01(_controller.currentHunger / nextRequirement.minCurrentHunger);
            conditions++;
        }
        
        float finalProgress = conditions > 0 ? progress / conditions : 1f;

        return finalProgress;
    }
}
