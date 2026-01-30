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

    public int hotelRoomCompleted =0;
    public int harvestFruit = 0;
    public int harvestEggMonsters = 0;

    public string lastLoginTimeString;
    public string totalPlayTimeString;

    [NonSerialized] public DateTime lastLoginTime;
    [NonSerialized] public TimeSpan totalPlayTime;

    public List<GameAreaData> gameAreas = new(); // List of game areas
    public List<MonsterSaveData> ownedMonsters = new(); // Now using List for full JsonUtility support
    public List<NPCSaveData> ownedNPCMonsters = new(); // For monsters that are owned but not in the world
    public List<OwnedItemData> ownedItems = new();
    public List<OwnedItemData> farmHarvestOwnedItems = new ();
    public List<string> ownedBiomes = new();
    public List<string> activeBiomesOnAreaID = new();
    public List<OwnedFacilityData> ownedFacilities = new();
    public List<OwnedDecorationData> ownedDecorations = new();
    public List<OwnedHotelFacilityData> ownedHotelFacilitiesData = new ();
    public List<HiredHotelFacilityData> hiredHotelFacilityData = new ();
    public List<HotelGiftWorldData> ownedHotelGiftWorldData = new ();
    public List<HiredFarmFacilityData> hiredFarmFacilitiesData = new ();
    public List<OwnedItemFarmData> ownedItemFarmDatas = new ();
    
    

    public string activeBiomeID = "default_biome";
    public bool isSkyEnabled = false;
    public bool isCloudEnabled = false;
    public bool isAmbientEnabled = false;
    public bool isRainEnabled = false;
    public bool isMondayReset = false;

    public List <int> listHotelGoldenTickets = new List <int> ();
    public List <int> listHotelNormalEggs = new List <int> ();
    public List <int> listHotelRareEggs = new List <int> ();
    public List <int> listIdHotelOpen = new List <int> (); // HotelLocker.cs
    public DateTime lastRefreshTimeHotelGoldenTickets;
    public DateTime lastRefreshTimeNormalEggs;
    public DateTime lastRefreshTimeRareEggs;
    public DateTime lastGatchaTimeReset;
    
    public DateTime lastRefreshGenerateGuest;
    public List <GuestRequestData> listGuestRequestData = new List <GuestRequestData> ();
    public DateTime lastRefreshTimeHotel;
    public List <HotelControllerData> listHotelControllerData = new List <HotelControllerData> ();
    public List <PetMonsterHotelData> listPetMonsterHotelData = new List <PetMonsterHotelData> ();

    public event System.Action <OwnedItemFarmData,int> eventItemFarmData;
    public event System.Action <OwnedItemFarmData,int> eventRemoveItemFarmData;

    public List<FertilizerMachineData> fertilizerMachineDatas = new ();
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
        PlayerHistoryManager.instance.GetLoadPlayerConfig (hotelRoomCompleted,harvestFruit,harvestEggMonsters);
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

    public void ClearItem (string itemID) // ItemInventoryUI.cs (Clear All Unused Data)
    {
        var existing = ownedItems.Find(i => i.itemID == itemID);
        if (existing != null)
        {
            ownedItems.Remove(existing);
        }
    }

    public int GetItemAmount(string itemID)
    {
        return ownedItems.Find(i => i.itemID == itemID)?.amount ?? 0;
    }

    #region FarmHarvestOwnedItems
     public void AddItemFarmHarvest(string itemID, ItemType type, int amount)
    {
        if (amount == 0 || string.IsNullOrEmpty(itemID)) return;

        var item = farmHarvestOwnedItems.Find(i => i.itemID == itemID);
        if (item == null)
        {
            farmHarvestOwnedItems.Add(new OwnedItemData { itemID = itemID, type = type, amount = Mathf.Max(0, amount) });
        }
        else
        {
            item.amount = Mathf.Max(0, item.amount + amount);
            if (item.amount == 0)
                farmHarvestOwnedItems.Remove(item);
        }
    }

    public void RemoveItemFarmHarvest(string itemID, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemID)) return;

        var existing = farmHarvestOwnedItems.Find(i => i.itemID == itemID);
        if (existing != null)
        {
            existing.amount -= amount;
            if (existing.amount <= 0)
                farmHarvestOwnedItems.Remove(existing);
        }
    }

    public void ClearItemFarmHarvest (string itemID) // ItemInventoryUI.cs (Clear All Unused Data)
    {
        var existing = farmHarvestOwnedItems.Find(i => i.itemID == itemID);
        if (existing != null)
        {
            farmHarvestOwnedItems.Remove(existing);
        }
    }

    public int GetItemFarmHarvestAmount(string itemID)
    {
        return farmHarvestOwnedItems.Find(i => i.itemID == itemID)?.amount ?? 0;
    }

    public List <OwnedItemData> GetFarmHarvestOwnedItems () {
        return farmHarvestOwnedItems;
    }
    #endregion
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

    public void SetActiveBiome(string biomeID, int gameAreaId)
    {
        // sets the currently active biome on an area

        if (gameAreaId < 0)
            throw new ArgumentOutOfRangeException(nameof(gameAreaId));

        // index valid -> replace
        if (gameAreaId < activeBiomesOnAreaID.Count)
        {
            activeBiomesOnAreaID[gameAreaId] = biomeID;
            return;
        }

        // index belum ada -> panjangkan list dulu sampai bisa menaruh di index tsb
        while (activeBiomesOnAreaID.Count < gameAreaId)
            activeBiomesOnAreaID.Add(string.Empty);

        // sekarang Add akan masuk tepat di index == gameAreaId
        activeBiomesOnAreaID.Add(biomeID);

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
    public int GetTotalOwnedDecorations () { // RewardAnimator.cs
        return ownedDecorations.Count;
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

    public void RemoveHotelFacilityData (string dataId) {
        OwnedHotelFacilityData target = GetHotelFacilityData (dataId);
        ownedHotelFacilitiesData.Remove(target);
    }

    public void ChangeHotelFacilityData (string dataId, bool isActive) {
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (data.id == dataId) {
                data.isActive = isActive;
                return;
            }
        }
    }

    public OwnedHotelFacilityData GetHotelFacilityData (string dataId) {
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (data.id == dataId) {
                return data;
            }
        }
        return null;
    }

    public void AddHiredHotelFacilityData (string dataId, int hiredValue)
    {
        if (GetHiredHotelFacilityData(dataId) == null) 
            hiredHotelFacilityData.Add(new HiredHotelFacilityData { id = dataId, isActive = true, hired = hiredValue});
        else
            GetHiredHotelFacilityData (dataId).hired += hiredValue;
    }

    public HiredHotelFacilityData GetHiredHotelFacilityData (string dataId) {
        foreach (HiredHotelFacilityData data in hiredHotelFacilityData) {
            if (data.id == dataId) return data;
        }
        return null;
    }

    public int GetTotalHiredServiceWithNpcServiceFeatures () { // HotelController.cs
        int result = 0;
        foreach (HiredHotelFacilityData data in hiredHotelFacilityData) {
            if (data.id == "robo_shroom" || data.id == "bellboy_shroom") { // memiliki fitur NpcService
                result += data.hired;
            }
        }

        return result;
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
    #region Farm Facility Logic
    public void AddHiredFarmFacilityData (string dataId, int hiredValue)
    {
        if (GetHiredFarmFacilityData(dataId) == null) 
            hiredFarmFacilitiesData.Add(new HiredFarmFacilityData { id = dataId, isActive = true, hired = hiredValue});
        else
            GetHiredFarmFacilityData (dataId).hired += hiredValue;
    }

    public HiredFarmFacilityData GetHiredFarmFacilityData (string dataId) {
        foreach (HiredFarmFacilityData data in hiredFarmFacilitiesData) {
            if (data.id == dataId) return data;
        }
        return null;
    }
    
    #endregion
    #region Item Farm Data Logic
    public void AddItemFarm(string itemID, int amount)
    {
        if (amount == 0 || string.IsNullOrEmpty(itemID)) return;

        var item = ownedItemFarmDatas.Find(i => i.itemID == itemID);
        if (item == null)
        {
            item = new OwnedItemFarmData { itemID = itemID, amount = Mathf.Max(0, amount) };
            ownedItemFarmDatas.Add(item);
        }
        else
        {
            item.amount = Mathf.Max(0, item.amount + amount);
            if (item.amount == 0)
                ownedItemFarmDatas.Remove(item);
        }
        Debug.Log ($"Item {itemID} {item.amount} bertambah {amount}");
        eventItemFarmData?.Invoke (item, amount);
    }

    public void RemoveItemFarm(string itemID, int amount, bool refreshEvent = false)
    { // EligibleMaterials.cs, PlayerInventory.cs
        if (amount <= 0 || string.IsNullOrEmpty(itemID)) return;
        Debug.Log ("berkurang : " + itemID);

        foreach (var s in this.ownedItemFarmDatas) {Debug.Log ("berkurang 2 :" + s.itemID);}
        var existing = ownedItemFarmDatas.Find(i => i.itemID == itemID);
        // Debug.Log ($"Item {itemID} {existing.amount} berkurang {amount}");
        if (existing != null)
        {
            existing.amount -= amount;
            if (existing.amount <= 0)
                ownedItemFarmDatas.Remove(existing);
        } else {
            Debug.LogError ("Not Found Owned Item Farm Data : " + itemID);
        }

        if (refreshEvent) {
            eventRemoveItemFarmData?.Invoke (existing, amount);
        }
    }

    public int GetItemFarmAmount(string itemID)
    {
        return ownedItemFarmDatas.Find(i => i.itemID == itemID)?.amount ?? 0;
    }

    public List <OwnedItemFarmData> GetOwnedItemFarmDatas () {
       // Debug.Log (ownedItemFarmDatas.Count);
        return ownedItemFarmDatas;
    }

    public void AddEventItemFarmData (System.Action <OwnedItemFarmData, int> actionValue) { // FarmShopPlantPanel
       // eventItemFarmData = null;
        eventItemFarmData += actionValue;
    }

    public void AddEventRemoveItemFarmData (System.Action <OwnedItemFarmData, int> actionValue) { // FarmShopPlantPanel
       // eventItemFarmData = null;
        eventRemoveItemFarmData += actionValue;
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
    #region Hotel
    public void SetLastRefreshTimeHotel (DateTime dateTime) { // HotelManager.cs
        lastRefreshTimeHotel = dateTime;
    }
    #endregion
    #region HotelControllerData
    public void AddHotelControllerData (HotelControllerData hotelControllerData) {
        listHotelControllerData.Add (hotelControllerData);
        SaveSystem.SaveAll();
    }

    public void RemoveHotelControllerData (HotelControllerData hotelControllerData) {
        listHotelControllerData.Remove (hotelControllerData);
        SaveSystem.SaveAll();
    }

    public List <HotelControllerData> GetListHotelControllerData () { // HotelManager.cs
        return listHotelControllerData; 
    }

    public void HotelControllerDataChangeCodeRequest (int idHotel, string codeRequest) { // HotelController.cs
        foreach (HotelControllerData data in listHotelControllerData) {
            if (data.idHotel == idHotel) {
                data.codeRequest = codeRequest;
                return;
            }
        }
    }

    public void HotelControllerDataChangeHappiness (int idHotel, int happiness) {
        HotelControllerData hotel = GetHotelControllerDataByIdHotel (idHotel);
        hotel.happiness = happiness;
    }

    public HotelControllerData GetHotelControllerDataByIdHotel (int idHotel) {
        foreach (HotelControllerData data in listHotelControllerData) {
            if (data.idHotel == idHotel) {
                return data;
            }
        }
        Debug.LogError ($"Id Hotel {idHotel} tidak ditemukan");
        return null;
    }

    public void SetHotelReward (int idHotel, bool holdReward) {
        HotelControllerData hotel = GetHotelControllerDataByIdHotel (idHotel);
        hotel.holdReward = holdReward;
    }
    #endregion
    #region PetMonsterHotelData
   public void SavePetMonsterHotelElement(PetMonsterHotelData data)
    { // untuk sistem yang bertabrakan dengan SaveAll ()
        // 1. Pastikan config sudah load
        var config = SaveSystem.PlayerConfig;

        if (config.listPetMonsterHotelData == null)
            config.listPetMonsterHotelData = new List<PetMonsterHotelData>();

        /*
        // 2. Cari element existing (KEY = idHotel)
        int index = config.listPetMonsterHotelData.FindIndex(x =>
            x.idHotel == data.idHotel &&
            x.guestStageGroupName == data.guestStageGroupName
        );
        */
        /*
        if (index >= 0)
        {
            // UPDATE ELEMENT
            config.listPetMonsterHotelData[index].guestStage = data.guestStage;
        }
        else
        {
            // ADD ELEMENT
            config.listPetMonsterHotelData.Add(data);
        }
        */
        config.listPetMonsterHotelData.Add(data);

        //Debug.Log("Pet element saved, total: " +
            //      config.listPetMonsterHotelData.Count);

        // 3. SAVE FILE (tetap full json, tapi aman)
        SaveSystem.SaveAll();
    }

    public List <PetMonsterHotelData> GetListPetMonsterHotelData () {
        return listPetMonsterHotelData;
    }

    public void RemovePetMonsterHotelData (PetMonsterHotelData data) {
        listPetMonsterHotelData.Remove (data);
       // SaveSystem.SaveAll ();
    }
    
    #endregion
  
    #region Fertilizer Machine Data
    public void AddFertilizerMachineData (MagicalGarden.Manager.FertilizerType fertilizerType, DateTime startDate) { // FertilizerManager.cs
        Debug.Log ("Save 1");
        if (GetFertilizerMachineData (fertilizerType) == null) {
            FertilizerMachineData newData = new FertilizerMachineData ();
            newData.id = fertilizerMachineDatas.Count;
            newData.fertilizerType = fertilizerType;
            newData.startDate = startDate;
            Debug.Log ("Save 2");
            fertilizerMachineDatas.Add (newData);
        }
    }

    public void RemoveFertilizerMachineData (MagicalGarden.Manager.FertilizerType type) {
        if (GetFertilizerMachineData (type) != null) {
            fertilizerMachineDatas.Remove (GetFertilizerMachineData (type));
        }
    }

    FertilizerMachineData GetFertilizerMachineData (MagicalGarden.Manager.FertilizerType type) {
        return fertilizerMachineDatas.Find(f => f.fertilizerType == type);
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
    public List<bool> areasIsActive;
}
[Serializable]
public class OwnedHotelFacilityData
{
    public string id;
    public bool isActive;
}
[Serializable]
public class HiredHotelFacilityData
{
    public string id;
    public bool isActive;
    public int hired;
}
[Serializable]
public class HiredFarmFacilityData
{
    public string id;
    public bool isActive;
    public int hired;
}
[Serializable]
public class OwnedItemFarmData
{
    public string itemID;
    // public ItemType type;
    public int amount;
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

[Serializable]
public class HotelControllerData
{
    public int idHotel = 0;
    public bool isDirty = false;
    public bool isOccupied = false;
    public string nameGuest = "";
    public string typeGuest = "";
    public int party = 0;
    public int price = 0;
    public TimeSpan stayDurationDays;
    public int happiness = 0;
    public DateTime checkInDate;
    public string rarity = "";
    public bool hasRequest = false;
    public string codeRequest = ""; // "RoomService", "Food", "Gift"
    public bool holdReward = false;
}

[Serializable]
public class PetMonsterHotelData
{
    public int idHotel = 0;
    public string guestStageGroupName = "";
    public int guestStage = 0;
}

[Serializable]
public class FertilizerMachineData
{
    public int id = 0;
    public MagicalGarden.Manager.FertilizerType fertilizerType;
    public DateTime startDate;
}




