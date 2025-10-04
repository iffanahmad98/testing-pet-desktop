using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect; // drag ScrollRect ke sini di Inspector
    [SerializeField] private float scrollStep = 0.1f; // seberapa jauh gesernya sekali klik

    // Panggil fungsi ini di OnClick Button "Kanan"
    public void ScrollRight()
    {
        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + scrollStep);
    }

    // Panggil fungsi ini di OnClick Button "Kiri"
    public void ScrollLeft()
    {
        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition - scrollStep);
    }
}