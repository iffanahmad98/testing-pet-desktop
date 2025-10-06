using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;


public class CoinController : MonoBehaviour, IPointerDownHandler, ITargetable, IPointerEnterHandler
{
    [SerializeField] CoinType type;
    [SerializeField] float rate;
    [SerializeField] int value;
    public CoinType Type => type;

    public bool IsTargetable => gameObject.activeInHierarchy;
    public Vector2 Position => rectTransform.anchoredPosition;

    private Animator animator;
    private RectTransform rectTransform;
    private bool isCollected = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rectTransform = GetComponentInChildren<RectTransform>();
    }

    public void Initialize(CoinType coinType)
    {
        isCollected = false;
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
        if (isCollected) return;

        var coinRectTransform = rectTransform.GetChild(1).transform;
        
        coinRectTransform.DOJump(coinRectTransform.position, 200, 1, 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                CoinManager.AddCoins(value);
                ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
            });        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCollected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnCollected();
    }
}

