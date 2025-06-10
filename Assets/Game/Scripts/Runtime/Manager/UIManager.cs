using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button UIMenuButton;
    public GameObject UINewMenuPanel; 
    public CanvasGroup UINewMenuCanvasGroup;
    public Button groundButton;
    public Button doorButton;
    public Button windowButton; // Keep this for normal mode
    public Button miniWindowButton; // Add this for mini window mode
    public Button shopButton;
    public Button settingsButton;
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
    [SerializeField] private GameObject gameContentParent; // Assign your main game content container
    [SerializeField] private float miniWindowScale = 3f;
    [SerializeField] private float miniWindowOpacity = 0.15f;

    private bool _onMenuOpened = false;
    private bool _isAnimating = false;
    private bool _isMiniWindowMode = false;
    private Vector3 _newMenuInitialPosition;
    private Vector3 _buttonInitialPosition;
    private Vector3 _originalWindowButtonScale;
    private Vector2 _originalWindowButtonPosition;
    private CanvasGroup _buttonCanvasGroup;
    private CanvasGroup _windowButtonCanvasGroup;
    private RectTransform _newMenuPanelRect;
    private RectTransform _buttonRect;
    private RectTransform _windowButtonRect;

    [System.Serializable]
    public class UIAnimationEvents
    {
        [System.Serializable] public class UnityEvent : UnityEngine.Events.UnityEvent { }
        
        public UnityEvent OnMenuOpened;
        public UnityEvent OnMenuClosed;
        public UnityEvent OnAnimationStarted;
        public UnityEvent OnAnimationCompleted;
    }

    [Header("Events")]
    public UIAnimationEvents animationEvents;

    private void Awake()
    {
        ServiceLocator.Register(this);
        _onMenuOpened = false;
        UINewMenuPanel.SetActive(false);
        
        _newMenuInitialPosition = UINewMenuPanel.GetComponent<RectTransform>().anchoredPosition;
        _buttonInitialPosition = UIMenuButton.GetComponent<RectTransform>().anchoredPosition;
        _buttonCanvasGroup = UIMenuButton.GetComponent<CanvasGroup>();
        if (_buttonCanvasGroup == null)
        {
            _buttonCanvasGroup = UIMenuButton.gameObject.AddComponent<CanvasGroup>();
        }
        
        _newMenuPanelRect = UINewMenuPanel?.GetComponent<RectTransform>();
        _buttonRect = UIMenuButton?.GetComponent<RectTransform>();
        
        // Initialize mini window components
        InitializeMiniWindow();
    }

    private void InitializeMiniWindow()
    {
        // Use miniWindowButton instead of windowButton for mini mode
        _windowButtonRect = miniWindowButton?.GetComponent<RectTransform>();
        _windowButtonCanvasGroup = miniWindowButton?.GetComponent<CanvasGroup>();
        
        if (_windowButtonCanvasGroup == null && miniWindowButton != null)
        {
            _windowButtonCanvasGroup = miniWindowButton.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (_windowButtonRect != null)
        {
            _originalWindowButtonScale = _windowButtonRect.localScale;
            _originalWindowButtonPosition = _windowButtonRect.anchoredPosition;
        }
        
        // Initially hide mini window button
        if (miniWindowButton != null)
        {
            miniWindowButton.gameObject.SetActive(false);
        }
    }
    
    void Start()
    {
        UIMenuButton?.onClick.AddListener(ShowMenu);
        groundButton?.onClick.AddListener(HideMenu);
        doorButton?.onClick.AddListener(MinimizeApplication);
        windowButton?.onClick.AddListener(ToggleMiniWindowMode); // Normal window button
        miniWindowButton?.onClick.AddListener(ToggleMiniWindowMode); // Mini window button
        
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            spawnPetButton?.onClick.AddListener(() => gameManager.BuyMons());
            spawnFoodButton?.onClick.AddListener(StartFoodPlacement);
            
            gameManager.OnCoinChanged += UpdateCoinCounterValue;
            gameManager.OnPoopChanged += UpdatePoopCounterValue;
            gameManager.OnCoinChanged?.Invoke(gameManager.coinCollected);
            gameManager.OnPoopChanged?.Invoke(gameManager.poopCollected);
        }
        
        var gachaManager = ServiceLocator.Get<GachaManager>();
        if (gachaManager != null)
        {
            gachaButton?.onClick.AddListener(() => gachaManager.RollGacha());
        }
    }

    public void ShowMenu()
    {
        if (_isAnimating) return;
        
        _onMenuOpened = !_onMenuOpened;
        
        if (_onMenuOpened)
        {
            StartCoroutine(MorphButtonToNewPanel());
        }
        else
        {
            StartCoroutine(MorphNewPanelToButton());
        }
    }

    public void HideMenu()
    {
        if (_isAnimating || !_onMenuOpened) return;
        
        _onMenuOpened = false;
        StartCoroutine(MorphNewPanelToButton());
    }

    private IEnumerator MorphButtonToNewPanel()
    {
        _isAnimating = true;
        animationEvents.OnAnimationStarted?.Invoke();
        
        UINewMenuPanel.SetActive(true);
        
        Vector3 newPanelStartPos = _newMenuInitialPosition;
        newPanelStartPos.y -= _newMenuPanelRect.rect.height;
        _newMenuPanelRect.anchoredPosition = newPanelStartPos;
        UINewMenuCanvasGroup.alpha = 0f;
        
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
            UINewMenuCanvasGroup.alpha = t;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        _buttonCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = buttonTargetPos;
        _newMenuPanelRect.anchoredPosition = _newMenuInitialPosition;
        UINewMenuCanvasGroup.alpha = 1f;
        
        UIMenuButton.interactable = false;
        
        animationEvents.OnMenuOpened?.Invoke();
        animationEvents.OnAnimationCompleted?.Invoke();
        _isAnimating = false;
    }
    
    private IEnumerator MorphNewPanelToButton()
    {
        _isAnimating = true;
        animationEvents.OnAnimationStarted?.Invoke();
        
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
            UINewMenuCanvasGroup.alpha = 1f - t;
            
            _buttonRect.anchoredPosition = Vector3.Lerp(buttonCurrentPos, _buttonInitialPosition, easeIn);
            _buttonCanvasGroup.alpha = t;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        _newMenuPanelRect.anchoredPosition = newPanelTargetPos;
        UINewMenuCanvasGroup.alpha = 0f;
        _buttonRect.anchoredPosition = _buttonInitialPosition;
        _buttonCanvasGroup.alpha = 1f;
        
        UINewMenuPanel.SetActive(false);
        animationEvents.OnMenuClosed?.Invoke();
        animationEvents.OnAnimationCompleted?.Invoke();
        _isAnimating = false;
    }

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
        ServiceLocator.Get<GameManager>().StartFoodPurchase(0);
    }

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

    private void MinimizeApplication()
    {
        var transparentWindow = ServiceLocator.Get<TransparentWindow>();
        if (transparentWindow != null)
        {
            transparentWindow.MinimizeWindow();
            HideMenu();
        }
        else
        {
            Debug.LogWarning("TransparentWindow service not found - cannot minimize");
            
            #if !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
            Application.Quit();
            #endif
        }
    }

    public void ToggleMiniWindowMode()
    {
        if (_isAnimating) return;
        
        _isMiniWindowMode = !_isMiniWindowMode;
        
        if (_isMiniWindowMode)
        {
            EnterMiniWindowMode();
        }
        else
        {
            ExitMiniWindowMode();
        }
    }

    private void EnterMiniWindowMode()
    {
        // Hide game content
        if (gameContentParent != null)
            gameContentParent.SetActive(false);

        // Hide the entire UI menu panel
        if (UINewMenuPanel != null)
            UINewMenuPanel.SetActive(false);

        // Show and transform mini window button
        if (miniWindowButton != null)
        {
            miniWindowButton.gameObject.SetActive(true);
            
            if (_windowButtonRect != null)
            {
                _windowButtonRect.localScale = _originalWindowButtonScale * miniWindowScale;
            }

            if (_windowButtonCanvasGroup != null)
            {
                _windowButtonCanvasGroup.alpha = miniWindowOpacity;
                _windowButtonCanvasGroup.interactable = true;
                _windowButtonCanvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void ExitMiniWindowMode()
    {
        // Show game content
        if (gameContentParent != null)
            gameContentParent.SetActive(true);

        // Hide mini window button and reset its properties for next time
        if (miniWindowButton != null)
        {
            // Reset mini window button to original state before hiding
            if (_windowButtonRect != null)
            {
                _windowButtonRect.localScale = _originalWindowButtonScale;
                _windowButtonRect.anchoredPosition = _originalWindowButtonPosition;
            }

            if (_windowButtonCanvasGroup != null)
            {
                _windowButtonCanvasGroup.alpha = 1f;
            }
            
            miniWindowButton.gameObject.SetActive(false);
        }

        // Always show UINewMenuPanel when exiting mini mode
        if (UINewMenuPanel != null)
        {
            UINewMenuPanel.SetActive(true);
        }

        // Restore menu state using CanvasGroup based on previous state
        if (_onMenuOpened && UINewMenuCanvasGroup != null)
        {
            UINewMenuCanvasGroup.alpha = 1f;
            UINewMenuCanvasGroup.interactable = true;
            UINewMenuCanvasGroup.blocksRaycasts = true;
        }
        else if (UINewMenuCanvasGroup != null)
        {
            // If menu wasn't open, make sure it's hidden properly
            UINewMenuCanvasGroup.alpha = 0f;
            UINewMenuCanvasGroup.interactable = false;
            UINewMenuCanvasGroup.blocksRaycasts = false;
        }
    }

    void OnDestroy()
    {
        UIMenuButton?.onClick.RemoveAllListeners();
        groundButton?.onClick.RemoveAllListeners();
        doorButton?.onClick.RemoveAllListeners();
        windowButton?.onClick.RemoveAllListeners(); 
        miniWindowButton?.onClick.RemoveAllListeners();
        spawnPetButton?.onClick.RemoveAllListeners();
        spawnFoodButton?.onClick.RemoveAllListeners();
        gachaButton?.onClick.RemoveAllListeners();
        
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCoinChanged -= UpdateCoinCounterValue;
            gameManager.OnPoopChanged -= UpdatePoopCounterValue;
        }
        
        ServiceLocator.Unregister<UIManager>();
    }
}