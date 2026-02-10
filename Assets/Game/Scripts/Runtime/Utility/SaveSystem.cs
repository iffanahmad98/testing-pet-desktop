using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.STP;


public static class SaveSystem
{
    private const string SaveFileName = "playerConfig.json";
    private const string UiDataSaveFileName = "uiConfig.json";

    private static PlayerConfig _playerConfig;
    private static UISaveData _uiSaveData;
    private static DateTime _sessionStartTime;
    public static PlayerConfig PlayerConfig => _playerConfig;
    public static UISaveData UiSaveData => _uiSaveData;
    public static void SavePoop(int poop) => _playerConfig.poops = poop; // Directly save to PlayerConfig (Not Used)
    public static int LoadPoop() => _playerConfig.poops;
    public static event Action <PlayerConfig> DataLoaded; // DecorationManager

    public static bool IsLoadFinished = false;
    

    public static void Initialize()
    {
        LoadPlayerConfig();
        LoadUiConfig();
        _sessionStartTime = DateTime.Now;

        Debug.Log($"Last login time: {_playerConfig.lastLoginTime}");
        // Handle first-time login
        if (_playerConfig.lastLoginTime == default)
        {
            _playerConfig.lastLoginTime = DateTime.Now;
        }

        // Check for time cheating
        CheckTimeDiscrepancy();

        IsLoadFinished = true;

        Debug.Log("IsFinished Load Save Data");
    }

    // Save all data when application pauses/quits
    public static void SaveAll()
    {
        UpdatePlayTime();
        SavePlayerConfig();
        SaveUiConfig();
    }

