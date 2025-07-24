using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicalGarden.Farm
{
    [CreateAssetMenu(fileName = "New Hotel Tile", menuName = "Tiles/Hotel Tile")]
    public class HotelTile : Tile
    {
        public AreaOffsetDirection offsetDirection = AreaOffsetDirection.None;

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
    public enum AreaOffsetDirection
    {
        None,
        Right,
        Left,
        Up,
        Down
    }
}