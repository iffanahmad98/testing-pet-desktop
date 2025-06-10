using MagicalGarden.Farm;
using MagicalGarden.Inventory;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        [Header("Tiles")]
        public TileBase tileSeed;
        public TileBase tileWater;
        public TileBase tileFertilizer;
        public TileBase highlightTile;
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
            HighlightHoverTiles();
            ActionToTile();
        }

        void HighlightHoverTiles()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemapSoil.WorldToCell(mouseWorldPos);

            // Kalau tile baru berbeda dengan sebelumnya
            if (cellPos != previousCellPos)
            {
                // Hapus highlight sebelumnya
                if (hasPreviousTile)
                {
                    tilemapHighlight.SetTile(previousCellPos, null);
                    hasPreviousTile = false;
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
        
        // public void ShowPlantInfoAtHoveredTile()
        // {
        //     var cellPos = GetHoveredCellPosition();
        //     if (cellPos.HasValue)
        //     {
        //         var seed = PlantManager.Instance.GetSeedAt(cellPos.Value);
        //         if (seed != null)
        //         {
        //             Debug.Log($"Plant: {seed.seedName} | Stage: {seed.stage} | Status: {seed.status}");
        //         }
        //     }
        // }

        // public bool IsValidPlantingSpot(Vector3Int cellPos)
        // {
        // if (!tilemapSoil.HasTile(cellPos)) return false;

        // var existingSeed = PlantManager.Instance.GetSeedAt(cellPos);
        // return existingSeed == null;
        // }


    }

}