using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public static Canvas Canvas { get; private set; }
    public static RectTransform CamRT { get; private set; }
    public static Camera MonsterCamera { get; private set; }
    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
        MonsterCamera = Canvas.transform.GetChild(0).GetComponent<Camera>();
        CamRT = MonsterCamera.GetComponent<RectTransform>();
    }
}
