using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Farm;
using MagicalGarden.Manager;

namespace MagicalGarden.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI quantityText;
        public TextMeshProUGUI nameText;
        public Button button;

        private InventoryItem currentItem;

        public void SetSlot(InventoryItem item)
        {
            currentItem = item;
            icon.sprite = item.itemData.icon;
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
            nameText.text = item.itemData.displayName;
            gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        public void ClearSlot()
        {
            currentItem = null;
            icon.sprite = null;
            quantityText.text = "";
            gameObject.SetActive(false);
        }
        public bool HasItem(ItemData item)
        {
            return currentItem != null && currentItem.itemData.itemId == item.itemId;
        }

        public void OnClick()
        {
            if (currentItem == null) return;
            // Debug.Log($"Clicked on item: {currentItem.itemData.displayName}");
            switch (currentItem.itemData.itemType)
            {
                case ItemType.Seed:
                    TileManager.Instance.SetActionSeed(currentItem.itemData);
                    CursorIconManager.Instance.ShowSeedIcon(currentItem.itemData.icon);
                    break;
                case ItemType.MonsterSeed:
                    TileManager.Instance.SetActionSeed(currentItem.itemData);
                    CursorIconManager.Instance.ShowSeedIcon(currentItem.itemData.icon);
                    break;
                case ItemType.Fertilizer:
                    TileManager.Instance.SetActionFertilizer(currentItem.itemData);
                    CursorIconManager.Instance.ShowSeedIcon(currentItem.itemData.icon);
                    break;
                default:
                    break;
            }
        }
    }
}