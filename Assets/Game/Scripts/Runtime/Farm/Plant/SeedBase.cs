using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Tilemaps;
using MagicalGarden.Inventory;
using Unity.Mathematics;

namespace MagicalGarden.Farm
{
    public enum PlantStatus {
        Normal,
        Layu,
        Mati
    }
    [System.Serializable]
    public class GrowthStage {
        public int requiredHours;
        public int requiredWater;
    }
    public abstract class SeedBase : MonoBehaviour
    {
        public string seedName;
        public int stage = 0;
        public float timeInStage = 0f;
        public System.DateTime lastWateredTime;
        public DateTime plantedTime;
        public DateTime lastUpdateTime;
        public PlantStatus status = PlantStatus.Normal;
        public Vector3Int cellPosition;
        public ItemData itemData;

        public abstract List<GrowthStage> GetGrowthRequirements();
        public abstract Monster GetMonster();

        public virtual void Update(float deltaHours)
        {
            if (status == PlantStatus.Mati || IsReadyToHarvest())
                return;
            if (deltaHours < 0f)
            {
                Debug.LogWarning($"[SeedBase] Negative deltaHours ({deltaHours}) prevented for seed {seedName}");
                return;
            }

            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;

            double hoursSinceWatered = (now - lastWateredTime).TotalHours;
            // Jika lebih dari 1 jam, hilangkan tile water
            if (hoursSinceWatered >= 1)
            {
                TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
            }

            bool WateringCurrently = hoursSinceWatered <= 1;

            if (!WateringCurrently)
            {
                CheckHealth();
                return;
            }

            timeInStage += deltaHours;
            var growthRequirements = GetGrowthRequirements();
            if (stage >= growthRequirements.Count)
                return;

            var currentStage = growthRequirements[stage];
            if (timeInStage >= currentStage.requiredHours)
            {
                stage++;
                if (stage < growthRequirements.Count)
                {
                    UpdateStage();
                    ResetStageProgress();
                }
                else
                {
                    UpdateStage(); // final stage
                }
            }
        }
        public void UpdateStage()
        {
            if (stage < itemData.stageTiles.Count)
            {
                TileManager.Instance.tilemapSeed.SetTile(cellPosition, itemData.stageTiles[stage]);
            }
            else
            {
                Debug.LogWarning($"No tile assigned for stage {stage} of seed {seedName}");
            }
        }

        public virtual void Water()
        {
            if (status == PlantStatus.Mati)
                return;

            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;

            // Cegah penyiraman ganda dalam 1 jam
            if ((now - lastWateredTime).TotalHours < 1)
                return;

            lastWateredTime = now;

            if (status == PlantStatus.Layu)
            {
                status = PlantStatus.Normal;
            }
        }

        public virtual void CheckHealth()
        {
            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;
            DateTime checkFrom = lastWateredTime == default(DateTime) ? plantedTime : lastWateredTime;

            double hoursSince = (now - checkFrom).TotalHours;

            if (hoursSince > 48)
                status = PlantStatus.Mati;
            else if (hoursSince > 24)
                status = PlantStatus.Layu;
            else
                status = PlantStatus.Normal;
        }

        public void ForceToHarvestStage()
        {
            stage = GetGrowthRequirements().Count - 1;
            // UpdateVisual();
        }

        public virtual bool IsReadyToHarvest()
        {
            return stage >= GetGrowthRequirements().Count;
        }

        public virtual void ResetStageProgress()
        {
            timeInStage = 0f;
            TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
            status = PlantStatus.Normal;

            // Jangan set lastWateredTime ke now di reset, biarkan default dulu
            lastWateredTime = default(DateTime);
        }

        public virtual float GetTimeUntilNextStage()
        {
            var growthRequirements = GetGrowthRequirements();
            if (stage >= growthRequirements.Count) return 0f;
            return growthRequirements[stage].requiredHours - timeInStage;
        }

        public virtual void UpdateVisualBasedOnStatus()
        {
            var tilemap = TileManager.Instance.tilemapSeed;

            switch (status)
            {
                // case PlantStatus.Normal:
                //     tilemap.SetTile(cellPosition, stageTiles[Mathf.Min(stage, stageTiles.Count - 1)]);
                //     break;
                // case PlantStatus.Layu:
                //     tilemap.SetTileFlags(cellPosition, TileFlags.None);
                //     tilemap.SetColor(cellPosition, Color.yellow); // contoh: layu → kuning
                //     break;
                // case PlantStatus.Mati:
                //     tilemap.SetTileFlags(cellPosition, TileFlags.None);
                //     tilemap.SetColor(cellPosition, Color.gray); // contoh: mati → abu-abu
                //     break;
            }
        }
        

        public virtual void Harvest()
        {
            Debug.Log($"{seedName} harvested at {cellPosition}");

            foreach (var drop in itemData.dropItems)
            {
                if (UnityEngine.Random.value <= drop.dropChance)
                {
                    int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
                    InventoryManager.Instance.AddItem(drop.item, amount);
                    // for (int i = 0; i < Mathf.Min(amount, 3); i++)
                    // {
                    //     AnimateDrop(drop.item, TileManager.Instance.tilemapSeed.CellToWorld(cellPosition) + Vector3.one * 0.5f);
                    // }
                }
            }

            InventoryManager.Instance.inventoryUI.RefreshUI();
            Clear();
        }

        private void AnimateDrop(ItemData item, Vector3 worldPos)
        {
            var inventorySlot = InventoryManager.Instance.inventoryUI.GetSlotForItem(item);

            if (inventorySlot == null) return;

            var prefab = InventoryManager.Instance.dropFlyIcon;
            var instance = Instantiate(prefab, worldPos, Quaternion.identity);

            var fly = instance.GetComponent<FlyToInventory>();
            fly.Init(item.icon, worldPos, inventorySlot.GetComponent<RectTransform>());
        }

        public virtual void Clear()
        {
            TileManager.Instance.tilemapSeed.SetTile(cellPosition, null);
            TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
            Destroy(this.gameObject);
        }

        // public virtual SeedData CloneSeedData()
        // {
        //     return new SeedData
        //     {
        //         seedName = this.seedName,
        //         stage = this.stage,
        //         timeInStage = this.timeInStage,
        //         status = this.status,
        //         plantedTime = this.plantedTime,
        //         lastWateredTime = this.lastWateredTime,
        //         cellPosition = this.cellPosition
        //     };
        // }
        

        public virtual void ApplyWeatherEffect() { }
    }
}