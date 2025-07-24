using UnityEngine;
using TMPro;
using MagicalGarden.Inventory;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MagicalGarden.Manager;
using MagicalGarden.Farm;
using DG.Tweening;

namespace MagicalGarden.Shop
{
    public class ItemShop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public Image itemIcon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI priceText;
        public Button buyButton;
        private SheetData currentItemData;
        private Sprite iconData;

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
                iconData = itemData.iconCrop;
            }
            else
            {
                Debug.LogWarning($"❌ ItemData tidak ditemukan untuk ID: {itemShop.seedName}");
                itemIcon.sprite = null;
            }
            titleText.text = itemShop.seedName;
            priceText.text = itemShop.seedPrice;
            tooltipText.text = $"Harga: {itemShop.seedPrice}\n" +
                               $"Siram: {itemShop.wateringInterval}\n" +
                               $"Durasi: {itemShop.growDurationStages}\n" +
                               $"Total: {itemShop.totalGrowTime} jam\n" +
                               $"Layu: {itemShop.wiltTimeDays} jam\n" +
                               $"Mati: {itemShop.deadTimeDays} jam\n\n" +
                               $"{itemShop.description}";

            tooltipPanel.SetActive(false);
            buyButton.onClick.AddListener(OnBuy);
        }

        void OnBuy()
        {
            transform.DOKill();
            transform.localScale = Vector3.one; 
            transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 10, 1);
            var itemData = PlantManager.Instance.GetItemById(currentItemData.seedName.ToLower());

            if (itemData != null)
            {
                InventoryManager.Instance.AddItem(itemData, 1);
                Farm.CoinManager.Instance.SpendCoins(int.Parse(currentItemData.seedPrice));
                InventoryManager.Instance.RefreshAllInventoryUI();

                Debug.Log($"✅ Bought seed: {itemData.itemId}");
            }
            else
            {
                Debug.LogError($"❌ Item with name '{titleText.text}' not found in ItemData list.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShopManager.Instance.SetInformation(currentItemData, iconData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}
