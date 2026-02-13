using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public partial class TutorialManager
{
    [Serializable]
    private class SimpleStepEffectEntry
    {
        public int stepIndex;
        public GameObject effectRoot;
    }

    [Header("Plain Step Effects")]
    [SerializeField] private List<SimpleStepEffectEntry> plainStepEffects = new List<SimpleStepEffectEntry>();

    [SerializeField] private float plainStepEffectDuration = 0.35f;

    [SerializeField] private AnimationCurve plainStepEffectEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float plainStepEffectHoldDuration = 1.0f;

    private void PlayPlainStepEffectForIndex(int stepIndex)
    {
        PlayStepEffectForIndex(plainStepEffects, stepIndex);
    }

    [Header("Hotel Step Effects")]
    [SerializeField] private List<SimpleStepEffectEntry> hotelStepEffects = new List<SimpleStepEffectEntry>();

    private void PlayHotelStepEffectForIndex(int stepIndex)
    {
        PlayStepEffectForIndex(hotelStepEffects, stepIndex);
    }

    private void PlayStepEffectForIndex(List<SimpleStepEffectEntry> effects, int stepIndex)
    {
        if (effects == null || effects.Count == 0)
            return;

        SimpleStepEffectEntry config = null;
        for (int i = 0; i < effects.Count; i++)
        {
            var entry = effects[i];
            if (entry != null && entry.stepIndex == stepIndex && entry.effectRoot != null)
            {
                config = entry;
                break;
            }
        }

        if (config == null || config.effectRoot == null)
            return;

        PlayScaleUpFadeEffect(config.effectRoot);
    }

    private void PlayScaleUpFadeEffect(GameObject go)
    {
        if (go == null)
            return;

        var rect = go.GetComponent<RectTransform>();
        var canvasGroup = go.GetComponent<CanvasGroup>();

        if (rect == null || canvasGroup == null)
            return;

        go.SetActive(true);

        rect.DOKill();
        canvasGroup.DOKill();

        var targetScale = rect.localScale;
        if (targetScale == Vector3.zero)
            targetScale = Vector3.one;

        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Sequence: scale-up + fade-in → (tahan) → scale-down + fade-out → nonaktifkan.
        var seq = DOTween.Sequence();

        seq.Append(rect.DOScale(targetScale, plainStepEffectDuration).SetEase(plainStepEffectEase));
        seq.Join(canvasGroup.DOFade(1f, plainStepEffectDuration).SetEase(Ease.OutQuad));

        if (plainStepEffectHoldDuration > 0f)
        {
            seq.AppendInterval(plainStepEffectHoldDuration);
        }

        seq.Append(rect.DOScale(Vector3.zero, plainStepEffectDuration).SetEase(plainStepEffectEase));
        seq.Join(canvasGroup.DOFade(0f, plainStepEffectDuration).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            go.SetActive(false);
        });
    }
}
