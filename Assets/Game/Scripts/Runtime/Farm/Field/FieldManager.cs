using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;
using MagicalGarden.Manager;
namespace MagicalGarden.Farm
{
    public class FieldManager : MonoBehaviour
    {
        public GameObject bubbleLockUI;
        public List<FieldBlock> blocks = new List<FieldBlock>();
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
                bubble.GetComponent<UnlockBubbleUI>().Setup(item, new Vector3Int(item.blockId.x,item.blockId.y, 0));
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

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int tilePos = new Vector3Int(center.x + dx, center.y + dy, 0);

                    if (unlocked)
                    {
                        if (tilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, null);
                            // Debug.Log($"[Unlock] Removed locked tile at {tilePos}");
                        }
                    }
                    else
                    {
                        if (!tilemap.HasTile(tilePos))
                        {
                            tilemap.SetTile(tilePos, lockedTile);
                            // Debug.Log($"[Lock] Set locked tile at {tilePos}");
                        }
                    }
                }
            }
        }
    }
}