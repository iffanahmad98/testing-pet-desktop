using UnityEngine;
using DG.Tweening;

public class DOTweenScaleBounce : MonoBehaviour, IDOTweenPlayable
{
    [Header("Scale Settings")]
    public float startScale = 1f;
    public float peakScale = 1.4f;
    public float endScale = 1f;

    public float durationUpScale = 0.25f;
    public float durationDownScale = 0.25f;

    public float timeDuration = 0.5f;
    [Header("Position Settings (use local positions for non-UI, anchored for UI)")]
    public Vector3 startPosition;
    public Vector3 peakPosition;
    public Vector3 endPosition;

    public float durationUpMove = 0.25f;
    public float durationDownMove = 0.25f;

    [Header("Options")]
    public bool playOnStart = false;
    public bool useStartPosition = false;
    public bool treatAsUIAnchored = true; // kalau true => gunakan RectTransform.DOAnchorPos / DOAnchorPos3D

    Sequence seq;
    RectTransform rectT;
    Transform tf;

    void Awake()
    {
        rectT = GetComponent<RectTransform>();
        tf = transform;

        if (useStartPosition)
        {
            if (rectT != null && treatAsUIAnchored)
            {
                // anchoredPosition menggunakan Vector2 (x,y) — z diabaikan kecuali DOAnchorPos3D dipakai
                rectT.anchoredPosition = new Vector2(startPosition.x, startPosition.y);
                rectT.localPosition = new Vector3(rectT.localPosition.x, rectT.localPosition.y, startPosition.z);
            }
            else
            {
                tf.localPosition = startPosition;
            }
        }

        tf.localScale = Vector3.one * startScale;
    }

    void Start () {
        if (playOnStart) Play ();
    }

    public void Play()
    {
        if (seq != null && seq.IsActive())
            seq.Kill();

        // Pastikan posisi awal tepat sebelum tween DIMULAI
        if (useStartPosition)
        {
            if (rectT != null && treatAsUIAnchored)
            {
                rectT.anchoredPosition = new Vector2(startPosition.x, startPosition.y);
                rectT.localPosition = new Vector3(rectT.localPosition.x, rectT.localPosition.y, startPosition.z);
            }
            else
            {
                tf.localPosition = startPosition;
            }
        }

        tf.localScale = Vector3.one * startScale;

        seq = DOTween.Sequence();

        // --- SCALE + MOVE UP ---
        seq.Join(tf.DOScale(peakScale, durationUpScale).SetEase(Ease.OutCubic));

        if (rectT != null && treatAsUIAnchored)
            seq.Join(rectT.DOAnchorPos3D(peakPosition, durationUpMove).SetEase(Ease.OutCubic));
        else
            seq.Join(tf.DOLocalMove(peakPosition, durationUpMove).SetEase(Ease.OutCubic));

        // --- Tunggu kalau diperlukan ---
        if (timeDuration > 0f)
            seq.AppendInterval(timeDuration);

        // --- SCALE + MOVE DOWN ---
        seq.Append(tf.DOScale(endScale, durationDownScale).SetEase(Ease.InCubic));

        if (rectT != null && treatAsUIAnchored)
            seq.Join(rectT.DOAnchorPos3D(endPosition, durationDownMove).SetEase(Ease.InCubic));
        else
            seq.Join(tf.DOLocalMove(endPosition, durationDownMove).SetEase(Ease.InCubic));
    }


    // optional helper untuk memaksa treatAsUIAnchored sesuai ada/tidaknya RectTransform
    void Reset()
    {
        rectT = GetComponent<RectTransform>();
        if (rectT == null) treatAsUIAnchored = false;
    }
}
