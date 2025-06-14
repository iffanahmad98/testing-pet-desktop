using UnityEngine;
using TMPro;
using MagicalGarden.Inventory;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MagicalGarden.Manager;
using MagicalGarden.Farm;

namespace MagicalGarden.Shop
{
    public class ItemShop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public Image itemIcon;
        public TextMeshProUGUI titleText;
        public Button buyButton;
        private SheetData currentItemData;

        [Header("Tooltip Panel")]
        public GameObject tooltipPanel;
        public TextMeshProUGUI tooltipText;
        public void Setup(SheetData itemShop, Sprite iconSprite)
        {
            currentItemData = itemShop;
            var itemData = PlantManager.Instance.GetItemById(itemShop.seedName.ToLower());
            if (itemData != null)
            {
                itemIcon.sprite = itemData.icon;
            }
            else
            {
                Debug.LogWarning($"❌ ItemData tidak ditemukan untuk ID: {itemShop.seedName}");
                itemIcon.sprite = null;
            }
            titleText.text = itemShop.seedName;
            tooltipText.text = $"Harga: {itemShop.seedPrice}\n" +
                               $"Siram: {itemShop.wateringInterval}\n" +
                               $"Durasi: {itemShop.growDurationStages}\n" +
                               $"Total: {itemShop.totalGrowTime} jam\n" +
                               $"Layu: {itemShop.wiltTime} jam\n" +
                               $"Mati: {itemShop.deadTime} jam\n\n" +
                               $"{itemShop.description}";

            tooltipPanel.SetActive(false);
            buyButton.onClick.AddListener(OnBuy);
        }

        void OnBuy()
        {
            var itemData = PlantManager.Instance.GetItemById(currentItemData.seedName.ToLower());

            if (itemData != null)
            {
                InventoryManager.Instance.AddItem(itemData, 1);
                CoinManager.Instance.SpendCoins(10);
                InventoryManager.Instance.inventoryUI.RefreshUI();

                Debug.Log($"✅ Bought seed: {itemData.itemId}");
            }
            else
            {
                Debug.LogError($"❌ Item with name '{titleText.text}' not found in ItemData list.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltipPanel.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
