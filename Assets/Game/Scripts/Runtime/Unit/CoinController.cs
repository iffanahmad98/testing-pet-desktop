using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Coin
{
    public CoinType coinType;
    public float onSpawnRate;
    public float offSpawnRate;
    public bool InGame = true;
    public Sprite coinImg;
}

public class CoinController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] CoinType type;
    [SerializeField] float rate;
    [SerializeField] int value;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(CoinType coinType)
    {
        type = coinType;
        value = (int)type;
        if (coinType == CoinType.Gold)
        {
            animator.SetTrigger("Gold");
        }
        else if (coinType == CoinType.Silver)
        {
            animator.SetTrigger("Silver");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ServiceLocator.Get<GameManager>().OnCoinChanged?.Invoke(ServiceLocator.Get<GameManager>().coinCollected += value);
        SaveSystem.SaveCoin(ServiceLocator.Get<GameManager>().coinCollected);
        SaveSystem.Flush();
        ServiceLocator.Get<GameManager>().DespawnToPool(gameObject);
    }
}

