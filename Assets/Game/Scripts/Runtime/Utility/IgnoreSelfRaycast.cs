using UnityEngine;
using UnityEngine.UI;

public class IgnoreSelfRaycast : MonoBehaviour, ICanvasRaycastFilter
{
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // Selalu return false → parent tidak pernah dianggap kena raycast
        // Child tetap normal karena filter hanya berlaku pada object ini
        return false;
    }
}
