using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public partial class TutorialManager
{
    [Header("Storage Config")]
    [SerializeField] private string playerPrefsKeyPrefix = "tutorial_";

    [Header("Mode Tutorial")]
    [SerializeField] private bool useTutorialSteps = true;

    [Header("Tutorial Steps")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Dialog Config")]
    [SerializeField] private Transform dialogRoot;

    [SerializeField] private List<SimpleTutorialPanelStep> simpleTutorialPanels = new List<SimpleTutorialPanelStep>();

    [Header("Simple Panels Animation")]
    [SerializeField] private float simplePanelShowDuration = 0.4f;
    [SerializeField] private AnimationCurve simplePanelShowEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Global UI References")]
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
    [SerializeField] private MonsterDataSO briabitTutorialData;

    private bool _briabitSpawned;
    private RectTransform _tutorialMonsterRect;
    private MonsterController _tutorialMonsterController;

    private float _simpleStepShownTime;
    private readonly HashSet<Button> _simpleNextButtonsHooked = new();
    private Coroutine _simpleNextDelayRoutine;

    [Header("Mouse Hint Config")]
    [SerializeField] private RectTransform rightClickMouseHintPrefab;

    private RectTransform _rightClickMouseHintInstance;

    public static Button GlobalSkipTutorialButton { get; private set; }

    private bool IsSimpleMode => !useTutorialSteps;

    private void OnEnable()
    {
        CoinController.OnAnyPlayerCollected += OnCoinCollectedByPlayer;
    }

    private void OnDisable()
    {
        CoinController.OnAnyPlayerCollected -= OnCoinCollectedByPlayer;
    }

    private void EnsureProgressStore()
    {
        if (_progressStore == null)
        {
            _progressStore = new PlayerPrefsTutorialProgressStore(playerPrefsKeyPrefix);
        }
    }

    private void OnCoinCollectedByPlayer(CoinController coin)
    {
        if (!IsSimpleMode)
            return;

        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_simplePanelIndex < 0 || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        var step = simpleTutorialPanels[_simplePanelIndex];
        if (step == null || !step.useCoinCollectAsNext)
            return;

        RequestNextSimplePanel();
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
                SetupTutorialMonsterController(existing);
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
                    SetupTutorialMonsterController(controller);
                }
            }
        }
        _briabitSpawned = true;
    }

    private void SetupTutorialMonsterController(MonsterController controller)
    {
        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.OnClicked -= OnTutorialMonsterClicked;
        }

        _tutorialMonsterController = controller;

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.OnClicked += OnTutorialMonsterClicked;
        }
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
