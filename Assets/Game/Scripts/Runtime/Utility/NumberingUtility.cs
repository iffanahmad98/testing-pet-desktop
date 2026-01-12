using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

public class NumberingUtility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text text;

    [Header("Settings")]
    [SerializeField] float changeSpeed = 60f; // angka per detik
    [SerializeField] Color decreaseColor = Color.red;
    [SerializeField] Color normalColor = Color.white;

    [Header("Scale Tween")]
    [SerializeField] float scaleDuration = 0.15f;
    [SerializeField] Vector3 decreaseScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] Vector3 normalScale = Vector3.one;

    public int lastNumber { get; private set; }

    Coroutine numberRoutine;
    Tween scaleTween;

    void Awake()
    {
        if (text == null)
            Debug.LogError("TMP_Text belum di-assign!");
    }

    /// <summary>
    /// Set angka awal tanpa animasi
    /// </summary>
    public void SetImmediate(int value)
    {
        if (numberRoutine != null)
            StopCoroutine(numberRoutine);

        scaleTween?.Kill();

        lastNumber = value;
        text.text = lastNumber.ToString();
        text.color = normalColor;
        text.transform.localScale = normalScale;
    }

    /// <summary>
    /// Panggil ini jika targetNumber < lastNumber
    /// </summary>
    public void AnimateTo(int targetNumber)
    {
        if (targetNumber >= lastNumber)
            return;

        if (numberRoutine != null)
            StopCoroutine(numberRoutine);

        numberRoutine = StartCoroutine(DecreaseRoutine(targetNumber));
    }

    IEnumerator DecreaseRoutine(int targetNumber)
    {
        text.color = decreaseColor;

        // SCALE DOWN
        scaleTween?.Kill();
        scaleTween = text.transform
            .DOScale(decreaseScale, scaleDuration)
            .SetEase(Ease.OutBack);

        float current = lastNumber;

        while (current > targetNumber)
        {
            current -= changeSpeed * Time.deltaTime;
            current = Mathf.Max(current, targetNumber);

            lastNumber = Mathf.FloorToInt(current);
            text.text = lastNumber.ToString();

            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // HARD SYNC
        lastNumber = targetNumber;
        text.text = targetNumber.ToString();

        text.color = normalColor;

        // SCALE BACK TO NORMAL
        scaleTween?.Kill();
        scaleTween = text.transform
            .DOScale(normalScale, scaleDuration)
            .SetEase(Ease.OutBack);

        numberRoutine = null;
    }
}
