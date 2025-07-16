using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickableObject : MonoBehaviour
{
    [Header("Menu")]
    public UnityEvent onEnterTrigger;
    public UnityEvent onExitTrigger;

    [Header("Hover Effect")]
    public float hoverScaleMultiplier = 1.1f;
    public float scaleSpeed = 5f;

    private Vector3 originalScale;
    private bool isHovered = false;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        Vector3 targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        isHovered = true;
    }

    private void OnMouseExit()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        isHovered = false;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        ShowMenu();
    }

    public void ShowMenu()
    {
        onEnterTrigger.Invoke();
    }

    public void HideMenu()
    {
        onExitTrigger.Invoke();
    }
}