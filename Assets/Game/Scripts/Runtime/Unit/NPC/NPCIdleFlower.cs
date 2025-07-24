using UnityEngine;
using UnityEngine.UIElements;

public class NPCIdleFlower : MonoBehaviour, ITargetable
{
    private bool _isTargetable = true;
    private RectTransform _rectTransform;
    private Vector2 _position;
    [SerializeField] private Transform IdleStation1;
    [SerializeField] private Transform IdleStation2;
    public bool IsTargetable => _isTargetable;
    public Vector2 Position => _position;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _position = _rectTransform.anchoredPosition;
    }

    public GameObject GetIdleStation(int npcIndex)
    {
        return npcIndex == 1 ? IdleStation1.gameObject : IdleStation2.gameObject;
    }
}
