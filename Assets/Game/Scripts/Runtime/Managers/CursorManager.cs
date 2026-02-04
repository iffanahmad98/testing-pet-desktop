using UnityEngine;

public enum CursorType { Default, Monster, Poop }

public class CursorManager : MonoBehaviour
{
    [SerializeField] CursorMapConfigSO map;
    public Vector2 hotspot;

    void Awake()
    {
        //hotspot = Vector2.zero;
        Reset();
        ServiceLocator.Register(this);
    }

    public void Set(CursorType t) => Set(t, hotspot);
    public void Set(CursorType t, Vector2 h) => Cursor.SetCursor(map.Get(t), h, CursorMode.Auto);
    public void Reset() => Cursor.SetCursor(map.defaultTex, hotspot, CursorMode.Auto);

    public Texture2D GetTexture(CursorType t) => map != null ? map.Get(t) : null;

    void OnDestroy() => ServiceLocator.Unregister<CursorManager>();
}
