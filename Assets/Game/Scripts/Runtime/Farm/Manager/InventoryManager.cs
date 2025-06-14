using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MagicalGarden.Farm;
using MagicalGarden.Manager;

namespace MagicalGarden.Inventory
{

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;
        public GameObject dropFlyIcon;
        public List<InventoryItem> items = new List<InventoryItem>();
        public InventoryUI inventoryUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddItem(ItemData itemData, int amount)
        {
            if (itemData.itemType == ItemType.Crop)
            {
                PlantManager.Instance.AddAmountHarvest();
            }
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

        public bool HasItems(List<ItemStack> requiredItems)
        {
            foreach (var stack in requiredItems)
            {
                if (!HasItem(stack.item, stack.quantity))
                    return false;
            }
            return true;
        }

        public bool RemoveItems(List<ItemStack> requiredItems)
        {
            if (!HasItems(requiredItems))
                return false;

            foreach (var stack in requiredItems)
            {
                RemoveItem(stack.item, stack.quantity);
            }
            return true;
        }

        public InventoryItem GetItem(string itemId)
        {
            return items.FirstOrDefault(i => i.itemData.itemId == itemId);
        }

        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            return items.Where(i => i.itemData.itemType == type).ToList();
        }

        public void InventoryToogle()
        {
            inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        }
    }
    public enum ItemType
    {
        Seed,
        Crop,
        Tool,
        Fertilizer,
        MonsterSeed,
    }
    public enum ItemRarity
    {
        Normal,
        Rare,
        Epic,
        Legendary
    }
}