    public static void SaveMon(MonsterSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.monsterId))
        {
            Debug.LogWarning("Tried to save null or invalid monster data.");
            return;
        }
        _playerConfig.SaveMonsterData(data);
        SaveAll(); // Make sure to save the whole config after updating
    }

    public static bool LoadMon(string monsterId, out MonsterSaveData data)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            data = null;
            return false;
        }

        return _playerConfig.LoadMonsterData(monsterId, out data);
    }

    public static void DeleteMon(string instanceID)
    {
        if (string.IsNullOrEmpty(instanceID)) return;

        _playerConfig.DeleteMonster(instanceID);
        SaveAll(); // Ensure the config is persisted
    }

    public static void SaveMonIDs(List<string> ids)
    {
        _playerConfig.SetAllMonsterIDs(ids);
        SavePlayerConfig();
    }

    public static List<string> LoadMonIDs()
    {
        return _playerConfig.GetAllMonsterIDs();
    }
    public static void SaveNPCMon(NPCSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.monsterId))
        {
            Debug.LogWarning("Tried to save null or invalid NPC monster data.");
            return;
        }
        _playerConfig.SaveNPCMonsterData(data);
        SaveAll(); // Save after update
    }

    public static bool LoadNPCMon(string instanceId, out NPCSaveData data)
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            data = null;
            return false;
        }

        return _playerConfig.LoadNPCMonsterData(instanceId, out data);
    }

    public static void DeleteNPCMon(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return;

        _playerConfig.DeleteNPCMonster(instanceId);
        SaveAll(); // Save after deletion
    }

    public static void SaveNPCMonIDs(List<string> ids)
    {
        _playerConfig.SetAllNPCMonsterIDs(ids);
        SavePlayerConfig();
    }

    public static List<string> LoadNPCMonIDs()
    {
        return _playerConfig.GetAllNPCMonsterIDs();
    }


    public static void Flush() => PlayerPrefs.Save();

    public static void ResetSaveData()
    {
        CoinManager.Coins = 10000;
        SavePoop(0);
        _playerConfig.ClearAllMonsterData();
        _playerConfig.ClearAllNPCMonsterData();
        _playerConfig.ownedItems.Clear();
        _playerConfig.ownedBiomes.Clear();
        _playerConfig.activeBiomeID = "default_biome";
        SavePlayerConfig();
    }


    #region Time Tracking
    public static DateTime GetLastLoginTime()
    {
        return _playerConfig.lastLoginTime;
    }

    public static TimeSpan GetTotalPlayTime()
    {
        return _playerConfig.totalPlayTime;
    }

    public static TimeSpan GetCurrentSessionPlayTime()
    {
        return DateTime.Now - _sessionStartTime;
    }

    public static void SetMondayReset()
    {
        _playerConfig.isMondayReset = true;

        SaveAll();
    }

    public static void SetLastGatchaTimeReset(DateTime time)
    {
        _playerConfig.lastGatchaTimeReset = time;
    }

    public static DateTime GetLastGachaTimeReset()
    {
        return _playerConfig.lastGatchaTimeReset;
    }

    private static void UpdatePlayTime()
    {
        _playerConfig.totalPlayTime += GetCurrentSessionPlayTime();
        _playerConfig.lastLoginTime = DateTime.Now;
    }

    private static void CheckTimeDiscrepancy()
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan timeSinceLastLogin = currentTime - _playerConfig.lastLoginTime;

        // Detect if system time was set backwards
        if (timeSinceLastLogin.TotalSeconds < 0)
        {
            Debug.LogWarning("System time was set backwards! Possible cheating attempt.");
            // Handle time cheating (e.g., penalize player or reset progress)
            HandleTimeCheating();
        }

        // Detect if player was away for too long
        if (timeSinceLastLogin.TotalHours > 24)
        {
            Debug.Log($"Player was away for {timeSinceLastLogin.TotalHours} hours");
            // Handle long absence (e.g., update monster states)
            // HandleLongAbsence(timeSinceLastLogin);
        }
    }

    private static void HandleTimeCheating()
    {
        // Implement your anti-cheat measures here
        // For example: reduce coins, reset progress, or show warning
        _playerConfig.coins = Mathf.Max(0, _playerConfig.coins - 100);
    }

    private static void HandleLongAbsence(TimeSpan timeAway)
    {
        float hoursAway = (float)timeAway.TotalHours;

        foreach (var monster in _playerConfig.ownedMonsters)
        {
            // Reduce happiness based on time away (example: -2% per hour)
            monster.currentHappiness -= hoursAway * 2f;
            monster.currentHappiness = Mathf.Clamp(monster.currentHappiness, 0f, 100f);

            // Reduce hunger over time (example: 5 units/hour)
            monster.currentHunger -= hoursAway * 5f;
            monster.currentHunger = Mathf.Clamp(monster.currentHunger, 0f, 100f);

            // Optional: Reduce health if hunger drops too low
            if (monster.currentHunger <= 40f)
            {
                monster.currentHealth -= hoursAway * 2f;
                monster.currentHealth = Mathf.Clamp(monster.currentHealth, 0f, 100f);
            }
        }
    }

    #endregion

    #region File Operations
    private static void LoadPlayerConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (!File.Exists(path))
        {
            CreateNewPlayerConfig();
            return;
        }

        string raw = File.ReadAllText(path);

        // detect plaintext atau encrypted (BERLAKU DI EDITOR DAN BUILD)
        string trimmed = raw.TrimStart();
        string json;

        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            json = raw; // plaintext
        else
            json = CryptoHelper.Decrypt(raw); // encrypted

        _playerConfig = JsonConvert.DeserializeObject<PlayerConfig>(json, _jsonSettings);

        if (_playerConfig == null)
        {
            CreateNewPlayerConfig();
            return;
        }

        _playerConfig.SyncFromSerializable();
        _playerConfig.SyncLootUseable();
        _playerConfig.SyncGuestRequestData();

        DataLoaded?.Invoke(_playerConfig);

        Debug.Log("Game data loaded successfully");
    }


    /*
    private static void LoadUiConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, UiDataSaveFileName);

        if (File.Exists(path))
        {
            try
            {
              //  string json = File.ReadAllText(path);
              string json = "";
                 #if UNITY_EDITOR
                json = File.ReadAllText(path);
                #else
                json = CryptoHelper.Decrypt(File.ReadAllText(path));
                #endif
                   
                _uiSaveData = JsonConvert.DeserializeObject<UISaveData>(json, _jsonSettings);

            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load UI Config: " + e.Message);
                _uiSaveData = new();
            }
        }
        else
        {
            Debug.LogError("Path not exist. Create new Ui Save Data");
            _uiSaveData = new();
        }
    }
    */
   private static void LoadUiConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, UiDataSaveFileName);

        if (!File.Exists(path))
        {
            Debug.Log("UI save not found. Create new.");
            _uiSaveData = new UISaveData();
            return;
        }

        string raw = File.ReadAllText(path);

        // detect plaintext / encrypted (BERLAKU DI EDITOR DAN BUILD)
        string trimmed = raw.TrimStart();
        string json;

        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            json = raw; // plaintext
        else
            json = CryptoHelper.Decrypt(raw); // encrypted

        _uiSaveData = JsonConvert.DeserializeObject<UISaveData>(json, _jsonSettings);

        if (_uiSaveData == null)
        {
            Debug.LogWarning("UI save invalid. Create new.");
            _uiSaveData = new UISaveData();
        }
    }

    private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented, // datetime
        Converters = { new Vector3Converter(), new TimeSpanConverter() } // vector3, TimeSpan
    };

    private static void SavePlayerConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        // Debug.Log("Before sync: " + PlayerConfig.listPetMonsterHotelData.Count);
        try
        {
            _playerConfig.SyncToSerializable(); // Convert DateTime to strings, etc.

            string json = JsonConvert.SerializeObject(_playerConfig, _jsonSettings);

            // File.WriteAllText(path, json);
            #if UNITY_EDITOR
            File.WriteAllText(path, json);
            #else
            File.WriteAllText(path, CryptoHelper.Encrypt(json));
            #endif
            
            Debug.Log("Game data saved successfully");
            // Debug.Log("After sync: " + PlayerConfig.listPetMonsterHotelData.Count);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game data: " + e.Message);
        }
    }

    private static void SaveUiConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, UiDataSaveFileName);
        // Debug.Log("Before sync: " + PlayerConfig.listPetMonsterHotelData.Count);
        try
        {
            string json = JsonConvert.SerializeObject(_uiSaveData, _jsonSettings);

            // File.WriteAllText(path, json);
            #if UNITY_EDITOR
            File.WriteAllText(path, json);
            #else
            File.WriteAllText(path, CryptoHelper.Encrypt(json));
            #endif

            Debug.Log("Ui Save Data is saved successfully");
            // Debug.Log("After sync: " + PlayerConfig.listPetMonsterHotelData.Count);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save ui save data: " + e.Message);
        }
    }


    private static void CreateNewPlayerConfig()
    {
        _playerConfig = new PlayerConfig();
        _playerConfig.lastLoginTime = DateTime.Now;
        _playerConfig.gameAreas.Add(new GameAreaData
        {
            name = $"Game Area 1",
            index = 0 // Index is zero-based
        });
        _playerConfig.SyncToSerializable();
        Debug.Log("Created new game data");
    }

    public static void DeleteAllSaveData()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        _playerConfig = new PlayerConfig();
        _playerConfig.lastLoginTime = DateTime.Now;
        Debug.Log("All game data deleted");
    }
    public static void UpdateItemData(string itemID, ItemType category, int amount)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot update item data.");
            return;
        }

        _playerConfig.AddItem(itemID, category, amount);
        SaveAll();
    }
    #endregion

    #region Item Data Operation
    public static bool TryBuyItem(ItemDataSO itemData)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot buy item.");
            return false;
        }

        if (itemData == null)
        {
            Debug.LogWarning("ItemData is null.");
            return false;
        }

        int itemPrice = itemData.price;

        // Deduct coins via CoinManager (handles check, update, save, event)
        if (!CoinManager.SpendCoins(itemPrice))
        {
            Debug.Log($"Not enough coins to buy {itemData.itemName}. Needed: {itemPrice}, Owned: {CoinManager.Coins}");
            return false;
        }

        // Add item to inventory
        _playerConfig.AddItem(itemData.itemID, itemData.category, 1);

        // Save changes (item update only, coins already saved by CoinManager)
        SaveAll();

        Debug.Log($"Purchased {itemData.itemName} and {itemData.category} for {itemPrice} coins. Remaining: {CoinManager.Coins}");

        return true;
    }
    #endregion
    #region Monster Data Operations
    public static bool TryBuyMonster(MonsterDataSO monsterData)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot buy monster.");
            return false;
        }

        if (monsterData == null)
        {
            Debug.LogWarning("MonsterData is null.");
            return false;
        }

        int monsterPrice = monsterData.monsterPrice;

        // Deduct coins via CoinManager (handles check, update, save, event)
        if (!CoinManager.SpendCoins(monsterPrice))
        {
            Debug.Log($"Not enough coins to buy {monsterData.name}. Needed: {monsterPrice}, Owned: {CoinManager.Coins}");
            return false;
        }

        Debug.Log($"In area {_playerConfig.lastGameAreaIndex} we have {MonsterManager.instance.activeMonsters.Count} monsters");
        if (MonsterManager.instance.activeMonsters.Count >= 25)
        {
            Debug.Log("We have reached a limit of 25 monsters");
            TooltipManager.Instance.StartHoverForDuration("You already have maximum number of monsters in this area.", 4.0f);
            
            return false;
        }

        // Save changes (monster update only, coins already saved by CoinManager)
        SaveAll();

        Debug.Log($"Bought {monsterData.name} for {monsterPrice} coins. Remaining: {CoinManager.Coins}");

        return true;
    }
    #endregion
    #region Biome Operations

    public static bool TryBuyBiome(string biomeID, int price)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot buy biome.");
            return false;
        }

        if (string.IsNullOrEmpty(biomeID))
        {
            Debug.LogWarning("Invalid biome ID.");
            return false;
        }

        if (_playerConfig.HasBiome(biomeID))
        {
            Debug.Log($"Biome {biomeID} already owned.");
            return true;
        }

        if (!CoinManager.SpendCoins(price))
        {
            Debug.Log($"Not enough coins to buy biome {biomeID}. Needed: {price}, Owned: {CoinManager.Coins}");
            return false;
        }

        _playerConfig.AddOwnedBiome(biomeID);
        SaveAll();
        Debug.Log($"Bought biome {biomeID} for {price} coins. Remaining: {CoinManager.Coins}");
        return true;
    }

    public static void AddOwnedBiome(string biomeID)
    {
        if (string.IsNullOrEmpty(biomeID)) return;
        _playerConfig.AddOwnedBiome(biomeID);
        SaveAll();
    }

    public static bool IsBiomeOwned(string biomeID)
    {
        return _playerConfig.HasBiome(biomeID);
    }

    public static string GetActiveBiome()
    {
        return _playerConfig.activeBiomeID;
    }

    public static void SetActiveBiome(string biomeID)
    {
        // If blank or null, clear the active biome
        if (string.IsNullOrEmpty(biomeID))
        {
            _playerConfig.SetActiveBiome("");
            SaveAll();
            return;
        }

        // Otherwise, validate ownership before setting
        if (_playerConfig.HasBiome(biomeID))
        {
            _playerConfig.SetActiveBiome(biomeID);
            SaveAll();
        }
        else
        {
            Debug.LogWarning($"Attempted to set biome '{biomeID}' as active but it's not owned.");
        }
    }

    public static void SetSkyEnabled(bool enabled)
    {
        _playerConfig.isSkyEnabled = enabled;
        SaveAll();
    }

    public static bool IsSkyEnabled()
    {
        return _playerConfig.isSkyEnabled;
    }

    public static void SetCloudEnabled(bool enabled)
    {
        _playerConfig.isCloudEnabled = enabled;
        SaveAll();
    }

    public static bool IsCloudEnabled()
    {
        return _playerConfig.isCloudEnabled;
    }

    public static void SetAmbientEnabled(bool enabled)
    {
        _playerConfig.isAmbientEnabled = enabled;
        SaveAll();
    }

    public static bool IsAmbientEnabled()
    {
        return _playerConfig.isAmbientEnabled;
    }
    #endregion
    #region NPC Operations
    public static bool IsNPCOwned(string npcID)
    {
        return GetPlayerConfig().ownedNPCMonsters.Any(n => n.monsterId == npcID);
    }
    public static bool HasNPC(string npcID)
    {
        if (string.IsNullOrEmpty(npcID))
            return false;

        return _playerConfig.ownedNPCMonsters.Any(npc => npc.monsterId == npcID);
    }

    public static void AddNPC(string npcID)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning("Tried to add null or empty NPC ID.");
            return;
        }

        if (HasNPC(npcID))
        {
            Debug.LogWarning($"NPC '{npcID}' already owned.");
            return;
        }

        // Create a new entry and add it to the list
        var npcData = new NPCSaveData
        {
            monsterId = npcID,
            isActive = 1 // Set active by default
        };

        _playerConfig.ownedNPCMonsters.Add(npcData);
        SaveAll();
        Debug.Log($"Added NPC '{npcID}' to owned NPCs.");
    }
    public static void ToggleNPCActiveState(string monsterId, bool isActive)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            Debug.LogWarning("Tried to toggle active state for null or empty instance ID.");
            return;
        }

        var npc = _playerConfig.ownedNPCMonsters.FirstOrDefault(n => n.monsterId == monsterId);
        if (npc == null)
        {
            Debug.LogWarning($"NPC with monster ID '{monsterId}' not found.");
            return;
        }

        npc.isActive = isActive ? 1 : 0; // 1 for active, 0 for inactive
        SaveAll();
        Debug.Log($"Set NPC '{npc.monsterId}' active state to {isActive}.");
    }
    public static bool IsNPCActive(string npcID)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning("Tried to check active state for null or empty NPC ID.");
            return false;
        }

        var npc = _playerConfig.ownedNPCMonsters.FirstOrDefault(n => n.monsterId == npcID);
        Debug.Log($"Checking if NPC '{npcID}' is active: {npc != null}");
        return npc != null && npc.isActive == 1; // 1 means active
    }

    #endregion
    #region  Facility Operations
    public static bool IsFacilityOwned(string facilityID)
    {
        //bool hasFacility = GetPlayerConfig().ownedFacilities.Any(f => f.facilityID == facilityID);
        var facilityToCheck = GetPlayerConfig().ownedFacilities.FirstOrDefault(f => f.facilityID == facilityID);

        if (facilityToCheck != null)
        {
            var areaIndex = facilityToCheck.areasOwnFacility.IndexOf(GetPlayerConfig().lastGameAreaIndex);
            return areaIndex > -1;
        }

        return GetPlayerConfig().ownedFacilities.Any(f => f.facilityID == facilityID);
    }



    public static bool TryPurchaseFacility(FacilityDataSO facility)
    {
        var config = GetPlayerConfig();

        if (config.ownedFacilities.Any(f => f.facilityID == facility.facilityID))
            return true; // Already owned

        // Use CoinManager for consistency
        if (!CoinManager.SpendCoins(facility.price))
        {
            Debug.Log($"Not enough coins to buy {facility.name}. Needed: {facility.price}, Owned: {CoinManager.Coins}");
            return false;
        }

        // Create OwnedFacilityData object using constructor
        var ownedFacility = new OwnedFacilityData(facility.facilityID, facility.cooldownSeconds);
        ownedFacility.AddAreaOwnership(config.lastGameAreaIndex);
        config.ownedFacilities.Add(ownedFacility);

        SaveAll();
        Debug.Log($"Bought facility {facility.name} for {facility.price} coins. Remaining: {CoinManager.Coins}");
        return true;
    }

    public static void MarkFacilityOwned(string facilityID)
    {
        var config = GetPlayerConfig();

        for (int i = 0; i < config.ownedFacilities.Count; i++) 
        {
            int temp = i;

            if (config.ownedFacilities[temp].facilityID == facilityID)
            {
                config.ownedFacilities[temp].AddAreaOwnership(config.lastGameAreaIndex);
                return;
            }
        }

        // Add to owned facilities with 0 cooldown (for free toggle facilities)
        var ownedFacility = new OwnedFacilityData(facilityID, 0f);
        ownedFacility.AddAreaOwnership(config.lastGameAreaIndex);
        config.ownedFacilities.Add(ownedFacility);

        Debug.Log($"Marked facility {facilityID} as owned (active)");
    }

    public static void RemoveFacilityOwnership(string facilityID)
    {
        var config = GetPlayerConfig();

        // Remove from owned facilities
        var facilityToRemove = config.ownedFacilities.FirstOrDefault(f => f.facilityID == facilityID);
        if (facilityToRemove != null)
        {
            // config.ownedFacilities.Remove(facilityToRemove);
            var areaIndex = facilityToRemove.areasOwnFacility.IndexOf(config.lastGameAreaIndex);
            facilityToRemove.areasOwnFacility.RemoveAt(areaIndex);
            Debug.Log($"Removed facility {facilityID} ownership (inactive)");
        }
    }
    #endregion
    #region Decoration Operations
    public static bool IsDecorationOwned(string decorationID)
    {
        return _playerConfig.ownedDecorations.Any(d => d.decorationID == decorationID);
    }
    /*
    public static string GetActiveDecoration()
    {
        var activeDecoration = _playerConfig.ownedDecorations.FirstOrDefault(d => d.isActive);
        foreach (var s in  _playerConfig.ownedDecorations) {Debug.Log (s.decorationID);}
        return activeDecoration?.decorationID ?? string.Empty;
    }
    */
    public static bool TryPurchaseDecoration(DecorationDataSO decoration)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot buy decoration.");
            return false;
        }

        if (decoration == null)
        {
            Debug.LogWarning("DecorationData is null.");
            return false;
        }

        int decorationPrice = decoration.price;

        // Deduct coins via CoinManager (handles check, update, save, event)
        if (!CoinManager.SpendCoins(decorationPrice))
        {
            Debug.Log($"Not enough coins to buy {decoration.decorationName}. Needed: {decorationPrice}, Owned: {CoinManager.Coins}");
            return false;
        }

        // Add decoration to owned list
        _playerConfig.AddDecoration(decoration.decorationID);

        SaveAll();
        Debug.Log($"Bought decoration {decoration.decorationName} for {decorationPrice} coins. Remaining: {CoinManager.Coins}");
        return true;
    }
    public static void ToggleDecorationActiveState(string decorationID, bool isActive)
    {
        if (string.IsNullOrEmpty(decorationID))
        {
            Debug.LogWarning("Tried to toggle active state for null or empty decoration ID.");
            return;
        }

        var decoration = _playerConfig.ownedDecorations.FirstOrDefault(d => d.decorationID == decorationID);
        if (decoration == null)
        {
            Debug.LogWarning($"Decoration with ID '{decorationID}' not found.");
            return;
        }

        decoration.isActive = isActive;
        SaveAll();
        Debug.Log($"Set decoration '{decoration.decorationID}' active state to {isActive}.");
    }

    public static void SetActiveDecoration(string decorationID)
    {
        foreach (var d in _playerConfig.ownedDecorations)
            d.isActive = (d.decorationID == decorationID);

        SaveAll();
    }

    public static bool GetDecorationActiveStatus(string decorationID)
    {
        var data = _playerConfig.ownedDecorations
            .FirstOrDefault(d => d.decorationID == decorationID);

        return data != null && data.isActive;
    }

   
    #endregion

    #region Game Area Operations
    public static void SaveActiveGameAreaIndex(int areaIndex)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot save game area index.");
            return;
        }

        _playerConfig.lastGameAreaIndex = areaIndex;
        SaveAll();
        Debug.Log($"Saved active game area index: {areaIndex}");
    }

    public static int LoadActiveGameAreaIndex()
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, returning default game area index 0.");
            return 0;
        }

        return _playerConfig.lastGameAreaIndex;
    }
    #endregion

    public static PlayerConfig GetPlayerConfig()
    {
        if (_playerConfig == null)
        {
            Debug.LogError("PlayerConfig was null, creating new one.");
           // CreateNewPlayerConfig();
        }
        return _playerConfig;
    }


}
