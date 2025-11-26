using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIShakeWithButton : MonoBehaviour
{
    [Header("Button Reference")]
    public Button button;

    [Header("Target to Shake")]
    public GameObject targetUI;

    [Header("Shake Settings")]
    public float duration = 0.2f;
    public float strength = 20f;
    public int vibrato = 10;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(Shake);
        }
    }

    public void Shake()
    {
        if (targetUI == null)
            targetUI = gameObject;

        RectTransform rt = targetUI.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.DOShakeAnchorPos(duration, strength, vibrato);
        }
    }
}
