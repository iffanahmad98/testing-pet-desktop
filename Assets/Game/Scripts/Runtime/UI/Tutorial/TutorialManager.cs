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
    private string _currentTutorialId;

    // State dialog yang sedang aktif (kalau step menggunakan dialog)
    private ITutorialDialogView _activeDialogView;
    private TutorialStep _activeDialogStep;
    private int _activeDialogIndex;

    private void Awake()
    {
        _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        HideAllTutorialPanels();

        foreach (var step in tutorialSteps)
        {
            if (step.nextButton != null)
            {
                var capturedId = step.id;
                step.nextButton.onClick.RemoveAllListeners();
                step.nextButton.onClick.AddListener(() => CompleteTutorial(capturedId));
            }
        }
    }
    public bool HasCompletedTutorial(string tutorialId)
    {
        return HasCompleted(tutorialId);
    }

    public bool TryStartTutorial(string tutorialId)
    {
        return TryStart(tutorialId);
    }

    public void CompleteTutorial(string tutorialId)
    {
        Complete(tutorialId);
    }

    public void ResetTutorial(string tutorialId)
    {
        Reset(tutorialId);
    }
    public void ResetAllTutorials()
    {
        ResetAll();
    }

    public bool HasCompleted(string tutorialId)
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        return _progressStore.IsCompleted(tutorialId);
    }

    public bool TryStart(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return false;

        if (HasCompleted(tutorialId))
            return false;

        var step = FindStepById(tutorialId);
        if (step == null)
        {
            Debug.LogWarning($"Tutorial step dengan ID '{tutorialId}' tidak ditemukan di TutorialManager.");
            return false;
        }

        _currentTutorialId = tutorialId;
        // Jika step ini dikonfigurasi memakai dialog, jalankan dialog sequence.
        if (step.useDialog && step.dialogPrefab != null && step.dialogLines != null && step.dialogLines.Count > 0)
        {
            StartDialogStep(step);
        }
        else
        {
            // Fallback ke perilaku lama: show panel biasa.
            ShowOnly(step);
        }
        return true;
    }

    public void Complete(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        _progressStore.MarkCompleted(tutorialId);

        var step = FindStepById(tutorialId);
        if (step != null && step.panelRoot != null)
        {
            step.panelRoot.SetActive(false);
        }

        if (_currentTutorialId == tutorialId)
            _currentTutorialId = null;
    }

    public void Reset(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        _progressStore.Clear(tutorialId);
    }

    public void ResetAll()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        foreach (var step in tutorialSteps)
        {
            if (string.IsNullOrEmpty(step.id))
                continue;

            _progressStore.Clear(step.id);
        }
    }

    // ------------ Dialog handling ------------

    private void StartDialogStep(TutorialStep step)
    {
        // Sembunyikan semua panel lain supaya fokus ke dialog.
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
    }

    private void OnDialogNextClicked()
    {
        if (_activeDialogStep == null || _activeDialogView == null)
            return;

        _activeDialogIndex++;

        if (_activeDialogIndex >= _activeDialogStep.dialogLines.Count)
        {
            // Sequence selesai: tutup dialog dan tandai tutorial complete.
            var finishedId = _currentTutorialId;

            var dialogGo = (_activeDialogView as MonoBehaviour)?.gameObject;
            if (dialogGo != null)
            {
                Destroy(dialogGo);
            }

            _activeDialogView = null;
            _activeDialogStep = null;
            _activeDialogIndex = 0;

            if (!string.IsNullOrEmpty(finishedId))
            {
                Complete(finishedId);
            }
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

        _activeDialogView.SetDialog(line.speakerName, line.text, isLast);
        _activeDialogView.Show();
    }
    private TutorialStep FindStepById(string tutorialId)
    {
        return tutorialSteps.Find(s => string.Equals(s.id, tutorialId, StringComparison.Ordinal));
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


