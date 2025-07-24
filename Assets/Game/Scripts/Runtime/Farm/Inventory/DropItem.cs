using UnityEngine;
namespace MagicalGarden.Inventory
{
    [System.Serializable]
    public class DropItem
    {
        public ItemData item;
        [Range(0f, 1f)] public float dropChance = 1f;
        [Min(1)] public int minAmount = 1;
        [Min(1)] public int maxAmount = 1;
    }
}