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

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Vector2 _initialAnchoredPos;

    public Button NextButton => nextButton;

    private void Awake()
    {

        _rect = GetComponent<RectTransform>();
        if (_rect != null)
            _initialAnchoredPos = _rect.anchoredPosition;
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

        gameObject.SetActive(false);
    }
}
