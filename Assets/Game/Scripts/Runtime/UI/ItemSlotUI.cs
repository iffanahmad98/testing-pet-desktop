using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI amountText;

    private ItemDataSO itemData;
    private int itemAmount;

    public void Initialize(ItemDataSO data, int amount)
    {
        itemData = data;
        itemAmount = amount;

        iconImage.sprite = itemData.itemImgs[0];
        amountText.text = $"{amount} pcs";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            return;
        }

        if (itemAmount <= 0) return;

        var placementManager = ServiceLocator.Get<PlacementManager>();
        GameObject prefabToPlace = placementManager.GetPrefabForItemType(itemData.category);
        RectTransform canvas = placementManager.GetCanvasParent();

        if (prefabToPlace == null)
        {
            Debug.LogWarning($"No prefab assigned for category {itemData.category}");
            return;
        }

        placementManager.StartPlacement(
            prefabToPlace,
            canvas,
            OnConfirmPlacement,
            OnCancelPlacement,
            allowMultiple: itemData.category == ItemType.Food,
            isMedicine: itemData.category == ItemType.Medicine,
            previewSprite: itemData.itemImgs[0]
        );
    }

    private void OnConfirmPlacement(Vector2 position)
    {
        if (itemAmount <= 0)
        {
            Debug.LogWarning("Tried to place item with 0 amount");
            return;
        }

        ServiceLocator.Get<MonsterManager>().SpawnItem(itemData, position);

        itemAmount--;
        amountText.text = $"{itemAmount} pcs";
        SaveSystem.UpdateItemData(itemData.itemID, itemAmount);

        if (itemAmount <= 0)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            Destroy(gameObject); // Remove slot if no items left
        }
    }

    private void OnCancelPlacement()
    {
        Debug.Log("Placement cancelled.");
    }
}
