using UnityEngine;

public enum CursorType { Default, Monster, Poop }

public class CursorManager : MonoBehaviour
{
    [SerializeField] CursorMap map;
    [SerializeField] Vector2 hotspot;

    void Awake()
    {
        hotspot = Vector2.zero;
        Reset();
        ServiceLocator.Register(this);
    }

    private Vector2 SetHotspot(CursorType t)
    {
        var cursorTexture = map.Get(t);
        hotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        return hotspot;
    }

    public void Set(CursorType t) => Cursor.SetCursor(map.Get(t), hotspot, CursorMode.Auto);
    public void Reset() => Cursor.SetCursor(map.defaultTex, hotspot, CursorMode.Auto);
    void OnDestroy() => ServiceLocator.Unregister<CursorManager>();
}
