using System;
using UnityEngine;

public partial class TutorialManager
{
    private void Awake()
    {
        GlobalSkipTutorialButton = skipTutorialButton;

        _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        CacheUIButtonsFromUIManager();

        HideAllTutorialPanels();
        if (skipTutorialButton != null)
        {
            skipTutorialButton.onClick.RemoveListener(SkipAllTutorials);
            skipTutorialButton.onClick.AddListener(SkipAllTutorials);
        }

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            var stepIndex = i;
            var step = tutorialSteps[i];
            if (step.nextButton != null)
            {
                step.nextButton.onClick.RemoveAllListeners();
                step.nextButton.onClick.AddListener(() =>
                {
                    _currentStepIndex = stepIndex;
                    CompleteCurrent();
                });
            }
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
        if (IsSimpleMode)
        {
            if (!ShouldRunSimpleTutorialOnStart())
            {
                gameObject.SetActive(false);
                return;
            }

            GrantTutorialStartItemsIfNeeded();

            DisableUIManagerButtonsForTutorial();
            SpawnTutorialMonsterIfNeeded();
            StartSimpleTutorialSequence();
            return;
        }

        if (!ShouldRunStepTutorialOnStart())
        {
            gameObject.SetActive(false);
            return;
        }

        DisableUIManagerButtonsForTutorial();
        SpawnTutorialMonsterIfNeeded();
        var started = TryStartNext();
        if (!started)
        {
            Debug.LogWarning("TutorialManager: gagal auto-start tutorial berikutnya. Cek konfigurasi tutorialSteps.");
        }
    }

    private bool ShouldRunSimpleTutorialOnStart()
    {
        if (IsSimpleTutorialAlreadyCompleted())
        {
            return false;
        }

        return simpleTutorialPanels != null && simpleTutorialPanels.Count > 0;
    }

    private bool ShouldRunStepTutorialOnStart()
    {
        if (!HasAnyPending())
        {
            Debug.Log("TutorialManager: semua tutorial sudah selesai, skip auto-start.");
            return false;
        }

        return true;
    }

    public bool HasAnyPending()
    {
        if (IsSimpleMode)
            return false;

        EnsureProgressStore();

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (!_progressStore.IsCompleted(i))
                return true;
        }

        return false;
    }

    public bool TryStartNext()
    {
        if (IsSimpleMode)
            return false;

        EnsureProgressStore();

        TutorialStep step = null;
        _currentStepIndex = -1;

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (_progressStore.IsCompleted(i))
                continue;

            if (tutorialSteps[i] == null)
                continue;

            _currentStepIndex = i;
            step = tutorialSteps[i];
            break;
        }

        if (step == null)
            return false;

        if (step.useDialog && step.dialogPrefab != null && step.dialogLines != null && step.dialogLines.Count > 0)
        {
            StartDialogStep(step);
        }
        else
        {
            ShowOnly(step);
        }

        return true;
    }

    public void CompleteCurrent()
    {
        if (_currentStepIndex < 0 || _currentStepIndex >= tutorialSteps.Count)
            return;

        EnsureProgressStore();

        _progressStore.MarkCompleted(_currentStepIndex);

        var step = tutorialSteps[_currentStepIndex];
        if (step != null && step.panelRoot != null)
        {
            step.panelRoot.SetActive(false);
        }

        _currentStepIndex = -1;
        HidePointerIfAny();
        RestoreUIManagerButtonsInteractable();
        this.gameObject.SetActive(false);
    }

    public void ResetAll()
    {
        if (IsSimpleMode)
            return;

        EnsureProgressStore();
        _progressStore.ClearAll(tutorialSteps.Count);
    }

    public void SkipAllTutorials()
    {
        if (!IsSimpleMode)
        {
            EnsureProgressStore();

            for (int i = 0; i < tutorialSteps.Count; i++)
            {
                _progressStore.MarkCompleted(i);
            }
            HideAllTutorialPanels();

            if (_activeDialogView != null)
            {
                var dialogGo = (_activeDialogView as MonoBehaviour)?.gameObject;
                if (dialogGo != null)
                {
                    UnityEngine.Object.Destroy(dialogGo);
                }
            }

            _activeDialogView = null;
            _activeDialogStep = null;
            _activeDialogIndex = 0;
            _currentStepIndex = -1;
        }
        else
        {
            if (simpleTutorialPanels != null)
            {
                for (int i = 0; i < simpleTutorialPanels.Count; i++)
                {
                    var step = simpleTutorialPanels[i];
                    if (step != null && step.panelRoot != null)
                        step.panelRoot.SetActive(false);
                }
            }

            _simplePanelIndex = -1;

            if (_tutorialMonsterController != null)
            {
                _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
            }

            MarkSimpleTutorialCompleted();
        }

        RestoreUIManagerButtonsInteractable();
        gameObject.SetActive(false);
    }
}
