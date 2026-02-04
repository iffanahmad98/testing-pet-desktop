using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public partial class TutorialManager : MonoBehaviour, ITutorialService
{
    private void Awake()
    {
        GlobalSkipTutorialButton = skipTutorialButton;

        _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        CacheUIButtonsFromUIManager();

        HideAllTutorialPanels();

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
            DisableUIManagerButtonsForTutorial();
            StartSimpleTutorialSequence();
            return;
        }

        if (!HasAnyPending())
        {
            Debug.Log("TutorialManager: semua tutorial sudah selesai, skip auto-start.");
            return;
        }

        DisableUIManagerButtonsForTutorial();
        var started = TryStartNext();
        if (!started)
        {
            Debug.LogWarning("TutorialManager: gagal auto-start tutorial berikutnya. Cek konfigurasi tutorialSteps.");
        }
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
                    Destroy(dialogGo);
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
        }

        RestoreUIManagerButtonsInteractable();
        gameObject.SetActive(false);
    }

    private void StartSimpleTutorialSequence()
    {
        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
        {
            Debug.LogWarning("TutorialManager: simpleTutorialPanels kosong, tidak ada tutorial sederhana yang ditampilkan.");
            return;
        }
        for (int i = 0; i < simpleTutorialPanels.Count; i++)
        {
            var step = simpleTutorialPanels[i];
            if (step == null)
                continue;

            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);

            var nextButton = GetSimpleStepNextButton(step);
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(ShowNextSimplePanel);
            }
        }

        _simplePanelIndex = 0;
        if (simpleTutorialPanels[_simplePanelIndex] != null && simpleTutorialPanels[_simplePanelIndex].panelRoot != null)
        {
            PlaySimplePanelShowAnimation(simpleTutorialPanels[_simplePanelIndex].panelRoot);
        }
    }
    public void ShowNextSimplePanel()
    {
        if (useTutorialSteps)
            return;

        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_simplePanelIndex < 0)
        {
            StartSimpleTutorialSequence();
            return;
        }

        if (_simplePanelIndex < simpleTutorialPanels.Count)
        {
            var currentStep = simpleTutorialPanels[_simplePanelIndex];
            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);
        }

        _simplePanelIndex++;

        if (_simplePanelIndex >= simpleTutorialPanels.Count)
        {
            RestoreUIManagerButtonsInteractable();
            gameObject.SetActive(false);
            return;
        }

        var nextStep = simpleTutorialPanels[_simplePanelIndex];
        if (nextStep != null && nextStep.panelRoot != null)
        {
            PlaySimplePanelShowAnimation(nextStep.panelRoot);
        }
    }

    private void PlaySimplePanelShowAnimation(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);

        var rect = panel.GetComponent<RectTransform>();
        var canvasGroup = panel.GetComponent<CanvasGroup>();

        if (rect == null || canvasGroup == null)
            return;

        rect.DOKill();
        canvasGroup.DOKill();

        var targetPos = rect.anchoredPosition;
        var startPos = targetPos;
        startPos.y -= rect.rect.height;
        rect.anchoredPosition = startPos;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        rect.DOAnchorPos(targetPos, simplePanelShowDuration).SetEase(simplePanelShowEase);
        canvasGroup.DOFade(1f, simplePanelShowDuration).SetEase(Ease.OutQuad);
    }


    private void StartDialogStep(TutorialStep step)
    {
        HideAllTutorialPanels();

        _activeDialogStep = step;
        _activeDialogIndex = 0;

        Transform parent = dialogRoot != null ? dialogRoot : transform.root;
        var viewInstance = Instantiate(step.dialogPrefab, parent);
        _activeDialogView = viewInstance;

        BindDialogNextButton();
        ShowCurrentDialogLine();
    }

    private void BindDialogNextButton()
    {
        if (_activeDialogView == null)
            return;

        var nextButton = _activeDialogView.NextButton;
        if (nextButton == null)
            return;

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnDialogNextClicked);
        Debug.Log($"[TutorialManager] BindDialogNextButton ke {nextButton.gameObject.name}");
    }

    private void OnDialogNextClicked()
    {

        Debug.Log($"TutorialManager: Next dialog line untuk stepIndex={_currentStepIndex}, index sebelumnya = {_activeDialogIndex}.");
        _activeDialogIndex++;

        if (_activeDialogIndex >= _activeDialogStep.dialogLines.Count)
        {
            Debug.Log($"TutorialManager: semua dialog line untuk stepIndex={_currentStepIndex} sudah selesai. Menghancurkan dialog dan menandai step selesai.");
            var dialogGo = (_activeDialogView as MonoBehaviour)?.gameObject;
            if (dialogGo != null)
            {
                Destroy(dialogGo);
            }

            _activeDialogView = null;
            _activeDialogStep = null;
            _activeDialogIndex = 0;

            Debug.Log($"TutorialManager: dialog tutorial untuk stepIndex={_currentStepIndex} selesai, memanggil CompleteCurrent.");
            CompleteCurrent();
        }
        else
        {
            ShowCurrentDialogLine();
        }
    }

    private void ShowCurrentDialogLine()
    {
        if (_activeDialogView == null || _activeDialogStep == null)
            return;

        if (_activeDialogIndex < 0 || _activeDialogIndex >= _activeDialogStep.dialogLines.Count)
            return;

        var line = _activeDialogStep.dialogLines[_activeDialogIndex];
        bool isLast = _activeDialogIndex == _activeDialogStep.dialogLines.Count - 1;

        Debug.Log($"TutorialManager: tampilkan dialog line {_activeDialogIndex + 1}/{_activeDialogStep.dialogLines.Count} untuk stepIndex={_currentStepIndex}.");
        _activeDialogView.SetDialog(line.speakerName, line.text, isLast);
        _activeDialogView.Show();
    }

    private void HideAllTutorialPanels()
    {
        foreach (var step in tutorialSteps)
        {
            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);
        }
    }

    private void ShowOnly(TutorialStep stepToShow)
    {
        foreach (var step in tutorialSteps)
        {
            if (step.panelRoot == null)
                continue;

            step.panelRoot.SetActive(step == stepToShow);
        }
    }
}


