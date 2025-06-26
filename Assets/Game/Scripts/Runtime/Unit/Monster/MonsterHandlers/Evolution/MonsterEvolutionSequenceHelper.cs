using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;
using CartoonFX;

public static class MonsterEvolutionSequenceHelper
{
    public static Sequence PlayEvolutionUISequence(
        Camera evolveCam,
        SkeletonGraphic spineGraphic,
        UIParticle evolutionParticle,
        Material whiteFlashMaterial,
        SkeletonDataAsset nextEvolutionSkeleton,
        System.Action onEvolutionDataUpdate,
        Vector3 monsterPosition)
    {
        var originalMaterial = spineGraphic.material;
        var originalFOV = evolveCam.fieldOfView;
        var originalPosition = MainCanvas.CamRT.anchoredPosition;
        var particle = evolutionParticle.gameObject;
        var particleCg = particle.GetComponent<CanvasGroup>();

        var effect = particle.GetComponent<CFXR_Effect>();
        var shake = effect.cameraShake;
        shake.useMainCamera = false;
        shake.cameras = new List<Camera> { MainCanvas.MonsterCamera };
        shake.enabled = true; 

        var seq = DOTween.Sequence();

        // 1. Camera zoom in
        seq.AppendCallback(() =>
        {
            MainCanvas.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            MainCanvas.Canvas.worldCamera = evolveCam;
        });
        seq.Append(MainCanvas.CamRT.DOAnchorPos(monsterPosition, 0.5f).SetEase(Ease.InOutSine));
        seq.Append(evolveCam.DOFieldOfView(2.5f, 0.8f).SetEase(Ease.InOutSine));

        // 2. Flash/Glow effect
        seq.AppendCallback(() =>
        {
            spineGraphic.material = whiteFlashMaterial;
        });
        seq.Join(spineGraphic.DOFade(0.5f, 0.2f).SetLoops(8, LoopType.Yoyo));

        // 3. Play particle
        seq.JoinCallback(() => {
            particle.SetActive(true);
            particleCg.alpha = 0f; 
            particleCg.DOFade(1f, 0.2f).SetEase(Ease.InOutSine);
            shake.StartShake(); // Show particle
        });

        // 4. Change skeleton data (evolve)
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => {
            spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            spineGraphic.Initialize(true);
            particle.SetActive(false); // Hide particle
            particleCg.DOFade(0f, 0.2f).SetEase(Ease.InOutSine);
            shake.StopShake(); // Stop camera shake
            onEvolutionDataUpdate?.Invoke();
        });

        // 5. Restore material, stop particle
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => spineGraphic.material = originalMaterial);
        seq.AppendInterval(1.5f);

        // 6. Camera zoom out
        seq.Append(evolveCam.DOFieldOfView(originalFOV, 0.8f).SetEase(Ease.InOutSine));
        seq.Join(MainCanvas.CamRT.DOAnchorPos(originalPosition, 0.5f).SetEase(Ease.InOutSine));
        seq.AppendCallback(() =>
        {
            MainCanvas.Canvas.worldCamera = null;
            MainCanvas.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        });

        return seq;
    }
}
