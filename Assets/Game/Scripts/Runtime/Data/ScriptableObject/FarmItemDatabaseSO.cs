using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Inventory;

[CreateAssetMenu(fileName = "Farm Item Database", menuName = "Farm/Farm Item Database")]
public class FarmItemDatabaseSO : ScriptableObject
{
    public List<ItemData> allDatas = new List<ItemData>();
    public ItemData GetItemData(string idValue)
    {
        return allDatas.Find(data => data.itemId == idValue);
    }

    public ItemData GetItemData (int element) {
        return allDatas[element];
    }

    public List<ItemData> GetListItemData () {
        return allDatas;
    }
}
