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

        // [HideInInspector]
        public List<SheetData> itemList = new();

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
        }

        IEnumerator FetchSheetData()
        {
            UnityWebRequest www = UnityWebRequest.Get(sheetUrl);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Berhasil ambil data dari GSheet");
                string rawJson = www.downloadHandler.text;
                
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


    }
    [System.Serializable]
    public class SeedListWrapper
    {
        public List<SheetData> items;
    }


    [System.Serializable]
    public class SheetData
    {
        [JsonProperty("List")]
        public string seedName;
        [JsonProperty("Seed Price")]
        public string seedPrice;

        [JsonProperty("Durasi Siram")]
        public string wateringInterval;

        [JsonProperty("Durasi Tumbuh\n(Stage 1-2-3)(jam)")]
        public string growDurationStages;

        [JsonProperty("Total Durasi Tumbuh (jam)")]
        public int totalGrowTime;

        [JsonProperty("Waktu Layu (Jam)")]
        public int wiltTime;

        [JsonProperty("Waktu Mati\n(tanpa disiram)")]
        public int deadTime;

        [JsonProperty("Information")]
        public string description;
    }

}