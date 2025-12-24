using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerConfig
{
    public int coins = 10000;
    public int poops = 0;
    public int lastGameAreaIndex = 0; // Default to first game area
    public int maxGameArea = 1; // Tracks the highest game area index created
    public int goldenTicket = 0;
    public int normalEgg = 0;
    public int rareEgg = 0;
    public int hotelGift = 0;

    public string lastLoginTimeString;
    public string totalPlayTimeString;

    [NonSerialized] public DateTime lastLoginTime;
    [NonSerialized] public TimeSpan totalPlayTime;

    public List<GameAreaData> gameAreas = new(); // List of game areas
    public List<MonsterSaveData> ownedMonsters = new(); // Now using List for full JsonUtility support
    public List<NPCSaveData> ownedNPCMonsters = new(); // For monsters that are owned but not in the world
    public List<OwnedItemData> ownedItems = new();
    public List<string> ownedBiomes = new();
    public List<OwnedFacilityData> ownedFacilities = new();
    public List<OwnedDecorationData> ownedDecorations = new();
    public List<OwnedHotelFacilityData> ownedHotelFacilitiesData = new ();
    public List<HotelGiftWorldData> ownedHotelGiftWorldData = new ();

    public string activeBiomeID = "default_biome";
    public bool isSkyEnabled = false;
    public bool isCloudEnabled = false;
    public bool isAmbientEnabled = false;
    public bool isRainEnabled = false;

    public List <int> listHotelGoldenTickets = new List <int> ();
    public List <int> listHotelNormalEggs = new List <int> ();
    public List <int> listHotelRareEggs = new List <int> ();
    public DateTime lastRefreshTimeHotelGoldenTickets;
    public DateTime lastRefreshTimeNormalEggs;
    public DateTime lastRefreshTimeRareEggs;
    
    public DateTime lastRefreshGenerateGuest;
    public List <GuestRequestData> listGuestRequestData = new List <GuestRequestData> ();
    
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

    public void SyncLootUseable () 
    {
        GoldenTicket.instance.LoadLoot (goldenTicket);
        NormalEgg.instance.LoadLoot (normalEgg);
        RareEgg.instance.LoadLoot (rareEgg);
        HotelGift.instance.LoadLoot (hotelGift);
    }

    public void SyncGuestRequestData () {
        Debug.Log ("Sync Guest Request Data, total ada : " + listGuestRequestData.Count);
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

        var existing = ownedMonsters.Find(m => m.instanceId == data.instanceId);
        if (existing != null)
        {
            int index = ownedMonsters.IndexOf(existing);
            ownedMonsters[index] = data;
        }
        else
        {
            ownedMonsters.Add(data);
        }
    }

    public bool LoadMonsterData(string instanceId, out MonsterSaveData data)
    {
        data = ownedMonsters.Find(m => m.instanceId == instanceId);
        return data != null;
    }

    public void DeleteMonster(string instanceId)
    {
        ownedMonsters.RemoveAll(m => m.instanceId == instanceId);
    }

    public List<string> GetAllMonsterIDs()
    {
        return ownedMonsters.Select(m => m.instanceId).ToList();
    }

    public void SetAllMonsterIDs(List<string> ids)
    {
        ownedMonsters = ownedMonsters.Where(m => ids.Contains(m.instanceId)).ToList();
    }

    public void ClearAllMonsterData()
    {
        ownedMonsters.Clear();
    }

    public void SaveNPCMonsterData(NPCSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.monsterId)) return;

        var existing = ownedNPCMonsters.Find(m => m.monsterId == data.monsterId);
        if (existing != null)
        {
            int index = ownedNPCMonsters.IndexOf(existing);
            ownedNPCMonsters[index] = data;
        }
        else
        {
            ownedNPCMonsters.Add(data);
        }
    }

    public bool LoadNPCMonsterData(string monsterId, out NPCSaveData data)
    {
        data = ownedNPCMonsters.Find(m => m.monsterId == monsterId);
        return data != null;
    }

    public void DeleteNPCMonster(string monsterId)
    {
        ownedNPCMonsters.RemoveAll(m => m.monsterId == monsterId);
    }

    public List<string> GetAllNPCMonsterIDs()
    {
        return ownedNPCMonsters.Select(m => m.monsterId).ToList();
    }

    public void SetAllNPCMonsterIDs(List<string> ids)
    {
        ownedNPCMonsters = ownedNPCMonsters.Where(m => ids.Contains(m.monsterId)).ToList();
    }

    public void ClearAllNPCMonsterData()
    {
        ownedNPCMonsters.Clear();
    }

    // Biome Logic
    public void AddOwnedBiome(string biomeID)
    {
        if (!ownedBiomes.Contains(biomeID))
            ownedBiomes.Add(biomeID);
    }

    public bool HasBiome(string biomeID)
    {
        // If empty, consider it always valid for default biome logic
        if (string.IsNullOrEmpty(biomeID))
            return true;

        return ownedBiomes.Contains(biomeID);
    }

    public void SetActiveBiome(string biomeID)
    {
        // Allow clearing the active biome with an empty string
        if (string.IsNullOrEmpty(biomeID) || HasBiome(biomeID))
        {
            activeBiomeID = biomeID;
        }
    }

    // Game Area specific monster operations
    public List<MonsterSaveData> GetMonstersForGameArea(int gameAreaIndex)
    {
        return ownedMonsters.Where(m => m.gameAreaId == gameAreaIndex).ToList();
    }

    public void SetMonsterGameArea(string instanceId, int gameAreaIndex)
    {
        var monster = ownedMonsters.Find(m => m.instanceId == instanceId);
        if (monster != null)
        {
            monster.gameAreaId = gameAreaIndex;
        }
    }

    public void MoveMonsterToGameArea(string instanceId, int fromArea, int toArea)
    {
        var monster = ownedMonsters.Find(m => m.instanceId == instanceId && m.gameAreaId == fromArea);
        if (monster != null)
        {
            monster.gameAreaId = toArea;
        }
    }

    public int GetMonsterCountForGameArea(int gameAreaIndex)
    {
        return ownedMonsters.Count(m => m.gameAreaId == gameAreaIndex);
    }

    public void DeleteMonstersFromGameArea(int gameAreaIndex)
    {
        ownedMonsters.RemoveAll(m => m.gameAreaId == gameAreaIndex);
    }
    // Facility Logic
    public bool HasFacility(string id) =>
       ownedFacilities.Any(f => f.facilityID == id);

    public bool CanUseFacility(string id)
    {
        var facility = ownedFacilities.Find(f => f.facilityID == id);
        return facility == null || Time.time >= facility.nextUsableTime;
    }

    public void SetFacilityCooldown(string id, float cooldown)
    {
        var facility = ownedFacilities.Find(f => f.facilityID == id);
        if (facility != null)
            facility.nextUsableTime = Time.time + cooldown;
    }

    public void AddFacility(string id, float cooldown = 0f)
    {
        if (!HasFacility(id))
            ownedFacilities.Add(new OwnedFacilityData(id, Time.time + cooldown));
    }
    public bool HasNPC(string npcId)
    {
        return ownedNPCMonsters.Any(n => n.monsterId == npcId);
    }
    #region Decoration Logic
    public bool HasDecoration(string decorationID)
    {
        return ownedDecorations.Any(d => d.decorationID == decorationID);
    }
    public void AddDecoration(string decorationID, bool isActive = false)
    {
        if (!HasDecoration(decorationID))
            ownedDecorations.Add(new OwnedDecorationData { decorationID = decorationID, isActive = isActive });
    }
    #endregion
    #region Hotel Facility Logic
    public bool HasHotelFacility(string id)
    {
        return ownedHotelFacilitiesData.Any(d => d.id == id);
    }
    
    public bool HasHotelFacilityAndIsActive(string id)
    {
        return ownedHotelFacilitiesData.Any(d => d.id == id && d.isActive == true);
    }

    // HotelFacilitesMenu
    public void AddHotelFacilityData (string dataId)
    {
        if (!HasHotelFacility(dataId))
            ownedHotelFacilitiesData.Add(new OwnedHotelFacilityData { id = dataId, isActive = true});
    }

    public void ChangeHotelFacilityData (string dataId, bool isActive) {
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (data.id == dataId) {
                data.isActive = isActive;
                return;
            }
        }
    }
    #endregion
    #region Hotel Gift World
    public void AddHotelGiftWorld (Vector3 position) {// HotelGiftHandler
        ownedHotelGiftWorldData.Add(new HotelGiftWorldData { dataPosition = position});
        Debug.Log ("Melakukan Save Hotel Gift");
        SaveSystem.SaveAll ();
    }

    public void RemoveHotelGiftWorld (Vector3 position) { // HotelGiftHandler
        ownedHotelGiftWorldData.Remove(GetHotelGiftWorldData (position));
        Debug.Log ("Melakukan Save Hotel Gift");
        SaveSystem.SaveAll ();
    }

    HotelGiftWorldData GetHotelGiftWorldData (Vector3 targetPosition) {
        foreach (HotelGiftWorldData data in ownedHotelGiftWorldData) {
            if (Vector3.Distance (targetPosition, data.dataPosition) <0.5f) {
                return data;
            }
        }

        Debug.LogError ("There is no HotelGiftWorldData similar with this position !");
        return null;
    }
    #endregion
    #region Guest Request Data
    public void AddGuestRequestData (GuestRequestData guestRequestData) { // HotelManager.cs
        
        listGuestRequestData.Add (guestRequestData);
    }

    public void RemoveGuestRequestData (GuestRequestData guestRequestData) { // HotelManager.cs
        listGuestRequestData.Remove (guestRequestData);
    }

    public void ClearAllGuestRequestData () {
        listGuestRequestData.Clear ();   
    }

    public void SetLastRefreshGenerateGuest (DateTime dateTime) { // HotelManager.cs
        lastRefreshGenerateGuest = dateTime;
    }

    public List <GuestRequestData> GetListGuestRequestData () { // HotelManager.cs
        return listGuestRequestData;
    }
    
    #endregion
}

