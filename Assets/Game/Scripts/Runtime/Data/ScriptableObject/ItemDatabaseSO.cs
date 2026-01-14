using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemDataSO> allItems;

    public ItemDataSO GetItem(string id)
    {
        return allItems.Find(item => item.itemID == id);
    }

    // ✅ NEW: Get all items by ItemType category (e.g. Food, Medicine, etc.)
    public List<ItemDataSO> GetItemsByCategory(ItemType type)
    {
        return allItems.Where(item => item.category == type).ToList();
    }

    // Optional: Get a single item by category (e.g. first food item)
    public ItemDataSO GetFirstItemByCategory(ItemType type)
    {
        return allItems.FirstOrDefault(item => item.category == type);
    }

    public List <ItemDataSO> GetAllItems () {
        return allItems;
    }
}
