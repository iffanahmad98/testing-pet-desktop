#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using Unity.EditorCoroutines.Editor;

public class MonsterDataGenerator
{
    #region Public API
    public static void ShowUpdateWindow()
    {
        string path = EditorUtility.OpenFilePanel("Select Monster CSV", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            string csvContent = System.IO.File.ReadAllText(path);
            GenerateFromCSV(csvContent);
        }
    }
    
    public static void GenerateFromCSV(string csvContent)
    {
        var monsters = ParseCSV(csvContent);
        
        foreach (var monster in monsters)
        {
            UpdateOrCreateMonsterDataSO(monster);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    #endregion
    
    #region CSV Parsing
    
    private static List<MonsterCSVData> ParseCSV(string csvContent)
    {
        var monsters = new List<MonsterCSVData>();
        
        // Handle different line endings
        csvContent = csvContent.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = csvContent.Split('\n');
        
        Debug.Log($"🔍 ParseCSV: Total lines = {lines.Length}");
        
        for (int i = 4; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            if (string.IsNullOrEmpty(line) || 
                line.StartsWith(";;;;;;") || 
                line.StartsWith(",,,,,,") ||  // NEW: Skip Google Sheets empty lines
                line.Contains("100,00%") ||
                line.Contains("100.00%") ||  // NEW: Handle decimal format
                line.Contains("*Kalau")) 
            {
                Debug.Log($"⏭️ Skipping line {i}: {line.Substring(0, Mathf.Min(30, line.Length))}...");
                continue;
            }
            
            string[] values = line.Split(',');  // NEW: Use comma instead of semicolon
            
            // NEW: Handle Google Sheets format with leading empty column
            if (values.Length > 0 && string.IsNullOrEmpty(values[0]))
            {
                // Remove the first empty column
                string[] newValues = new string[values.Length - 1];
                System.Array.Copy(values, 1, newValues, 0, values.Length - 1);
                values = newValues;
            }
            
            if (values.Length < 23 || string.IsNullOrEmpty(values[0])) // Changed from 11 to 23
            {
                Debug.Log($"⏭️ Skipping line {i}: insufficient data ({values.Length} columns)");
                continue;
            }
            
            Debug.Log($"✅ Processing: {values[0]} ({values.Length} columns)");
            
            var monster = ParseMonsterFromCSV(values);
            if (monster != null)
            {
                monsters.Add(monster);
                Debug.Log($"🎉 Added: {monster.name} ({monster.type})");
            }
        }
        
        Debug.Log($"📊 Final result: {monsters.Count} monsters parsed");
        return monsters;
    }
    
    private static MonsterCSVData ParseMonsterFromCSV(string[] values)
    {
        try
        {
            var monster = new MonsterCSVData();
            
            monster.name = values[0].Trim();
            monster.type = ParseMonsterType(values[1].Trim());
            monster.pupType = ParsePupType(values[2].Trim());
            // Skip values[3] - this is the empty Picture column
            
            monster.stage1 = new StageData
            {
                fullness = ParseFloat(values[4]),     // Column 5: Fullness
                hp = ParseFloat(values[5]),           // Column 6: HP
                timeEvolveDays = ParseFloat(values[6]), // Column 7: Time Evolve
                gachaChance = values[7],              // Column 8: Chance Gatcha
                gachaChanceDecimal = ParseGachaChance(values[7]),
                platCoinHour = ParseFloat(values[8]), // Column 9: Platinum Coin (renamed from silverCoinHour)
                goldCoinHour = ParseFloat(values[9]),   // Column 10: Gold Coin
                priceBuy = ParseFloat(values[10]),    // Column 11: Price Buy
                priceSell = ParseFloat(values[11])    // Column 12: Price Sell
            };
            
            monster.stage2 = new StageData
            {
                fullness = ParseFloatWithDefault(values, 12, monster.stage1.fullness * 1.5f),  // Column 13
                hp = ParseFloatWithDefault(values, 13, monster.stage1.hp * 2f),               // Column 14
                timeEvolveDays = ParseFloatWithDefault(values, 14, monster.stage1.timeEvolveDays), // Column 15
                platCoinHour = ParseFloatWithDefault(values, 15, monster.stage1.platCoinHour), // Column 16: Platinum
                goldCoinHour = ParseFloatWithDefault(values, 16, monster.stage1.goldCoinHour * 3f), // Column 17: Gold
                priceSell = ParseFloatWithDefault(values, 17, monster.stage1.priceSell * 2f)   // Column 18
            };
            
            monster.stage3 = new StageData
            {
                fullness = ParseFloatWithDefault(values, 18, monster.stage2.fullness * 1.2f),  // Column 19
                hp = ParseFloatWithDefault(values, 19, monster.stage2.hp * 1.4f),             // Column 20
                timeEvolveDays = 0,
                platCoinHour = ParseFloatWithDefault(values, 20, monster.stage2.platCoinHour), // Column 21: Platinum
                goldCoinHour = ParseFloatWithDefault(values, 21, monster.stage2.goldCoinHour * 2f), // Column 22: Gold
                priceSell = ParseFloatWithDefault(values, 22, monster.stage2.priceSell * 2f)   // Column 23
            };
            
            ApplyDefaultEvolutionLogic(monster);
            
            return monster;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing monster: {values[0]} - {e.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region Data Type Parsing
    
    private static float ParseFloat(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0f;
        
        value = value.Replace(',', '.').Trim();
        
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            return result;
            
        return 0f;
    }
    
    private static float ParseFloatWithDefault(string[] values, int index, float defaultValue)
    {
        if (index >= values.Length || string.IsNullOrEmpty(values[index]))
            return defaultValue;
            
        float parsed = ParseFloat(values[index]);
        return parsed > 0 ? parsed : defaultValue;
    }
    
    private static MonsterType ParseMonsterType(string type)
    {
        if (string.IsNullOrEmpty(type)) return MonsterType.Common;
        
        if (System.Enum.TryParse<MonsterType>(type.Trim(), true, out var result))
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
    
    private static string ParsePupType(string pupType)
    {
        if (string.IsNullOrEmpty(pupType)) return "Normal";
        
        pupType = pupType.Trim();
        
        if (pupType.Equals("Spakle", System.StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("Sparkle", System.StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("Spakle ", System.StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("sparkle", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Sparkle";
        }
        
        if (pupType.Equals("Normal", System.StringComparison.OrdinalIgnoreCase) ||
            pupType.Equals("normal", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Normal";
        }
        
        return "Normal";
    }
    
    private static float ParseGachaChance(string gachaChance)
    {
        if (string.IsNullOrEmpty(gachaChance)) return 0f;
        
        string cleanValue = gachaChance.Replace("%", "").Replace(',', '.').Trim();
        
        if (float.TryParse(cleanValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result / 100f;
        }
        
        return 0f;
    }
    
    #endregion
    
    #region Data Logic
    
    private static void ApplyDefaultEvolutionLogic(MonsterCSVData monster)
    {
        if (monster.stage2.hp <= 0)
        {
            monster.stage2 = null;
            monster.stage3 = null;
            return;
        }
        
        if (monster.stage3.hp <= 0)
        {
            monster.stage3 = null;
            return;
        }
        
        if (monster.stage2.timeEvolveDays <= 0)
            monster.stage2.timeEvolveDays = monster.stage1.timeEvolveDays;
    }
    
    #endregion
    
    #region Data Mapping
    
    private static void MapCSVToMonsterData(MonsterCSVData csvData, MonsterDataSO asset)
    {
        asset.monsterName = csvData.name;
        asset.monType = csvData.type;
        asset.hp = csvData.stage1.hp;
        asset.maxNutritionStage1 = csvData.stage1.fullness;
        
        // NEW: Map stage-specific hunger values
        if (csvData.stage2 != null)
            asset.maxNutritionStage2 = csvData.stage2.fullness;
        
        if (csvData.stage3 != null)
            asset.maxNutritionStage3 = csvData.stage3.fullness;
        
        // Map coin drop rates for all stages
        asset.goldCoinDropRateStage1 = csvData.stage1.goldCoinHour;
        asset.platCoinDropRateStage1 = csvData.stage1.platCoinHour; // Updated from silverCoinHour

        if (csvData.stage2 != null)
        {
            asset.goldCoinDropRateStage2 = csvData.stage2.goldCoinHour;
            asset.platCoinDropRateStage2 = csvData.stage2.platCoinHour; // Updated from silverCoinHour
        }
        
        if (csvData.stage3 != null)
        {
            asset.goldCoinDropRateStage3 = csvData.stage3.goldCoinHour;
            asset.platCoinDropRateStage3 = csvData.stage3.platCoinHour; // Updated from silverCoinHour
        }
        
        asset.monsterPrice = (int)csvData.stage1.priceBuy;
        
        // NEW: Map gacha data
        asset.gachaChancePercent = csvData.stage1.gachaChanceDecimal;
        asset.gachaChanceDisplay = csvData.stage1.gachaChance;
        asset.isGachaOnly = csvData.stage1.priceBuy <= 0;
        
        // Map sell prices for each stage
        asset.sellPriceStage1 = (int)csvData.stage1.priceSell;
        asset.sellPriceStage2 = csvData.stage2 != null ? (int)csvData.stage2.priceSell : 0;
        asset.sellPriceStage3 = csvData.stage3 != null ? (int)csvData.stage3.priceSell : 0;
        
        asset.canEvolve = csvData.stage2 != null;
        
        if (csvData.stage3 != null)
            asset.evolutionLevel = 0;
        else if (csvData.stage2 != null)
            asset.evolutionLevel = 0;
        else
            asset.evolutionLevel = 0;
        
        // FIX: Embed evolution requirements directly
        if (csvData.stage2 != null)
        {
            var requirements = new List<EvolutionRequirement>();
            
            // Stage 1 → Stage 2 (Level 1 → Level 2)
            var req1 = CreateEvolutionRequirement(csvData, 2, csvData.stage1.timeEvolveDays); // Changed from 1 to 2
            requirements.Add(req1);
            
            if (csvData.stage3 != null)
            {
                // Stage 2 → Stage 3 (Level 2 → Level 3)
                var req2 = CreateEvolutionRequirement(csvData, 3, csvData.stage2.timeEvolveDays); // Changed from 2 to 3
                req2.minCurrentHappiness = req1.minCurrentHappiness + 5f;
                req2.minCurrentHunger = req1.minCurrentHunger + 5f;
                req2.minFoodConsumed = req1.minFoodConsumed * 2;
                req2.minInteractions = req1.minInteractions * 2;
                req2.evolutionName = $"{csvData.name} Stage 3";
                req2.description = $"Evolve to {csvData.name} Stage 3";
                requirements.Add(req2);
            }
            
            asset.evolutionRequirements = requirements.ToArray();
        }
        else
        {
            asset.evolutionRequirements = new EvolutionRequirement[0];
        }
    }
    
    private static void SetMonsterDataDefaults(MonsterCSVData csvData, MonsterDataSO asset)
    {
        switch (csvData.type)
        {
            case MonsterType.Legend:
                asset.moveSpd = 80f;
                asset.pokeHappinessValue = 5f;
                break;
            case MonsterType.Mythic:
                asset.moveSpd = 90f;
                asset.pokeHappinessValue = 4f;
                break;
            case MonsterType.Rare:
                asset.moveSpd = 100f;
                asset.pokeHappinessValue = 3f;
                break;
            case MonsterType.Uncommon:
                asset.moveSpd = 110f;
                asset.pokeHappinessValue = 2.5f;
                break;
            case MonsterType.Common:
                asset.moveSpd = 120f;
                asset.pokeHappinessValue = 2f;
                break;
        }
        
        asset.hungerDepleteRate = 0.05f;
        asset.poopRate = 20f;
        asset.baseHunger = 50f;
        asset.baseHappiness = 0f;
        asset.foodDetectionRange = 200f;
        asset.eatDistance = 5f;
        asset.areaHappinessRate = 0.2f;
        asset.hungerHappinessThreshold = 20f;
        asset.hungerHappinessDrainRate = 2f;
        asset.evolutionLevel = 1; 
        
        // ADD: Map poop type from CSV pupType
        asset.poopType = csvData.pupType == "Sparkle" ? PoopType.Sparkle : PoopType.Normal;
        
        // ADD: Generate unique ID if not exists
        if (string.IsNullOrEmpty(asset.id))
        {
            asset.id = csvData.name.ToLower().Replace(" ", "_");
        }
        
        // ADD: Set evolution flags properly
        asset.isEvolved = false; // Always start as base form
        asset.canEvolve = csvData.stage2 != null;
    }
    
    private static EvolutionRequirement CreateEvolutionRequirement(MonsterCSVData csvData, int targetLevel, float timeEvolveDays)
    {
        var req = new EvolutionRequirement();
        req.targetEvolutionLevel = targetLevel; // Now 2 or 3 instead of 1 or 2
        req.minTimeAlive = timeEvolveDays * 24 * 60 * 60;
        
        switch (csvData.type)
        {
            case MonsterType.Legend:
                req.minCurrentHappiness = 95f;
                req.minCurrentHunger = 85f;
                req.minFoodConsumed = 50;
                req.minInteractions = 100;
                break;
            case MonsterType.Mythic:
                req.minCurrentHappiness = 90f;
                req.minCurrentHunger = 80f;
                req.minFoodConsumed = 30;
                req.minInteractions = 60;
                break;
            case MonsterType.Rare:
                req.minCurrentHappiness = 85f;
                req.minCurrentHunger = 75f;
                req.minFoodConsumed = 20;
                req.minInteractions = 40;
                break;
            default:
                req.minCurrentHappiness = 80f;
                req.minCurrentHunger = 70f;
                req.minFoodConsumed = 15;
                req.minInteractions = 25;
                break;
        }
        
        req.evolutionName = $"{csvData.name} Stage {targetLevel}";
        req.description = $"Evolve to {csvData.name} Stage {targetLevel}";
        
        return req;
    }
    
    #endregion
    
    #region ScriptableObject Creation

    private static void UpdateOrCreateMonsterDataSO(MonsterCSVData csvData)
    {
        string assetPath = $"Assets/Game/Data/Monsters/{csvData.name}_Data.asset";
        var existingAsset = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(assetPath);
        
        MonsterDataSO asset = existingAsset ?? ScriptableObject.CreateInstance<MonsterDataSO>();
        
        // FIX: Call the mapping method that embeds evolution requirements
        MapCSVToMonsterData(csvData, asset);
        
        if (existingAsset == null)
        {
            SetMonsterDataDefaults(csvData, asset);
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(existingAsset);
        }
    }

    #endregion

    #region Google Sheets Integration

    [MenuItem("Tools/Monster Data/🔄 Fetch from Google Sheets", priority = 0)]
    public static void FetchFromGoogleSheets()
    {
        var updater = Object.FindFirstObjectByType<MonsterDataUpdater>();
        if (updater == null)
        {
            if (EditorUtility.DisplayDialog("Missing Component", 
                "No MonsterDataUpdater found in scene. Create one?", "Create", "Cancel"))
            {
                CreateMonsterDataUpdater();
                updater = Object.FindFirstObjectByType<MonsterDataUpdater>();
            }
            else
            {
                return;
            }
        }
        
        if (string.IsNullOrEmpty(updater.googleSheetURL))
        {
            EditorUtility.DisplayDialog("Missing URL", 
                "Please set the Google Sheets URL in the MonsterDataUpdater component first!", "OK");
            Selection.activeObject = updater;
            return;
        }
        
        EditorCoroutineUtility.StartCoroutine(FetchAndGenerateCoroutine(updater.googleSheetURL), updater);
    }

    [MenuItem("Tools/Monster Data/📂 Load from Local CSV", priority = 1)]
    public static void LoadFromLocalCSV()
    {
        ShowUpdateWindow(); // Your existing method
    }

    [MenuItem("Tools/Monster Data/🗑️ Clear All Monster Assets", priority = 10)]
    public static void ClearAllMonsterAssets()
    {
        if (!EditorUtility.DisplayDialog("Clear Monster Assets", 
            "Delete all existing monster data assets?", "Delete", "Cancel"))
            return;
            
        string[] guids = AssetDatabase.FindAssets("t:MonsterDataSO", new[] { "Assets/Game/Data/Monsters" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"✅ Cleared {guids.Length} monster data assets");
        EditorUtility.DisplayDialog("Cleared", $"Deleted {guids.Length} assets", "OK");
    }

    private static void CreateMonsterDataUpdater()
    {
        GameObject go = new GameObject("MonsterDataUpdater");
        var updater = go.AddComponent<MonsterDataUpdater>();
        updater.updateOnGameStart = false; // Don't auto-update in editor
        
        Debug.Log("✅ Created MonsterDataUpdater GameObject");
    }

    private static System.Collections.IEnumerator FetchAndGenerateCoroutine(string url)
    {
        Debug.Log($"🔄 Fetching CSV from: {url}");
        EditorUtility.DisplayProgressBar("Fetching Data", "Connecting to Google Sheets...", 0.3f);
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            request.timeout = 15;
            yield return request.SendWebRequest();
            
            EditorUtility.DisplayProgressBar("Fetching Data", "Processing data...", 0.8f);
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ CSV downloaded successfully! ({request.downloadHandler.text.Length} characters)");
                
                // Generate assets
                GenerateFromCSV(request.downloadHandler.text);
                
                // Count generated assets
                string[] guids = AssetDatabase.FindAssets("t:MonsterDataSO", new[] { "Assets/Game/Data/Monsters" });
                Debug.Log($"✅ Generated {guids.Length} monster data assets successfully!");
                
                EditorUtility.DisplayDialog("Success!", 
                    $"Successfully fetched and generated {guids.Length} monster data assets!", "OK");
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch CSV: {request.error}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to fetch data:\n{request.error}", "OK");
            }
            
            EditorUtility.ClearProgressBar();
        }
    }

    #endregion
}

#region Data Classes

[System.Serializable]
public class MonsterCSVData
{
    public string name;
    public MonsterType type;
    public string pupType;
    public StageData stage1;
    public StageData stage2;
    public StageData stage3;
}

[System.Serializable]
public class StageData
{
    public float fullness;
    public float hp;
    public float timeEvolveDays;
    public string gachaChance;
    public float gachaChanceDecimal;
    public float goldCoinHour;
    public float platCoinHour; // Renamed from silverCoinHour to match MonsterDataSO
    public float priceBuy;
    public float priceSell;
}

#endregion

#endif