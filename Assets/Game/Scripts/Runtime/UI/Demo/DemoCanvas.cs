using UnityEngine;
using UnityEngine.UI;
using System;
using MagicalGarden.Inventory;
using MagicalGarden.Manager;
using UnityEngine.SceneManagement;
public class DemoCanvas : MonoBehaviour
{
    public static DemoCanvas Instance;
    [Header ("Build Settings")]
    [SerializeField] bool isDemo = false;

    [Header("UI")]
    [SerializeField] Image demoPanel;
    [SerializeField] Image resetPanel;
    [SerializeField] Button buyButton;
    [SerializeField] Button resetButton;
    [Header("Data")]
    PlayerConfig playerConfig;
    public bool onClearing = false;
    void Awake () {
        Instance = this;    
    }
    void Start()
    {
        if (isDemo) {
            resetButton.onClick.AddListener (ResetData);
            buyButton.onClick.AddListener (BuyGame);
            playerConfig = SaveSystem.PlayerConfig;
            Invoke("nStart", 0.5f);
        }
    }

    void nStart()
    {
        playerConfig.SaveFirstLoginTime ();
        CheckExpiredDemoDay();
    }

    void CheckExpiredDemoDay()
    {
        DateTime firstLogin = playerConfig.firstLoginTime;

        double daysPassed = (DateTime.Now - firstLogin).TotalDays;

        if (daysPassed >= 14)
        {
            onClearing = true;
            demoPanel.gameObject.SetActive(true);
        }
        else
        {
            demoPanel.gameObject.SetActive(false);
        }
    }

    void ResetData () {
        
        demoPanel.gameObject.SetActive(false);
        ResetPlantData ();
        // playerConfig.ResetFirstLoginTime ();
        
        ResetPlayerConfig ();
        onClearing = false;
        resetPanel.gameObject.SetActive (true);
      //  LoadSceneRepeat ();
    }

    void BuyGame () {
        Application.OpenURL("https://store.steampowered.com/app/4146150/Petal_Pals/");
    }

    public bool IsDemo () {
        return isDemo;
    }

    void LoadSceneRepeat () {
        SceneManager.LoadScene ("Pet");
    }
    #region SaveSystem
    void ResetPlayerConfig () {
       SaveSystem.DeleteAllSaveData (); 
       SaveSystem.ResetSaveData ();
       SaveSystem.CreateNewPlayerConfig ();
       SaveSystem.LoadPlayerConfig();
       SaveSystem.LoadUiConfig();
    }
    #endregion
    #region Plant
    void ResetPlantData () {
        string path = Application.persistentDataPath + "/plants.json";

        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

       // plants.Clear();
       // farmAreaIdsPurchased.Clear();

        SaveToJsonPlant();
    }

    public void SaveToJsonPlant()
        {
           // var dataList = GetSaveDataList();
            var wrapper = new PlantSaveWrapper
            {
                data = new (),
                farmAreaIds = new ()
            };

            string json = JsonUtility.ToJson(wrapper, true);

            string path = Application.persistentDataPath + "/plants.json";

        #if UNITY_EDITOR
            // Editor : plaintext
            System.IO.File.WriteAllText(path, json);
        #else
            // Build : encrypted
            string encrypted = CryptoHelper.Encrypt(json);
            System.IO.File.WriteAllText(path, encrypted);
        #endif

            Debug.Log("Tanaman disimpan ke file.");
        }
    #endregion
}