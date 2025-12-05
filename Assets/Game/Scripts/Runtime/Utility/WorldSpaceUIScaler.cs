using UnityEngine.UI;
using UnityEngine;

public class WorldSpaceUIScaler : MonoBehaviour
{
    public Camera cam;
    public float baseOrthographicSize = 11.12f;
    public float baseScale = 1f;

    public float offsetX = 0.5f;
    public float offsetY = 0f;

    private RectTransform rect;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        // === 1. SCALE ===
        float ratio = cam.orthographicSize / baseOrthographicSize;
        rect.localScale = Vector3.one * (baseScale * ratio);

        // === 2. POSITION ===
        float orthographicWidth = cam.orthographicSize * cam.aspect;
        float leftX = cam.transform.position.x - orthographicWidth;

        Vector3 pos = rect.position;
        pos.x = leftX + offsetX * ratio;
        pos.y = cam.transform.position.y + offsetY * ratio;
        rect.position = pos;

        // === 3. FORCE LAYOUT UPDATE ===
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}
