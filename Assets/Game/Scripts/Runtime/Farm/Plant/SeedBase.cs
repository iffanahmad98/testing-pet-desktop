using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Tilemaps;
using MagicalGarden.Inventory;
using MagicalGarden.Manager;
using MagicalGarden.AI;
using DG.Tweening;
// using UnityEditor.PackageManager;
using Spine.Unity;

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
        public bool typeMonster;
        public int stage = 0;
        public float timeInStage = 0f;
        public System.DateTime lastWateredTime;
        public DateTime plantedTime;
        public DateTime lastUpdateTime;
        public PlantStatus status = PlantStatus.Normal;
        public Vector3Int cellPosition;
        public ItemData itemData;
        public Image markHarvest;
        public Image markWater;
        private Tween currentTween;

        [Header ("Debug")]
        public bool debugWatered = false;

        public abstract List<GrowthStage> GetGrowthRequirements();
        public abstract Monster GetMonster();

        public virtual void UpdateGrowth(float deltaHours, float fertilizerBoost = 0)
        {
            if (deltaHours < 0f)
            {
                Debug.LogWarning($"[SeedBase] deltaHours negatif ({deltaHours}) dicegah untuk {seedName}");
                return;
            }

            if (IsReadyToHarvest ()) {
                OffWaterIcon ();
            }
            if (status == PlantStatus.Mati || IsReadyToHarvest())
                return;

            DateTime now = PlantManager.Instance?.simulatedNow ?? DateTime.Now;
            double hoursSinceWatered = (now - lastWateredTime).TotalHours;

            // Hilangkan tile air jika tanaman tidak disiram > 1 jam
            if (hoursSinceWatered >= 1)
            {
                TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
            }

            bool isWatered = lastWateredTime > DateTime.MinValue;
            
            if (!debugWatered) {
                if (!isWatered || hoursSinceWatered < 1)
                {
                    Debug.LogWarning($"⏱️ Tidak bisa tumbuh. Disiram: {isWatered}, Selisih jam: {hoursSinceWatered:F2}");
                    CheckHealth();
                    return;
                }
            }

            
            float boostedHours = deltaHours * (1f + fertilizerBoost / 100f);
            timeInStage += boostedHours;

            var growthRequirements = GetGrowthRequirements();
            if (stage >= growthRequirements.Count)
                return;

            var currentStage = growthRequirements[stage];

            // Debug.LogError($"[{seedName}] Update: +{boostedHours:F2}h (base: {deltaHours:F2}h, boost: {fertilizerBoost}%) | Total timeInStage: {timeInStage:F2}/{currentStage.requiredHours}h");

            // Naik ke tahap berikutnya jika cukup waktu
            if (timeInStage >= currentStage.requiredHours)
            {
                stage++;
                if (stage < growthRequirements.Count)
                {
                    UpdateStage();
                }
                else
                {
                    UpdateStage(); // final stage
                    markHarvest.sprite = itemData.markHarvest;
                    AnimateHarvestIcon();

                    // Notif plant ready to harvest is at index 27
                    MonsterManager.instance.audio.PlayFarmSFX(27);
                    Debug.Log($"[{seedName}] Siap panen! Final stage tercapai di posisi {cellPosition}");
                }
                ResetStageProgress();
            }

            if (IsNeedWater ()) {
                AnimateWaterIcon ();
            } else {
                OffWaterIcon ();
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

            OffWaterIcon (); 
        }

        public virtual bool IsNeedWater ()
        {
        
            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;
            
            // Cegah penyiraman ganda dalam 1 jam
            if ((now - lastWateredTime).TotalHours < 1)
                return false;
            else
                return true;

        }

        public virtual void CheckHealth()
        {
            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;
            DateTime checkFrom = lastWateredTime == default(DateTime) ? plantedTime : lastWateredTime;

            double hoursSince = (now - checkFrom).TotalHours;
            if (!debugWatered) {
                if (hoursSince > 48)
                {
                    status = PlantStatus.Mati;
                    TileManager.Instance.tilemapSeed.SetTile(cellPosition, PlantManager.Instance.stageWilted);
                }
                else if (hoursSince > 24)
                {
                    // notification for farm quest is at index 28
                    MonsterManager.instance.audio.PlayFarmSFX(28);

                    status = PlantStatus.Layu;
                    TileManager.Instance.tilemapSeed.SetTile(cellPosition, PlantManager.Instance.stageWilted);
                }
                else
                { 
                    status = PlantStatus.Normal;
                }
            } else {
                status = PlantStatus.Normal;
            }
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
                case PlantStatus.Normal:
                    tilemap.SetTile(cellPosition, itemData.stageTiles[Mathf.Min(stage, itemData.stageTiles.Count - 1)]);
                    break;
                case PlantStatus.Layu:
                    tilemap.SetTileFlags(cellPosition, TileFlags.None);
                    tilemap.SetColor(cellPosition, Color.yellow); // contoh: layu → kuning
                    break;
                case PlantStatus.Mati:
                    tilemap.SetTileFlags(cellPosition, TileFlags.None);
                    tilemap.SetColor(cellPosition, Color.gray); // contoh: mati → abu-abu
                    break;
            }
        }


        public virtual void Harvest()
        {
            Debug.Log($"{seedName} harvested at {cellPosition}");
            if (typeMonster)
            {
                GatchaMonsterEgg(TileManager.Instance.tilemapSeed.CellToWorld(cellPosition));
                markHarvest.gameObject.SetActive(false);
                PlayerHistoryManager.instance.SetHarvestEggMonsters (1);
            }
            else
            {
                /* (Not Used)
                foreach (var drop in itemData.dropItems)
                {
                    if (UnityEngine.Random.value <= drop.dropChance)
                    {
                        int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
                        // (Not Used)InventoryManager.Instance.AddItem(drop.item, amount);
                        InventoryManager.Instance.AddAssistant (drop.item, amount);
                        // for (int i = 0; i < Mathf.Min(amount, 3); i++)
                        // {
                        //     AnimateDrop(drop.item, TileManager.Instance.tilemapSeed.CellToWorld(cellPosition) + Vector3.one * 0.5f);
                        // }
                    }
                }
                */
                InventoryManager.Instance.AddItemToHarvestTab(itemData, 1);
               // InventoryManager.Instance.AddAssistant (itemData);
                InventoryManager.Instance.RefreshAllInventoryUI();
                markHarvest.gameObject.SetActive(false);
                Clear();
            }
        }

        public void GatchaMonsterEgg(Vector3 cellPosition)
        {
            TileManager.Instance.disableTileSelect = true;
            GameManager.Instance.DisableCameraRig();
            CursorIconManager.Instance.HideSeedIcon();
            UIManager.Instance.ToggleMenuBar();
            PlantManager.Instance.cameraMove.FocusOnTarget(cellPosition, 4f, 1f);
            // Simulasikan evolusi 3 detik
            StartCoroutine(WaitAndZoomOut(cellPosition));
        }

        IEnumerator WaitAndZoomOut(Vector3 _cellPosition)
        {
           // Debug.Log ("Particle 1");
            yield return new WaitForSeconds(2f);
            TileManager.Instance.tilemapSeed.SetTile(cellPosition, null);
            TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
            var monsterEggPrefab = Instantiate(PlantManager.Instance.monsterEggPrefab, _cellPosition + PlantManager.Instance.offsetEgg, Quaternion.identity);
            yield return new WaitForSeconds(1f);
            monsterEggPrefab.GetComponent<EggMonsterController>().vfxShinePrefab.SetActive(true);
            var monsterPrefab = Instantiate(HotelManager.Instance.GetRandomGuestPrefab(), _cellPosition + PlantManager.Instance.offsetEgg, Quaternion.identity);
            monsterPrefab.transform.localScale = Vector3.zero;
            monsterPrefab.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f).SetEase(Ease.OutBack);
            monsterPrefab.GetComponent<PetMonsterHotel>().RunIdle();
            var skeleton = monsterPrefab.GetComponent<SkeletonAnimation>();
            var meshRenderer = skeleton.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Particle Effect"; // Ganti dengan nama layer kamu
            monsterEggPrefab.GetComponent<EggMonsterController>().menu.SetActive(true);
            monsterEggPrefab.GetComponent<EggMonsterController>().monsterGatcha = monsterPrefab;
           // Debug.Log ("Particle 2");
            // yield return new WaitForSeconds(5f);
            // PlantManager.Instance.cameraMove.ResetZoom(0.5f);
        }

        // private void AnimateDrop(ItemData item, Vector3 worldPos)
        // {
        //     var InventoryItemCell = InventoryManager.Instance.inventoryUI.GetSlotForItem(item);

        //     if (InventoryItemCell == null) return;

        //     var prefab = InventoryManager.Instance.dropFlyIcon;
        //     var instance = Instantiate(prefab, worldPos, Quaternion.identity);

        //     var fly = instance.GetComponent<FlyToInventory>();
        //     fly.Init(item.icon, worldPos, InventoryItemCell.GetComponent<RectTransform>());
        // }

        public virtual void Clear()
        {
          //  Debug.Log ("Particle Clear");
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

        private void AnimateHarvestIcon()
        {
            if (markHarvest == null) return;

            currentTween?.Kill();

            markHarvest.color = new Color(1, 1, 1, 0);
            markHarvest.transform.localScale = Vector3.one * 0.8f;
            markHarvest.gameObject.SetActive(true);

            // Animasi bounce terasa (scale naik lebih besar lalu balik ke 1)
            Sequence seq = DOTween.Sequence();
            seq.Append(markHarvest.DOFade(1f, 0.2f));
            seq.Join(markHarvest.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack)); // scale lebih besar dulu
            seq.Append(markHarvest.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));  // kembali ke normal
            currentTween = seq;
        }

        public void AnimateWaterIcon()
        {
            if (IsReadyToHarvest())  {
                OffWaterIcon (); 
                return;
            }
            if (markWater == null) return;
            markWater.gameObject.SetActive(true);
            /*
            currentTween?.Kill();

            markWater.color = new Color(1, 1, 1, 0);
            markWater.transform.localScale = Vector3.one * 0.8f;
            markWater.gameObject.SetActive(true);

            // Animasi bounce terasa (scale naik lebih besar lalu balik ke 1)
            Sequence seq = DOTween.Sequence();
            seq.Append(markWater.DOFade(1f, 0.2f));
            seq.Join(markWater.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack)); // scale lebih besar dulu
            seq.Append(markWater.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));  // kembali ke normal
            currentTween = seq;
            */
        }

        void OffWaterIcon () {
            markWater.gameObject.SetActive(false);
        }
    }
}