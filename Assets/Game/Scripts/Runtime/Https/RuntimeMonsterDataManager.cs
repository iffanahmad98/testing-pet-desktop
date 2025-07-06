using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class RuntimeMonsterDataManager : MonoBehaviour
{
    #region Singleton
    public static RuntimeMonsterDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Inspector Fields
    [Header("Google Sheets")]
    public string googleSheetURL;

    [Header("Update Settings")]
    public bool updateOnGameStart = true;
    public KeyCode manualUpdateKey = KeyCode.F5;
    public bool useLocalFallbackIfOffline = true;
    #endregion

    #region Events
    public static event Action OnUpdateStarted;
    public static event Action<string> OnUpdateProgress;
    public static event Action OnUpdateCompleted;
    public static event Action<string> OnUpdateFailed;
    #endregion

    #region Runtime Data Storage
    private Dictionary<string, RuntimeMonsterData> monsterDatabase = new Dictionary<string, RuntimeMonsterData>();

    [Serializable]
    public class RuntimeMonsterData
    {
        public string name;
        public MonsterType type;
        public string pupType;

        public StageData stage1;
        public StageData stage2;
        public StageData stage3;

        [Serializable]
        public class StageData
        {
            public float fullness;
            public float maxHP;
            public float timeEvolveDays;
            public string gachaChance;
            public float gachaChanceDecimal;
            public float platCoinHour;
            public float goldCoinHour;
            public float priceBuy;
            public float priceSell;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        LoadLocalData();

        if (updateOnGameStart)
        {
            StartCoroutine(UpdateMonsterData());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(manualUpdateKey))
        {
            StartCoroutine(UpdateMonsterData());
        }
    }
    #endregion

    #region Data Operations
    public IEnumerator UpdateMonsterData()
    {
        OnUpdateStarted?.Invoke();
        OnUpdateProgress?.Invoke("Connecting to server...");

        using (UnityWebRequest request = UnityWebRequest.Get(googleSheetURL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OnUpdateProgress?.Invoke("Processing monster data...");
                ProcessCSVData(request.downloadHandler.text);
                SaveLocalData(); // Cache the data
                OnUpdateCompleted?.Invoke();
            }
            else
            {
                string errorMsg = $"Failed to fetch monster data: {request.error}";
                Debug.LogWarning(errorMsg);

                if (useLocalFallbackIfOffline && monsterDatabase.Count == 0)
                {
                    LoadLocalData();
                    OnUpdateProgress?.Invoke("Using cached monster data");
                    OnUpdateCompleted?.Invoke();
                }
                else
                {
                    OnUpdateFailed?.Invoke(errorMsg);
                }
            }
        }
    }

    private void ProcessCSVData(string csvContent)
    {
        // Clear current database
        monsterDatabase.Clear();

        // Parse CSV (similar to your MonsterDataGenerator)
        var monsters = ParseCSV(csvContent);

        // Add to runtime database
        foreach (var monster in monsters)
        {
            monsterDatabase[monster.name] = monster;
        }

        Debug.Log($"Runtime monster database updated with {monsterDatabase.Count} entries");
    }

    private List<RuntimeMonsterData> ParseCSV(string csvContent)
    {
        var monsters = new List<RuntimeMonsterData>();

        // Handle different line endings
        csvContent = csvContent.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = csvContent.Split('\n');

        Debug.Log($"🔍 ParseCSV: Total lines = {lines.Length}");

        for (int i = 3; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line) ||
                line.StartsWith(";;;;;;") ||
                line.StartsWith(",,,,,,") ||
                line.Contains("100,00%") ||
                line.Contains("100.00%") ||
                line.Contains("*Kalau"))
            {
                continue;
            }

            string[] values = line.Split(',');

            // Handle Google Sheets format with leading empty column
            if (values.Length > 0 && string.IsNullOrEmpty(values[0]))
            {
                string[] newValues = new string[values.Length - 1];
                Array.Copy(values, 1, newValues, 0, values.Length - 1);
                values = newValues;
            }

            if (values.Length < 23 || string.IsNullOrEmpty(values[0]))
            {
                continue;
            }

            var monster = ParseMonsterFromCSV(values);
            if (monster != null)
            {
                monsters.Add(monster);
            }
        }

        return monsters;
    }

    private RuntimeMonsterData ParseMonsterFromCSV(string[] values)
    {
        try
        {
            var monster = new RuntimeMonsterData();

            monster.name = values[0].Trim();
            monster.type = ParseMonsterType(values[1].Trim());
            monster.pupType = ParsePupType(values[2].Trim());

            monster.stage1 = new RuntimeMonsterData.StageData
            {
                fullness = ParseFloat(values[4]),
                maxHP = ParseFloat(values[5]),
                timeEvolveDays = ParseFloat(values[6]),
                gachaChance = values[7],
                gachaChanceDecimal = ParseGachaChance(values[7]),
                platCoinHour = ParseFloat(values[8]),
                goldCoinHour = ParseFloat(values[9]),
                priceBuy = ParseFloat(values[10]),
                priceSell = ParseFloat(values[11])
            };

            monster.stage2 = new RuntimeMonsterData.StageData
            {
                fullness = ParseFloatWithDefault(values, 12, monster.stage1.fullness * 1.5f),
                maxHP = ParseFloatWithDefault(values, 13, monster.stage1.maxHP * 2f),
                timeEvolveDays = ParseFloatWithDefault(values, 14, monster.stage1.timeEvolveDays),
                platCoinHour = ParseFloatWithDefault(values, 15, monster.stage1.platCoinHour),
                goldCoinHour = ParseFloatWithDefault(values, 16, monster.stage1.goldCoinHour * 3f),
                priceSell = ParseFloatWithDefault(values, 17, monster.stage1.priceSell * 2f)
            };

            monster.stage3 = new RuntimeMonsterData.StageData
            {
                fullness = ParseFloatWithDefault(values, 18, monster.stage2.fullness * 1.2f),
                maxHP = ParseFloatWithDefault(values, 19, monster.stage2.maxHP * 1.4f),
                timeEvolveDays = 0,
                platCoinHour = ParseFloatWithDefault(values, 20, monster.stage2.platCoinHour),
                goldCoinHour = ParseFloatWithDefault(values, 21, monster.stage2.goldCoinHour * 2f),
                priceSell = ParseFloatWithDefault(values, 22, monster.stage2.priceSell * 2f)
            };

            ApplyDefaultEvolutionLogic(monster);

            return monster;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing monster: {values[0]} - {e.Message}");
            return null;
        }
    }
    #endregion

    #region Helper Methods
    private float ParseFloat(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0f;

        value = value.Replace(',', '.').Trim();

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            return result;

        return 0f;
    }

    private float ParseFloatWithDefault(string[] values, int index, float defaultValue)
    {
        if (index >= values.Length || string.IsNullOrEmpty(values[index]))
            return defaultValue;

        float parsed = ParseFloat(values[index]);
        return parsed > 0 ? parsed : defaultValue;
    }

    private MonsterType ParseMonsterType(string type)
    {
        if (string.IsNullOrEmpty(type)) return MonsterType.Common;

        if (Enum.TryParse<MonsterType>(type.Trim(), true, out var result))
            return result;

        string lowerType = type.ToLower().Trim();
        switch (lowerType)
        {
            case "legendary": return MonsterType.Legend;
            case "mythical": return MonsterType.Mythic;
            case "epic": return MonsterType.Rare;
            default: return MonsterType.Common;
        }
    }

    private string ParsePupType(string pupType)
    {
        if (string.IsNullOrEmpty(pupType)) return "Normal";

        pupType = pupType.Trim();

        if (pupType.Equals("Spakle", StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("Sparkle", StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("Spakle ", StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("sparkle", StringComparison.OrdinalIgnoreCase))
        {
            return "Sparkle";
        }

        return "Normal";
    }

    private float ParseGachaChance(string gachaChance)
    {
        if (string.IsNullOrEmpty(gachaChance)) return 0f;

        string cleanValue = gachaChance.Replace("%", "").Replace(',', '.').Trim();

        if (float.TryParse(cleanValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result / 100f; // Convert percentage to decimal
        }

        return 0f;
    }

    private void ApplyDefaultEvolutionLogic(RuntimeMonsterData monster)
    {
        // Default evolution logic if values not specified
        if (monster.stage2.maxHP <= 0)
        {
            monster.stage2.maxHP = monster.stage1.maxHP * 2;
        }

        if (monster.stage3.maxHP <= 0)
        {
            monster.stage3.maxHP = monster.stage2.maxHP * 1.4f;
        }

        if (monster.stage2.timeEvolveDays <= 0)
        {
            monster.stage2.timeEvolveDays = monster.stage1.timeEvolveDays * 1.5f;
        }
    }
    #endregion

    #region Data Access API

    // Get a single monster's data
    public RuntimeMonsterData GetMonsterData(string monsterName)
    {
        if (monsterDatabase.TryGetValue(monsterName, out RuntimeMonsterData data))
        {
            return data;
        }

        Debug.LogWarning($"Monster '{monsterName}' not found in runtime database");
        return null;
    }

    // Get all monster data
    public List<RuntimeMonsterData> GetAllMonsters()
    {
        return new List<RuntimeMonsterData>(monsterDatabase.Values);
    }

    // Get monsters by type
    public List<RuntimeMonsterData> GetMonstersByType(MonsterType type)
    {
        List<RuntimeMonsterData> results = new List<RuntimeMonsterData>();

        foreach (var monster in monsterDatabase.Values)
        {
            if (monster.type == type)
            {
                results.Add(monster);
            }
        }

        return results;
    }

    // Get gacha chances for all monsters
    public Dictionary<string, float> GetGachaChances()
    {
        Dictionary<string, float> chances = new Dictionary<string, float>();

        foreach (var monster in monsterDatabase.Values)
        {
            chances[monster.name] = monster.stage1.gachaChanceDecimal;
        }

        return chances;
    }
    #endregion

    #region Local Storage

    // Save current data to PlayerPrefs for offline use
    private void SaveLocalData()
    {
        try
        {
            string json = JsonUtility.ToJson(new SerializableMonsterDatabase(monsterDatabase));
            PlayerPrefs.SetString("MonsterDatabase", json);
            PlayerPrefs.Save();
            Debug.Log("Monster database cached locally");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save monster database locally: {e.Message}");
        }
    }

    // Load data from PlayerPrefs
    private void LoadLocalData()
    {
        try
        {
            if (PlayerPrefs.HasKey("MonsterDatabase"))
            {
                string json = PlayerPrefs.GetString("MonsterDatabase");
                var database = JsonUtility.FromJson<SerializableMonsterDatabase>(json);

                monsterDatabase.Clear();
                foreach (var monster in database.monsters)
                {
                    monsterDatabase[monster.name] = monster;
                }

                Debug.Log($"Loaded {monsterDatabase.Count} monsters from local cache");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load monster database from cache: {e.Message}");
        }
    }

    // Helper class for serialization
    [Serializable]
    private class SerializableMonsterDatabase
    {
        public List<RuntimeMonsterData> monsters;

        public SerializableMonsterDatabase(Dictionary<string, RuntimeMonsterData> database)
        {
            monsters = new List<RuntimeMonsterData>(database.Values);
        }

        // Required for deserialization
        public SerializableMonsterDatabase()
        {
            monsters = new List<RuntimeMonsterData>();
        }
    }
    #endregion
}
