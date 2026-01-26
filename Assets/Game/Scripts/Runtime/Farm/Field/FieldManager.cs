using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
using System.IO;
namespace MagicalGarden.Farm
{
    public class FieldManager : MonoBehaviour
    {
        [Header("UI Setting")]
        public GameObject bubbleLockUI;
        public GameObject unlockVFX;
        [HideInInspector] public List<FieldBlock> blocks = new List<FieldBlock>();
        public List <FarmAreaEligibleDataSO> listFarmAreaEligibleDataSO = new ();
        private string SaveFilePath => Application.persistentDataPath + "/field_save.json";

        [Header("Config (JSON)")]
        public TextAsset fieldConfigJson;           // assign file JSON (Resources atau drag ke Inspector)
        public bool loadConfigOnStart = true;
        public static FieldManager Instance;

        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            /*
            // 1) Jika diinginkan, load config JSON (requirement, default unlock)
            if (loadConfigOnStart && fieldConfigJson != null && blocks.Count == 0)
            {
                LoadConfigFromText(fieldConfigJson.text, clearExisting: true);
            }
            // 2) Buat overlay & bubble untuk semua block yang terkunci
            foreach (var item in blocks)
            {
                UpdateOverlayVisual(item.blockId, false);
                var bubble = Instantiate(bubbleLockUI, gameObject.transform);
                bubble.GetComponent<UnlockBubbleUI>().Setup(item, new Vector3Int(item.blockId.x, item.blockId.y, 0), item.farmAreaEligibleDataSO);
                item.bubbleUI = bubble;
            }
            */
            // 3) Overlay save pemain (jika ada), akan auto-destroy bubble yang sudah unlock
            // LoadFromJson();
        }

        public void LoadFromConfig () { // PLant
            // LoadConfigFromText(fieldConfigJson.text, clearExisting: true);
            if (loadConfigOnStart && fieldConfigJson != null && blocks.Count == 0)
            {
                LoadConfigFromText(fieldConfigJson.text, clearExisting: true);
            }
            // 2) Buat overlay & bubble untuk semua block yang terkunci
            foreach (var item in blocks)
            {
                UpdateOverlayVisual(item.blockId, false);
                var bubble = Instantiate(bubbleLockUI, gameObject.transform);
                bubble.GetComponent<UnlockBubbleUI>().Setup(item, new Vector3Int(item.blockId.x, item.blockId.y, 0), item.farmAreaEligibleDataSO);
                item.bubbleUI = bubble;
            }
        }

        public FieldBlock GetBlockById(Vector2Int id)
        {
            return blocks.FirstOrDefault(b => b.blockId == id);
        }

        public bool IsTileUnlocked(Vector3Int tilePos)
        {
            Vector2Int blockId = new Vector2Int(tilePos.x / 3, tilePos.y / 3);
            return blocks.Any(b => b.blockId == blockId && b.isUnlocked);
        }

        public bool CanUnlock(Vector2Int id, int playerHarvestCount, int playerCoins)
        {
            FieldBlock block = GetBlockById(id);
            if (block == null || block.isUnlocked) return false;

            return playerHarvestCount >= block.requiredHarvest &&
                playerCoins >= block.requiredCoins;
        }

