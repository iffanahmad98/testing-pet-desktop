using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MagicalGarden.Inventory
{
    public enum ItemType
    {
        Seed,
        Crop,
        Tool,
        Fertilizer,
        MonsterSeed,
    }
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;
        public List<InventoryItem> items = new List<InventoryItem>();
        public InventoryUI inventoryUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddItem(ItemData itemData, int amount)
        {
            if (itemData.isStackable)
            {
                var existingItem = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
                if (existingItem != null)
                {
                    existingItem.quantity += amount;
                    return;
                }
            }

            items.Add(new InventoryItem(itemData, amount));
            inventoryUI.RefreshUI();
        }

        public bool RemoveItem(ItemData itemData, int amount)
        {
            var item = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
            if (item == null || item.quantity < amount) return false;

            item.quantity -= amount;
            if (item.quantity <= 0)
            {
                items.Remove(item);
            }
            inventoryUI.RefreshUI();
            return true;
        }

        public bool HasItem(ItemData itemData, int amount = 1)
        {
            var item = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
            return item != null && item.quantity >= amount;
        }

        public InventoryItem GetItem(string itemId)
        {
            return items.FirstOrDefault(i => i.itemData.itemId == itemId);
        }

        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            return items.Where(i => i.itemData.itemType == type).ToList();
        }
    }
}