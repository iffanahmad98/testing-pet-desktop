#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

[CustomEditor(typeof(MonsterDataUpdater))]
public class MonsterDataUpdaterEditor : Editor
{
    private bool showAdvancedOptions = false;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MonsterDataUpdater updater = (MonsterDataUpdater)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Development Testing", EditorStyles.boldLabel);
        
        // Primary Option: Online Fetch
        EditorGUILayout.LabelField("🌐 Online Data (Recommended)", EditorStyles.miniBoldLabel);
        
        if (string.IsNullOrEmpty(updater.googleSheetURL))
        {
            EditorGUILayout.HelpBox("Please set Google Sheet URL first!", MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("🔄 Fetch Latest Data from Google Sheets", GUILayout.Height(35)))
            {
                _ = TestFetchCSVAsync(updater);
            }
            
            EditorGUILayout.HelpBox("Use this to get the most recent data from your Google Sheet.", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Secondary Option: Local Import
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "📁 Advanced Options", true);
        
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Local File Import", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("📂 Import from Local CSV File", GUILayout.Height(25)))
            {
                string path = EditorUtility.OpenFilePanel("Select Monster CSV", "", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    string csvContent = System.IO.File.ReadAllText(path);
                    MonsterDataGenerator.GenerateFromCSV(csvContent);
                    Debug.Log("✅ CSV processed from local file!");
                    
                    EditorUtility.DisplayDialog("Success", 
                        "Monster data imported from local CSV file!", "OK");
                }
            }
            
            EditorGUILayout.HelpBox("Use local import for:\n• Testing CSV format changes\n• Working offline\n• Using backup data", MessageType.None);
            
            EditorGUILayout.Space();
            
            // Utility Buttons
            EditorGUILayout.LabelField("Utilities", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("🗂️ Open Monster Data Folder"))
            {
                string path = "Assets/Game/Data/Monsters";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder("Assets/Game/Data", "Monsters");
                }
                EditorUtility.RevealInFinder(path);
            }
            
            if (GUILayout.Button("📊 Count Generated Assets"))
            {
                string[] guids = AssetDatabase.FindAssets("t:MonsterDataSO", new[] { "Assets/Game/Data/Monsters" });
                EditorUtility.DisplayDialog("Asset Count", 
                    $"Found {guids.Length} Monster Data assets in the project.", "OK");
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private static async Task TestFetchCSVAsync(MonsterDataUpdater updater)
    {
        Debug.Log("🔄 Fetching latest monster data from Google Sheets...");
        
        // Show progress bar
        EditorUtility.DisplayProgressBar("Fetching Data", "Connecting to Google Sheets...", 0.3f);
        
        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(updater.googleSheetURL))
            {
                request.timeout = 15;
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    EditorUtility.DisplayProgressBar("Fetching Data", 
                        $"Downloading... {(operation.progress * 100):F0}%", operation.progress);
                    await Task.Delay(50);
                }
                
                EditorUtility.DisplayProgressBar("Fetching Data", "Processing data...", 0.8f);
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ CSV fetch successful! ({request.downloadHandler.text.Length} characters)");
                    
                    // Generate ScriptableObjects
                    MonsterDataGenerator.GenerateFromCSV(request.downloadHandler.text);
                    
                    // Count generated assets
                    string[] guids = AssetDatabase.FindAssets("t:MonsterDataSO", new[] { "Assets/Game/Data/Monsters" });
                    
                    Debug.Log($"✅ Generated {guids.Length} monster data assets successfully!");
                    
                    EditorUtility.DisplayDialog("Success!", 
                        $"Successfully updated {guids.Length} monster data assets from Google Sheets!", "OK");
                }
                else
                {
                    Debug.LogError($"❌ CSV fetch failed: {request.error}");
                    EditorUtility.DisplayDialog("Error", 
                        $"Failed to fetch data from Google Sheets:\n{request.error}", "OK");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
#endif