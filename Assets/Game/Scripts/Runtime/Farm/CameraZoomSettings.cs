using UnityEngine;

public class CameraZoomSettings : MonoBehaviour
{
    public Camera cam;
    public float defaultOrthographicSize = 11.12f;
    public float cameraLastZoom = 10.00f;

    public static CameraZoomSettings instance;
    
    void Awake () {
        instance = this;
    }
    
    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        cam.orthographicSize = 11.12f;
    }

    // HotelFacilitiesPodiumCard 
    public void SetDefaultCameraZoom()
    {
        cameraLastZoom = cam.orthographicSize;
        cam.orthographicSize = defaultOrthographicSize;
    }

    // HotelFacilitiesPodiumCard
    public void SetLastCameraZoom()
    {
        cam.orthographicSize = cameraLastZoom;
    }
}
