using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragArea : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public RectTransform targetToDrag; // panel utama yang ingin dipindahkan
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Opsional: efek saat mulai drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetToDrag == null || canvas == null) return;
        targetToDrag.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}