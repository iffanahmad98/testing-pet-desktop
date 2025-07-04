using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerConfig
{
    public int coins = 10000;
    public int poops = 0;

    public string lastLoginTimeString;
    public string totalPlayTimeString;

    [NonSerialized] public DateTime lastLoginTime;
    [NonSerialized] public TimeSpan totalPlayTime;

    public List<MonsterSaveData> monsters = new(); // Now using List for full JsonUtility support
    public List<OwnedItemData> ownedItems = new();
    public string activeBiomeID = "default_biome";
    public List<string> ownedBiomes = new();
    public bool isSkyEnabled = true;
    public bool isCloudEnabled = true;
    public bool isAmbientEnabled = true;



    // Serialization Sync
    public void SyncToSerializable()
    {
        lastLoginTimeString = lastLoginTime.ToString("o");
        totalPlayTimeString = totalPlayTime.ToString();
    }

    public void SyncFromSerializable()
    {
        DateTime.TryParse(lastLoginTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out lastLoginTime);
        TimeSpan.TryParse(totalPlayTimeString, out totalPlayTime);
    }

    // Inventory Logic
    public void AddItem(string itemID, ItemType type, int amount)
    {
        if (amount == 0 || string.IsNullOrEmpty(itemID)) return;

        var item = ownedItems.Find(i => i.itemID == itemID);
        if (item == null)
        {
            ownedItems.Add(new OwnedItemData { itemID = itemID, type = type, amount = Mathf.Max(0, amount) });
        }
        else
        {
            item.amount = Mathf.Max(0, item.amount + amount);
            if (item.amount == 0)
                ownedItems.Remove(item);
        }
    }

    public void RemoveItem(string itemID, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemID)) return;

        var existing = ownedItems.Find(i => i.itemID == itemID);
        if (existing != null)
        {
            existing.amount -= amount;
            if (existing.amount <= 0)
                ownedItems.Remove(existing);
        }
    }

    public int GetItemAmount(string itemID)
    {
        return ownedItems.Find(i => i.itemID == itemID)?.amount ?? 0;
    }

    // Monster Save Logic
    public void SaveMonsterData(MonsterSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.instanceId)) return;

        var existing = monsters.Find(m => m.instanceId == data.instanceId);
        if (existing != null)
        {
            int index = monsters.IndexOf(existing);
            monsters[index] = data;
        }
        else
        {
            monsters.Add(data);
        }
    }

    public bool LoadMonsterData(string instanceId, out MonsterSaveData data)
    {
        data = monsters.Find(m => m.instanceId == instanceId);
        return data != null;
    }


    public void DeleteMonster(string instanceId)
    {
        monsters.RemoveAll(m => m.instanceId == instanceId);
    }

    public List<string> GetAllMonsterIDs()
    {
        return monsters.Select(m => m.instanceId).ToList();
    }


    public void SetAllMonsterIDs(List<string> ids)
    {
        monsters = monsters.Where(m => ids.Contains(m.instanceId)).ToList();
    }

    public void ClearAllMonsterData()
    {
        monsters.Clear();
    }
    // Biome Logic
    public void AddOwnedBiome(string biomeID)
    {
        if (!ownedBiomes.Contains(biomeID))
            ownedBiomes.Add(biomeID);
    }

    public bool HasBiome(string biomeID)
    {
        return ownedBiomes.Contains(biomeID);
    }

    public void SetActiveBiome(string biomeID)
    {
        if (HasBiome(biomeID))
            activeBiomeID = biomeID;
    }

}


[Serializable]
public class OwnedItemData
{
    public string itemID;
    public ItemType type;
    public int amount;
}

