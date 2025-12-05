using UnityEngine;
using DG.Tweening;

public class DOTweenColorWorld : MonoBehaviour, IDOTweenPlayable
{
    [Header("Color Settings")]
    public Color startColor = Color.white;
    public Color peakColor = Color.white;
    public Color endColor = Color.white;

    public float durationUpColor = 0.25f;
    public float durationDownColor = 0.25f;
    public float holdTime = 0.2f;

    public bool useStartColor = false;
    public bool useEndColor = false;

    SpriteRenderer sr;
    Sequence seq;

    [Header("Options")]
    public bool playOnStart = false;

    void Start()
    {
        if (playOnStart) Play ();
    }

    void StartInitialize()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("DOTweenColorWorld membutuhkan SpriteRenderer!");
            enabled = false;
            return;
        }

        if (useStartColor)
            SetColor(startColor);
    }

    public void Play()
    {
        StartInitialize();

        if (seq != null && seq.IsActive())
            seq.Kill();

        seq = DOTween.Sequence();

        // Reset ke startColor
        if (useStartColor)
            SetColor(startColor);

        // --- COLOR UP (ke peakColor) ---
        seq.Append(sr.DOColor(peakColor, durationUpColor).SetEase(Ease.OutCubic));

        // Tahan sedikit
        if (holdTime > 0f)
            seq.AppendInterval(holdTime);

        // --- COLOR DOWN (ke endColor) ---
        if (useEndColor)
        {
            seq.Append(sr.DOColor(endColor, durationDownColor).SetEase(Ease.InCubic));
        }
    }

    void SetColor(Color c)
    {
        sr.color = c;
    }
}
