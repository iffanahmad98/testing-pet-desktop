using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MagicalGarden.Inventory
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class ItemData : Rewardable
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public Sprite iconCrop;
        public ItemType itemType;
        public ItemRarity rarity;
        public bool isStackable = true;
        public int price;
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
        [Header ("Rewardable")]
        public Vector3 rewardScale = new Vector3 (1,1,1);
        public override string ItemId => itemId;
        public override string ItemName => displayName;
        public override Sprite RewardSprite => icon;
        public override Vector3 RewardScale => rewardScale;
        #region Rewardable
        
        public override void RewardGotItem(int quantities)
        {
            Debug.Log($"You got item {displayName} x{quantities} (doesnt has save system yet)");
           // SaveSystem.PlayerConfig.AddItem (itemId,itemType,quantities);
           // SaveSystem.SaveAll ();
        }
        #endregion
    }
}