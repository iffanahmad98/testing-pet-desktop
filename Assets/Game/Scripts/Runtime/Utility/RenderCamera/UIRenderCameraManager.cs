using UnityEngine;

public class UIRenderCameraManager : MonoBehaviour
{
    public static UIRenderCameraManager Instance;

    public Camera renderCamera;
    public RenderTexture renderTexture;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!renderCamera)
        {
            GameObject camObj = new GameObject("UI_RenderCamera");
            renderCamera = camObj.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.backgroundColor = Color.clear;
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = 3f;
        }

        if (!renderTexture)
        {
            renderTexture = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
        }

        renderCamera.targetTexture = renderTexture;
        renderCamera.cullingMask = 1 << LayerMask.NameToLayer("Motion UI");
    }
}
