using System.Collections;
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
    public Button windowButton;
    public Button shopButton;
    public Button settingsButton;
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

    private bool _onMenuOpened = false;
    private bool _isAnimating = false;
    private Vector3 _newMenuInitialPosition;
    private Vector3 _buttonInitialPosition;
    private CanvasGroup _buttonCanvasGroup;
    private RectTransform _newMenuPanelRect;
    private RectTransform _buttonRect;

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
    }
    
    void Start()
    {
        UIMenuButton.onClick.AddListener(ShowMenu);
        groundButton?.onClick.AddListener(HideMenu);
        
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
            
            float fadeProgress = t < 0.3f ? t / 0.3f : (t > 0.7f ? (1f - t) / 0.3f : 1f);
            
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

    void OnDestroy()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnCoinChanged -= UpdateCoinCounterValue;
            gameManager.OnPoopChanged -= UpdatePoopCounterValue;
        }
        
        ServiceLocator.Unregister<UIManager>();
    }
}