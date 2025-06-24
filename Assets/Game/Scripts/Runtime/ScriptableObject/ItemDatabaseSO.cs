using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemDataSO> allItems;

    public ItemDataSO GetItem(string id)
    {
        return allItems.Find(item => item.itemID == id);
    }
}
