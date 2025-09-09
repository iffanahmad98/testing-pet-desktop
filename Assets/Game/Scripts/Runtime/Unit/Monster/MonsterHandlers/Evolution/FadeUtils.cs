using DG.Tweening;
using UnityEngine;

public static class FadeUtils
{
    // Fade child visual → setelah selesai, nonaktifkan root
    public static void FadeOutVisualThenHideRoot(GameObject visual, GameObject root, float duration = 0.25f)
    {
        if (visual == null || root == null) { if (root != null) root.SetActive(false); return; }

        // Coba CanvasGroup dulu
        if (visual.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.DOFade(0f, duration).SetEase(Ease.InOutSine)
              .OnComplete(() => root.SetActive(false));
            return;
        }

        // Kalau bukan UI: coba SpriteRenderer
        if (visual.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.DOFade(0f, duration).SetEase(Ease.InOutSine)
              .OnComplete(() => root.SetActive(false));
            return;
        }

        // Tidak ada komponen yang bisa di-fade → langsung disable root
        root.SetActive(false);
    }

    // Kebalikan: aktifkan root → visual fade in
    public static void ShowRootThenFadeInVisual(GameObject visual, GameObject root, float duration = 0.25f)
    {
        if (visual == null || root == null) return;

        root.SetActive(true);

        if (visual.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.alpha = 0f;
            cg.DOFade(1f, duration).SetEase(Ease.InOutSine);
            return;
        }

        if (visual.TryGetComponent<SpriteRenderer>(out var sr))
        {
            var c = sr.color; c.a = 0f; sr.color = c;
            sr.DOFade(1f, duration).SetEase(Ease.InOutSine);
        }
    }
}