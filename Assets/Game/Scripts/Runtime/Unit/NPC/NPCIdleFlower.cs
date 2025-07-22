using UnityEngine;

public class NPCIdleFlower : MonoBehaviour, ITargetable
{
    private bool _isTargetable = true;
    private RectTransform _rectTransform;
    private Vector2 _position;
    [SerializeField] private Transform NPC1Position;
    [SerializeField] private Transform NPC2Position;
    public bool IsTargetable => _isTargetable;
    public Vector2 Position => _position;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _position = _rectTransform.anchoredPosition;
    }
}
