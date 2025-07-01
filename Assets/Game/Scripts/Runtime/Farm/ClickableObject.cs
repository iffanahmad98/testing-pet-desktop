using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickableObject : MonoBehaviour
{
    [Header("Menu")]
    public GameObject menuToShow;

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
        Vector3 targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void OnMouseEnter()
    {
        isHovered = true;
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }

    private void OnMouseDown()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        if (menuToShow != null)
        {
            menuToShow.SetActive(true);
        }
    }

    public void HideMenu()
    {
        if (menuToShow != null)
        {
            menuToShow.SetActive(false);
        }
    }
}