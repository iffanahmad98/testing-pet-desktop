using UnityEngine;

namespace MagicalGarden.Farm
{
    [System.Serializable]
    public class FieldBlock
    {
        public Vector2Int blockId;
        public bool isUnlocked = false;
        public int requiredHarvest = 0;
        public int requiredCoins = 0;
    }
}