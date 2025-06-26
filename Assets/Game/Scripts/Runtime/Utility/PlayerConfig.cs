using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerConfig
{
    public int coins = 100;
    public int poop = 0;

    public string lastLoginTimeString;
    public string totalPlayTimeString;

    [NonSerialized] public DateTime lastLoginTime;
    [NonSerialized] public TimeSpan totalPlayTime;

    public List<string> monsterIDs = new List<string>();
    public Dictionary<string, MonsterSaveData> monsters = new Dictionary<string, MonsterSaveData>();
    public SettingsData settings = new SettingsData();
    public bool notificationsEnabled = true;

    // 🔹 New: List of owned items
    public List<OwnedItemData> ownedItems = new List<OwnedItemData>();

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

    public void AddItem(string itemID, int amount)
    {
        var item = ownedItems.Find(i => i.itemID == itemID);
        if (item == null)
        {
            ownedItems.Add(new OwnedItemData { itemID = itemID, amount = Mathf.Max(0, amount) });
        }
        else if (amount <= 0)
        {
            item.amount = Mathf.Max(0, item.amount + amount);
        }
        else
        {
            return; // No change if amount is zero or negative
        }
    }

    public void RemoveItem(string itemID, int amount)
    {
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
        var existing = ownedItems.Find(i => i.itemID == itemID);
        return existing?.amount ?? 0;
    }
}

[Serializable]
public class SettingsData
{
    public float gameAreaWidth = 1920f;
    public float gameAreaHeight = 1080f;
    public float gameAreaX = 0f;
    public float gameAreaY = 0f;
    public float uiScale = 1f;
    public int languageIndex = 0;
    public int screenState = 0;

    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}
[Serializable]
public class OwnedItemData
{
    public string itemID; // You can use the ItemDataSO.name or a unique ID
    public int amount;
}
