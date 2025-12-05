using UnityEngine;
using DG.Tweening;
using System;

public class DOTweenBounceWorld : MonoBehaviour, IDOTweenPlayable
{
    [Header("Scale Settings")]
    public float startScale = 1f;
    public float peakScale = 1.4f;
    public float endScale = 1f;

    public float durationUpScale = 0.25f;
    public float durationDownScale = 0.25f;

    [Header("Move Offset Settings")]
    public bool useStartPosition = false;
    public bool useDownAnimation = false;

    // Semua posisi adalah OFFSET relatif terhadap posisi awal
    public Vector3 startOffset;
    public Vector3 peakOffset;
    public Vector3 endOffset;

    public float durationUpMove = 0.25f;
    public float durationDownMove = 0.25f;
    public float holdTime = 0.2f;

    Sequence seq;
    Transform tf;

    Vector3 originalLocalPos;

    // HotelRandomLoot Reference
    public event Action <HotelRandomLootConfig, HotelRandomLootObject> OnTransitionFinished;
    HotelRandomLootConfig config; 
    HotelRandomLootObject configObject;

    [Header("Options")]
    public bool playOnStart = false;
    public bool playScaling = true;
    public bool destroyWhenDone = true;
    void Start () {
        if (playOnStart) Play ();
    }
    
    public void StartPlay(HotelRandomLootConfig configValue, HotelRandomLootObject configObjectValue)
    {
        config = configValue;
        configObject = configObjectValue;
        Play();
    }

    public void Play()
{
    tf = transform;
    originalLocalPos = tf.localPosition;

    if (seq != null && seq.IsActive())
        seq.Kill();

    seq = DOTween.Sequence();

    // Set start state
    if (useStartPosition)
        tf.localPosition = originalLocalPos + startOffset;
    else
        tf.localPosition = originalLocalPos;

    if (playScaling)
        tf.localScale = Vector3.one * startScale;

    // -------------------
    // UP PHASE
    // -------------------
    if (playScaling) {
        seq.Append(tf.DOScale(peakScale, durationUpScale).SetEase(Ease.OutCubic));
    }

    seq.Join(tf.DOLocalMove(originalLocalPos + peakOffset, durationUpMove).SetEase(Ease.OutCubic));

    // Hold
    if (holdTime > 0f)
        seq.AppendInterval(holdTime);

    // -------------------
    // DOWN PHASE
    // -------------------
    if (useDownAnimation)
    {
        if (playScaling) {
            seq.Append(tf.DOScale(endScale, durationDownScale).SetEase(Ease.InCubic));
        }

        seq.Join(tf.DOLocalMove(originalLocalPos + endOffset, durationDownMove).SetEase(Ease.InCubic));
    }

    seq.OnComplete(() =>
    {
        OnTransitionFinished?.Invoke(config, configObject);
        if (destroyWhenDone)
        Destroy(gameObject);
    });
}

}
