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
        public GameObject bubbleLockUI;
        public GameObject unlockVFX;
        public List<FieldBlock> blocks = new List<FieldBlock>();
        private string SaveFilePath => Application.persistentDataPath + "/field_save.json";
        public static FieldManager Instance;

        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            foreach (var item in blocks)
            {
                UpdateOverlayVisual(item.blockId, false);
                var bubble = Instantiate(bubbleLockUI, gameObject.transform);
                bubble.GetComponent<UnlockBubbleUI>().Setup(item, new Vector3Int(item.blockId.x, item.blockId.y, 0));
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
            bool autoSave = false)
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
                    ui.Setup(block, new Vector3Int(block.blockId.x, block.blockId.y, 0));
                block.bubbleUI = bubble;
            }

            if (autoSave)
                SaveToJson();

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