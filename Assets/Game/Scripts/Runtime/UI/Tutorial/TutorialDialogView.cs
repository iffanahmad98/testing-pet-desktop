using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI.Core;
using DG.Tweening;

public class TutorialDialogView : MonoBehaviour, ITutorialDialogView
{
    [Header("UI References")]
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TypewriterCore typewriter;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.4f;
    [SerializeField] private AnimationCurve showEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public RectTransform _rect;
    public CanvasGroup _canvasGroup;
    private Vector2 _initialAnchoredPos;

    private Button[] _cachedButtons;
    private bool[] _cachedInteractables;

    public Button NextButton => nextButton;

    public event Action OnNextClicked;

    private void Awake()
    {
        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_rect != null)
            _initialAnchoredPos = _rect.anchoredPosition;

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNextClicked);
            nextButton.onClick.AddListener(HandleNextClicked);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(HandleNextClicked);

        RestoreButtonsInteractableState();
    }

    private void HandleNextClicked()
    {
        OnNextClicked?.Invoke();
    }

    public void SetDialog(string speakerName, string text, bool isLastStep)
    {
        if (speakerText != null)
            speakerText.text = speakerName;

        if (nextButtonLabel != null)
            nextButtonLabel.text = isLastStep ? "OK" : "Next";

        if (typewriter != null)
            typewriter.ShowText(text);
        else
            Debug.LogWarning($"TutorialDialogView: TypewriterCore belum di-assign untuk '{gameObject.name}', teks dialog tidak bisa ditampilkan.");
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (_rect == null || _canvasGroup == null)
            return;

        _rect.DOKill();
        _canvasGroup.DOKill();

        var startPos = _initialAnchoredPos;
        startPos.y -= _rect.rect.height;
        _rect.anchoredPosition = startPos;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        CacheAndRestrictButtonsToNext();

        Debug.Log($"[TutorialDialogView] Show - button.interactable={nextButton?.interactable}, canvasGroup.interactable={_canvasGroup.interactable}, blocksRaycasts={_canvasGroup.blocksRaycasts}");

        transform.SetAsLastSibling();

        _rect.DOAnchorPos(_initialAnchoredPos, showDuration).SetEase(showEase);
        _canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad);
    }

    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        RestoreButtonsInteractableState();

        gameObject.SetActive(false);
    }

    private void CacheAndRestrictButtonsToNext()
    {
        if (nextButton == null)
            return;

        if (_cachedButtons == null)
        {
            _cachedButtons = FindObjectsOfType<Button>(true);
            if (_cachedButtons == null || _cachedButtons.Length == 0)
                return;

            _cachedInteractables = new bool[_cachedButtons.Length];
            for (int i = 0; i < _cachedButtons.Length; i++)
            {
                var btn = _cachedButtons[i];
                _cachedInteractables[i] = btn != null && btn.interactable;
            }
        }

        for (int i = 0; i < _cachedButtons.Length; i++)
        {
            var btn = _cachedButtons[i];
            if (btn != null)
                btn.interactable = btn == nextButton;
        }
    }

    private void RestoreButtonsInteractableState()
    {
        if (_cachedButtons == null || _cachedInteractables == null)
            return;

        int len = Mathf.Min(_cachedButtons.Length, _cachedInteractables.Length);
        int restoredCount = 0;
        for (int i = 0; i < len; i++)
        {
            if (_cachedButtons[i] != null)
            {
                _cachedButtons[i].interactable = _cachedInteractables[i];
                restoredCount++;
            }
        }

        Debug.Log($"[TutorialDialogView] RestoreButtonsInteractableState selesai, {restoredCount} button dikembalikan ke state awal.");

        _cachedButtons = null;
        _cachedInteractables = null;
    }
}
