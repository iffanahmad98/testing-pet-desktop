using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MagicalGarden.Inventory
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public ItemType itemType;
        public ItemRarity rarity;
        public bool isStackable = true;
        public string description;
        public List<TileBase> stageTiles;
        public List<DropItem> dropItems;
    }
}