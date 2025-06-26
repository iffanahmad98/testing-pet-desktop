using MagicalGarden.Farm;
using MagicalGarden.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicalGarden.Manager
{
    public enum TileAction
    {
        None,
        Seed,
        Water,
        Fertilizer,
        Harvest,
        Remove
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
        public Tilemap tilemapHotel;
        [Header("Tiles")]
        public TileBase tileSeed;
        public TileBase tileWater;
        public TileBase tileFertilizer;
        public TileBase highlightTile;
        public TileBase lockedTile;
        private Vector3Int previousCellPos = Vector3Int.zero;
        private bool hasPreviousTile = false;
        private ItemData currentItemdata;
        public bool disableTileSelect = false;

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
                    CursorIconManager.Instance.ShowWateringIcon();
                    currentAction = TileAction.Water;
                    break;
                case "Fertilizer":
                    currentAction = TileAction.Fertilizer;
                    break;
                case "Harvest":
                    CursorIconManager.Instance.ShowHarvestIcon();
                    currentAction = TileAction.Harvest;
                    break;
                case "Remove":
                    CursorIconManager.Instance.ShowRemoveIcon();
                    currentAction = TileAction.Remove;
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

        public void SetActionFertilizer(ItemData itemData)
        {
            currentAction = TileAction.Fertilizer;
            currentItemdata = itemData;
        }
        void Update()
        {
            if (disableTileSelect) return;
            // if (Input.GetMouseButtonDown(0))
            // {
            //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //     Vector3Int cellPos = tilemapHighlight.WorldToCell(mouseWorldPos);

            //     if (tilemapHighlight.HasTile(cellPos))
            //     {
            //         UIManager.Instance.TogglefertizerUI();
            //     }
            // }
            if (Input.GetMouseButtonDown(1)) // Klik kanan
            {
                SetAction("None");
            }
            HighlightHoverTiles();
            ActionToTile();
        }

        void HighlightHoverTiles()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemapSoil.WorldToCell(mouseWorldPos);

            // Jika tile tidak bisa ditanami, hapus highlight dan return
            if (!IsPlantable(cellPos))
            {
                if (hasPreviousTile)
                {
                    tilemapHighlight.SetTile(previousCellPos, null);
                    hasPreviousTile = false;
                    Farm.UIManager.Instance.HidePlantInfo();
                }
                return;
            }

            // Jika tile baru berbeda atau tile sebelumnya sudah hilang highlight-nya
            if (cellPos != previousCellPos || !hasPreviousTile)
            {
                // Hapus highlight sebelumnya
                if (hasPreviousTile)
                {
                    tilemapHighlight.SetTile(previousCellPos, null);
                    hasPreviousTile = false;
                    Farm.UIManager.Instance.HidePlantInfo();
                }

                // Kalau mouse di atas tile tanah (soil)
                if (tilemapSoil.HasTile(cellPos))
                {
                    tilemapHighlight.SetTile(cellPos, highlightTile);
                    previousCellPos = cellPos;
                    hasPreviousTile = true;
                }
                else
                {
                    previousCellPos = cellPos; // tetap update agar tidak spam
                }
            }

            if (tilemapSeed.HasTile(cellPos))
            {
                ShowPlantInfoAtHoveredTile(cellPos);
            }
        }

        void ActionToTile()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = tilemapSeed.WorldToCell(mouseWorldPos); // semua tilemap share grid yang sama
                if (!tilemapSoil.HasTile(cellPos)) return;
                if (!IsPlantable(cellPos)) return;
                switch (currentAction)
                {
                    case TileAction.Seed:
                        if (currentItemdata.itemType == Inventory.ItemType.MonsterSeed)
                        {
                            if (PlantManager.Instance.CanPlantMonsterSeedAt(cellPos))
                            {
                                PlantManager.Instance.PlantSeedAt(cellPos, currentItemdata, true);
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
                        PlantManager.Instance.PlantFertilizeAt(cellPos, currentItemdata);
                        break;
                    case TileAction.Remove:
                        PlantManager.Instance.RemovePlantAt(cellPos);
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
            // return tilemapLocked.HasTile(position);
            TileBase tile = tilemapSoil.GetTile(position);

            if (tile is CustomTile customTile)
            {
                if (!(customTile.tileType == TileType.Plantable))
                {
                    return true;
                }
            }
            return false;
        }
        
        private bool IsPlantable(Vector3Int position)
        {
            TileBase tile = tilemapSoil.GetTile(position);

            if (tile is CustomTile customTile)
            {
                if (customTile.tileType == TileType.Plantable)
                {
                    return true;
                }
            }
            return false;
        }

        public void ShowPlantInfoAtHoveredTile(Vector3Int cellPos)
        {
            var plant = PlantManager.Instance.GetPlantAt(cellPos);
            if (plant != null)
            {
                Farm.UIManager.Instance.ShowPlantInfo(plant.GetPlantStatusText(), cellPos);

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
                    Vector3 world = tilemapSoil.CellToWorld(pos) + new Vector3(0f, 0.5f, 0);
                    Handles.color = Color.black;
                    Handles.Label(world, $"({pos.x},{pos.y})");
                }
            }
#endif
        }
    }

}