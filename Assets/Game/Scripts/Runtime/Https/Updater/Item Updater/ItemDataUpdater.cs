using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ItemDataUpdater : MonoBehaviour
{
    #region Inspector Fields

    [Header("Google Sheets URLs")]
    [TextArea]
    public string foodSheetURL;
    [TextArea]
    public string medicineSheetURL;
    [TextArea]
    public string monsterSheetURL; // Single URL for both common and uncommon monsters
    
    [Header("Update Settings")]
    public bool updateOnGameStart = true;
    public KeyCode manualUpdateKey = KeyCode.F6;
    
    #endregion
    
    #region Events - For Future UI Integration
    
    public static event Action OnUpdateStarted;
    public static event Action<string> OnUpdateProgress;
    public static event Action OnUpdateCompleted;
    public static event Action<string> OnUpdateFailed;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        #if UNITY_EDITOR
        if (updateOnGameStart)
        {
            StartCoroutine(UpdateItemData());
        }
        #else
        Debug.Log("Item data updates disabled in builds - using pre-generated data");
        #endif
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(manualUpdateKey))
        {
            StartCoroutine(UpdateItemData());
        }
    }
    
    #endregion
    
    #region Data Fetching
    public IEnumerator UpdateItemData()
    {
        #if UNITY_EDITOR
        OnUpdateStarted?.Invoke();
        
        // Fetch Food Data
        if (!string.IsNullOrEmpty(foodSheetURL))
        {
            OnUpdateProgress?.Invoke("Fetching food data...");
            yield return StartCoroutine(FetchDataFromURL(foodSheetURL, ItemType.Food));
        }
        
        // Fetch Medicine Data
        if (!string.IsNullOrEmpty(medicineSheetURL))
        {
            OnUpdateProgress?.Invoke("Fetching medicine data...");
            yield return StartCoroutine(FetchDataFromURL(medicineSheetURL, ItemType.Medicine));
        }
        
        // Fetch Monster Data (both common and uncommon)
        if (!string.IsNullOrEmpty(monsterSheetURL))
        {
            OnUpdateProgress?.Invoke("Fetching monster data...");
            yield return StartCoroutine(FetchDataFromURL(monsterSheetURL, ItemType.CommonMonster)); // Will auto-detect type based on CSV data
        }
        
        OnUpdateCompleted?.Invoke();
        #else
        Debug.LogWarning("CSV updates not available in builds");
        yield break;
        #endif
    }
    
    private IEnumerator FetchDataFromURL(string url, ItemType itemType)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                OnUpdateProgress?.Invoke($"Processing {itemType} data...");
                //ItemDataGenerator.GenerateFromCSV(request.downloadHandler.text, itemType); // Uncomment this line
            }
            else
            {
                string errorMsg = $"Failed to fetch {itemType} data: {request.error}";
                Debug.LogError(errorMsg);
                OnUpdateFailed?.Invoke(errorMsg);
            }
        }
    }
    
    #endregion
}
