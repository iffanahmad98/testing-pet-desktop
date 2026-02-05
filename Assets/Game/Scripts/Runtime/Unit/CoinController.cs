using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;


public class CoinController : MonoBehaviour, IPointerDownHandler, ITargetable, IPointerEnterHandler
{
    [SerializeField] CoinType type;
    [SerializeField] float rate;
    [SerializeField] int value;
    public CoinType Type => type;

    public bool IsTargetable => gameObject.activeInHierarchy && ReservedBy == null;
    public Vector2 Position => rectTransform.anchoredPosition;

    // NPC reservation system to prevent multiple NPCs targeting the same coin
    public MonsterController ReservedBy { get; private set; }

    private Animator animator;
    private RectTransform rectTransform;
    private bool isCollected = false;

    public static event Action<CoinController> OnAnyPlayerCollected;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rectTransform = GetComponentInChildren<RectTransform>();
    }

    public void Initialize(CoinType coinType, int multiplier = 1)
    {
        isCollected = false;
        ReservedBy = null; // Reset reservation
        type = coinType;
        value = (int)type * multiplier;
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

    /// <summary>
    /// Reserve this coin for a specific NPC
    /// </summary>
    public void Reserve(MonsterController npc)
    {
        ReservedBy = npc;
    }

    /// <summary>
    /// Release the reservation (e.g., if NPC changes target)
    /// </summary>
    public void ReleaseReservation()
    {
        ReservedBy = null;
    }

    private void Collected(bool fromPlayer)
    {
        if (isCollected) return;

        isCollected = true;

        if (fromPlayer)
        {
            OnAnyPlayerCollected?.Invoke(this);
        }

        var coinRectTransform = rectTransform.GetChild(1).transform;

        MonsterManager.instance.audio.PlaySFX("collect_coin");

        coinRectTransform.DOJump(coinRectTransform.position, 200, 1, 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                CoinManager.AddCoins(value);
                ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();
                ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
            });

    }

    public void OnCollected()
    {
        Collected(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Collected(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Collected(true);
    }
}

