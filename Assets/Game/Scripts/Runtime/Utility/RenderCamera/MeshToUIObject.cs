using UnityEngine;
using UnityEngine.UI;

public class MeshToUIObject : MonoBehaviour
{
    public RawImage targetRawImage;

    void Start()
    {
        targetRawImage.texture = UIRenderCameraManager.Instance.renderTexture;

        int layer = LayerMask.NameToLayer("Motion UI");
        gameObject.layer = layer;
    }
}
