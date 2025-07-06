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
        public Sprite iconCrop;
        public ItemType itemType;
        public ItemRarity rarity;
        public bool isStackable = true;
        [TextArea(3, 10)]
        public string description;
        public List<TileBase> stageTiles;
        public TileBase stageWilted;
        public Sprite markHarvest;
        public List<DropItem> dropItems;
        //for fertilizer
        [Header("Fertilizer")]
        [Range(1, 100)]
        public int boost = 0;
        public float timeUse;
        public int recipeCountPopNormal;
        public int recipeCountPopRare;
        [Header("Seed")]
        public int needHourWatering;
        public int needHourGrow;
        // [Header("Animation")]
    }
}