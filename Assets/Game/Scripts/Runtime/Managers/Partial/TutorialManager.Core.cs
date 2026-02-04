using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class TutorialManager
{
    [Header("Storage Config")]
    [Tooltip("Prefix key PlayerPrefs untuk status tutorial. Contoh: tutorial_")]
    [SerializeField] private string playerPrefsKeyPrefix = "tutorial_";

    [Header("Mode Tutorial")]
    [Tooltip("Jika true, gunakan daftar TutorialStep (dengan dialog, panel, dan progress store). Jika false, gunakan daftar GameObject sederhana yang di-set active berurutan.")]
    [SerializeField] private bool useTutorialSteps = true;

    [Header("Tutorial Steps")] 
    [Tooltip("Daftar semua tutorial / step yang ada di game.")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Dialog Config")]
    [Tooltip("Parent transform untuk instansiasi prefab dialog. Kalau kosong akan pakai root Canvas / transform ini.")]
    [SerializeField] private Transform dialogRoot;

    [Tooltip("Daftar step panel sederhana yang akan diaktifkan berurutan ketika tidak menggunakan TutorialStep (useTutorialSteps = false).")]
    [SerializeField] private List<SimpleTutorialPanelStep> simpleTutorialPanels = new List<SimpleTutorialPanelStep>();

    [Header("Simple Panels Animation")]
    [Tooltip("Durasi animasi saat panel sederhana muncul (slide up + fade).")]
    [SerializeField] private float simplePanelShowDuration = 0.4f;
    [Tooltip("Curve animasi slide up untuk panel sederhana.")]
    [SerializeField] private AnimationCurve simplePanelShowEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Global UI References")]
    [Tooltip("Button global untuk skip tutorial yang tetap boleh di-klik saat dialog tampil.")]
    [SerializeField] private Button skipTutorialButton;

    private ITutorialProgressStore _progressStore;
    private int _currentStepIndex = -1;
    private ITutorialDialogView _activeDialogView;
    private TutorialStep _activeDialogStep;
    private int _activeDialogIndex;

    private int _simplePanelIndex = -1;

    private Button[] _uiButtonsCache;
    private bool[] _uiButtonsInteractableCache;

    public static Button GlobalSkipTutorialButton { get; private set; }

    private bool IsSimpleMode => !useTutorialSteps;

    private void EnsureProgressStore()
    {
        if (_progressStore == null)
        {
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);
        }
    }
}
