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

        // 1. Enable darken filter if BiomeManager exists
        seq.AppendCallback(() =>
        {
            if (biomeManager != null)
            {
                biomeManager.ToggleFilters("Darken", true);
            }

            var monsterRect = spineGraphic.rectTransform;
            var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();

            // Set anchor and pivot to center for proper scaling
            monsterRect.anchorMin = new Vector2(0.5f, 0.5f);
            monsterRect.anchorMax = new Vector2(0.5f, 0.5f);
            monsterRect.pivot = new Vector2(0.5f, 0.5f);

            // Also set parent anchor/pivot for proper centering
            parentRect.anchorMin = new Vector2(0.5f, 0.5f);
            parentRect.anchorMax = new Vector2(0.5f, 0.5f);
            parentRect.pivot = new Vector2(0.5f, 0.5f);

            // Change orientation to face left (positive scale)
            if (spineGraphic.rectTransform.localScale.x < 0)
            {
                var currentScale = spineGraphic.rectTransform.localScale;
                currentScale.x = Mathf.Abs(currentScale.x); // Make it positive to face left
                spineGraphic.rectTransform.localScale = currentScale;
            }
        });

        // Phase 1: Move parent MonsterController to center and start scaling (simulates camera focusing)
        parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
        seq.Append(parentRect.DOAnchorPos(new Vector2(0f, -50f), 1.5f).SetEase(Ease.InOutCubic));
        seq.Join(parentRect.DOScale(Vector3.one * 4.0f, 1.5f).SetEase(Ease.InOutCubic));

        // Phase 2: Continue scaling parent bigger (simulates zoom-in effect)  
        seq.Append(parentRect.DOScale(Vector3.one * 4.0f, 2.0f).SetEase(Ease.InOutSine));

        var anim = spineGraphic.GetComponentInParent<Animator>();   // ambil Animator di parent (atau ganti referensinya)
        const string STATE_NAME = "StepEvolved 1";                     // ganti sesuai nama state di Animator Controller
        const int    LAYER      = 0;
        const float  XFADE      = 0.1f;

        seq.AppendCallback(() =>
        {
            if (anim == null)
            {
                Debug.LogWarning("[EvolutionUI] Animator not found near spineGraphic.");
                return;
            }

            // Kalau pakai Trigger:
            // anim.ResetTrigger("Evolve");
            // anim.SetTrigger("Evolve");

            // Atau langsung crossfade ke state:
            anim.CrossFadeInFixedTime(STATE_NAME, XFADE, LAYER, 0f);
        });

        // Tahan sequence sampai animasinya benar-benar selesai (atau timeout)
        seq.Append(WaitForAnimatorState(anim, STATE_NAME, LAYER, timeout: 8f));

        // Add shake to parent during zoom
        // seq.JoinCallback(() =>
        // {
        //     parentRect.DOShakePosition(3.5f, strength: 8f, vibrato: 15, randomness: 50f);
        // });

        // // 2. Play RAMP UP particle during zoom (monster still visible)
        // seq.JoinCallback(() =>
        // {
        //     rampUpParticle.gameObject.SetActive(true);
        //     rampUpCg.alpha = 0f;
        //     rampUpCg.DOFade(1f, 1.0f).SetEase(Ease.InOutSine);

        //     // Scale shake effect during zoom (apply to parent)
        //     parentRect.DOShakeScale(3.5f, strength: 0.2f, vibrato: 8, randomness: 90f);
        // });

        // // 3. Peak zoom moment - transition to background particle
        // seq.AppendInterval(2.5f); // Wait for initial ramp up
        // seq.AppendCallback(() => {
        //     // Fade out ramp up particle
        //     rampUpCg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => {
        //         rampUpParticle.gameObject.SetActive(false);
        //     });

        //     // Start background particle behind the pet
        //     backgroundParticle.gameObject.SetActive(true);
        //     backgroundParticle.transform.SetSiblingIndex(0); // Move to back
        //     backgroundCg.alpha = 0f;
        //     backgroundCg.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);

        //     // Intense shake at transformation moment
        //     spineGraphic.rectTransform.DOShakePosition(0.5f, strength: 20f, vibrato: 40, randomness: 90f);
        // });

        // // NEW: Add skeleton swapping sequence after background particle starts
        // seq.AppendInterval(0.5f); // Wait for background particle to fade in
        // seq.AppendCallback(() =>
        // {
        //     // Simpan skeleton awal
        //     var originalSkeleton = spineGraphic.skeletonDataAsset;
        //     var rt = spineGraphic.rectTransform;
        //     var swapSeq = DOTween.Sequence();

        //     // Particle foreground 1 di awal
        //     if (foregroundParticle1 != null)
        //     {
        //         foregroundParticle1.gameObject.SetActive(true);
        //         foreground1Cg.alpha = 0f;
        //         foreground1Cg.DOFade(1f, 0.8f).SetEase(Ease.InOutSine);
        //     }

        //     void AppendSwap(
        //         SkeletonDataAsset asset,
        //         float upDur,                 // durasi scale 0->1 (juga dipakai untuk shake)
        //         float posShakeStrength,
        //         int posVibrato,
        //         bool triggerFg2 = false,
        //         float fg2Fade = 1.0f,
        //         bool endAtOne = false,       // true untuk step terakhir agar berhenti di 1
        //         float downMult = 0.6f,       // pengali durasi turun 1->0
        //         float holdAtOne = 0f         // 0 = tanpa hold supaya benar2 no-gap
        //     )
        //     {
        //         float downDur = Mathf.Max(0.02f, upDur * downMult);

        //         // ganti skeleton + reset scale
        //         swapSeq.AppendCallback(() =>
        //         {
        //             spineGraphic.skeletonDataAsset = asset;
        //             spineGraphic.Initialize(true);

        //             rt.DOKill();
        //             rt.localScale = Vector3.zero;

        //             if (triggerFg2 && foregroundParticle2)
        //             {
        //                 foregroundParticle2.gameObject.SetActive(true);
        //                 if (foreground2Cg) { foreground2Cg.alpha = 0f; foreground2Cg.DOFade(1f, fg2Fade).SetEase(Ease.InOutSine); }
        //             }
        //         });

        //         // 0 -> 1 sambil shake posisi (tidak ada interval tambahan)
        //         swapSeq.Append(rt.DOScale(Vector3.one, upDur).SetEase(Ease.OutBack));
        //         swapSeq.Join(rt.DOShakePosition(upDur, posShakeStrength, posVibrato, 90f));

        //         if (holdAtOne > 0f)
        //             swapSeq.AppendInterval(holdAtOne);

        //         // shake scale singkat persis setelah mencapai 1 (opsional, durasi pendek)
        //         swapSeq.Append(rt.DOShakeScale(0.06f, 0.10f, 10, 90f));

        //         if (!endAtOne)
        //         {
        //             // langsung turun 1 -> 0 TANPA interval
        //             swapSeq.Append(rt.DOScale(Vector3.zero, downDur).SetEase(Ease.InBack));
        //         }
        //         else
        //         {
        //             // final: tetap di 1 (boleh kasih settle kecil)
        //             swapSeq.Append(rt.DOScale(1.02f, 0.05f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine));
        //         }

        //         // ⛔️ TIDAK ADA AppendInterval di sini → benar2 no delay antar swap
        //     }

        //     // nyalakan FG1 di awal kalau perlu
        //     if (foregroundParticle1)
        //     {
        //         foregroundParticle1.gameObject.SetActive(true);
        //         if (foreground1Cg) { foreground1Cg.alpha = 0f; foreground1Cg.DOFade(1f, 0.6f).SetEase(Ease.InOutSine); }
        //     }

        //     // jumlah step
        //     int steps = 16;

        //     // generate swap bolak-balik tanpa jeda
        //     for (int i = 0; i < steps - 1; i++)
        //     {
        //         // variasi durasi & shake makin cepat/kuat
        //         float t = (float)i / (steps - 1);
        //         float up = Mathf.Lerp(0.28f, 0.08f, t);             // 0.28s → 0.08s
        //         float strength = Mathf.Lerp(16f, 36f, t);           // 16 → 36
        //         int vibrato = Mathf.RoundToInt(Mathf.Lerp(20f, 44f, t));

        //         bool toEvolved = (i % 2 == 0); // ganjil/genap
        //         var asset = toEvolved ? nextEvolutionSkeleton : originalSkeleton;

        //         AppendSwap(asset, up, strength, vibrato,
        //                 triggerFg2: (i == 2)); // contoh: FG2 nyala pada step 3
        //     }

        // // FINAL: berhenti di EVOLUSI & scale=1 (tanpa delay)
        // // AppendSwap(nextEvolutionSkeleton, 0.08f, 38f, 45, endAtOne: true);
        // swapSeq.AppendCallback(() =>
        //     {
        //         // Paksa skeleton akhir = evolusi
        //         spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
        //         spineGraphic.Initialize(true);

        //         // Hentikan semua tween yg mungkin masih nempel
        //         rt.DOKill(true);              // true = complete tweens, aman

        //         // Tampilkan langsung
        //         rt.localScale = Vector3.one;  // langsung 1 (no DOScale)

        //         // (opsional) rapikan supaya pasti “diam” dan terlihat
        //         // rt.anchoredPosition3D = Vector3.zero;
        //         // rt.localRotation = Quaternion.identity;

        //         // (opsional) kalau pakai CanvasGroup untuk FG
        //         if (foreground1Cg) foreground1Cg.alpha = 1f;
        //         if (foreground2Cg) foreground2Cg.alpha = 1f;
        //     });
        // });

        // Wait for swap sequence to complete (total duration: ~2.6 seconds)
        seq.AppendInterval(2.7f);

        // Fade out monster while background particle is on (monster fades, background stays)
        var monsterCanvasGroup = spineGraphic.GetComponent<CanvasGroup>();
        if (monsterCanvasGroup == null)
        {
            monsterCanvasGroup = spineGraphic.gameObject.AddComponent<CanvasGroup>();
        }
        seq.Append(monsterCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutSine));

        // Change skeleton while monster is invisible (background particle still playing behind)
        seq.AppendCallback(() =>
        {
            spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            spineGraphic.Initialize(true);
            onEvolutionDataUpdate?.Invoke();
        });

        // Fade monster back in with new skeleton (background particle still playing behind)
        seq.Append(monsterCanvasGroup.DOFade(1f, 0.8f).SetEase(Ease.InOutSine));

        // Wait a moment, then transition to finish particle
        seq.AppendInterval(3.0f);
        seq.AppendCallback(() =>
        {
            // Fade out background particle
            backgroundCg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                backgroundParticle.gameObject.SetActive(false);
                // Reset sibling index when done
                backgroundParticle.transform.SetSiblingIndex(-1);
            });

            // Fade out foreground particles
            if (foreground1Cg != null)
            {
                foreground1Cg.DOFade(0f, 0.8f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    foregroundParticle1.gameObject.SetActive(false);
                });
            }

            if (foreground2Cg != null)
            {
                foreground2Cg.DOFade(0f, 0.8f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    foregroundParticle2.gameObject.SetActive(false);
                });
            }

            // Start finish particle
            // finishParticle.gameObject.SetActive(true);
            // finishCg.alpha = 0f;
            // finishCg.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        });

        // Let finish particle play for a while, then fade out
        // seq.AppendInterval(2.0f); // Monster visible with finish particle
        // seq.AppendCallback(() => {
        //     // Fade out finish particle
        //     finishCg.DOFade(0f, 1.5f).SetEase(Ease.InOutSine).OnComplete(() => {
        //         finishParticle.gameObject.SetActive(false);
        //     });
        // });

        // 4. Wait for finish particle fade to complete
        seq.AppendInterval(2f); // Wait for particle fade

        // 5. Play jumping animation sequence after evolution completes (BEFORE zoom out)
        seq.AppendCallback(() =>
        {
            // Play jumping animation
            if (spineGraphic != null && spineGraphic.AnimationState != null)
            {
                var data = spineGraphic.Skeleton?.Data;
                if (data != null)
                {
                    string name = null;

                    if (data.FindAnimation("Jumping") != null) name = "Jumping";
                    else if (data.FindAnimation("jumping") != null) name = "jumping";
                    else if (data.FindAnimation("jump") != null) name = "jump";
                    else if (data.FindAnimation("Jump") != null) name = "Jump";

                    if (name != null)
                        spineGraphic.AnimationState.SetAnimation(0, name, false);
                    else
                        Debug.LogWarning("Animation 'Jumping/jumping' tidak ditemukan.", spineGraphic);
                }
            }
        });

        // Wait for jump animation to complete (adjust duration as needed)
        seq.AppendInterval(1.0f);

        // Return to idle animation
        seq.AppendCallback(() =>
        {
            if (spineGraphic.AnimationState != null)
            {
                spineGraphic.AnimationState.SetAnimation(0, "idle", true);
            }
        });

        // Wait a moment in idle before starting zoom out
        seq.AppendInterval(0.5f);

        // 6. SIMULATE ZOOM OUT: Scale back down and return to original position
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>().DOScale(Vector3.one * 1.2f, 1.0f).SetEase(Ease.InOutCubic));
        seq.Join(spineGraphic.rectTransform.DOAnchorPos(originalPos, 1.0f).SetEase(Ease.InOutCubic));
        // ADD: Return parent to original position
        seq.Join(spineGraphic.transform.parent.GetComponent<RectTransform>().DOAnchorPos(originalParentPos, 1.0f).SetEase(Ease.InOutCubic));

        // 7. Final scale to normal size and ensure final positioning
        seq.Append(spineGraphic.transform.parent.GetComponent<RectTransform>().DOScale(originalParentScale, 0.5f).SetEase(Ease.OutBack));

        seq.AppendCallback(() =>
        {
            // Restore original anchor and pivot settings
            spineGraphic.rectTransform.anchoredPosition = originalPos;
            spineGraphic.rectTransform.anchorMin = originalAnchorMin;
            spineGraphic.rectTransform.anchorMax = originalAnchorMax;
            spineGraphic.rectTransform.pivot = originalPivot;
            spineGraphic.rectTransform.localScale = originalScale;

            // Restore parent anchor and pivot settings
            var parentRect = spineGraphic.transform.parent.GetComponent<RectTransform>();
            parentRect.anchoredPosition = originalParentPos;
            parentRect.localScale = originalParentScale;
            parentRect.anchorMin = originalParentAnchorMin;
            parentRect.anchorMax = originalParentAnchorMax;
            parentRect.pivot = originalParentPivot;

            // Disable blur effect at the end of evolution
            if (biomeManager != null)
            {
                biomeManager.ToggleFilters("Darken", false);
            }
        });

        return seq;
    }
}
