using UnityEngine;
using UnityEngine.EventSystems;
public class ClickManager : MonoBehaviour
{
    public LayerMask interactableLayer;
    public Camera mainCamera;
    private ClickableObject lastClickedObject = null;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, interactableLayer);

            if (hit != null)
            {
                var clickable = hit.GetComponent<ClickableObject>();
                if (clickable != null)
                {
                    if (lastClickedObject == clickable)
                    {
                        clickable.HideMenu();
                        lastClickedObject = null;
                        Debug.Log("Menu disembunyikan: " + hit.name);
                    }
                    else
                    {
                        if (lastClickedObject != null)
                        {
                            lastClickedObject.HideMenu();
                        }

                        clickable.ShowMenu();
                        lastClickedObject = clickable;
                        Debug.Log("Menu ditampilkan: " + hit.name);
                    }

                    return;
                }
            }

            // Klik di luar objek: tidak melakukan apa-apa
        }
    }
}