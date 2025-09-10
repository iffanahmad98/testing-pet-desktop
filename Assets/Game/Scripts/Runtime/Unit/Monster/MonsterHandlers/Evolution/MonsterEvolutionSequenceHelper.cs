using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;
using CartoonFX;
using UnityEngine.UI;
using System.Linq;

public static class MonsterEvolutionSequenceHelper
{
    private static Queue<GameObject> _sparklePool = new Queue<GameObject>();
    private static List<GameObject> _activeSparkles = new List<GameObject>();
    private static GameObject _sparkleParent;
    private static GameObject _evolutionParticle;

    static Tween WaitForAnimatorState(Animator anim, string stateName, int layer = 0, float timeout = 8f)
    {
        // Tween dummy; kita complete saat anim selesai
        var t = DOVirtual.DelayedCall(timeout, () => {}, false).SetUpdate(true);
        bool entered = false;

        t.OnUpdate(() =>
        {
            if (anim == null) { t.Kill(true); return; }

            var info = anim.GetCurrentAnimatorStateInfo(layer);
            // Sudah masuk state yang dimaksud?
            if (!entered && info.IsName(stateName)) entered = true;

            // Jika sudah masuk dan progress >= 1 dan tidak sedang transisi, anggap selesai
            if (entered && info.IsName(stateName) && info.normalizedTime >= 1f && !anim.IsInTransition(layer))
                t.Kill(true); // complete tween
        });

        return t;
    }

    public static Sequence PlayEvolutionUISequence(
        Camera evolveCam,
        SkeletonGraphic spineGraphic,
        UIParticle[] evolutionParticles, // Changed to array
        SkeletonDataAsset nextEvolutionSkeleton,
        System.Action onEvolutionDataUpdate
    )
    {
        // Use different particles for different phases
        UIParticle rampUpParticle = (evolutionParticles != null && evolutionParticles.Length > 0)
            ? evolutionParticles[0] : null;  // Index 0 for ramp up
        UIParticle backgroundParticle = (evolutionParticles != null && evolutionParticles.Length > 8)
            ? evolutionParticles[8] : null;  // Index 8 for background
        UIParticle finishParticle = (evolutionParticles != null && evolutionParticles.Length > 3)
            ? evolutionParticles[3] : null;  // Index 3 for finish
        UIParticle foregroundParticle1 = (evolutionParticles != null && evolutionParticles.Length > 6)
            ? evolutionParticles[6] : null;  // Index 6 for foreground effect 1
        UIParticle foregroundParticle2 = (evolutionParticles != null && evolutionParticles.Length > 7)
            ? evolutionParticles[7] : null;  // Index 7 for foreground effect 2

        if (rampUpParticle == null || backgroundParticle == null || finishParticle == null)
        {
            Debug.LogError("Evolution particles missing! Need particles at indices 0, 3, and 8");
            return DOTween.Sequence();
        }

        // Get components for all particles
        var rampUpCg = rampUpParticle.GetComponent<CanvasGroup>();
        var backgroundCg = backgroundParticle.GetComponent<CanvasGroup>();
        var finishCg = finishParticle.GetComponent<CanvasGroup>();
        var foreground1Cg = foregroundParticle1?.GetComponent<CanvasGroup>();
        var foreground2Cg = foregroundParticle2?.GetComponent<CanvasGroup>();

        var originalFOV = evolveCam.fieldOfView;
        var originalPosition = MainCanvas.CamRT.anchoredPosition;

        // Get BiomeManager from ServiceLocator
        var biomeManager = ServiceLocator.Get<BiomeManager>();

        // Store original monster scale, position, AND anchor/pivot settings for restoration
        var originalScale = spineGraphic.rectTransform.localScale;
        var originalPos = spineGraphic.rectTransform.anchoredPosition;
        var originalAnchorMin = spineGraphic.rectTransform.anchorMin;
        var originalAnchorMax = spineGraphic.rectTransform.anchorMax;
        var originalPivot = spineGraphic.rectTransform.pivot;

        // ALSO store parent (MonsterController) original settings
        var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
        var originalParentScale = parentRect.localScale;
        var originalParentPos = parentRect.anchoredPosition;
        var originalParentAnchorMin = parentRect.anchorMin;
        var originalParentAnchorMax = parentRect.anchorMax;
        var originalParentPivot = parentRect.pivot;

        var seq = DOTween.Sequence();
        // Sequence seq = DOTween.Sequence().SetUpdate(true); // pake unscaled time (opsional)

        // 0) Setup awal (anchor/pivot/orient)
        seq.AppendCallback(() =>
        {
            if (biomeManager != null) biomeManager.ToggleFilters("Darken", true);

            var monsterRect = spineGraphic.rectTransform;
            var parentRect0 = spineGraphic.transform.parent.GetComponent<RectTransform>();

            // anchor & pivot center
            monsterRect.anchorMin = monsterRect.anchorMax = new Vector2(0.5f, 0.5f);
            monsterRect.pivot     = new Vector2(0.5f, 0.5f);

            parentRect0.anchorMin = parentRect0.anchorMax = new Vector2(0.5f, 0.5f);
            parentRect0.pivot     = new Vector2(0.5f, 0.5f);

            // face ke kiri (x scale positif)
            if (monsterRect.localScale.x < 0f)
            {
                var s = monsterRect.localScale;
                s.x = Mathf.Abs(s.x);
                monsterRect.localScale = s;
            }
        });

        // Siapkan Animator dan konstanta ANIM lebih awal
        var anim = spineGraphic.GetComponentInParent<Animator>();
        const string STATE_NAME = "StepEvolved 1";
        const int    LAYER      = 0;
        const float  XFADE      = 0.10f;

        // 1) MULAI ANIMASI DI AWAL (tanpa menunggu tween move/scale)
        seq.InsertCallback(0f, () =>
        {
            if (anim == null)
            {
                Debug.LogWarning("[EvolutionUI] Animator not found near spineGraphic.");
                return;
            }
            anim.CrossFadeInFixedTime(STATE_NAME, XFADE, LAYER, 0f);
        });

        // 2) Phase 1: Move + Scale (simulasi fokus kamera)
        parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
        seq.Append(parentRect.DOAnchorPos(new Vector2(0f, -50f), 1.5f).SetEase(Ease.InOutCubic));
        seq.Join(  parentRect.DOScale(Vector3.one * 4.0f, 1.5f).SetEase(Ease.InOutCubic));

        // 3) Phase 2: lanjut zoom-in (pakai nilai lebih besar agar terasa naik)
        seq.Append(parentRect.DOScale(Vector3.one * 4.5f, 1.0f).SetEase(Ease.InOutSine));

        // 4) Tunggu anim state selesai (atau timeout) — pakai helper kamu
        seq.Append(WaitForAnimatorState(anim, STATE_NAME, LAYER, timeout: 8f));

        // 5) Jeda sebentar sebelum fade-out monster
        seq.AppendInterval(2.7f);

        // 6) Fade-out monster (background particle tetap main)
        var monsterCanvasGroup = spineGraphic.GetComponent<CanvasGroup>();
        if (monsterCanvasGroup == null) monsterCanvasGroup = spineGraphic.gameObject.AddComponent<CanvasGroup>();
        seq.Append(monsterCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine));

