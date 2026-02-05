using System.Collections;
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

    [Header("Tutorial Monster")]
    [Tooltip("Monster yang akan di-spawn khusus saat tutorial dimulai (mis. Briabit). Spawn menggunakan logika normal MonsterManager (random di game area).")]
    [SerializeField] private MonsterDataSO briabitTutorialData;

    private bool _briabitSpawned;
    private RectTransform _tutorialMonsterRect;

    private float _simpleStepShownTime;

    // Menyimpan tombol Next yang sudah pernah didaftarkan listener ShowNextSimplePanel,
    // supaya 1 tombol yang dipakai di beberapa simple step tidak memicu 2x panggilan.
    private readonly HashSet<Button> _simpleNextButtonsHooked = new();
    private Coroutine _simpleNextDelayRoutine;

    [Header("Mouse Hint Config")]
    [Tooltip("Prefab gambar hint mouse right-click. Akan di-instansiasi sekali dan diposisikan dekat pet saat step simple yang memintanya aktif.")]
    [SerializeField] private RectTransform rightClickMouseHintPrefab;

    private RectTransform _rightClickMouseHintInstance;

    public static Button GlobalSkipTutorialButton { get; private set; }

    private bool IsSimpleMode => !useTutorialSteps;

    private void EnsureProgressStore()
    {
        if (_progressStore == null)
        {
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);
        }
    }

    private void SpawnTutorialMonsterIfNeeded()
    {
        if (_briabitSpawned)
            return;

        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null)
            return;

        if (monsterManager.activeMonsters != null && monsterManager.activeMonsters.Count > 0)
        {
            var existing = monsterManager.activeMonsters[0];
            if (existing != null)
            {
                _tutorialMonsterRect = existing.GetComponent<RectTransform>();
            }
            _briabitSpawned = true;
            return;
        }

        if (briabitTutorialData != null)
        {
            int before = monsterManager.activeMonsters != null ? monsterManager.activeMonsters.Count : 0;
            monsterManager.SpawnMonster(briabitTutorialData);

            if (monsterManager.activeMonsters != null && monsterManager.activeMonsters.Count > before)
            {
                var controller = monsterManager.activeMonsters[monsterManager.activeMonsters.Count - 1];
                if (controller != null)
                {
                    _tutorialMonsterRect = controller.GetComponent<RectTransform>();
                }
            }
        }
        _briabitSpawned = true;
    }

    private bool IsClickOnTutorialMonster()
    {
        if (_tutorialMonsterRect == null)
            return false;

        var canvas = _tutorialMonsterRect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return false;

        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(_tutorialMonsterRect, Input.mousePosition, cam);
    }

    /// <summary>
    /// Meminta pindah ke simple panel berikutnya dengan jeda sesuai konfigurasi step aktif.
    /// Dipanggil dari tombol Next atau interaksi klik pet.
    /// </summary>
    private void RequestNextSimplePanel()
    {
        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
        {
            ShowNextSimplePanel();
            return;
        }

        if (_simplePanelIndex < 0 || _simplePanelIndex >= simpleTutorialPanels.Count)
        {
            ShowNextSimplePanel();
            return;
        }

        // Jika sudah ada timer jalan, abaikan klik tambahan supaya tidak dobel.
        if (_simpleNextDelayRoutine != null)
            return;

        var step = simpleTutorialPanels[_simplePanelIndex];
        float delay = step != null ? Mathf.Max(0f, step.nextStepDelay) : 0f;

        if (delay <= 0f)
        {
            ShowNextSimplePanel();
        }
        else
        {
            _simpleNextDelayRoutine = StartCoroutine(SimpleNextDelayRoutine(delay));
        }
    }

    private IEnumerator SimpleNextDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _simpleNextDelayRoutine = null;
        ShowNextSimplePanel();
    }
}
