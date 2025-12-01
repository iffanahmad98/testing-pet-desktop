using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasFixer : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private Camera uiCamera;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && uiCamera != null)
        {
            // Re-assign kamera agar posisi & scale canvas tetap akurat
            canvas.worldCamera = uiCamera;
            canvas.planeDistance = 30f;
        }
    }
}
