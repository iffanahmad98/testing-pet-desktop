using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Farm;
using MagicalGarden.Manager;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace MagicalGarden.Inventory
{
    public class InventoryItemCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 10, 1);
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
                case ItemType.Tool:
                    InventoryManager.Instance.SetInformationItem(currentItem.itemData.description, currentItem.itemData.icon, ItemType.Fertilizer);
                    break;
                case ItemType.Crop:
                    TileManager.Instance.SetAction("None");
                    CursorIconManager.Instance.HideSeedIcon();
                    break;
                default:
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem == null || InventoryManager.Instance == null) return;
            UpdateInformationPanel(currentItem);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
        
        private void UpdateInformationPanel(InventoryItem item)
        {
            var inv = InventoryManager.Instance;
            var data = item.itemData;
            if (data == null) return;

            switch (data.itemType)
            {
                case ItemType.Seed:
                    inv.SetInformationItem(data.description, data.icon, ItemType.Seed);
                    // detail tambahan utk Seed
                    inv.SetDescAdditionalSeed(
                        data.needHourWatering.ToString(),
                        data.needHourGrow.ToString()
                    );
                    inv.ShowOnlySeed();
                    inv.SetDescAdditionalSeed(currentItem.itemData.needHourWatering.ToString(), currentItem.itemData.needHourGrow.ToString());
                    break;

                case ItemType.MonsterSeed:
                    inv.SetInformationItem(data.description, data.icon, ItemType.MonsterSeed);
                    inv.ShowOnlySeed();
                    break;

                case ItemType.Fertilizer:
                    inv.SetInformationItem(data.description, data.icon, ItemType.Fertilizer);
                    inv.SetDescAdditionalFertilizer(
                        data.timeUse.ToString(),
                        data.recipeCountPopNormal.ToString(),
                        data.recipeCountPopRare.ToString()
                    );
                    inv.ShowOnlyFertilizer();
                    inv.SetDescAdditionalFertilizer(currentItem.itemData.timeUse.ToString(), currentItem.itemData.recipeCountPopNormal.ToString(), currentItem.itemData.recipeCountPopRare.ToString());
                    break;

                case ItemType.Tool:
                    // mengikuti logika lama (kamu sebelumnya kirim tipe Fertilizer)
                    inv.SetInformationItem(data.description, data.icon, ItemType.Fertilizer);
                    break;

                case ItemType.Crop:
                    inv.SetInformationItem(data.description, data.icon, ItemType.Fertilizer);
                    inv.ShowOnlyCrop();
                    break;
            }
        }
    }
}