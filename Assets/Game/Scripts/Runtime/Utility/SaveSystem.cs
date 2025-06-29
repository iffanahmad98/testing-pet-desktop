using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;


public static class SaveSystem
{
    private const string SaveFileName = "playerConfig.json";
    private static PlayerConfig _playerConfig;
    public static PlayerConfig PlayerConfig => _playerConfig;
    private static DateTime _sessionStartTime;

    // Global game statss
    private const string CoinKey = "Coin";
    private const string PoopKey = "Poop";
    private const string MonsterKey = "MonsterIDs";

    public static void SaveCoin(int money) => _playerConfig.coins = money;
    public static int LoadCoin() => _playerConfig.coins;
    public static void SavePoop(int poop) => _playerConfig.poops = poop; // Directly save to PlayerConfig
    public static int LoadPoop() => _playerConfig.poops;
    public static void Initialize()
    {

        LoadPlayerConfig();
        _sessionStartTime = DateTime.Now;

        Debug.Log($"Last login time: {_playerConfig.lastLoginTime}");
        // Handle first-time login
        if (_playerConfig.lastLoginTime == default)
        {
            _playerConfig.lastLoginTime = DateTime.Now;
        }

        // Check for time cheating
        CheckTimeDiscrepancy();
    }
    // Save all data when application pauses/quits
    public static void SaveAll()
    {
        UpdatePlayTime();
        SavePlayerConfig();
    }


    // Pets
    // public static void SaveMon(MonsterSaveData data)
    // {
    //     string key = $"Pet{data.monsterId}";
    //     string json = JsonUtility.ToJson(data);
    //     PlayerPrefs.SetString(key, json);

    //     // Save the ID to the master list
    //     SaveMonsterIDToList(data.monsterId);
    //     PlayerPrefs.Save();
    // }

    // public static bool LoadMon(string petID, out MonsterSaveData data)
    // {
    //     string key = $"Pet{petID}";
    //     if (PlayerPrefs.HasKey(key))
    //     {
    //         data = JsonUtility.FromJson<MonsterSaveData>(PlayerPrefs.GetString(key));
    //         return true;
    //     }

    //     data = null;
    //     return false;
    // }
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

    public static void DeleteMon(string monsterID)
    {
        if (string.IsNullOrEmpty(monsterID)) return;

        _playerConfig.DeleteMonster(monsterID);
        SaveAll(); // Ensure the config is persisted
    }


    // public static void DeleteMon(string monsterID)
    // {
    //     // Delete the monster data
    //     string key = $"Pet{monsterID}";
    //     PlayerPrefs.DeleteKey(key);

    //     // Remove from master list
    //     List<string> existingIDs = LoadSavedMonIDs();
    //     if (existingIDs.Contains(monsterID))
    //     {
    //         existingIDs.Remove(monsterID);
    //         SaveMonIDs(existingIDs);
    //     }

    //     PlayerPrefs.Save();
    // }

    // private static void SaveMonsterIDToList(string monsterID)
    // {
    //     List<string> existingIDs = LoadSavedMonIDs();
    //     if (!existingIDs.Contains(monsterID))
    //     {
    //         existingIDs.Add(monsterID);
    //         SaveMonIDs(existingIDs);
    //     }
    // }

    // public static void SaveMonIDs(List<string> ids)
    // {
    //     PlayerPrefs.SetString(MonsterKey, string.Join(",", ids));
    // }

    // public static List<string> LoadSavedMonIDs()
    // {
    //     string csv = PlayerPrefs.GetString(MonsterKey, "");
    //     return string.IsNullOrEmpty(csv)
    //         ? new List<string>()
    //         : new List<string>(csv.Split(','));
    // }
    // public static void DeleteMon(string monsterID)
    // {
    //     _playerConfig.DeleteMonster(monsterID);
    //     SavePlayerConfig();
    // }

    public static void SaveMonIDs(List<string> ids)
    {
        _playerConfig.SetAllMonsterIDs(ids);
        SavePlayerConfig();
    }

    public static List<string> LoadSavedMonIDs()
    {
        return _playerConfig.GetAllMonsterIDs();
    }


    public static void Flush() => PlayerPrefs.Save();

    // public static void ResetSaveData()
    // {
    //     PlayerPrefs.SetInt(CoinKey, 100);
    //     PlayerPrefs.SetInt(PoopKey, 0);
    //     PlayerPrefs.SetString(MonsterKey, "");

    //     // Clear all pet data
    //     var keys = PlayerPrefs.GetString(MonsterKey, "").Split(',');
    //     foreach (var key in keys)
    //     {
    //         if (!string.IsNullOrEmpty(key))
    //             PlayerPrefs.DeleteKey($"Pet{key}");
    //     }

    //     PlayerPrefs.Save();
    // }
    public static void ResetSaveData()
    {
        PlayerPrefs.SetInt(CoinKey, 100);
        PlayerPrefs.SetInt(PoopKey, 0);

        _playerConfig.ClearAllMonsterData();
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
            HandleLongAbsence(timeSinceLastLogin);
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

        foreach (var monster in _playerConfig.monsters)
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

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                _playerConfig = JsonConvert.DeserializeObject<PlayerConfig>(json);
                _playerConfig.SyncFromSerializable();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game data: " + e.Message);
                CreateNewPlayerConfig();
            }
        }
        else
        {
            CreateNewPlayerConfig();
        }
    }



    private static void SavePlayerConfig()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        try
        {
            _playerConfig.SyncToSerializable(); // Convert DateTime to strings, etc.
            string json = JsonConvert.SerializeObject(_playerConfig, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log("Game data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game data: " + e.Message);
        }
    }



    private static void CreateNewPlayerConfig()
    {
        _playerConfig = new PlayerConfig();
        _playerConfig.lastLoginTime = DateTime.Now;
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
    public static void UpdateItemData(string itemID, int amount)
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig is null, cannot update item data.");
            return;
        }

        _playerConfig.AddItem(itemID, amount);
        SaveAll();
    }
    #endregion

    public static PlayerConfig GetPlayerConfig()
    {
        if (_playerConfig == null)
        {
            Debug.LogWarning("PlayerConfig was null, creating new one.");
            CreateNewPlayerConfig();
        }
        return _playerConfig;
    }

}
