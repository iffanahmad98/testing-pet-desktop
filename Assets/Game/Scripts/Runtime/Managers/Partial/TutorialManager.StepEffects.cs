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

    [Header("Simple Step Effects")]
    [SerializeField] private List<SimpleStepEffectEntry> simpleStepEffects = new List<SimpleStepEffectEntry>();

    [SerializeField] private float simpleStepEffectDuration = 0.35f;

    [SerializeField] private AnimationCurve simpleStepEffectEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float simpleStepEffectHoldDuration = 1.0f;

    private void PlaySimpleStepEffectForIndex(int stepIndex)
    {
        if (simpleStepEffects == null || simpleStepEffects.Count == 0)
            return;

        SimpleStepEffectEntry config = null;
        for (int i = 0; i < simpleStepEffects.Count; i++)
        {
            var entry = simpleStepEffects[i];
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

        seq.Append(rect.DOScale(targetScale, simpleStepEffectDuration).SetEase(simpleStepEffectEase));
        seq.Join(canvasGroup.DOFade(1f, simpleStepEffectDuration).SetEase(Ease.OutQuad));

        if (simpleStepEffectHoldDuration > 0f)
        {
            seq.AppendInterval(simpleStepEffectHoldDuration);
        }

        seq.Append(rect.DOScale(Vector3.zero, simpleStepEffectDuration).SetEase(simpleStepEffectEase));
        seq.Join(canvasGroup.DOFade(0f, simpleStepEffectDuration).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            go.SetActive(false);
        });
    }
}
