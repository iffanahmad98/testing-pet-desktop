using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    public RectTransform target;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        rect.position = target.position;
    }
}
