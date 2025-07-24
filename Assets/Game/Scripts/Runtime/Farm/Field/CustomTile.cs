using UnityEngine;
using UnityEngine.Tilemaps;

namespace MagicalGarden.Farm
{
    [CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
    public class CustomTile : Tile
    {
        public TileType tileType;

        [Header("Visual Settings")]
        public Vector3 customScale = Vector3.one;
        public float customRotationZ = 0f;
        public Vector3 customOffset = Vector3.zero;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            tileData.transform = Matrix4x4.TRS(
                customOffset,
                Quaternion.Euler(0f, 0f, customRotationZ),
                customScale
            );
        }
    }

    public enum TileType
    {
        NonWalkable,
        Walkable,
        Locked,
        Plantable,
        Interact
    }
}