        // 7) Ganti skeleton saat invisible
        seq.AppendCallback(() =>
        {
            spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            spineGraphic.Initialize(true);
            onEvolutionDataUpdate?.Invoke();
        });

        // 8) Fade-in monster dengan skeleton baru
        seq.Append(monsterCanvasGroup.DOFade(1f, 0.8f).SetEase(Ease.InOutSine));

        // 9) Jeda lalu matikan background/foreground particle
        seq.AppendInterval(3.0f);
        seq.AppendCallback(() =>
        {
            // Background
            if (backgroundCg != null)
            {
                backgroundCg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    if (backgroundParticle) backgroundParticle.gameObject.SetActive(false);
                    if (backgroundParticle) backgroundParticle.transform.SetSiblingIndex(-1);
                });
            }

            // Foreground 1
            if (foreground1Cg != null && foregroundParticle1 != null)
            {
                foreground1Cg.DOFade(0f, 0.8f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    foregroundParticle1.gameObject.SetActive(false);
                });
            }

            // Foreground 2
            if (foreground2Cg != null && foregroundParticle2 != null)
            {
                foreground2Cg.DOFade(0f, 0.8f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    foregroundParticle2.gameObject.SetActive(false);
                });
            }
        });

        // 10) Tunggu fade particle selesai
        seq.AppendInterval(2f);

        // 11) Mainkan anim “Jumping” singkat sebelum zoom-out
        seq.AppendCallback(() =>
        {
            if (spineGraphic != null && spineGraphic.AnimationState != null)
            {
                var data = spineGraphic.Skeleton?.Data;
                if (data != null)
                {
                    string name = null;
                    if      (data.FindAnimation("Jumping") != null) name = "Jumping";
                    else if (data.FindAnimation("jumping") != null) name = "jumping";
                    else if (data.FindAnimation("jump")    != null) name = "jump";
                    else if (data.FindAnimation("Jump")    != null) name = "Jump";

                    if (name != null) spineGraphic.AnimationState.SetAnimation(0, name, false);
                    else Debug.LogWarning("Animation 'Jumping/jumping/jump/Jump' tidak ditemukan.", spineGraphic);
                }
            }
        });

        // 12) Tunggu jump singkat, lalu kembali idle
        seq.AppendInterval(1.0f);
        seq.AppendCallback(() =>
        {
            if (spineGraphic.AnimationState != null)
                spineGraphic.AnimationState.SetAnimation(0, "idle", true);
        });

        // 13) Jeda kecil sebelum zoom-out
        seq.AppendInterval(0.5f);

        // 14) Zoom-out & kembali ke posisi/skalanya
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>()
            .DOScale(Vector3.one * 1.2f, 1.0f).SetEase(Ease.InOutCubic));
        seq.Join(spineGraphic.rectTransform
            .DOAnchorPos(originalPos, 1.0f).SetEase(Ease.InOutCubic));
        seq.Join(spineGraphic.transform.parent.GetComponent<RectTransform>()
            .DOAnchorPos(originalParentPos, 1.0f).SetEase(Ease.InOutCubic));

        // 15) Final scale parent ke ukuran normal
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>()
            .DOScale(originalParentScale, 0.5f).SetEase(Ease.OutBack));

        // 16) Restore semua properti & matikan filter
        seq.AppendCallback(() =>
        {
            // restore anak
            spineGraphic.rectTransform.anchoredPosition = originalPos;
            spineGraphic.rectTransform.anchorMin = originalAnchorMin;
            spineGraphic.rectTransform.anchorMax = originalAnchorMax;
            spineGraphic.rectTransform.pivot     = originalPivot;
            spineGraphic.rectTransform.localScale= originalScale;

            // restore parent
            var pr = spineGraphic.transform.parent.GetComponent<RectTransform>();
            pr.anchoredPosition = originalParentPos;
            pr.localScale       = originalParentScale;
            pr.anchorMin        = originalParentAnchorMin;
            pr.anchorMax        = originalParentAnchorMax;
            pr.pivot            = originalParentPivot;

            if (biomeManager != null) biomeManager.ToggleFilters("Darken", false);
        });

        // jangan lupa return sequence
        return seq;
    }
}
