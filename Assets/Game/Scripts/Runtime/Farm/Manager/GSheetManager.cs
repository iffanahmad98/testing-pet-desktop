using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;
using MagicalGarden.Inventory;

namespace MagicalGarden.Manager
{
    public class GSheetManager : MonoBehaviour
    {
        public static GSheetManager Instance;

        [Header("Google Sheet Settings")]
        [Tooltip("URL dari Web App Google Script")]
        public string sheetUrl = "";
        public string sheetMonsterSeedUrl = "";

        // [HideInInspector]
        public List<SheetData> itemList = new();
        public List<SheetMonsterSeedData> itemMonsterSeedList = new();

        public Action OnDataLoaded;

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

        private void Start()
        {
            ResfreshData();
        }

        public void ResfreshData()
        {
            StartCoroutine(FetchSheetData());
            StartCoroutine(FetchMonsterSeedData());
        }

        IEnumerator FetchSheetData()
        {
            UnityWebRequest www = UnityWebRequest.Get(sheetUrl);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Berhasil ambil data dari GSheet");
                string rawJson = www.downloadHandler.text;
                Debug.LogError(rawJson);
                // itemList = JsonConvert.DeserializeObject<List<SheetData>>(rawJson);
                // Debug.Log($"📦 Jumlah item: {itemList.Count}");
                // OnDataLoaded?.Invoke();
                try
                {
                    itemList = JsonConvert.DeserializeObject<List<SheetData>>(rawJson);
                    Debug.Log($"📦 Jumlah item: {itemList.Count}");
                    OnDataLoaded?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("❌ Gagal parsing JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Gagal ambil data dari GSheet: " + www.error);
            }
        }
        IEnumerator FetchMonsterSeedData()
        {
            UnityWebRequest www = UnityWebRequest.Get(sheetMonsterSeedUrl);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Berhasil ambil data monster seed dari GSheet");
                string rawJson = www.downloadHandler.text;
                Debug.LogError(rawJson);
                try
                {
                    itemMonsterSeedList = JsonConvert.DeserializeObject<List<SheetMonsterSeedData>>(rawJson);
                    Debug.Log($"👾 Jumlah monster seed: {itemMonsterSeedList.Count}");
                }
                catch (Exception e)
                {
                    Debug.LogError("❌ Gagal parsing monster seed JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Gagal ambil data monster seed dari GSheet: " + www.error);
            }
        }

    }
    [System.Serializable]
    public class SeedListWrapper
    {
        public List<SheetData> items;
    }
    [System.Serializable]

    public class SheetData
    {
        [JsonProperty("Farming")]
        public string farmingType;
        [JsonProperty("No.")]
        public string number;
        [JsonProperty("List")]
        public string seedName;
        [JsonProperty("Seed Price (gold)")]
        public string seedPrice;
        [JsonProperty("Seed Quantity")]
        public string seedQuantity;
        [JsonProperty("Durasi Siram (jam)")]
        public string wateringInterval;
        [JsonProperty("Durasi Tumbuh\n(Stage 1-2-3)(jam)")]
        public string growDurationStages; // Tetap string jika ingin parsing manual "1,2,3"
        [JsonProperty("Total Durasi Tumbuh (jam)")]
        public string totalGrowTime;
        [JsonProperty("Waktu Layu (hari)")]
        public string wiltTimeDays;
        [JsonProperty("Waktu Mati\n(hari) ")]
        public string deadTimeDays;
        [JsonProperty("Information")]
        public string description;
    }

    [System.Serializable]
    public class SheetMonsterSeedData
    {
        [JsonProperty("Name")]
        public string name;

        [JsonProperty("Type")]
        public string type;

        [JsonProperty("Chance")]
        public string changeMonster;

        [JsonProperty("Price")]
        public string price;
    }

}