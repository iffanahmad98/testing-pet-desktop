using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager : MonoBehaviour
{
    #region Inspector Fields

    [Header("UI Panels")] public UIPanels panels;

    [Header("Buttons")] public UIMainButtons buttons;

    [Header("Buttons On Shop Panel")] public UIShopButtons shopButtons;

    public TextMeshProUGUI messageText;

    [Header("UI VFX")]
    public GameObject unlockedBtnVfxPrefab;
    public RectTransform vfxParent;

    [Header("Animation Settings")]
    [SerializeField]
    private float animationDuration = 0.4f;

    [SerializeField] private float buttonSlideDistance = 100f;
    [SerializeField] private AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve easeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Mini Window Mode")]
    [SerializeField]
    private GameObject gameContentParent;

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

    private static GameObject currentPanel; // simpan panel aktif sekarang
    private static CanvasGroup currentCanvas; // simpan canvas group aktif
    
    #endregion
    #region Public Fields
    public static bool transparentMode = false; // UIManager.MiniWindow, TooltipManager.cs
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        ServiceLocator.Register(this);
        transparentWindow = ServiceLocator.Get<TransparentWindow>();
        _onFloatMenuOpened = false;

        HideAllPanels();

        _newMenuInitialPosition = panels.UIFloatMenuPanel.GetComponent<RectTransform>().anchoredPosition;
        _buttonInitialPosition = buttons.UIMenuButton.GetComponent<RectTransform>().anchoredPosition;
        _buttonCanvasGroup = buttons.UIMenuButton.GetComponent<CanvasGroup>();
        if (_buttonCanvasGroup == null)
            _buttonCanvasGroup = buttons.UIMenuButton.gameObject.AddComponent<CanvasGroup>();

        _newMenuPanelRect = panels.UIFloatMenuPanel?.GetComponent<RectTransform>();
        _buttonRect = buttons.UIMenuButton?.GetComponent<RectTransform>();

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
}