using UnityEngine;

[ExecuteAlways]
public class AutoFitCamera : MonoBehaviour
{
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!cam.orthographic)
            return;

        float targetAspect = referenceWidth / referenceHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        // Camera orthographicSize menyesuaikan screen yg aktif
        if (currentAspect >= targetAspect)
        {
            // Layar lebih lebar → height yang menjadi patokan
            cam.orthographicSize = referenceHeight / 200f;
        }
        else
        {
            // Layar lebih sempit → width yang menjadi patokan
            float difference = targetAspect / currentAspect;
            cam.orthographicSize = (referenceHeight / 200f) * difference;
        }
    }
}
