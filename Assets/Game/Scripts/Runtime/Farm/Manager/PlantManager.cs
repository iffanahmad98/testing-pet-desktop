using UnityEngine;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using MagicalGarden.Inventory;

namespace MagicalGarden.Farm
{
    public class PlantManager : MonoBehaviour
    {
        public static PlantManager Instance;
        public DateTime simulatedNow = DateTime.Now;
        public GameObject plantPrefab;
        public Transform poolPlant;
        private Dictionary<Vector3Int, PlantController> plants = new Dictionary<Vector3Int, PlantController>();
        private int totalHarvest = 0;
        public float updateIntervalSeconds = 60f;
        public event Action OnHarvestChanged;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("Simulated Time")]
        public float timeMultiplier = 60f; // 1 detik real = 1 menit in-game

        private void Update()
        {
            // Tambahkan waktu simulasi setiap frame
            simulatedNow = simulatedNow.AddSeconds(Time.deltaTime * timeMultiplier);
        }
        private void Awake()
        {
            Instance = this;
        }

        public void AddAmountHarvest()
        {
            totalHarvest++;
            OnHarvestChanged?.Invoke();
        }

        public int GetAmountHarvest()
        {
            return totalHarvest;
        }

        public IEnumerable<PlantController> GetAllSeeds()
        {
            return plants.Values;
        }

        public void PlantSeedAt(Vector3Int cellPosition, ItemData itemdata)
        {
            if (plants.ContainsKey(cellPosition))
            {
                Debug.Log("Tile sudah ada tanaman.");
                return;
            }
            if (!InventoryManager.Instance.HasItem(itemdata, 1)) return;
            bool removed = InventoryManager.Instance.RemoveItem(itemdata, 1);
            if (!removed) return;
            var plantObj = Instantiate(plantPrefab, cellPosition, Quaternion.identity);
            plantObj.transform.parent = poolPlant;

            var plant = plantObj.GetComponent<PlantController>();
            plant.seed.lastUpdateTime = simulatedNow;
            plant.seed.plantedTime = simulatedNow;
            plant.seed.cellPosition = cellPosition;
            plant.seed.itemData = itemdata;

            TileManager.Instance.tilemapSeed.SetTile(cellPosition, itemdata.stageTiles[0]);
            plants[cellPosition] = plant;
            if (!InventoryManager.Instance.HasItem(itemdata, 1))
            {
                CursorIconManager.Instance.HideSeedIcon();
            }
        }

        public void PlantWaterAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                plant.seed.Water();
            }
            else
            {
                Debug.LogWarning($"No plant found at {cellPosition} to water.");
            }
        }

        public void PlantFertilizeAt(Vector3Int cellPosition, ItemData itemdata)
        {
            if (plants.TryGetValue(cellPosition, out PlantController plant))
            {
                if (plant.Fertilize != null)
                {
                    Debug.Log("Sudah dipupuk.");
                    return;
                } 
                plant.Fertilize = itemdata;
            }
            else
            {
                Debug.Log("tidak ada tanaman");
                return;
            }
            if (!InventoryManager.Instance.HasItem(itemdata, 1)) return;
            bool removed = InventoryManager.Instance.RemoveItem(itemdata, 1);
            if (!removed) return;

            TileManager.Instance.tilemapFertilizer.SetTile(cellPosition, TileManager.Instance.tileFertilizer);
            if (!InventoryManager.Instance.HasItem(itemdata, 1))
            {
                CursorIconManager.Instance.HideSeedIcon();
            }
        }
        //bisa harvert kalau sudah siap
        public void HarvestAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                if (plant.seed.IsReadyToHarvest())
                {
                    plant.seed.Harvest();
                    plants.Remove(cellPosition);
                }
                else
                {
                    Debug.Log("Seed not ready to be harvested.");
                }
            }
        }
        //plant monster seed jika sekeliling tanamannya sama
        public bool CanPlantMonsterSeedAt(Vector3Int center)
        {
            // Posisi sekeliling (atas, bawah, kiri, kanan)
            // List<Vector3Int> sides = new List<Vector3Int> {
            //     center + Vector3Int.up,
            //     center + Vector3Int.down,
            //     center + Vector3Int.left,
            //     center + Vector3Int.right
            // };

            List<Vector3Int> allNeighbors = new List<Vector3Int>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    allNeighbors.Add(center + new Vector3Int(x, y, 0));
                }
            }

            string referencePlantType = null;

            foreach (var pos in allNeighbors)
            {
                if (!plants.TryGetValue(pos, out var plant) || plant == null)
                    return false;

                string thisPlantType = plant.seed.itemData.itemId; // atau pakai seed.plantType jika ada
                if (referencePlantType == null)
                    referencePlantType = thisPlantType;
                else if (thisPlantType != referencePlantType)
                    return false;
            }

            return true;
        }
        //buang tanaman
        public void RemovePlantAt(Vector3Int cellPosition)
        {
            // if (plantedSeeds.TryGetValue(cellPosition, out var seed))
            // {
            //     Destroy(seed.gameObject);
            //     plantedSeeds.Remove(cellPosition);
            // }
        }

        public Dictionary<Vector3Int, string> GetSaveData()
        {
            var data = new Dictionary<Vector3Int, string>();
            // foreach (var pair in plantedSeeds)
            // {
            //     data[pair.Key] = pair.Value.GetSaveData(); // Misalnya return JSON atau string ID
            // }
            return data;
        }
        //saat pindah scene atau memang butuh untuk pengecekan lagi
        public void RefreshAllPlants()
        {
            foreach (var plant in plants.Values)
            {
                plant.seed.UpdateStage(); // Bisa juga UpdateVisual, UpdateGrowth, dll
            }
        }

        public void CheckMutationAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                // seed.TryMutateBasedOnNeighbors(plantedSeeds);
            }
        }

        public SeedBase GetSeedAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                return plant.seed;
            }
            return null;
        }

        public PlantController GetPlantAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                return plant;
            }
            return null;
        }
        //neaxt idea
        // IsCellOccupied(Vector3Int pos)
        // GetNeighbors(Vector3Int pos)
        // GetSeedTypeAt(Vector3Int pos)
    }
}

