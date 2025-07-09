using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using Unity.EditorCoroutines.Editor;
#endif

public class ItemDataGenerator
{
    #region Public API
    
    public static void ShowUpdateWindow()
    {
        string path = EditorUtility.OpenFilePanel("Select Item CSV", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            string csvContent = System.IO.File.ReadAllText(path);
            
            // Ask user for item type
            int choice = EditorUtility.DisplayDialogComplex("Item Type", 
                "What type of items are in this CSV?", "Food", "Medicine", "Cancel");
            
            if (choice == 0)
                GenerateFromCSV(csvContent, ItemType.Food);
            else if (choice == 1)
                GenerateFromCSV(csvContent, ItemType.Medicine);
        }
    }

    public static void GenerateFromCSV(string csvContent, ItemType itemType)
    {
        var items = ParseCSV(csvContent, itemType);

        foreach (var item in items)
        {
            UpdateOrCreateItemDataSO(item);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Generated {items.Count} {itemType} items successfully!");
    }

    #endregion

    #region CSV Parsing

    private static string[] ParseCSVLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.Trim());
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        // Add the last value
        values.Add(currentValue.Trim());

        return values.ToArray();
    }

    private static List<ItemCSVData> ParseCSV(string csvContent, ItemType itemType)
    {
        var items = new List<ItemCSVData>();

        // Handle different line endings
        csvContent = csvContent.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = csvContent.Split('\n');

        Debug.Log($"🔍 ParseCSV: Total lines = {lines.Length} for {itemType}");

        // Skip header row (start from line 1)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // Check if this line starts with a quote (indicating a new item)
            // or if it's a continuation of a multi-line description
            if (!line.StartsWith("\"") && !char.IsLetter(line[0]) && items.Count > 0)
            {
                // This might be a continuation of the previous item's description
                Debug.Log($"⚠️ Possible description continuation at line {i}: {line.Substring(0, Mathf.Min(50, line.Length))}...");
                continue;
            }

            string[] values = ParseCSVLine(line);

            if (values.Length < 3 || string.IsNullOrEmpty(values[0]))
            {
                Debug.Log($"⏭️ Skipping line {i}: insufficient data ({values.Length} columns) - {line.Substring(0, Mathf.Min(30, line.Length))}...");
                continue;
            }

            // Additional validation: first column should look like an item name
            string itemName = values[0].Trim().Replace("\"", "");
            if (itemName.StartsWith("Effect:") || itemName.StartsWith("Ingredients:") || itemName.StartsWith("Form:"))
            {
                Debug.Log($"⏭️ Skipping description line {i}: {itemName}");
                continue;
            }

            Debug.Log($"✅ Processing: {itemName} ({values.Length} columns)");

            var item = ParseItemFromCSV(values, itemType);
            if (item != null)
            {
                items.Add(item);
                Debug.Log($"🎉 Added: {item.itemName} ({item.category})");
            }
        }

        Debug.Log($"📊 Final result: {items.Count} items parsed");
        return items;
    }

    private static ItemCSVData ParseItemFromCSV(string[] values, ItemType itemType)
    {
        try
        {
            var item = new ItemCSVData();

            item.itemName = values[0].Trim().Replace("\"", "");
            item.category = itemType;
            
            // Parse price
            item.price = ParseInt(values[1]);
            
            // Parse nutrition value (HP+ for medicine, Fullness for food)
            item.nutritionValue = ParseFloat(values[2]);
            
            // Parse description (remove quotes)
            if (values.Length > 3)
            {
                item.description = values[3].Trim().Replace("\"", "");
            }

            // Generate unique ID
            item.itemID = GenerateItemID(item.itemName, itemType);

            return item;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing item: {values[0]} - {e.Message}");
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

    private static int ParseInt(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        value = value.Trim();

        if (int.TryParse(value, out int result))
            return result;

        return 0;
    }

    private static string GenerateItemID(string itemName, ItemType itemType)
    {
        string prefix = itemType == ItemType.Food ? "food_" : "med_";
        string cleanName = itemName.ToLower()
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("+", "plus")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("|", "_")
            .Replace("*", "_")
            .Replace("<", "_")
            .Replace(">", "_");
        
        // Remove any remaining invalid characters using regex
        cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"[^a-z0-9_]", "");
        
        // Remove consecutive underscores
        cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"_{2,}", "_");
        
        // Remove leading/trailing underscores
        cleanName = cleanName.Trim('_');
        
        return prefix + cleanName;
    }

    #endregion

    #region ScriptableObject Creation

    private static void UpdateOrCreateItemDataSO(ItemCSVData csvData)
    {
        string folderPath = $"Assets/Game/Data/Items/{csvData.category}";
        
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Data/Items"))
            {
                AssetDatabase.CreateFolder("Assets/Game/Data", "Items");
            }
            AssetDatabase.CreateFolder("Assets/Game/Data/Items", csvData.category.ToString());
        }

        // Create safe file name
        string safeFileName = CreateSafeFileName(csvData.itemName);
        string assetPath = $"{folderPath}/{safeFileName}_Data.asset";
        
        var existingAsset = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);

        ItemDataSO asset = existingAsset ?? ScriptableObject.CreateInstance<ItemDataSO>();

        // Map CSV data to ScriptableObject
        MapCSVToItemData(csvData, asset);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"✅ Created new asset: {assetPath}");
        }
        else
        {
            EditorUtility.SetDirty(existingAsset);
            Debug.Log($"🔄 Updated existing asset: {assetPath}");
        }
    }

    private static void MapCSVToItemData(ItemCSVData csvData, ItemDataSO asset)
    {
        asset.itemID = csvData.itemID;
        asset.itemName = csvData.itemName;
        asset.description = csvData.description;
        asset.category = csvData.category;
        asset.price = csvData.price;
        asset.nutritionValue = csvData.nutritionValue;
        
        // Set default sprites array if null
        if (asset.itemImgs == null || asset.itemImgs.Length == 0)
        {
            asset.itemImgs = new Sprite[1]; // Base sprite slot
        }
    }

    private static string CreateSafeFileName(string itemName)
    {
        // Remove or replace invalid file name characters
        string safeName = itemName
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("+", "plus")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("|", "_")
            .Replace("*", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace("#", "")
            .Replace("%", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("@", "at")
            .Replace("$", "");
        
        // Remove any remaining invalid characters
        safeName = System.Text.RegularExpressions.Regex.Replace(safeName, @"[^a-zA-Z0-9_\-]", "");
        
        // Remove consecutive underscores/dashes
        safeName = System.Text.RegularExpressions.Regex.Replace(safeName, @"[_\-]{2,}", "_");
        
        // Remove leading/trailing underscores/dashes
        safeName = safeName.Trim('_', '-');
        
        // Ensure it's not empty
        if (string.IsNullOrEmpty(safeName))
        {
            safeName = "UnnamedItem";
        }
        
        // Limit length to avoid file system issues
        if (safeName.Length > 50)
        {
            safeName = safeName.Substring(0, 50).TrimEnd('_', '-');
        }
        
        return safeName;
    }

    #endregion

    #region Menu Items

    [MenuItem("Tools/Item Data/🔄 Fetch from Google Sheets", priority = 20)]
    public static void FetchFromGoogleSheets()
    {
        var updater = Object.FindFirstObjectByType<ItemDataUpdater>();
        if (updater == null)
        {
            if (EditorUtility.DisplayDialog("Missing Component",
                "No ItemDataUpdater found in scene. Create one?", "Create", "Cancel"))
            {
                CreateItemDataUpdater();
                updater = Object.FindFirstObjectByType<ItemDataUpdater>();
            }
            else
            {
                return;
            }
        }

        if (string.IsNullOrEmpty(updater.foodSheetURL) && string.IsNullOrEmpty(updater.medicineSheetURL))
        {
            EditorUtility.DisplayDialog("Missing URLs",
                "Please set at least one Google Sheets URL in the ItemDataUpdater component first!", "OK");
            Selection.activeObject = updater;
            return;
        }

        EditorCoroutineUtility.StartCoroutine(FetchAndGenerateCoroutine(updater), updater);
    }

    [MenuItem("Tools/Item Data/📂 Load from Local CSV", priority = 21)]
    public static void LoadFromLocalCSV()
    {
        ShowUpdateWindow();
    }

    [MenuItem("Tools/Item Data/🗑️ Clear All Item Assets", priority = 30)]
    public static void ClearAllItemAssets()
    {
        if (!EditorUtility.DisplayDialog("Clear Item Assets",
            "Delete all existing item data assets?", "Delete", "Cancel"))
            return;

        string[] foodGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Food" });
        string[] medGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Medicine" });

        int totalDeleted = 0;

        foreach (string guid in foodGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            totalDeleted++;
        }

        foreach (string guid in medGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            totalDeleted++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Cleared {totalDeleted} item data assets");
        EditorUtility.DisplayDialog("Cleared", $"Deleted {totalDeleted} assets", "OK");
    }

    private static void CreateItemDataUpdater()
    {
        GameObject go = new GameObject("ItemDataUpdater");
        var updater = go.AddComponent<ItemDataUpdater>();
        updater.updateOnGameStart = false;

        Debug.Log("✅ Created ItemDataUpdater GameObject");
    }

    private static System.Collections.IEnumerator FetchAndGenerateCoroutine(ItemDataUpdater updater)
    {
        Debug.Log("🔄 Fetching item data from Google Sheets...");
        EditorUtility.DisplayProgressBar("Fetching Data", "Starting update...", 0.1f);

        yield return EditorCoroutineUtility.StartCoroutine(updater.UpdateItemData(), updater);

        // Count generated assets
        string[] foodGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Food" });
        string[] medGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Medicine" });
        int totalAssets = foodGuids.Length + medGuids.Length;

        Debug.Log($"✅ Generated {totalAssets} item data assets successfully!");

        EditorUtility.DisplayDialog("Success!",
            $"Successfully generated {totalAssets} item data assets!\n" +
            $"Food: {foodGuids.Length}\nMedicine: {medGuids.Length}", "OK");

        EditorUtility.ClearProgressBar();
    }

    #endregion
}

#region Data Classes

[System.Serializable]
public class ItemCSVData
{
    public string itemID;
    public string itemName;
    public string description;
    public ItemType category;
    public int price;
    public float nutritionValue;
}

#endregion
