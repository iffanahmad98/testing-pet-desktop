using DG.Tweening;
using UnityEngine;

public class DotweenScaleBounceLoop : MonoBehaviour
{
    public Transform targetObject;

    public Vector3 startScale = Vector3.one;
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
    public Vector3 endScale = Vector3.one;

    public float durationUp = 0.25f;
    public float durationDown = 0.25f;

    void Start()
    {
        targetObject.localScale = startScale;

        targetObject
            .DOScale(targetScale, durationUp)
            .SetEase(Ease.OutBack)
            .SetLoops(-1, LoopType.Yoyo);   // naik turun otomatis
    }
}