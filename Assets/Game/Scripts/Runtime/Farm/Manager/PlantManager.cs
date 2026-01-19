using UnityEngine;
using System.Collections.Generic;
using System;
using MagicalGarden.Inventory;
using MagicalGarden.Manager;
using System.Linq;
using UnityEngine.Tilemaps;
using DG.Tweening;
using Unity.Mathematics;
// using System.Numerics;

namespace MagicalGarden.Farm
{
    public class PlantManager : MonoBehaviour
    {
        public static PlantManager Instance;
        [Header("Plant Setting")]
        public GameObject plantPrefab;
        public Transform poolPlant;
        public TileBase stageWilted;
        private Dictionary<Vector3Int, PlantController> plants = new Dictionary<Vector3Int, PlantController>();
        private int totalHarvest = 0;
        public event Action OnHarvestChanged;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("Simulated Time Plant")]
        public float updateIntervalSeconds = 60f;
        public float timeMultiplier = 60f; // 1 detik real = 1 menit in-game
        public DateTime simulatedNow = DateTime.Now;

        [Header("Monster Egg Harvest")]
        public GameObject monsterEggPrefab;
        public Vector3 offsetEgg;
        public CameraDragMove cameraMove;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SaveToJson();
                Debug.Log("⌨️ [Save] Tombol S ditekan — Tanaman disimpan ke file JSON.");
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                LoadFromJson();
                Debug.Log("⌨️ [Load] Tombol L ditekan — Data tanaman dimuat dari file JSON.");
            }
            // Tambahkan waktu simulasi setiap frame
            simulatedNow = simulatedNow.AddSeconds(Time.deltaTime * timeMultiplier);
        }
        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            LoadAllItems();
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

        public void PlantSeedAt(Vector3Int cellPosition, ItemData itemdata, bool monsterSeed = false)
        {
            if (plants.ContainsKey(cellPosition))
            {
                Debug.Log("Tile sudah ada tanaman.");
                return;
            }

            if (!InventoryManager.Instance.HasItem(itemdata, 1)) return;

            bool removed = InventoryManager.Instance.RemoveItem(itemdata, 1);
            if (!removed) return;

            Vector3 worldPos = TileManager.Instance.tilemapSeed.CellToWorld(cellPosition) + new Vector3(0f, 0.5f, 0);
            var plantObj = Instantiate(plantPrefab, worldPos, Quaternion.identity);
            plantObj.transform.parent = poolPlant;
            plantObj.transform.localScale = Vector3.one; // Pastikan scale awal normal

            var plant = plantObj.GetComponent<PlantController>();
            plant.seed.lastUpdateTime = simulatedNow;
            plant.seed.plantedTime = simulatedNow;
            plant.seed.cellPosition = cellPosition;
            plant.seed.itemData = itemdata;
            plant.seed.typeMonster = monsterSeed;
            plant.seed.seedName = itemdata.displayName;

            
            plants[cellPosition] = plant;

            if (!InventoryManager.Instance.HasItem(itemdata, 1))
            {
                CursorIconManager.Instance.HideSeedIcon();
            }

            // === 🎉 DOTween Bounce Effect ===
            Vector3 worldPosCenter = TileManager.Instance.tilemapSeed.GetCellCenterWorld(cellPosition);

            GameObject bounceVisual = new GameObject("TileBounce");
            bounceVisual.transform.position = worldPosCenter;
            bounceVisual.transform.localScale = Vector3.zero;

            var spriteRenderer = bounceVisual.AddComponent<SpriteRenderer>();
            TileBase tile = itemdata.stageTiles[0];

            if (tile is Tile tileData)
            {
                spriteRenderer.sprite = tileData.sprite;
            }
            else
            {
                Debug.LogWarning("Tile tidak bisa di-cast ke Tile, sprite tidak dapat diambil.");
            }
            spriteRenderer.sortingLayerName = "World";
            spriteRenderer.sortingOrder = 10;

            // Ambil posisi tile di world space
            Vector3 tileWorldPos = TileManager.Instance.tilemapSeed.CellToWorld(cellPosition);

            // Sesuaikan posisi visual agar berada di tengah tile (0.5f, 0.5f) + offset ke atas
            float tileHeightOffset = 0.8f; // Atur sesuai tinggi tile kamu
            Vector3 spawnPos = tileWorldPos + new Vector3(0, 0.5f + tileHeightOffset, 0f);

            // Atur posisi awal visual
            bounceVisual.transform.position = spawnPos;

            // Tween: scale up (bounce), then scale down, then destroy
            bounceVisual.transform.DOScale(3f, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                bounceVisual.transform.DOScale(2.5f, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    GameObject.Destroy(bounceVisual);
                    TileManager.Instance.tilemapSeed.SetTile(cellPosition, itemdata.stageTiles[0]);
                });
            });
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
        public void RemovePlantAt(Vector3Int cellPosition)
        {
            if (plants.TryGetValue(cellPosition, out var plant))
            {
                // Hapus tile di tilemap
                TileManager.Instance.tilemapSeed.SetTile(cellPosition, null);
                TileManager.Instance.tilemapWater.SetTile(cellPosition, null);
                TileManager.Instance.tilemapFertilizer.SetTile(cellPosition, null);

                // Hancurkan GameObject tanaman
                if (plant != null)
                {
                    Destroy(plant.gameObject);
                }

                // Hapus dari dictionary
                plants.Remove(cellPosition);

                Debug.Log($"Tanaman di {cellPosition} telah dicabut.");
            }
            else
            {
                Debug.LogWarning($"Tidak ada tanaman di {cellPosition} untuk dicabut.");
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
                    // plants.Remove(cellPosition);
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
        [HideInInspector]
        public List<ItemData> allItems;
        public ItemData GetItemById(string id)
        {
            return allItems.Find(item => item.itemId == id);
        }
        public void LoadAllItems()
        {
            allItems = Resources.LoadAll<ItemData>("Items/Seeds").ToList();
        }
        [ContextMenu("MyHopeful - CallToSomething")]
        public void Test()
        {
            Debug.LogError(GetItemById("grapes"));
        }
#region SAVE LOAD
        public List<Manager.PlantSaveData> GetSaveDataList()
        {
            var saveList = new List<Manager.PlantSaveData>();

            foreach (var pair in plants)
            {
                var plant = pair.Value;
                var seed = plant.seed;

                var data = new Manager.PlantSaveData
                {
                    cellPosition = pair.Key,
                    itemId = seed.itemData.itemId,
                    currentStage = seed.stage,
                    timeInStage = seed.timeInStage,
                    plantedTime = seed.plantedTime.ToString("o"),
                    lastUpdateTime = seed.lastUpdateTime.ToString("o"),
                    lastWateredTime = seed.lastWateredTime.ToString("o"),
                    status = seed.status,
                    isFertilized = plant.Fertilize != null,
                    isMonsterSeed = plant.seed.typeMonster
                };

                saveList.Add(data);
            }

            return saveList;
        }

        public void LoadFromSaveData(List<Manager.PlantSaveData> saveDataList)
        {
            // 🔁 Kosongkan poolPlant terlebih dahulu
            foreach (Transform child in poolPlant)
            {
                Destroy(child.gameObject);
            }

            plants.Clear(); // Juga kosongkan dictionary plants jika itu menyimpan data aktif tanaman
            foreach (var data in saveDataList)
            {
                ItemData item = GetItemById(data.itemId);
                if (item == null) continue;

                Vector3 worldPos = TileManager.Instance.tilemapSeed.CellToWorld(data.cellPosition) + new Vector3(0f, 0.5f, 0);
                var plantObj = Instantiate(plantPrefab, worldPos, Quaternion.identity);
                plantObj.transform.parent = poolPlant;

                var plant = plantObj.GetComponent<PlantController>();
                var seed = plant.seed;

                seed.cellPosition = data.cellPosition;
                seed.itemData = item;
                seed.seedName = item.displayName;
                seed.stage = data.currentStage;
                seed.timeInStage = data.timeInStage;
                seed.plantedTime = DateTime.Parse(data.plantedTime);
                seed.lastUpdateTime = DateTime.Parse(data.lastUpdateTime);
                seed.lastWateredTime = DateTime.Parse(data.lastWateredTime);
                seed.status = data.status;

                TileManager.Instance.tilemapSeed.SetTile(data.cellPosition, item.stageTiles[data.currentStage]);

                if ((simulatedNow - seed.lastWateredTime).TotalHours <= 1)
                {
                    TileManager.Instance.tilemapWater.SetTile(data.cellPosition, TileManager.Instance.tileWater);
                }

                if (data.isFertilized)
                {
                    plant.Fertilize = item;
                    TileManager.Instance.tilemapFertilizer.SetTile(data.cellPosition, TileManager.Instance.tileFertilizer);
                }
                if (data.isMonsterSeed)
                {
                    plant.seed.typeMonster = data.isMonsterSeed;
                }

                if (seed.IsReadyToHarvest())
                {
                    seed.markHarvest.sprite = item.markHarvest;
                    seed.markHarvest.gameObject.SetActive(true);
                }

                plants[data.cellPosition] = plant;
                //update stage
                float deltaHours = (float)(simulatedNow - seed.lastUpdateTime).TotalHours;
                if (deltaHours > 0f)
                {
                    float fertilizerBoost = plant.Fertilize != null ? plant.Fertilize.boost : 0;
                    seed.UpdateGrowth(deltaHours, fertilizerBoost);
                    //cek status layu/mati untuk update tile
                    if (seed.status == PlantStatus.Mati)
                    {
                        TileManager.Instance.tilemapSeed.SetTile(data.cellPosition, stageWilted);
                    }
                    if (seed.status == PlantStatus.Layu)
                    {
                        TileManager.Instance.tilemapSeed.SetTile(data.cellPosition, stageWilted);
                    }
                    seed.lastUpdateTime = simulatedNow;
                }
            }
        }
        public void SaveToJson()
        {
            var dataList = GetSaveDataList();
            var wrapper = new PlantSaveWrapper { data = dataList };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/plants.json", json);
            Debug.Log("Tanaman disimpan ke file.");
        }

        public void LoadFromJson()
        {
            string path = Application.persistentDataPath + "/plants.json";
            if (!System.IO.File.Exists(path)) return;

            string json = System.IO.File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<PlantSaveWrapper>(json);
            LoadFromSaveData(wrapper.data);
            Debug.Log("Tanaman berhasil dimuat dari file.");
        }
#endregion
#region Plant AI Service
    public Dictionary<Vector3Int, PlantController> GetPlants () {
        return plants;
    }

    public Dictionary<Vector3Int, PlantController> GetPlantsAvailableWater()
    {
        Dictionary<Vector3Int, PlantController> result =
            new Dictionary<Vector3Int, PlantController>();

        foreach (KeyValuePair<Vector3Int, PlantController> kvp in plants)
        {
            SeedBase seedBase = kvp.Value.seed;

            // contoh kondisi (ganti sesuai logic kamu)
            if (seedBase != null && seedBase.IsNeedWater())
            {
                result.Add(kvp.Key, kvp.Value);
            }
        }

        return result;
    }

    /*
    public Dictionary <Vector3Int, PlantController> GetPlantsAvailableHarvest () {

    }
    */
#endregion
    }
}

