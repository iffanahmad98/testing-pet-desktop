using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Smoothly resizes a layout root when its children change size.
/// Attach this to the same GameObject that holds the ContentSizeFitter.
/// </summary>
[RequireComponent(typeof(ContentSizeFitter), typeof(RectTransform))]
public class UISmoothFitter : MonoBehaviour
{
    [SerializeField] float duration = .25f;
    [SerializeField] Ease ease      = Ease.OutQuad;

    [SerializeField] ContentSizeFitter csf;
    [SerializeField] RectTransform rt;
    Tween current;

    void Awake()
    {
        csf = GetComponent<ContentSizeFitter>();
        rt  = GetComponent<RectTransform>();
    }

    /// Call this whenever you know “something inside just changed size”.
    public void Kick()
    {
        // Force a layout pass so CSF updates to the *final* size first
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        Vector2 target = rt.sizeDelta;   // what CSF decided
        csf.enabled = false;             // freeze it during the tween

        current?.Kill();
        current = rt.DOSizeDelta(target, duration)
                    .SetEase(ease)
                    .OnComplete(() => csf.enabled = true);
    }
}
