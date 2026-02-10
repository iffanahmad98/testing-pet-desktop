using UnityEngine;

public interface ITutorialPointer
{
    void PointTo(RectTransform target, Vector2 offset);

    void PointToWorld(Transform worldTarget, Vector3 worldOffset);
    void Hide();
}
