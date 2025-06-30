using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MedicineController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IConsumable
{
    private ItemDataSO itemData;
    public event System.Action OnPlaced;
    public MonsterController claimedBy;
    public float nutritionValue;

    [SerializeField] private Image medicineImage;

    private RectTransform rectTransform;
    private Vector2 dragOffset;
    public bool IsBeingDragged { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(ItemDataSO data, RectTransform groundRect = null)
    {
        itemData = data;

        medicineImage.sprite = itemData.itemImgs[0]; // Only 1 image needed
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

    public void OnPointerDown(PointerEventData eventData)
    {
        IsBeingDragged = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsBeingDragged) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos))
        {
            rectTransform.anchoredPosition = localPos + dragOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsBeingDragged = false;

        MonsterController targetMonster = TryFindMonsterUnderPointer(eventData);
        Debug.Log($"[MedicineController] Placing medicine on monster: {targetMonster?.name}");

        if (targetMonster != null)
        {
            // Heal sick monster
            targetMonster.GiveMedicine(nutritionValue);
            ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
        }
        else
        {
            // Invalid placement: not on a monster or monster isn't sick
            ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
        }
    }
    private MonsterController TryFindMonsterUnderPointer(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var monster = result.gameObject.GetComponentInParent<MonsterController>();
            if (monster != null)
                return monster;
        }
        return null;
    }

}