        public void UnlockBlock(Vector2Int blockId)
        {
            FieldBlock block = GetBlockById(blockId);
            if (block == null || block.isUnlocked) return;
            {
                block.isUnlocked = true;
                Vector3 worldPos = TileManager.Instance.tilemapSoil.CellToWorld(new Vector3Int(blockId.x, blockId.y, 0));
                Instantiate(unlockVFX, worldPos, Quaternion.identity);
                UpdateOverlayVisual(blockId, true);

                // unlock new farm area is at index 8
                MonsterManager.instance.audio.PlayFarmSFX(8);
            }
        }
        public void UpdateOverlayVisual(Vector2Int center, bool unlocked)
        {
            var tilemap = TileManager.Instance.tilemapSoil;
            if (tilemap == null)
            {
                Debug.LogWarning("Tilemap Locked belum diset di TileManager.");
                return;
            }
            TileBase lockedTile = TileManager.Instance.lockedTile;
            TileBase unlockedTile = TileManager.Instance.unlockedTile;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int tilePos = new Vector3Int(center.x + dx, center.y + dy, 0);

                    if (unlocked)
                    {
                        if (tilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, unlockedTile);
                            // Debug.Log($"[Unlock] Removed locked tile at {tilePos}");
                        }
                    }
                    else
                    {
                        if (tilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, lockedTile);
                            // Debug.Log($"[Lock] Set locked tile at {tilePos}");
                        }
                    }
                }
            }
        }

        // === CONFIG LOADER ===
        [System.Serializable]
        private class FieldBlockConfig
        {
            public int x;
            public int y;
            public int requiredCoins;
            public int requiredHarvest;
            public int requiredHaveMonster;
            public int requiredHarvestEgg;
            public bool defaultUnlocked;
        }

        [System.Serializable]
        private class FieldConfigFile
        {
            public List<FieldBlockConfig> blocks = new List<FieldBlockConfig>();
        }

        public void LoadConfigFromText(string json, bool clearExisting = true)
        {
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[FieldManager] fieldConfigJson kosong.");
                return;
            }

            var cfg = JsonUtility.FromJson<FieldConfigFile>(json);
            if (cfg == null || cfg.blocks == null || cfg.blocks.Count == 0)
            {
                Debug.LogWarning("[FieldManager] Tidak ada blocks pada config JSON.");
                return;
            }

            if (clearExisting)
            {
                // bersihkan block & bubble lama
                foreach (var b in blocks)
                {
                    if (b?.bubbleUI != null)
                    {
                        if (Application.isPlaying) Destroy(b.bubbleUI);
                        else DestroyImmediate(b.bubbleUI);
                    }
                }
                blocks.Clear();
            }
            
            int farmId = 2;
            // apply semua dari config
           // Debug.Log ("Load 2");
            foreach (var c in cfg.blocks)
            {
                if (PlantManager.Instance.IsFarmAreaUnlocked (farmId)) {
                    Debug.Log ("Farm id ini " + farmId);
                    farmId++;
                    continue;
                }

                var id = new Vector2Int(c.x, c.y);
                SetBlockConfig(
                    id,
                    requiredCoins: c.requiredCoins,
                    requiredHarvest: c.requiredHarvest,
                    requiredHaveMonster: c.requiredHaveMonster,
                    requiredHarvestEgg: c.requiredHarvestEgg,
                    unlocked: c.defaultUnlocked,
                    refreshVisual: false,
                    spawnBubbleIfLocked: false,   // bubble dibuat setelah ini
                    autoSave: false,
                    listFarmAreaEligibleDataSO[farmId-2],
                    farmId // +1 karena element 0, idnya 1)
                );
                farmId++;
            }

            // refresh visual sekali
            foreach (var b in blocks)
                UpdateOverlayVisual(b.blockId, b.isUnlocked);
        }

        public void SaveToJson()
        {
            FieldSaveData saveData = new FieldSaveData();

            foreach (var block in blocks)
            {
                saveData.blocks.Add(new FieldBlockSaveData
                {
                    blockId = block.blockId,
                    isUnlocked = block.isUnlocked
                });
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log("Field data saved to " + SaveFilePath);
        }

        public void LoadFromJson()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("Save file not found.");
                return;
            }

            string json = File.ReadAllText(SaveFilePath);
            FieldSaveData loadedData = JsonUtility.FromJson<FieldSaveData>(json);

            foreach (var data in loadedData.blocks)
            {
                FieldBlock block = GetBlockById(data.blockId);
                if (block != null)
                {
                    block.isUnlocked = data.isUnlocked;
                    UpdateOverlayVisual(block.blockId, block.isUnlocked);

                    // Destroy bubble jika blok sudah terbuka
                    if (block.isUnlocked && block.bubbleUI != null)
                    {
                        Destroy(block.bubbleUI);
                        block.bubbleUI = null;
                    }
                }
            }

            Debug.Log("Field data loaded.");
        }
        #region Testing
        public FieldBlock SetBlockConfig(
            
            Vector2Int blockId,
            int? requiredCoins = null,
            int? requiredHarvest = null,
            int? requiredHaveMonster = null,
            int? requiredHarvestEgg = null,
            bool? unlocked = null,
            bool refreshVisual = true,
            bool spawnBubbleIfLocked = true,
            bool autoSave = false,
            FarmAreaEligibleDataSO farmAreaEligbleDataSOValue = null,
            int numberId = 0 )
        {
            // Cari block
            var block = GetBlockById(blockId);
            if (block == null)
            {
                block = new FieldBlock { blockId = blockId };
                blocks.Add(block);
            }

            // Update nilai hanya jika disuplai
            if (requiredCoins.HasValue)       block.requiredCoins = requiredCoins.Value;
            if (requiredHarvest.HasValue)     block.requiredHarvest = requiredHarvest.Value;
            if (requiredHaveMonster.HasValue) block.requiredHaveMonster = requiredHaveMonster.Value;
            if (requiredHarvestEgg.HasValue)  block.requiredHarvestEgg = requiredHarvestEgg.Value;
            if (unlocked.HasValue)            block.isUnlocked = unlocked.Value;

            // Refresh overlay tiles
            if (refreshVisual)
                UpdateOverlayVisual(block.blockId, block.isUnlocked);

            // Kelola bubble UI
            if (block.isUnlocked)
            {
                if (block.bubbleUI != null)
                {
                    Destroy(block.bubbleUI);
                    block.bubbleUI = null;
                }
            }
            else if (spawnBubbleIfLocked && block.bubbleUI == null && bubbleLockUI != null)
            {
                var bubble = Instantiate(bubbleLockUI, transform);
                var ui = bubble.GetComponent<UnlockBubbleUI>();
                if (ui != null)
                    ui.Setup(block, new Vector3Int(block.blockId.x, block.blockId.y, 0),farmAreaEligbleDataSOValue);
                block.bubbleUI = bubble;
            }

            if (autoSave)
                SaveToJson();
            
            block.farmAreaEligibleDataSO = farmAreaEligbleDataSOValue;
            block.numberId = numberId;
            return block;
        }
        #endregion
    }
    [System.Serializable]
    public class FieldBlockSaveData
    {
        public Vector2Int blockId;
        public bool isUnlocked;
    }

    [System.Serializable]
    public class FieldSaveData
    {
        public List<FieldBlockSaveData> blocks = new List<FieldBlockSaveData>();
    }
    
}