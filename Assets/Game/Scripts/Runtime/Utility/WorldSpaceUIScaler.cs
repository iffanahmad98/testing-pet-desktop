using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WorldCanvasFitCamera : MonoBehaviour
{
    public Camera cam;
    private RectTransform rect;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        float ortho = cam.orthographicSize;
        float worldHeight = ortho * 2f;                  // tinggi world yg terlihat kamera
        float worldWidth = worldHeight * cam.aspect;     // lebar world yg terlihat kamera

        // Set ukuran canvas ke ukuran kamera secara presisi
        rect.sizeDelta = new Vector2(worldWidth, worldHeight);

        // Posisi tepat di tengah kamera
        rect.position = cam.transform.position 
                      + new Vector3(0, 0, 1f); // Z harus tetap di depan kamera
    }
}
