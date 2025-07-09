using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class MonsterDataUpdater : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Google Sheets")]
    [TextArea]
    public string googleSheetURL;
    
    [Header("Update Settings")]
    public bool updateOnGameStart = true;
    public KeyCode manualUpdateKey = KeyCode.F5;
    
    #endregion
    
    #region Events - For Future UI Integration
    
    public static event Action OnUpdateStarted;
    public static event Action<string> OnUpdateProgress; // For progress messages
    public static event Action OnUpdateCompleted;
    public static event Action<string> OnUpdateFailed;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        #if UNITY_EDITOR
        if (updateOnGameStart)
        {
            StartCoroutine(UpdateMonsterData());
        }
        #else
        Debug.Log("Monster data updates disabled in builds - using pre-generated data");
        #endif
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(manualUpdateKey))
        {
            StartCoroutine(UpdateMonsterData());
        }
    }
    
    #endregion
    
    #region Data Fetching
    
    public IEnumerator UpdateMonsterData()
    {
        #if UNITY_EDITOR
        OnUpdateStarted?.Invoke();
        OnUpdateProgress?.Invoke("Connecting to server...");
        
        using (UnityWebRequest request = UnityWebRequest.Get(googleSheetURL))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                OnUpdateProgress?.Invoke("Processing monster data...");
                MonsterDataGenerator.GenerateFromCSV(request.downloadHandler.text);
                OnUpdateCompleted?.Invoke();
            }
            else
            {
                string errorMsg = $"Failed to fetch monster data: {request.error}";
                Debug.LogError(errorMsg);
                OnUpdateFailed?.Invoke(errorMsg);
            }
        }
        #else
        Debug.LogWarning("CSV updates not available in builds");
        yield break;
        #endif
    }
    
    #endregion
}