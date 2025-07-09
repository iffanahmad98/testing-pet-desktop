using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Threading.Tasks;

[CustomEditor(typeof(ItemDataUpdater))]
public class ItemDataUpdaterEditor : Editor
{
    private bool showAdvancedOptions = false;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ItemDataUpdater updater = (ItemDataUpdater)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Development Testing", EditorStyles.boldLabel);
        
        // Primary Option: Online Fetch
        EditorGUILayout.LabelField("🌐 Online Data (Recommended)", EditorStyles.miniBoldLabel);
        
        bool hasAnyURL = !string.IsNullOrEmpty(updater.foodSheetURL) || !string.IsNullOrEmpty(updater.medicineSheetURL);
        
        if (!hasAnyURL)
        {
            EditorGUILayout.HelpBox("Please set at least one Google Sheet URL first!", MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("🔄 Fetch Latest Data from Google Sheets", GUILayout.Height(35)))
            {
                _ = TestFetchCSVAsync(updater);
            }
            
            // Show which URLs are configured
            if (!string.IsNullOrEmpty(updater.foodSheetURL))
                EditorGUILayout.HelpBox("✅ Food data URL configured", MessageType.Info);
            if (!string.IsNullOrEmpty(updater.medicineSheetURL))
                EditorGUILayout.HelpBox("✅ Medicine data URL configured", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Secondary Option: Local Import
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "📁 Advanced Options", true);
        
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Local File Import", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("📂 Import Food CSV File", GUILayout.Height(25)))
            {
                string path = EditorUtility.OpenFilePanel("Select Food CSV", "", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    string csvContent = System.IO.File.ReadAllText(path);
                    ItemDataGenerator.GenerateFromCSV(csvContent, ItemType.Food);
                    Debug.Log("✅ Food CSV processed from local file!");
                    
                    EditorUtility.DisplayDialog("Success", 
                        "Food data imported from local CSV file!", "OK");
                }
            }
            
            if (GUILayout.Button("📂 Import Medicine CSV File", GUILayout.Height(25)))
            {
                string path = EditorUtility.OpenFilePanel("Select Medicine CSV", "", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    string csvContent = System.IO.File.ReadAllText(path);
                    ItemDataGenerator.GenerateFromCSV(csvContent, ItemType.Medicine);
                    Debug.Log("✅ Medicine CSV processed from local file!");
                    
                    EditorUtility.DisplayDialog("Success", 
                        "Medicine data imported from local CSV file!", "OK");
                }
            }
            
            EditorGUILayout.HelpBox("Use local import for:\n• Testing CSV format changes\n• Working offline\n• Using backup data", MessageType.None);
            
            EditorGUILayout.Space();
            
            // Utility Buttons
            EditorGUILayout.LabelField("Utilities", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("🗂️ Open Item Data Folder"))
            {
                string path = "Assets/Game/Data/Items";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder("Assets/Game/Data", "Items");
                }
                EditorUtility.RevealInFinder(path);
            }
            
            if (GUILayout.Button("📊 Count Generated Assets"))
            {
                string[] foodGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Food" });
                string[] medGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Medicine" });
                int total = foodGuids.Length + medGuids.Length;
                
                EditorUtility.DisplayDialog("Asset Count", 
                    $"Found {total} Item Data assets:\n• Food: {foodGuids.Length}\n• Medicine: {medGuids.Length}", "OK");
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private static async Task TestFetchCSVAsync(ItemDataUpdater updater)
    {
        Debug.Log("🔄 Fetching latest item data from Google Sheets...");
        
        EditorUtility.DisplayProgressBar("Fetching Data", "Starting fetch...", 0.1f);
        
        try
        {
            int successCount = 0;
            int totalTasks = 0;
            
            // Count how many URLs we have
            if (!string.IsNullOrEmpty(updater.foodSheetURL)) totalTasks++;
            if (!string.IsNullOrEmpty(updater.medicineSheetURL)) totalTasks++;
            
            // Fetch Food Data
            if (!string.IsNullOrEmpty(updater.foodSheetURL))
            {
                EditorUtility.DisplayProgressBar("Fetching Data", "Fetching food data...", 0.3f);
                
                using (UnityWebRequest request = UnityWebRequest.Get(updater.foodSheetURL))
                {
                    request.timeout = 15;
                    var operation = request.SendWebRequest();
                    
                    while (!operation.isDone)
                    {
                        await Task.Delay(50);
                    }
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        ItemDataGenerator.GenerateFromCSV(request.downloadHandler.text, ItemType.Food);
                        successCount++;
                        Debug.Log($"✅ Food data fetch successful!");
                    }
                    else
                    {
                        Debug.LogError($"❌ Food data fetch failed: {request.error}");
                    }
                }
            }
            
            // Fetch Medicine Data
            if (!string.IsNullOrEmpty(updater.medicineSheetURL))
            {
                EditorUtility.DisplayProgressBar("Fetching Data", "Fetching medicine data...", 0.7f);
                
                using (UnityWebRequest request = UnityWebRequest.Get(updater.medicineSheetURL))
                {
                    request.timeout = 15;
                    var operation = request.SendWebRequest();
                    
                    while (!operation.isDone)
                    {
                        await Task.Delay(50);
                    }
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        ItemDataGenerator.GenerateFromCSV(request.downloadHandler.text, ItemType.Medicine);
                        successCount++;
                        Debug.Log($"✅ Medicine data fetch successful!");
                    }
                    else
                    {
                        Debug.LogError($"❌ Medicine data fetch failed: {request.error}");
                    }
                }
            }
            
            EditorUtility.DisplayProgressBar("Fetching Data", "Counting assets...", 0.9f);
            
            // Count generated assets
            string[] foodGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Food" });
            string[] medGuids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { "Assets/Game/Data/Items/Medicine" });
            int totalAssets = foodGuids.Length + medGuids.Length;
            
            if (successCount > 0)
            {
                Debug.Log($"✅ Generated {totalAssets} item data assets successfully!");
                
                EditorUtility.DisplayDialog("Success!", 
                    $"Successfully updated item data from Google Sheets!\n\n" +
                    $"Food items: {foodGuids.Length}\n" +
                    $"Medicine items: {medGuids.Length}\n" +
                    $"Total: {totalAssets}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    "Failed to fetch any item data from Google Sheets. Check console for details.", "OK");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
