using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
public class MeshToUIBridge : MonoBehaviour
{
    [Header("UI Target")]
    public RawImage targetRawImage;

    [Header("Render Texture")]
    public Vector2Int renderSize = new Vector2Int(512, 512);

    [Header("Camera View Control")]
    [Tooltip("Semakin kecil = zoom in, semakin besar = zoom out")]
    public float orthographicSize = 1.5f;

    [Tooltip("Geser view kamera (X = kiri/kanan, Y = atas/bawah)")]
    public Vector2 cameraOffset;

    [Tooltip("Jarak kamera ke mesh (biasanya negatif)")]
    public float cameraZ = -5f;

    Camera uiCamera;
    RenderTexture rt;

    void Start()
    {
        // RenderTexture
        rt = new RenderTexture(renderSize.x, renderSize.y, 16, RenderTextureFormat.ARGB32);
        rt.Create();

        targetRawImage.texture = rt;

        // Camera
        GameObject camObj = new GameObject("UI_Mesh_Camera");
        camObj.transform.SetParent(transform);
        uiCamera = camObj.AddComponent<Camera>();

        uiCamera.clearFlags = CameraClearFlags.Color;
        uiCamera.backgroundColor = Color.clear;
        uiCamera.orthographic = true;
        uiCamera.targetTexture = rt;

        // Layer isolasi
        int layer = LayerMask.NameToLayer("Motion UI");
        if (layer == -1)
        {
            Debug.LogError("Layer 'Motion UI' belum dibuat");
            return;
        }

        gameObject.layer = layer;
        uiCamera.cullingMask = 1 << layer;

        ApplyCameraSettings();
    }

    void LateUpdate()
    {
        ApplyCameraSettings();
    }

    void ApplyCameraSettings()
    {
        if (!uiCamera) return;

        uiCamera.orthographicSize = orthographicSize;
        uiCamera.transform.localPosition = new Vector3(
            cameraOffset.x,
            cameraOffset.y,
            cameraZ
        );
    }

    void OnDestroy()
    {
        if (rt != null)
            rt.Release();
    }
}
