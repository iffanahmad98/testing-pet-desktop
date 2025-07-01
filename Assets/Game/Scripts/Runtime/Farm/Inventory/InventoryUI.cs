using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Farm;

namespace MagicalGarden.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public GameObject slotPrefab;
        public GameObject dropFlyIcon;
        public Transform itemContainer;
        public GridContentResizer gridContentResizer;
        public List<ItemType> itemTypes;

        private List<InventorySlot> slotList = new List<InventorySlot>();

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (InventoryManager.Instance == null) return;
            gridContentResizer.Refresh(itemContainer.childCount);
            var allItems = InventoryManager.Instance.items;
            var items = allItems.FindAll(item => itemTypes.Contains(item.itemData.itemType));
            // Expand slot jika perlu
            while (slotList.Count < items.Count)
            {
                var slotGO = Instantiate(slotPrefab, itemContainer);
                var slot = slotGO.GetComponent<InventorySlot>();
                // slot.button.onClick.AddListener(slot.OnClick);
                slotList.Add(slot);
            }

            for (int i = 0; i < slotList.Count; i++)
            {
                if (i < items.Count)
                {
                    slotList[i].SetSlot(items[i]);
                }
                else
                {
                    slotList[i].ClearSlot();
                }
            }
        }

        public Transform GetSlotForItem(ItemData item)
        {
            foreach (var slot in slotList)
            {
                if (slot.HasItem(item))
                    return slot.transform;
            }
            return null;
        }
    }
}