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
        private Dictionary<Vector3Int, SeedBase> plantedSeeds = new Dictionary<Vector3Int, SeedBase>();
        public float updateInterval = 60f;
        private float timer = 0f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            Instance = this;
        }

        public IEnumerable<SeedBase> GetAllSeeds()
        {
            return plantedSeeds.Values;
        }

        public void PlantSeedAt(Vector3Int cellPosition, ItemData itemdata)
        {
            var plantObj = Instantiate(plantPrefab, cellPosition, Quaternion.identity);
            plantObj.transform.parent = poolPlant;
            var seed = plantObj.GetComponent<SeedBase>();
            seed.lastUpdateTime = simulatedNow;
            seed.plantedTime = simulatedNow;
            seed.cellPosition = cellPosition;
            seed.itemData = itemdata;
            TileManager.Instance.tilemapSeed.SetTile(cellPosition, itemdata.stageTiles[0]);
            InventoryManager.Instance.RemoveItem(itemdata, 1);
            plantedSeeds[cellPosition] = seed;
        }

        public void PlantWaterAt(Vector3Int cellPosition)
        {
            if (plantedSeeds.TryGetValue(cellPosition, out var seed))
            {
                seed.Water();
            }
            else
            {
                Debug.LogWarning($"No plant found at {cellPosition} to water.");
            }
        }

        public void PlantFertilizeAt(Vector3Int cellPosition)
        {

        }
        //bisa harvert kalau sudah siap
        public void HarvestAt(Vector3Int cellPosition)
        {
            if (plantedSeeds.TryGetValue(cellPosition, out var seed))
            {
                if (seed.IsReadyToHarvest())
                {
                    seed.Harvest();
                    plantedSeeds.Remove(cellPosition);
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
                if (!plantedSeeds.TryGetValue(pos, out var seed) || seed == null)
                    return false;

                 string thisPlantType = seed.itemData.itemId; // atau pakai seed.plantType jika ada
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
            foreach (var seed in plantedSeeds.Values)
            {
                seed.UpdateStage(); // Bisa juga UpdateVisual, UpdateGrowth, dll
            }
        }

        public void CheckMutationAt(Vector3Int cellPosition)
        {
            if (plantedSeeds.TryGetValue(cellPosition, out var seed))
            {
                // seed.TryMutateBasedOnNeighbors(plantedSeeds);
            }
        }
        //neaxt idea
        // IsCellOccupied(Vector3Int pos)
        // GetSeedAt(Vector3Int pos)
        // GetNeighbors(Vector3Int pos)
        // GetSeedTypeAt(Vector3Int pos)
    }
}

