using UnityEngine;
using UnityEngine.EventSystems;


public class CoinController : MonoBehaviour, IPointerDownHandler, ITargetable
{
    [SerializeField] CoinType type;
    [SerializeField] float rate;
    [SerializeField] int value;
    public CoinType Type => type;

    public bool IsTargetable => gameObject.activeInHierarchy;
    public Vector2 Position => rectTransform.anchoredPosition;

    private Animator animator;
    private RectTransform rectTransform;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(CoinType coinType)
    {
        type = coinType;
        value = (int)type;
        if (coinType == CoinType.Platinum)
        {
            animator.SetTrigger("Silver");
            transform.localScale = Vector3.one; 
        }
        if (coinType == CoinType.Gold)
        {
            animator.SetTrigger("Gold");
            transform.localScale = Vector3.one * 0.7f;
        }
    }

    public void OnCollected()
    {
        ServiceLocator.Get<MonsterManager>().OnCoinChanged?.Invoke(ServiceLocator.Get<MonsterManager>().coinCollected += value);
        SaveSystem.SaveCoin(ServiceLocator.Get<MonsterManager>().coinCollected);
        SaveSystem.Flush();
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCollected();
    }
}

