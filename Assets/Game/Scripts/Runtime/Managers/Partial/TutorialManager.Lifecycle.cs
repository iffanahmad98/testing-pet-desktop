using System;
using UnityEngine;

public partial class TutorialManager
{
    private void Update()
    {
        if (_currentMode == TutorialMode.Plain)
        {
            if (_plainPanelIndex < 0 || plainTutorials == null || _plainPanelIndex >= plainTutorials.Count)
                return;

            TrySubscribePlacementManager();
            TrySubscribeMonsterPoopClean();

            var step = plainTutorials[_plainPanelIndex];
            var config = step != null ? step.config : null;
            if (config == null)
                return;

            if (_tutorialMonsterRect == null)
            {
                var monsterManager = ServiceLocator.Get<MonsterManager>();
                if (monsterManager != null &&
                    monsterManager.activeMonsters != null &&
                    monsterManager.activeMonsters.Count > 0)
                {
                    var controller = monsterManager.activeMonsters[0];
                    if (controller != null)
                    {
                        _tutorialMonsterRect = controller.GetComponent<RectTransform>();
                        SetupTutorialMonsterController(controller);

                        MoveTutorialMonsterForPlainStep(step);
                        UpdateRightClickMouseHintForPlainStep(step);
                    }
                }
            }

            if (config.usePointer)
            {
                UpdatePointerForPlainStep(step);
            }

            UpdateCurrentHandPointerOffsetRealtime();

            UpdatePlainStepNextButtonsInteractable();
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            if (_hotelPanelIndex < 0 || hotelTutorials == null || _hotelPanelIndex >= hotelTutorials.Count)
                return;

            var step = hotelTutorials[_hotelPanelIndex];
            var config = step != null ? step.config : null;
            if (config == null)
                return;

            UpdatePointerForHotelStep(step);
            UpdateHotelStepNextButtonsInteractable();
        }
    }
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
        else if (_currentMode == TutorialMode.Hotel && Hotel != null)
        {
            if (plainTutorials != null)
            {
                Debug.Log("Disabling plain tutorial panels because hotel tutorial is active.");
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
        CacheAllButtonsForHotelMode();
        InitHandPointerResolver();

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

    private void Start()
    {
        if (_currentMode == TutorialMode.Plain)
        {
            if (!ShouldRunPlainTutorialOnStart())
            {
                gameObject.SetActive(false);
                return;
            }
            GrantTutorialStartItemsIfNeeded();
            DisableUIManagerButtonsForTutorial();
            SpawnTutorialMonsterIfNeeded();
            StartPlainTutorialSequence();
            ShowSkipButtonAnimated();
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            if (!ShouldRunHotelTutorialOnStart())
            {
                gameObject.SetActive(false);
                return;
            }
            StartHotelTutorialSequence();
            ShowSkipButtonAnimated();
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
        _uiButtonsCache = null;
        _uiButtonsInteractableCache = null;

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
        }
        MarkPlainTutorialCompleted();

        HideRightClickMouseHint();
        HidePointerIfAny();
        RestoreUIManagerButtonsInteractable();

        gameObject.SetActive(false);
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
        HidePointerIfAny();

        gameObject.SetActive(false);
    }
}
