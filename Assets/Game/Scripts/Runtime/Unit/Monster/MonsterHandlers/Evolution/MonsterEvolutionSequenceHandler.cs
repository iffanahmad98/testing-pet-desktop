using UnityEngine;
using DG.Tweening;
using Coffee.UIExtensions;
using Spine.Unity;

public class MonsterEvolutionSequenceHandler
{
    private Camera _mainCamera;
    private SkeletonGraphic _spineGraphic;
    private UIParticle _evolutionParticle;
    private Material _originalMaterial;
    private Material _whiteFlashMaterial;
    private Vector3 _originalCameraPos;
    private float _originalOrthoSize;

    public MonsterEvolutionSequenceHandler(Camera mainCamera, SkeletonGraphic spineGraphic, UIParticle evolutionParticle, Material whiteFlashMaterial)
    {
        _mainCamera = mainCamera;
        _spineGraphic = spineGraphic;
        _evolutionParticle = evolutionParticle;
        _whiteFlashMaterial = whiteFlashMaterial;
        _originalMaterial = _spineGraphic.material;
        _originalCameraPos = _mainCamera.transform.position;
        _originalOrthoSize = _mainCamera.orthographicSize;
    }

    public Sequence PlayEvolutionSequence(SkeletonDataAsset nextEvolutionSkeleton, System.Action onEvolutionDataUpdate)
    {
        var seq = DOTween.Sequence();

        // 1. Camera zoom in
        seq.Append(_mainCamera.DOOrthoSize(2.5f, 0.7f).SetEase(Ease.InOutSine)); // Adjust size as needed
        seq.Join(_mainCamera.transform.DOMove(_spineGraphic.transform.position + new Vector3(0,0,-10), 0.7f).SetEase(Ease.InOutSine));

        // 2. Flash/Glow effect
        seq.AppendCallback(() => _spineGraphic.material = _whiteFlashMaterial);
        seq.Append(_spineGraphic.DOFade(0.5f, 0.2f).SetLoops(4, LoopType.Yoyo)); // Flashing

        // 3. Play particle
        seq.AppendCallback(() => _evolutionParticle.Play());

        // 4. Change skeleton data (evolve)
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() => {
            _spineGraphic.skeletonDataAsset = nextEvolutionSkeleton;
            _spineGraphic.Initialize(true);
            onEvolutionDataUpdate?.Invoke(); // Update data (level, etc)
        });

        // 5. Restore material, stop particle
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() => {
            _spineGraphic.material = _originalMaterial;
            _evolutionParticle.Stop();
        });

        // 6. Camera zoom out
        seq.Append(_mainCamera.DOOrthoSize(_originalOrthoSize, 0.7f).SetEase(Ease.InOutSine));
        seq.Join(_mainCamera.transform.DOMove(_originalCameraPos, 0.7f).SetEase(Ease.InOutSine));

        return seq;
    }
}
