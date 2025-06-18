using UnityEngine;
using UnityEngine.Tilemaps;

namespace MagicalGarden.Farm
{
    [CreateAssetMenu(fileName = "New Interact Tile", menuName = "Tiles/Interact Tile")]
    public class InteractTile : Tile
    {
        public TileType tileType;
    }
}