using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    #region Inspector Fields

    [Header("UI Panels")]
    public GameObject UIFloatMenuPanel; 
    public CanvasGroup UIFloatMenuCanvasGroup;
    public GameObject SettingPanel;
    public CanvasGroup SettingCanvasGroup;
    public GameObject ShopPanel;
    public CanvasGroup ShopCanvasGroup;
    public GameObject CataloguePanel;
    public CanvasGroup CatalogueCanvasGroup;
    public GameObject InventoryPanel;
    public CanvasGroup InventoryCanvasGroup;

    [Header("Buttons")]
    public Button UIMenuButton;
    public Button InventoryButton; 
    public Button groundButton;
    public Button doorButton;
    public Button windowButton; 
    public Button miniWindowButton; 
    public Button shopButton;
    public Button closeShopButton;
    public Button settingsButton;
    public Button closeSettingsButton;
    public Button catalogueButton;
    public Button closeCatalogueButton;

    [Header("Temporary UI Elements")]
    public TextMeshProUGUI poopCounterText;
    public TextMeshProUGUI coinCounterText;
    public Button spawnPetButton;
    public Button spawnFoodButton;
    public Button gachaButton; 
    public TextMeshProUGUI messageText;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private float buttonSlideDistance = 100f;
    [SerializeField] private AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve easeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Mini Window Mode")]
    [SerializeField] private GameObject gameContentParent;
    [SerializeField] private float miniWindowScale = 3f;
    [SerializeField] private float gameAreaOpacity = 0.15f;
    [SerializeField] private float scaleAnimDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    #endregion

    #region Private Fields

    private bool _onFloatMenuOpened = false;
    private bool _isAnimating = false;
    private bool _isMiniWindowMode = false;
    private Vector3 _newMenuInitialPosition;
    private Vector3 _buttonInitialPosition;
    private Vector3 _originalWindowButtonScale;
    private Vector2 _originalWindowButtonPosition;
    private CanvasGroup _buttonCanvasGroup;
    private CanvasGroup _windowButtonCanvasGroup;
    private CanvasGroup _gameContentCanvasGroup; 
    private RectTransform _newMenuPanelRect;
    private RectTransform _buttonRect;
    private RectTransform _windowButtonRect;
    private TransparentWindow transparentWindow;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        ServiceLocator.Register(this);
        transparentWindow = ServiceLocator.Get<TransparentWindow>();
        _onFloatMenuOpened = false;

        HideAllPanels();

        _newMenuInitialPosition = UIFloatMenuPanel.GetComponent<RectTransform>().anchoredPosition;
        _buttonInitialPosition = UIMenuButton.GetComponent<RectTransform>().anchoredPosition;
        _buttonCanvasGroup = UIMenuButton.GetComponent<CanvasGroup>();
        if (_buttonCanvasGroup == null)
            _buttonCanvasGroup = UIMenuButton.gameObject.AddComponent<CanvasGroup>();

        _newMenuPanelRect = UIFloatMenuPanel?.GetComponent<RectTransform>();
        _buttonRect = UIMenuButton?.GetComponent<RectTransform>();

        InitMiniWindow();
    }

    private void Start()
    {
        RegisterButtonListeners();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
        UnsubscribeEvents();
        ServiceLocator.Unregister<UIManager>();
    }

    #endregion

    #region Event Subscription

    private void SubscribeEvents()
    {
        var monster = ServiceLocator.Get<MonsterManager>();
        if (monster != null)
        {
            spawnPetButton?.onClick.AddListener(() => monster.BuyMons());
            spawnFoodButton?.onClick.AddListener(StartFoodPlacement);

            monster.OnCoinChanged += UpdateCoinCounterValue;
            monster.OnPoopChanged += UpdatePoopCounterValue;
            monster.OnCoinChanged?.Invoke(monster.coinCollected);
            monster.OnPoopChanged?.Invoke(monster.poopCollected);
        }
    }

    private void UnsubscribeEvents()
    {
        var monster = ServiceLocator.Get<MonsterManager>();
        if (monster != null)
        {
            spawnPetButton?.onClick.RemoveAllListeners();
            spawnFoodButton?.onClick.RemoveAllListeners();

            monster.OnCoinChanged -= UpdateCoinCounterValue;
            monster.OnPoopChanged -= UpdatePoopCounterValue;
        }
    }

    #endregion

    #region Button Listeners

    private void RegisterButtonListeners()
    {
        UIMenuButton?.onClick.AddListener(FloatMenu);
        groundButton?.onClick.AddListener(GroundMenu);
        doorButton?.onClick.AddListener(MinimizeApplication);
        windowButton?.onClick.AddListener(ToggleMiniWindowMode);
        miniWindowButton?.onClick.AddListener(ToggleMiniWindowMode);

        settingsButton?.onClick.AddListener(() => FadePanel(SettingPanel, SettingCanvasGroup, true));
        shopButton?.onClick.AddListener(() => FadePanel(ShopPanel, ShopCanvasGroup, true));
        // catalogueButton?.onClick.AddListener(() => FadePanel(CataloguePanel, CatalogueCanvasGroup, true));

        closeSettingsButton?.onClick.AddListener(() => FadePanel(SettingPanel, SettingCanvasGroup, false));
        closeShopButton?.onClick.AddListener(() => FadePanel(ShopPanel, ShopCanvasGroup, false));
        // closeCatalogueButton?.onClick.AddListener(() => FadePanel(CataloguePanel, CatalogueCanvasGroup, false));
    }

    private void UnregisterButtonListeners()
    {
        UIMenuButton?.onClick.RemoveAllListeners();
        groundButton?.onClick.RemoveAllListeners();
        doorButton?.onClick.RemoveAllListeners();
        windowButton?.onClick.RemoveAllListeners();
        miniWindowButton?.onClick.RemoveAllListeners();
        settingsButton?.onClick.RemoveAllListeners();
        shopButton?.onClick.RemoveAllListeners();
        closeSettingsButton?.onClick.RemoveAllListeners();
        closeShopButton?.onClick.RemoveAllListeners();
        catalogueButton?.onClick.RemoveAllListeners();
        closeCatalogueButton?.onClick.RemoveAllListeners();
        gachaButton?.onClick.RemoveAllListeners();
    }

    #endregion

    #region Panel Management

    private void HideAllPanels()
    {
        UIFloatMenuPanel.SetActive(false);
        SettingPanel.SetActive(false);
        ShopPanel.SetActive(false);
        // CataloguePanel?.SetActive(false);
    }

    public void FadePanel(GameObject panel, CanvasGroup canvasGroup, bool fadeIn, float duration = 0.3f, float scalePop = 1.08f, float scaleDuration = 0.15f)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (fadeIn)
        {
            panel.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
            rect.localScale = Vector3.one;

            canvasGroup.DOFade(1f, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            rect.DOScale(1f, 0f); // Reset scale
            canvasGroup.DOFade(0f, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => panel.SetActive(false));
        }
    }

    #endregion

    #region Float Menu System

    public void FloatMenu()
    {
        if (_isAnimating) return;

        _onFloatMenuOpened = !_onFloatMenuOpened;

        if (_onFloatMenuOpened)
            StartCoroutine(FloatMenuAnim());
        else
            StartCoroutine(GroundMenuAnim());
    }

    public void GroundMenu()
    {
        if (_isAnimating || !_onFloatMenuOpened) return;

        _onFloatMenuOpened = false;
        StartCoroutine(GroundMenuAnim());
    }

    private IEnumerator FloatMenuAnim()
    {
        _isAnimating = true;

        UIFloatMenuPanel.SetActive(true);

        Vector3 newPanelStartPos = _newMenuInitialPosition;
        newPanelStartPos.y -= _newMenuPanelRect.rect.height;
        _newMenuPanelRect.anchoredPosition = newPanelStartPos;
        UIFloatMenuCanvasGroup.alpha = 0f;

        Vector3 buttonTargetPos = _buttonInitialPosition;
        buttonTargetPos.y += buttonSlideDistance;

        float duration = animationDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = easeOutCurve.Evaluate(t);

            _buttonCanvasGroup.alpha = 1f - t;
            _buttonRect.anchoredPosition = Vector3.Lerp(_buttonInitialPosition, buttonTargetPos, easeOut);

            _newMenuPanelRect.anchoredPosition = Vector3.Lerp(newPanelStartPos, _newMenuInitialPosition, easeOut);
            UIFloatMenuCanvasGroup.alpha = t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _buttonCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = buttonTargetPos;
        _newMenuPanelRect.anchoredPosition = _newMenuInitialPosition;
        UIFloatMenuCanvasGroup.alpha = 1f;

        UIMenuButton.interactable = false;
        _isAnimating = false;
    }

    private IEnumerator GroundMenuAnim()
    {
        _isAnimating = true;

        Vector3 newPanelTargetPos = _newMenuInitialPosition;
        newPanelTargetPos.y -= _newMenuPanelRect.rect.height;

        Vector3 buttonCurrentPos = _buttonInitialPosition;
        buttonCurrentPos.y += buttonSlideDistance;

        float duration = animationDuration;
        float elapsed = 0f;

        UIMenuButton.interactable = true;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeIn = easeInCurve.Evaluate(t);

            _newMenuPanelRect.anchoredPosition = Vector3.Lerp(_newMenuInitialPosition, newPanelTargetPos, easeIn);
            UIFloatMenuCanvasGroup.alpha = 1f - t;

            _buttonRect.anchoredPosition = Vector3.Lerp(buttonCurrentPos, _buttonInitialPosition, easeIn);
            _buttonCanvasGroup.alpha = t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _newMenuPanelRect.anchoredPosition = newPanelTargetPos;
        UIFloatMenuCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = _buttonInitialPosition;
        _buttonCanvasGroup.alpha = 1f;

        UIFloatMenuPanel.SetActive(false);
        _isAnimating = false;
    }

    #endregion

    #region Mini Window System

    private void InitMiniWindow()
    {
        _windowButtonRect = miniWindowButton?.GetComponent<RectTransform>();
        _windowButtonCanvasGroup = miniWindowButton?.GetComponent<CanvasGroup>();

        if (_windowButtonCanvasGroup == null && miniWindowButton != null)
            _windowButtonCanvasGroup = miniWindowButton.gameObject.AddComponent<CanvasGroup>();

        if (_windowButtonRect != null)
        {
            _originalWindowButtonScale = _windowButtonRect.localScale;
            _originalWindowButtonPosition = _windowButtonRect.anchoredPosition;
        }

        if (gameContentParent != null)
        {
            _gameContentCanvasGroup = gameContentParent.GetComponent<CanvasGroup>();
            if (_gameContentCanvasGroup == null)
                _gameContentCanvasGroup = gameContentParent.AddComponent<CanvasGroup>();
        }

        if (miniWindowButton != null)
            miniWindowButton.gameObject.SetActive(false);
    }

    public void ToggleMiniWindowMode()
    {
        if (_isAnimating) return;

        _isMiniWindowMode = !_isMiniWindowMode;

        if (_isMiniWindowMode)
            EnterMiniWindowMode();
        else
            ExitMiniWindowMode();
    }

    private void EnterMiniWindowMode()
    {
        if (_gameContentCanvasGroup != null)
        {
            _gameContentCanvasGroup.alpha = gameAreaOpacity;
            _gameContentCanvasGroup.interactable = false;
            _gameContentCanvasGroup.blocksRaycasts = false;
        }

        if (UIFloatMenuPanel != null)
            UIFloatMenuPanel.SetActive(false);

        if (miniWindowButton != null)
        {
            miniWindowButton.gameObject.SetActive(true);

            if (_windowButtonCanvasGroup != null)
            {
                _windowButtonCanvasGroup.alpha = 1f;
                _windowButtonCanvasGroup.interactable = true;
                _windowButtonCanvasGroup.blocksRaycasts = true;
            }

            if (_windowButtonRect != null)
                StartCoroutine(AnimateButtonScale(_originalWindowButtonScale, _originalWindowButtonScale * miniWindowScale));
        }

        transparentWindow?.SetTopMostMode(false);
    }

    private void ExitMiniWindowMode()
    {
        if (_gameContentCanvasGroup != null)
        {
            _gameContentCanvasGroup.alpha = 1f;
            _gameContentCanvasGroup.interactable = true;
            _gameContentCanvasGroup.blocksRaycasts = true;
        }

        if (miniWindowButton != null && _windowButtonRect != null)
            StartCoroutine(AnimateButtonScaleAndHide());
        else if (miniWindowButton != null)
            miniWindowButton.gameObject.SetActive(false);

        if (UIFloatMenuPanel != null)
            UIFloatMenuPanel.SetActive(true);

        if (_onFloatMenuOpened && UIFloatMenuCanvasGroup != null)
        {
            UIFloatMenuCanvasGroup.alpha = 1f;
            UIFloatMenuCanvasGroup.interactable = true;
            UIFloatMenuCanvasGroup.blocksRaycasts = true;
        }
        else if (UIFloatMenuCanvasGroup != null)
        {
            UIFloatMenuCanvasGroup.alpha = 0f;
            UIFloatMenuCanvasGroup.interactable = false;
            UIFloatMenuCanvasGroup.blocksRaycasts = false;
        }

        transparentWindow?.SetTopMostMode(true);
    }

    private IEnumerator AnimateButtonScale(Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            float t = elapsed / scaleAnimDuration;
            float easedT = scaleAnimCurve.Evaluate(t);

            _windowButtonRect.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _windowButtonRect.localScale = toScale;
    }

    private IEnumerator AnimateButtonScaleAndHide()
    {
        Vector3 fromScale = _windowButtonRect.localScale;
        Vector3 toScale = _originalWindowButtonScale;

        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            float t = elapsed / scaleAnimDuration;
            float easedT = scaleAnimCurve.Evaluate(t);

            _windowButtonRect.localScale = Vector3.Lerp(fromScale, toScale, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _windowButtonRect.localScale = toScale;
        _windowButtonRect.anchoredPosition = _originalWindowButtonPosition;
        miniWindowButton.gameObject.SetActive(false);
    }

    #endregion

    #region UI Feedback

    public void ShowMessage(string message, float duration = 1f)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), duration);
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }

    #endregion

    #region Monster System

    private void UpdateCoinCounterValue(int newCoinAmount)
    {
        coinCounterText.text = $"Coin : {newCoinAmount}";
    }

    private void UpdatePoopCounterValue(int newPoopAmount)
    {
        poopCounterText.text = $"Poop : {newPoopAmount}";
    }

    public void StartFoodPlacement()
    {
        ServiceLocator.Get<MonsterManager>().StartFoodPurchase(0);
    }

    #endregion

    #region Utility

    private void MinimizeApplication()
    {
        var transparentWindow = ServiceLocator.Get<TransparentWindow>();
        if (transparentWindow != null)
        {
            transparentWindow.MinimizeWindow();
            GroundMenu();
        }
        else
        {
            Debug.LogWarning("TransparentWindow service not found - cannot minimize");

#if !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
            Application.Quit();
#endif
        }
    }

    #endregion
}