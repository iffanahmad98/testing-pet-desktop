using System;
using System.Collections;
using UnityEngine;

public partial class TutorialManager
{
    private void OnEnable()
    {
        CoinController.OnAnyPlayerCollected += OnCoinCollectedByPlayer;
        _isSubscribedToMonsterPoopClean = false;
        TrySubscribePlacementManager();
        TrySubscribeMonsterPoopClean();

        if (_currentMode == TutorialMode.Plain)
        {
            if (hotelTutorials != null)
            {
                for (int i = 0; i < hotelTutorials.Count; i++)
                {
                    var step = hotelTutorials[i];
                    if (step != null && step.panelRoot != null)
                        step.panelRoot.SetActive(false);
                }
            }
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            if (plainTutorials != null)
            {
                for (int i = 0; i < plainTutorials.Count; i++)
                {
                    var step = plainTutorials[i];
                    if (step != null && step.panelRoot != null)
                        step.panelRoot.SetActive(false);
                }
            }
        }
    }

    private void OnDisable()
    {
        CoinController.OnAnyPlayerCollected -= OnCoinCollectedByPlayer;

        if (_isSubscribedToMonsterPoopClean)
        {
            var monsterManager = ServiceLocator.Get<MonsterManager>();
            if (monsterManager != null)
            {
                monsterManager.OnPoopCleaned -= OnPoopCleanedByPlayer;
            }
            _isSubscribedToMonsterPoopClean = false;
        }

        if (_isSubscribedToPlacementManager)
        {
            var placementManager = ServiceLocator.Get<PlacementManager>();
            if (placementManager != null)
            {
                placementManager.OnFoodPlacementConfirmed -= OnFoodPlacementConfirmed;
            }
            _isSubscribedToPlacementManager = false;
        }
    }

    private void Awake()
    {
        GlobalSkipTutorialButton = skipTutorialButton;
        // CacheUIButtonsFromUIManager();

        HideAllTutorialPanels();
        if (skipTutorialButton != null)
        {
            skipTutorialButton.onClick.RemoveAllListeners();
            skipTutorialButton.onClick.AddListener(() =>
            {
                if (_currentMode == TutorialMode.Plain)
                    SkipPlainTutorial();
                else if (_currentMode == TutorialMode.Hotel)
                    SkipHotelTutorial();
            });

            var cg = skipTutorialButton.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = skipTutorialButton.gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

    }

    private void OnDestroy()
    {
        if (GlobalSkipTutorialButton == skipTutorialButton)
        {
            GlobalSkipTutorialButton = null;
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SaveSystem.IsLoadFinished);

        TrySubscribePlacementManager();

        if (_currentMode == TutorialMode.Plain)
        {
            if (!ShouldRunPlainTutorialOnStart())
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                yield break;
            }

            GrantTutorialStartItemsIfNeeded();

            DisableUIManagerButtonsForTutorial();
            SpawnTutorialMonsterIfNeeded();
            TrySubscribeMonsterPoopClean();
            StartPlainTutorialSequence();
            ShowSkipButtonAnimated();
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            if (!ShouldRunHotelTutorialOnStart())
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                yield break;
            }

            // Lock camera untuk hotel tutorial
            LockCameraForTutorial();

            DisableUIManagerButtonsForTutorial();
            StartHotelTutorialSequence();
            ShowSkipButtonAnimated();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool ShouldRunPlainTutorialOnStart()
    {
        if (IsPlainTutorialAlreadyCompleted())
        {
            return false;
        }

        return plainTutorials != null && plainTutorials.Count > 0;
    }

    private bool ShouldRunHotelTutorialOnStart()
    {
        if (IsHotelTutorialAlreadyCompleted())
        {
            return false;
        }

        return hotelTutorials != null && hotelTutorials.Count > 0;
    }

    public bool HasAnyPending()
    {
        return false;
    }

    public bool TryStartNext()
    {
        // Step-based tutorial flow is no longer used.
        return false;
    }

    public void CompleteCurrent()
    {
        // Kept for ITutorialService compatibility; step-based flow has been removed.
    }

    public void ResetAll()
    {
        // No-op: there is no step-based progress to reset anymore.
    }

    public void SkipPlainTutorial()
    {
        var config = SaveSystem.PlayerConfig;
        if (config != null)
        {
            config.plainTutorialSkipped = true;
            SaveSystem.SaveAll();
        }

        if (plainTutorials != null)
        {
            for (int i = 0; i < plainTutorials.Count; i++)
            {
                var step = plainTutorials[i];
                if (step != null && step.panelRoot != null)
                    step.panelRoot.SetActive(false);
            }
        }
        _plainPanelIndex = -1;

        if (_plainNextDelayRoutine != null)
        {
            StopCoroutine(_plainNextDelayRoutine);
            _plainNextDelayRoutine = null;
        }

        if (_plainNextClickDelayRoutine != null)
        {
            StopCoroutine(_plainNextClickDelayRoutine);
            _plainNextClickDelayRoutine = null;
        }

        if (_plainFoodDropDelayRoutine != null)
        {
            StopCoroutine(_plainFoodDropDelayRoutine);
            _plainFoodDropDelayRoutine = null;
        }

        _foodDropCountForCurrentStep = 0;

        CancelHandPointerSubTutorial();

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
        }
        MarkPlainTutorialCompleted();

        HideRightClickMouseHint();
        HidePointerIfAny();
        RestoreUIManagerButtonsInteractable();

        Destroy(gameObject);
    }

    public void SkipHotelTutorial()
    {
        var config = SaveSystem.PlayerConfig;
        if (config != null)
        {
            config.hotelTutorialSkipped = true;
            SaveSystem.SaveAll();
        }

        if (hotelTutorials != null)
        {
            for (int i = 0; i < hotelTutorials.Count; i++)
            {
                var step = hotelTutorials[i];
                if (step != null && step.panelRoot != null)
                    step.panelRoot.SetActive(false);
            }
        }
        _hotelPanelIndex = -1;
        MarkHotelTutorialCompleted();
        RestoreUIManagerButtonsInteractable();
        CancelHandPointerSubTutorial();
        HidePointerIfAny();

        Destroy(gameObject);
    }
}
