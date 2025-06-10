using UnityEngine;
using System;
using MagicalGarden.Inventory;

namespace MagicalGarden.Farm
{
    public class PlantDebugController : MonoBehaviour
    {

        public float simulateHour = 1f;
        public float simulateDay = 24f;

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

            PlantManager.Instance.simulatedNow = PlantManager.Instance.simulatedNow.AddHours(hours);

            foreach (var plant in PlantManager.Instance.GetAllSeeds())
            {
                plant.Update(hours);
                plant.lastUpdateTime = PlantManager.Instance.simulatedNow;
            }
        }
        void Start()
        {
            var pumpkinSeed = Resources.Load<ItemData>("Items/PumpkinSeed");
            InventoryManager.Instance.AddItem(pumpkinSeed, 8);

            var tomatoSeed = Resources.Load<ItemData>("Items/TomatoSeed");
            InventoryManager.Instance.AddItem(tomatoSeed, 8);

            var wheatSeed = Resources.Load<ItemData>("Items/MonsterSeed");
            InventoryManager.Instance.AddItem(wheatSeed, 2);
        }
    }
}