using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TutorialManager : MonoBehaviour, ITutorialService
{
    [Header("Storage Config")]
    [Tooltip("Prefix key PlayerPrefs untuk status tutorial. Contoh: tutorial_")]
    [SerializeField] private string playerPrefsKeyPrefix = "tutorial_";

    [Header("Tutorial Steps")]
    [Tooltip("Daftar semua tutorial / step yang ada di game.")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Dialog Config")]
    [Tooltip("Parent transform untuk instansiasi prefab dialog. Kalau kosong akan pakai root Canvas / transform ini.")]
    [SerializeField] private Transform dialogRoot;

    private ITutorialProgressStore _progressStore;
    private int _currentStepIndex = -1;
    private ITutorialDialogView _activeDialogView;
    private TutorialStep _activeDialogStep;
    private int _activeDialogIndex;

    private void Awake()
    {
        _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

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
    public bool HasAnyPending()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (!_progressStore.IsCompleted(i))
                return true;
        }

        return false;
    }

    public bool TryStartNext()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

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

        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        _progressStore.MarkCompleted(_currentStepIndex);

        var step = tutorialSteps[_currentStepIndex];
        if (step != null && step.panelRoot != null)
        {
            step.panelRoot.SetActive(false);
        }

        _currentStepIndex = -1;
    }

    public void ResetAll()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        _progressStore.ClearAll(tutorialSteps.Count);
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


