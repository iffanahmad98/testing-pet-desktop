using MagicalGarden.Farm;
using MagicalGarden.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicalGarden.Farm
{
    public enum TileAction
    {
        None,
        Seed,
        Water,
        Fertilizer,
        Harvest
    }

    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance;
        public TileAction currentAction = TileAction.None;
        [Header("Tilemap")]
        public Tilemap tilemapSoil;
        public Tilemap tilemapSeed;
        public Tilemap tilemapWater;
        public Tilemap tilemapFertilizer;
        public Tilemap tilemapHighlight;
        public Tilemap tilemapLocked;
        [Header("Tiles")]
        public TileBase tileSeed;
        public TileBase tileWater;
        public TileBase tileFertilizer;
        public TileBase highlightTile;
        public TileBase lockedTile;
        private Vector3Int previousCellPos = Vector3Int.zero;
        private bool hasPreviousTile = false;
        private ItemData currentItemdata;

        private void Awake()
        {
            Instance = this;
        }

        public void SetAction(string action)
        {
            CursorIconManager.Instance.HideSeedIcon();
            switch (action)
            {
                case "Seed":
                    currentAction = TileAction.Seed;
                    break;
                case "Water":
                    currentAction = TileAction.Water;
                    break;
                case "Fertilizer":
                    currentAction = TileAction.Fertilizer;
                    break;
                case "Harvest":
                    currentAction = TileAction.Harvest;
                    break;
                default:
                    currentAction = TileAction.None;
                    break;
            }

            Debug.Log("Selected action: " + currentAction);
        }

        public void SetActionSeed(ItemData itemData)
        {
            currentAction = TileAction.Seed;
            currentItemdata = itemData;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = tilemapHighlight.WorldToCell(mouseWorldPos);

                if (tilemapHighlight.HasTile(cellPos))
                {
                    TileBase clickedTile = tilemapHighlight.GetTile(cellPos);
                    UIManager.Instance.TogglefertizerUI();
                }
            }
            HighlightHoverTiles();
            ActionToTile();
        }

        void HighlightHoverTiles()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemapSoil.WorldToCell(mouseWorldPos);
            if (IsTileLocked(cellPos)) return;
            // Kalau tile baru berbeda dengan sebelumnya
            if (cellPos != previousCellPos)
            {
                // Hapus highlight sebelumnya
                if (hasPreviousTile)
                {
                    tilemapHighlight.SetTile(previousCellPos, null);
                    hasPreviousTile = false;
                    UIManager.Instance.HidePlantInfo();
                }

                // Kalau mouse di atas tile tanah (soil)
                if (tilemapSoil.HasTile(cellPos))
                {
                    tilemapHighlight.SetTile(cellPos, highlightTile);
                    previousCellPos = cellPos;
                    hasPreviousTile = true;
                    ShowPlantInfoAtHoveredTile(cellPos);
                }
                else
                {
                    previousCellPos = cellPos; // tetap update pos agar tidak spam
                }
            }
        }

        void ActionToTile()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = tilemapSeed.WorldToCell(mouseWorldPos); // semua tilemap share grid yang sama
                if (!tilemapSoil.HasTile(cellPos)) return;
                if (IsTileLocked(cellPos)) return;
                switch (currentAction)
                {
                    case TileAction.Seed:
                        if (currentItemdata.itemType == ItemType.MonsterSeed)
                        {
                            if (PlantManager.Instance.CanPlantMonsterSeedAt(cellPos))
                            {
                                //bisa ditanami monster
                                PlantManager.Instance.PlantSeedAt(cellPos, currentItemdata);
                            }
                            else
                            {
                                Debug.LogError("tidak bisa ditanami monster");
                            }
                        }
                        else
                        {
                            PlantManager.Instance.PlantSeedAt(cellPos, currentItemdata);
                        }

                        break;
                    case TileAction.Water:
                        PlantManager.Instance.PlantWaterAt(cellPos);
                        tilemapWater.SetTile(cellPos, tileWater);
                        break;
                    case TileAction.Fertilizer:
                        tilemapFertilizer.SetTile(cellPos, tileFertilizer);
                        break;
                    case TileAction.Harvest:
                        PlantManager.Instance.HarvestAt(cellPos);
                        break;
                }
            }
        }

        public Vector3Int? GetHoveredCellPosition()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemapSoil.WorldToCell(mouseWorldPos);
            return tilemapSoil.HasTile(cellPos) ? (Vector3Int?)cellPos : null;
        }

        public void ResetAction()
        {
            currentAction = TileAction.None;
        }
        
        public bool IsTileLocked(Vector3Int position)
        {
            return tilemapLocked.HasTile(position);
        }

        public void ShowPlantInfoAtHoveredTile(Vector3Int cellPos)
        {
            var plant = PlantManager.Instance.GetPlantAt(cellPos);
            if (plant != null)
            {
                UIManager.Instance.ShowPlantInfo(plant.GetPlantStatusText(), cellPos);
                
            }
        }

        // public bool IsValidPlantingSpot(Vector3Int cellPos)
        // {
        // if (!tilemapSoil.HasTile(cellPos)) return false;

        // var existingSeed = PlantManager.Instance.GetSeedAt(cellPos);
        // return existingSeed == null;
        // }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (tilemapSoil == null) return;
            foreach (var pos in tilemapSoil.cellBounds.allPositionsWithin)
            {
                if (tilemapSoil.HasTile(pos))
                {
                    Vector3 world = tilemapSoil.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
                    Handles.color = Color.black;
                    Handles.Label(world, $"({pos.x},{pos.y})");
                }
            }
#endif
        }
    }

}