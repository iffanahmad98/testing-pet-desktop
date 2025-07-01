using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MedicineController : MonoBehaviour, IPointerUpHandler, IConsumable
{
    private ItemDataSO itemData;
    public float nutritionValue;

    public MonsterController claimedBy;
    public event System.Action OnPlaced;

    [SerializeField] private Image medicineImage;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(ItemDataSO data, RectTransform groundRect = null)
    {
        itemData = data;
        nutritionValue = data.nutritionValue;

        if (medicineImage != null && itemData.itemImgs.Length > 0)
        {
            medicineImage.sprite = itemData.itemImgs[0];
        }
    }

    public void Consume(MonsterController monster = null)
    {
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }

    public ItemDataSO GetItemData() => itemData;

    public void Place()
    {
        OnPlaced?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!ServiceLocator.Get<MonsterManager>().IsPositionInGameArea(rectTransform.anchoredPosition))
        {
            ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
        }
    }

    // Food-style claiming logic (optional if needed)
    public bool TryClaim(MonsterController monster)
    {
        if (claimedBy == null)
        {
            claimedBy = monster;
            return true;
        }
        return claimedBy == monster;
    }

    public void ReleaseClaim(MonsterController monster)
    {
        if (claimedBy == monster)
        {
            claimedBy = null;
        }
        else if (claimedBy != null)
        {
            Debug.LogWarning($"[FoodController] {monster.name} tried to release but {claimedBy.name} owns it");
        }
    }

    public void ForceReleaseClaim()
    {
        claimedBy = null;
    }

    public bool IsClaimedBy(MonsterController monster) => claimedBy == monster;
    public bool IsClaimed => claimedBy != null;
}
