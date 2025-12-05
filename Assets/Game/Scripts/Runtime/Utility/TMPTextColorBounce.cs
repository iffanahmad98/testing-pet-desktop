using UnityEngine;
using TMPro;
using DG.Tweening;

public class TMPTextColorBounce : MonoBehaviour, IDOTweenPlayable
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

    TMP_Text tmp;
    Sequence seq;

    [Header("Options")]
    public bool playOnStart = false;

    void Start()
    {
        tmp = GetComponent<TMP_Text>();
        if (tmp == null)
        {
            Debug.LogError("DOTweenColorTMP membutuhkan TMP_Text!");
            enabled = false;
            return;
        }

        if (useStartColor)
        {
            tmp.color = startColor;
            tmp.ForceMeshUpdate();           // FIX DELAY
        }

        if (playOnStart)
            Play();
    }

    public void Play()
    {
        // Kill sequence lama
        seq?.Kill();

        seq = DOTween.Sequence().SetAutoKill(false);

        // Reset
        if (useStartColor)
        {
            tmp.color = startColor;
            tmp.ForceMeshUpdate();          // FIX DELAY
        }

        // --- COLOR UP ---
        seq.Append(tmp.DOColor(peakColor, durationUpColor).SetEase(Ease.OutCubic));

        // HOLD
        if (holdTime > 0f)
            seq.AppendInterval(holdTime);

        // --- COLOR DOWN ---
        if (useEndColor)
        {
            seq.Append(tmp.DOColor(endColor, durationDownColor).SetEase(Ease.InCubic));
        }

        seq.PlayForward();
    }
}
