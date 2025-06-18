using UnityEngine;
using UnityEngine.Tilemaps;

namespace MagicalGarden.Farm
{
    [CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
    public class CustomTile : Tile
    {
        public TileType tileType;
    }

    public enum TileType
    {
        Walkable,
        Locked,
        Plantable,
        Interact
    }
}