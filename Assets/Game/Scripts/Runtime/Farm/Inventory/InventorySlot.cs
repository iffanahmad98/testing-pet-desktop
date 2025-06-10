using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Farm;

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

        public void OnClick()
        {
            if (currentItem == null) return;
            Debug.Log($"Clicked on item: {currentItem.itemData.displayName}");

            // Contoh: tanam jika seed
            if (currentItem.itemData.itemType == ItemType.Seed)
            {
                // InventoryManager.Instance.RemoveItem(currentItem.itemData, 1);
                TileManager.Instance.SetActionSeed(currentItem.itemData);
                CursorIconManager.Instance.ShowSeedIcon(currentItem.itemData.icon);
            }
            if (currentItem.itemData.itemType == ItemType.MonsterSeed)
            {
                // InventoryManager.Instance.RemoveItem(currentItem.itemData, 1);
                TileManager.Instance.SetActionSeed(currentItem.itemData);
                CursorIconManager.Instance.ShowSeedIcon(currentItem.itemData.icon);
            }
        }
    }
}