using UnityEngine;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;

public static class MonsterEvolutionSequenceHelper
{
    public static Sequence PlayEvolutionUISequence(
        Camera mainCamera,
        SkeletonGraphic spineGraphic,
        UIParticle evolutionParticle,
        Material whiteFlashMaterial,
        SkeletonDataAsset nextEvolutionSkeleton,
        System.Action onEvolutionDataUpdate)
    {
        var originalMaterial = spineGraphic.material;
        var originalCameraPos = mainCamera.transform.position;
        var originalOrthoSize = mainCamera.orthographicSize;

        var seq = DOTween.Sequence();

        // 1. Camera zoom in
        seq.Append(mainCamera.DOOrthoSize(2.5f, 0.7f).SetEase(Ease.InOutSine));
        seq.Join(mainCamera.transform.DOMove(spineGraphic.transform.position + new Vector3(0, 0, -10), 0.7f).SetEase(Ease.InOutSine));

        // 2. Flash/Glow effect
        seq.AppendCallback(() => spineGraphic.material = whiteFlashMaterial);
        seq.Append(spineGraphic.DOFade(0.5f, 0.2f).SetLoops(4, LoopType.Yoyo));

        // 3. Play particle
        seq.AppendCallback(() => evolutionParticle.Play());

        // 4. Change skeleton data (evolve)
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() => {
            spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            spineGraphic.Initialize(true);
            onEvolutionDataUpdate?.Invoke();
        });

        // 5. Restore material, stop particle
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() => {
            spineGraphic.material = originalMaterial;
            evolutionParticle.Stop();
        });

        // 6. Camera zoom out
        seq.Append(mainCamera.DOOrthoSize(originalOrthoSize, 0.7f).SetEase(Ease.InOutSine));
        seq.Join(mainCamera.transform.DOMove(originalCameraPos, 0.7f).SetEase(Ease.InOutSine));

        return seq;
    }
}
