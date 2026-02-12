using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public partial class TutorialManager
{
    [Header("Tutorial Mode")]
    [SerializeField] private TutorialMode _currentMode = TutorialMode.Plain;
    private enum TutorialMode { Plain, Hotel }

    [SerializeField] private GameObject Hotel;

    [Header("Camera Control")]
    [SerializeField] private MagicalGarden.Farm.CameraDragMove cameraController;

    [Header("Plain Tutorial Panels")]
    [SerializeField] private List<PlainTutorialPanelStep> plainTutorials = new List<PlainTutorialPanelStep>();
    private int _plainPanelIndex = -1;
    private float _plainStepShownTime;
    private readonly HashSet<Button> _plainNextButtonsHooked = new();
    private Coroutine _plainNextDelayRoutine;
    private Coroutine _plainNextClickDelayRoutine;
    private Coroutine _plainFoodDropDelayRoutine;
    [Header("Hotel Tutorial Panels")]
    [SerializeField] private List<HotelTutorialPanelStep> hotelTutorials = new List<HotelTutorialPanelStep>();
    private int _hotelPanelIndex = -1;
    private float _hotelStepShownTime;
    private readonly HashSet<Button> _hotelNextButtonsHooked = new();
    private Coroutine _hotelNextDelayRoutine;

    [Header("Panels Animation")]
    [SerializeField] private float plainPanelShowDuration = 0.4f;
    [SerializeField] private AnimationCurve plainPanelShowEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Global UI References")]
    [SerializeField] private Button skipTutorialButton;

    private Button[] _uiButtonsCache;
    private bool[] _uiButtonsInteractableCache;
    private bool[] _uiButtonsActiveCache;

    public GameObject[] gotohotel;

    [Header("Tutorial Monster")]
    [SerializeField] private MonsterDataSO briabitTutorialData;

    private bool _briabitSpawned;
    private RectTransform _tutorialMonsterRect;
    private MonsterController _tutorialMonsterController;

    [Header("Tutorial Start Items")]
    [SerializeField] private TutorialItemReward[] tutorialStartItems;

    private int _foodDropCountForCurrentStep;

    [Header("Mouse Hint Config")]
    [SerializeField] private RectTransform rightClickMouseHintPrefab;

    private RectTransform _rightClickMouseHintInstance;

    public static Button GlobalSkipTutorialButton { get; private set; }

    // Enhanced button protection system with automatic restoration
    private static readonly Dictionary<Button, TutorialButtonProtection> _protectedButtons = new Dictionary<Button, TutorialButtonProtection>();

    public static bool IsButtonProtectedByTutorial(Button button)
    {
        return button != null && _protectedButtons.ContainsKey(button);
    }

    private static void AddProtectedButton(Button button, System.Action tutorialListener)
    {
        Debug.Log($"[TutorialProtection] Adding protected button '{button?.name}'");
        if (button == null || tutorialListener == null) return;

        if (!_protectedButtons.ContainsKey(button))
        {
            _protectedButtons[button] = new TutorialButtonProtection(button, tutorialListener);
        }
    }

    private static void RemoveProtectedButton(Button button)
    {
        if (button != null && _protectedButtons.TryGetValue(button, out var protection))
        {
            protection.StopMonitoring();
            _protectedButtons.Remove(button);
        }
    }

    private class TutorialButtonProtection
    {
        private readonly Button _button;
        private readonly System.Action _tutorialListener;
        private UnityEngine.Coroutine _monitorCoroutine;

        public TutorialButtonProtection(Button button, System.Action tutorialListener)
        {
            _button = button;
            _tutorialListener = tutorialListener;
        }

        public void StartMonitoring()
        {
            if (_monitorCoroutine == null && _button != null)
            {
                var instance = UnityEngine.Object.FindObjectOfType<TutorialManager>();
                if (instance != null)
                {
                    _monitorCoroutine = instance.StartCoroutine(MonitorAndRestoreListener());
                }
            }
        }

        public void StopMonitoring()
        {
            if (_monitorCoroutine != null)
            {
                var instance = UnityEngine.Object.FindObjectOfType<TutorialManager>();
                if (instance != null)
                {
                    instance.StopCoroutine(_monitorCoroutine);
                }
                _monitorCoroutine = null;
            }
        }

        private System.Collections.IEnumerator MonitorAndRestoreListener()
        {
            while (_button != null)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);

                // Check if tutorial listener still exists
                if (!HasTutorialListener())
                {
                    Debug.LogWarning($"[TutorialProtection] Listener removed from '{_button.name}'! Restoring...");
                    RestoreListener();
                }
            }
        }

        private bool HasTutorialListener()
        {
            if (_button?.onClick == null) return false;

            var listenerCount = _button.onClick.GetPersistentEventCount();
            return listenerCount > 0;
        }

        private void RestoreListener()
        {
            if (_button?.onClick != null && _tutorialListener != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _tutorialListener.Invoke());
            }
        }
    }

    private bool _isSubscribedToPlacementManager;
    private bool _isSubscribedToMonsterPoopClean;

    private void TrySubscribeMonsterPoopClean()
    {
        if (_isSubscribedToMonsterPoopClean)
            return;

        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null)
            return;

        monsterManager.OnPoopCleaned += OnPoopCleanedByPlayer;
        _isSubscribedToMonsterPoopClean = true;
        Debug.Log("TutorialManager: Subscribed to MonsterManager.OnPoopCleaned");
    }

    private static IUIButtonResolver Create(TutorialMode mode)
    {
        return mode == TutorialMode.Plain
            ? new IndexUIButtonResolver()
            : new NameUIButtonResolver();
    }

    private bool HaveGivenTutorialStartItems()
    {
        var config = SaveSystem.PlayerConfig;
        return config != null && config.tutorialItemsGranted;
    }

    private void MarkTutorialStartItemsGiven()
    {
        var config = SaveSystem.PlayerConfig;
        if (config == null)
            return;

        if (!config.tutorialItemsGranted)
        {
            config.tutorialItemsGranted = true;
            SaveSystem.SaveAll();
        }
    }

    private void GrantTutorialStartItemsIfNeeded()
    {
        if (HaveGivenTutorialStartItems())
            return;

        if (tutorialStartItems == null || tutorialStartItems.Length == 0)
            return;

        foreach (var reward in tutorialStartItems)
        {
            if (reward == null || reward.item == null || reward.amount == 0)
                continue;

            SaveSystem.UpdateItemData(reward.item.itemID, reward.item.category, reward.amount);
        }

        MarkTutorialStartItemsGiven();
    }

    private void TrySubscribePlacementManager()
    {
        if (_isSubscribedToPlacementManager)
            return;

        var placementManager = ServiceLocator.Get<PlacementManager>();
        if (placementManager == null)
            return;

        placementManager.OnFoodPlacementConfirmed += OnFoodPlacementConfirmed;
        _isSubscribedToPlacementManager = true;

        Debug.Log("TutorialManager: Subscribed to PlacementManager.OnFoodPlacementConfirmed");
    }

    [System.Serializable]
    private class TutorialItemReward
    {
        public ItemDataSO item;
        public int amount = 10;
    }

    private bool IsPlainTutorialAlreadyCompleted()
    {
        var config = SaveSystem.PlayerConfig;
        if (config == null)
            return false;

        return config.plaintutorial || IsPlainTutorialSkipped();
    }

    private void MarkPlainTutorialCompleted()
    {
        var config = SaveSystem.PlayerConfig;
        if (config == null)
            return;

        if (!config.plaintutorial)
        {
            config.plaintutorial = true;
            SaveSystem.SaveAll();
        }
        Destroy(gameObject);
        foreach (GameObject item in gotohotel)
        {
            if (item != null)
                item.SetActive(true);
        }
    }

    private bool IsHotelTutorialAlreadyCompleted()
    {
        var config = SaveSystem.PlayerConfig;
        if (config == null)
            return false;

        return config.hotelTutorial || IsHotelTutorialSkipped();
    }

    private void MarkHotelTutorialCompleted()
    {
        var config = SaveSystem.PlayerConfig;
        if (config == null)
            return;

        if (!config.hotelTutorial)
        {
            config.hotelTutorial = true;
            SaveSystem.SaveAll();
        }

        UnlockCameraAfterTutorial();
    }

    private bool IsPlainTutorialSkipped()
    {
        var config = SaveSystem.PlayerConfig;
        return config != null && config.plainTutorialSkipped;
    }

    private bool IsHotelTutorialSkipped()
    {
        var config = SaveSystem.PlayerConfig;
        return config != null && config.hotelTutorialSkipped;
    }

    private void OnCoinCollectedByPlayer(CoinController coin)
    {
        if (_currentMode != TutorialMode.Plain)
            return;

        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;
        if (config == null)
            return;

        if (config.useFoodDropAsNext)
            return;

        if (config.useUIManagerButtonHandPointer)
            return;

        if (!config.useCoinCollectAsNext)
            return;

        RequestNextPlainPanel();
    }

    private void OnFoodPlacementConfirmed()
    {
        Debug.Log("TutorialManager: TryHandleFoodDropProgress from PlacementManager");
        TryHandleFoodDropProgress(true);
    }

    private void OnPoopCleanedByPlayer(PoopController poop)
    {
        Debug.Log($"TutorialManager: OnPoopCleanedByPlayer called for poop '{poop?.name}' at plain index {_plainPanelIndex}");
        TryHandlePoopCleanProgress();
    }

    private void TryHandleFoodDropProgress(bool incrementCount)
    {
        if (_currentMode != TutorialMode.Plain)
            return;

        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;
        if (config == null)
            return;

        if (!config.useFoodDropAsNext)
            return;

        if (incrementCount)
        {
            _foodDropCountForCurrentStep++;
        }

        int required = config.requiredFoodDropCount <= 0 ? 1 : config.requiredFoodDropCount;
        float delay = config.minFoodDropDelay > 0f ? config.minFoodDropDelay : 5f;
        float elapsed = Time.time - _plainStepShownTime;

        if (_foodDropCountForCurrentStep < required)
        {
            Debug.Log($"TutorialManager: food-drop progress pending (count={_foodDropCountForCurrentStep}/{required})");
            return;
        }

        if (elapsed < delay)
        {
            float remaining = delay - elapsed;
            if (_plainFoodDropDelayRoutine == null)
            {
                Debug.Log($"TutorialManager: food-drop count reached, waiting remaining delay {remaining:F2}s");
                _plainFoodDropDelayRoutine = StartCoroutine(FoodDropNextAfterDelayRoutine(remaining, _plainPanelIndex));
            }
            return;
        }

        Debug.Log("TutorialManager: food-drop conditions met, requesting next plain panel");
        CancelFoodPlacementIfAny();
        RequestNextPlainPanel();
    }

    private void TryHandlePoopCleanProgress()
    {
        if (_currentMode != TutorialMode.Plain)
        {
            Debug.Log("TutorialManager: TryHandlePoopCleanProgress ignored (not in plain mode)");
            return;
        }

        if (plainTutorials == null || plainTutorials.Count == 0)
        {
            Debug.Log("TutorialManager: TryHandlePoopCleanProgress ignored (no plainTutorials)");
            return;
        }

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
        {
            Debug.Log($"TutorialManager: TryHandlePoopCleanProgress ignored (invalid index {_plainPanelIndex})");
            return;
        }

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;
        if (config == null)
        {
            Debug.Log("TutorialManager: TryHandlePoopCleanProgress ignored (current step is null)");
            return;
        }

        if (!config.usePoopCleanAsNext)
        {
            Debug.Log("TutorialManager: TryHandlePoopCleanProgress ignored (usePoopCleanAsNext is false on this step)");
            return;
        }

        Debug.Log("TutorialManager: TryHandlePoopCleanProgress -> calling RequestNextPlainPanel");
        RequestNextPlainPanel();
    }

    private IEnumerator FoodDropNextAfterDelayRoutine(float remainingDelay, int stepIndex)
    {
        if (remainingDelay > 0f)
        {
            yield return new WaitForSeconds(remainingDelay);
        }
        else
        {
            yield return null;
        }

        _plainFoodDropDelayRoutine = null;

        if (_currentMode != TutorialMode.Plain)
            yield break;

        if (plainTutorials == null || plainTutorials.Count == 0)
            yield break;

        if (stepIndex < 0 || stepIndex >= plainTutorials.Count)
            yield break;

        if (_plainPanelIndex != stepIndex)
            yield break;

        var step = plainTutorials[stepIndex];
        var config = step != null ? step.config : null;
        if (config == null || !config.useFoodDropAsNext)
            yield break;

        int required = config.requiredFoodDropCount <= 0 ? 1 : config.requiredFoodDropCount;
        if (_foodDropCountForCurrentStep < required)
            yield break;

        Debug.Log("TutorialManager: food-drop delay satisfied, requesting next plain panel");
        CancelFoodPlacementIfAny();
        RequestNextPlainPanel();
    }

    private void CancelFoodPlacementIfAny()
    {
        var placementManager = ServiceLocator.Get<PlacementManager>();
        placementManager?.CancelPlacement();
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
                _briabitSpawned = true;
            }
            return;
        }

        var config = SaveSystem.PlayerConfig;
        bool hasOwnedMonsters = config != null && config.ownedMonsters != null && config.ownedMonsters.Count > 0;
        if (hasOwnedMonsters)
        {
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
                    monsterManager.SaveAllMonsters();
                    _briabitSpawned = true;
                }
            }
        }
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

    private void RequestNextPlainPanel()
    {
        Debug.Log(
    $"[PlainTutorial] RequestNextPlainPanel CALLED | " +
    $"mode={_currentMode} | index={_plainPanelIndex} | " +
    $"handPointerRunning={_isRunningHandPointerSubTutorial}"
);
        if (plainTutorials != null &&
            _plainPanelIndex >= 0 &&
            _plainPanelIndex < plainTutorials.Count)
        {
            var currentStep = plainTutorials[_plainPanelIndex];
            var currentConfig = currentStep != null ? currentStep.config : null;
            if (_isRunningHandPointerSubTutorial && currentConfig != null &&
                (currentConfig.useFoodDropAsNext || currentConfig.usePoopCleanAsNext))
            {
                EndHandPointerSubTutorial();
            }
        }

        if (_isRunningHandPointerSubTutorial)
        {
            Debug.LogWarning("[PlainTutorial] BLOCKED: HandPointerSubTutorial masih running");
            return;
        }

        if (plainTutorials == null || plainTutorials.Count == 0)
        {
            if (_plainNextDelayRoutine != null)
                return;

            Debug.Log(
                $"[PlainTutorial] Starting next delay coroutine | step={_plainPanelIndex}"
            );

            _plainNextDelayRoutine = StartCoroutine(SimpleNextDelayRoutine(0f));
            return;
        }

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
        {
            if (_plainNextDelayRoutine != null)
                return;

            _plainNextDelayRoutine = StartCoroutine(SimpleNextDelayRoutine(0f));
            return;
        }

        if (_plainNextDelayRoutine != null)
            return;

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;

        float delay = 0f;
        if (config != null)
        {
            if (config.useCoinCollectAsNext)
            {
                if (config.coinCollectNextStepDelay > 0f)
                {
                    delay = Mathf.Max(0f, config.coinCollectNextStepDelay);
                }
            }
            else
            {
                delay = Mathf.Max(0f, config.nextStepDelay);
            }
        }

        _plainNextDelayRoutine = StartCoroutine(SimpleNextDelayRoutine(delay));
    }

    private IEnumerator SimpleNextDelayRoutine(float delay)
    {
        Debug.Log($"[PlainTutorial] SimpleNextDelayRoutine START | delay={delay}");
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return null;
        }
        Debug.Log("[PlainTutorial] SimpleNextDelayRoutine END → ShowNextPlainPanel()");
        _plainNextDelayRoutine = null;
        ShowNextPlainPanel();
    }

    #region Camera Control Integration
    private void LockCameraForTutorial()
    {
        if (cameraController != null)
        {
            cameraController.LockForTutorial();
            Debug.Log("[TutorialManager] Camera locked for hotel tutorial");
        }
        else
        {
            // Fallback: find camera controller in scene
            cameraController = FindObjectOfType<MagicalGarden.Farm.CameraDragMove>();
            if (cameraController != null)
            {
                cameraController.LockForTutorial();
                Debug.Log("[TutorialManager] Camera locked for hotel tutorial (via FindObjectOfType)");
            }
            else
            {
                Debug.LogWarning("[TutorialManager] CameraDragMove not found, cannot lock camera");
            }
        }
    }

    private void UnlockCameraAfterTutorial()
    {
        if (cameraController != null)
        {
            cameraController.UnlockAfterTutorial();
            Debug.Log("[TutorialManager] Camera unlocked after hotel tutorial");
        }
        else
        {
            Debug.LogWarning("[TutorialManager] CameraDragMove reference is null, cannot unlock camera");
        }
    }

    #endregion
}
