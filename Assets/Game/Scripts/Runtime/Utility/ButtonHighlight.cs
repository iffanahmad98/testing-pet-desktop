using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Adds a 1.2× scale bump + colour tint on hover / press.
/// </summary>
[RequireComponent(typeof(Image))]
public class ButtonHighlight : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Highlight settings")]
    [ColorUsage(false)] public Color highlightColor = new(1f, 0.85f, 0.2f);
    [Range(1f, 2f)]    public float scaleMultiplier = 1.2f;
    public float tweenTime = 0.15f;
    public Ease  ease      = Ease.OutQuad;

    Image      img;
    Vector3    baseScale;
    Color      baseColor;
    Sequence   seq;          // pooled sequence (no GC after Awake)

    void Awake()
    {
        img       = GetComponent<Image>();
        baseColor = img.color;
        baseScale = transform.localScale;

        seq = DOTween.Sequence().SetAutoKill(false).Pause().SetRecyclable(true)
              .Append(img.DOColor(highlightColor, tweenTime).SetEase(ease))
              .Join(transform.DOScale(baseScale * scaleMultiplier, tweenTime).SetEase(ease));
    }

    public void OnPointerEnter (PointerEventData _) => seq.PlayForward();
    public void OnPointerExit  (PointerEventData _) => seq.PlayBackwards();
    public void OnPointerDown  (PointerEventData _) => seq.PlayForward();
    public void OnPointerUp    (PointerEventData _) => seq.PlayBackwards();
}
