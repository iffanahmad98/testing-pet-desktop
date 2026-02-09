using System;
using UnityEngine;

public partial class TutorialManager
{
    private void Awake()
    {
        GlobalSkipTutorialButton = skipTutorialButton;
        CacheUIButtonsFromUIManager();

        HideAllTutorialPanels();
        if (skipTutorialButton != null)
        {
            skipTutorialButton.onClick.RemoveListener(SkipAllTutorials);
            skipTutorialButton.onClick.AddListener(SkipAllTutorials);

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

    private bool ShouldRunPlainTutorialOnStart()
    {
        if (IsPlainTutorialAlreadyCompleted())
        {
            return false;
        }

        return plainTutorials != null && plainTutorials.Count > 0;
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

    public void SkipAllTutorials()
    {
        var config = SaveSystem.PlayerConfig;
        if (config != null)
        {
            config.allStepTutorialsSkippedGlobal = true;
            SaveSystem.SaveAll();
        }

        HideAllTutorialPanels();

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

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
        }
        MarkPlainTutorialCompleted();

        HideRightClickMouseHint();
        HidePointerIfAny();
        RestoreUIManagerButtonsInteractable();
        HideSkipButtonAnimated();
        gameObject.SetActive(false);
    }
}
