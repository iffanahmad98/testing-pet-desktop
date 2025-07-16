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
    public ItemDataSO ItemDataSO => itemData;
    private int itemAmount;
    private ItemInventoryUI inventoryUI;


    public void Initialize(ItemDataSO data, ItemType type, int amount)
    {
        itemData = data;
        itemAmount = amount;

        iconImage.sprite = itemData.itemImgs[0];
        amountText.text = $"{amount} pcs";
        inventoryUI = ServiceLocator.Get<ItemInventoryUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemAmount <= 0) return;

        // Check for Delete Mode first
        if (inventoryUI != null && inventoryUI.IsInDeleteMode)
        {
            inventoryUI.ConfirmDeleteItem(this);
            return;
        }

        // Right-click cancels placement
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            return;
        }

        // Normal placement behavior
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
        SaveSystem.UpdateItemData(itemData.itemID, itemData.category, itemAmount);

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
    public void ResetSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
        amountText.text = "";
    }

}
