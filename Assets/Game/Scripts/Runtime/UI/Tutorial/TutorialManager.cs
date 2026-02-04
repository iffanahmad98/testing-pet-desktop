using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Initial Tutorial Pet")]
    [Tooltip("Monster yang akan di-spawn saat tutorial pertama kali dimulai (misalnya Briarbit). Opsional.")]
    [SerializeField] private MonsterDataSO initialTutorialPet;
    [Tooltip("Titik spawn untuk pet awal tutorial (pakai anchoredPosition dari RectTransform ini). Opsional, default (0,0) kalau kosong.")]
    [SerializeField] private RectTransform initialTutorialPetSpawnPoint;

    [Header("Cursor Tutorial Guide")]
    [Tooltip("Script untuk menampilkan animasi cursor ke arah pet saat tutorial dimulai.")]
    [SerializeField] private TutorialCursorGuide cursorGuide;

    [Header("Global UI References")]
    [Tooltip("Button global untuk skip tutorial yang tetap boleh di-klik saat dialog tampil.")]
    [SerializeField] private Button skipTutorialButton;

    private ITutorialProgressStore _progressStore;
    private int _currentStepIndex = -1;
    private ITutorialDialogView _activeDialogView;
    private TutorialStep _activeDialogStep;
    private int _activeDialogIndex;

    public static Button GlobalSkipTutorialButton { get; private set; }

    private void Awake()
    {
        GlobalSkipTutorialButton = skipTutorialButton;

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

    private void OnDestroy()
    {
        if (GlobalSkipTutorialButton == skipTutorialButton)
        {
            GlobalSkipTutorialButton = null;
        }
    }

    private void Start()
    {
        if (!HasAnyPending())
        {
            Debug.Log("TutorialManager: semua tutorial sudah selesai, skip auto-start.");
            return;
        }

        var started = TryStartNext();
        if (!started)
        {
            Debug.LogWarning("TutorialManager: gagal auto-start tutorial berikutnya. Cek konfigurasi tutorialSteps.");
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

        var spawnedPet = EnsureInitialTutorialPet();
        if (spawnedPet != null && cursorGuide != null)
        {
            var petRect = spawnedPet.GetComponent<RectTransform>();
            cursorGuide.PlayGuideToTarget(petRect);
        }

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
        this.gameObject.SetActive(false);
    }

    public void ResetAll()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

        _progressStore.ClearAll(tutorialSteps.Count);
    }

    public void SkipAllTutorials()
    {
        if (_progressStore == null)
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);

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

        if (cursorGuide != null)
        {
            cursorGuide.StopGuide();
        }

        gameObject.SetActive(false);
    }

    private MonsterController EnsureInitialTutorialPet()
    {
        if (initialTutorialPet == null)
            return null;

        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null)
        {
            Debug.LogWarning("TutorialManager: MonsterManager tidak ditemukan, skip spawn pet awal.");
            return null;
        }

        if (monsterManager.activeMonsters != null && monsterManager.activeMonsters.Count > 0)
            return null;

        Vector2 spawnPos = Vector2.zero;
        if (initialTutorialPetSpawnPoint != null)
        {
            spawnPos = initialTutorialPetSpawnPoint.anchoredPosition;
        }

        Debug.Log("TutorialManager: spawn initial tutorial pet di posisi " + spawnPos);
        return monsterManager.SpawnMonsterAtCenterForTutorial(initialTutorialPet, spawnPos);
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


