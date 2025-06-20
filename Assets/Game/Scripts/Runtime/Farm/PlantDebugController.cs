using UnityEngine;
using System;
using MagicalGarden.Inventory;
using TMPro;
using MagicalGarden.Manager;

namespace MagicalGarden.Farm
{
    public class PlantDebugController : MonoBehaviour
    {

        public float simulateHour = 1f;
        public float simulateDay = 24f;
        public int addDays = 0;
        // public TextMeshProUGUI timeText;
        private void Awake() {
            DebugSetLastDateToYesterday();
        }

        private void Update()
        {
            if (PlantManager.Instance == null) return;

            DateTime current = PlantManager.Instance.simulatedNow;
            // timeText.text = $"🕒 {current:dd MMM yyyy - HH:mm:ss}";
        }

        public void SimulateNextHour()
        {
            SimulateTime(simulateHour);
        }

        public void SimulateNextDay()
        {
            SimulateTime(simulateDay);
        }

        private void SimulateTime(float hours)
        {
            if (PlantManager.Instance == null) return;



            foreach (var plant in PlantManager.Instance.GetAllSeeds())
            {
                float boost = plant.Fertilize?.boost ?? 0;
                plant.seed.Update(hours, boost);
                // plant.seed.lastUpdateTime = PlantManager.Instance.simulatedNow;
            }

            var oldTime = PlantManager.Instance.simulatedNow;
            PlantManager.Instance.simulatedNow = oldTime.AddHours(hours);
        }
        void Start()
        {
            var pumpkinSeed = Resources.Load<ItemData>("Items/Seeds/PumpkinSeed");
            var tomatoSeed = Resources.Load<ItemData>("Items/Seeds/TomatoSeed");
            var wheatSeed = Resources.Load<ItemData>("Items/Seeds/MonsterSeed");
            var normalfertilizer = Resources.Load<ItemData>("Items/Fertilizer/Normal Fertilizer");
            var manaNectar = Resources.Load<ItemData>("Items/Fertilizer/Mana Nectar");
            var moonlightPollen = Resources.Load<ItemData>("Items/Fertilizer/Moonlight Pollen");
            var spiritsap = Resources.Load<ItemData>("Items/Fertilizer/Spirit Sap");
            var pupNormal = Resources.Load<ItemData>("Items/Tools/Pup Normal");
            var pupRare = Resources.Load<ItemData>("Items/Tools/Pup Rare");
            InventoryManager.Instance.AddItem(pumpkinSeed, 8);
            InventoryManager.Instance.AddItem(tomatoSeed, 8);
            InventoryManager.Instance.AddItem(wheatSeed, 2);
            InventoryManager.Instance.AddItem(normalfertilizer, 10);
            InventoryManager.Instance.AddItem(manaNectar, 10);
            InventoryManager.Instance.AddItem(moonlightPollen, 10);
            InventoryManager.Instance.AddItem(spiritsap, 10);
            InventoryManager.Instance.AddItem(pupNormal, 10);
            InventoryManager.Instance.AddItem(pupRare, 10);
            CoinManager.Instance.AddCoins(1000);
            InventoryManager.Instance.inventoryUI.RefreshUI();
            // DebugPlantReadyHarvest(new Vector2Int(0, 0), tomatoSeed);

        }
        [ContextMenu("Debug: Set Last Date To Yesterday")]
        public void DebugSetLastDateToYesterday()
        {
            TimeManager.Instance.currentTime.Date.AddDays(addDays);
            HotelManager.Instance.lastGeneratedDate = TimeManager.Instance.currentTime;
            HotelManager.Instance.SaveLastDate();
            Debug.Log("📅 Simulasi hari baru. Guest request akan dibuat ulang saat start.");
        }
        
        // private void DebugPlantReadyHarvest(Vector2Int cellPosition, ItemData seedData)
        // {
        //     if (PlantManager.Instance == null || seedData == null) return;

        //     PlantManager.Instance.PlantSeedAt(new Vector3Int(cellPosition.x, cellPosition.y, 0), seedData);
        //     if (plant != null)
        //     {
        //         PlantManager.Instance.ForceToHarvestStage();
        //         plant.lastUpdateTime = PlantManager.Instance.simulatedNow;
        //         Debug.Log($"🌾 Tanaman siap panen ditanam di {cellPosition}");
        //     }
        // }
    }
}