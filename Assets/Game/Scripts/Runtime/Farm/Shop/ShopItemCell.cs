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
    public class ShopItemCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IUIButtonSource
    {
        [Header("UI References")]
        public Image itemIcon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI priceText;
        public Button buyButton;
        private SheetData currentItemData;
        private string currentItemId;
        private Sprite iconData;

        [Header("Tutorial Integration")]
        public string tutorialSeedIdForBuyButton;

        [Header("Tooltip Panel")]
        public GameObject tooltipPanel;
        public TextMeshProUGUI tooltipText;
        public void Setup(SheetData shopItemCell, Sprite iconSprite)
        {
            currentItemData = shopItemCell;
            var itemData = PlantManager.Instance.GetItemById(shopItemCell.seedName.ToLower());
            if (itemData != null)
            {
                itemIcon.sprite = itemData.icon;
                iconData = itemData.iconCrop;
                currentItemId = itemData.itemId;
            }
            else
            {
                Debug.LogWarning($"❌ ItemData tidak ditemukan untuk ID: {shopItemCell.seedName}");
                itemIcon.sprite = null;
            }
            titleText.text = shopItemCell.seedName;
            priceText.text = shopItemCell.seedPrice;
            tooltipText.text = $"Harga: {shopItemCell.seedPrice}\n" +
                               $"Siram: {shopItemCell.wateringInterval}\n" +
                               $"Durasi: {shopItemCell.growDurationStages}\n" +
                               $"Total: {shopItemCell.totalGrowTime} jam\n" +
                               $"Layu: {shopItemCell.wiltTimeDays} jam\n" +
                               $"Mati: {shopItemCell.deadTimeDays} jam\n\n" +
                               $"{shopItemCell.description}";

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

        // IUIButtonSource: expose buyButton ke TutorialManager jika item ini yang diinginkan tutorial (misalnya stroberry)
        public void CollectButtons(System.Collections.Generic.List<Button> target)
        {
            if (target == null || buyButton == null)
                return;

            // Jika tidak dikonfigurasi untuk tutorial, abaikan.
            if (string.IsNullOrEmpty(tutorialSeedIdForBuyButton))
            {
                Debug.Log($"[ShopItemCell/Tutorial] Skip CollectButtons untuk '{name}' karena tutorialSeedIdForBuyButton kosong.");
                return;
            }

            // Gunakan itemId dari ItemData (hasil lookup PlantManager) sebagai kunci tutorial.
            var itemId = currentItemId;
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.Log($"[ShopItemCell/Tutorial] Skip CollectButtons untuk '{name}' karena currentItemId kosong. Setup sudah dipanggil? seedName='{currentItemData?.seedName}'");
                return;
            }

            if (!string.Equals(itemId, tutorialSeedIdForBuyButton, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[ShopItemCell/Tutorial] ItemId '{itemId}' TIDAK cocok dengan tutorialId '{tutorialSeedIdForBuyButton}' untuk '{name}'.");
                return;
            }

            if (!target.Contains(buyButton))
            {
                target.Add(buyButton);
                Debug.Log($"[ShopItemCell/Tutorial] BUY BUTTON terdaftar ke UI cache untuk '{name}' (itemId='{itemId}').");
            }
            else
            {
                Debug.Log($"[ShopItemCell/Tutorial] BUY BUTTON sudah ada di UI cache untuk '{name}' (itemId='{itemId}').");
            }
        }
    }
}