[Serializable]
public class OwnedItemData
{
    public string itemID;
    public ItemType type;
    public int amount;
}

[Serializable]
public class NPCSaveData
{
    public string monsterId;
    public int isActive; // 0 = inactive, 1 = active
}

[Serializable]
public class GameAreaData
{
    public string name;
    public int index;
    public List<string> monsterIDs = new List<string>();
    public List<string> npcMonsterIDs = new List<string>();
}

[Serializable]
public class MonsterCollectionData
{
    public bool isUnlocked;
    public string monsterId;
    public string monsterName;
    public string monsterCount;
    public int evolutionStage;
}
[Serializable]
public class OwnedFacilityData
{
    public string facilityID;
    public float nextUsableTime; // Unix timestamp or game time

    public OwnedFacilityData(string id, float cooldownTime)
    {
        facilityID = id;
        nextUsableTime = cooldownTime;
    }
}
[Serializable]
public class OwnedDecorationData
{
    public string decorationID;
    public bool isActive;
}
[Serializable]
public class OwnedHotelFacilityData
{
    public string id;
    public bool isActive;
}
[Serializable]
public class HotelGiftWorldData
{
    public Vector3 dataPosition;
}

[Serializable]
public class GuestRequestData
{
    public string type = "";
    public int party = 0;
    public int price = 0;
    public TimeSpan stayDuration;
    public string guestName; 
}


