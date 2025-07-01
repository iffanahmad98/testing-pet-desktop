using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MagicalGarden.Inventory
{
    [CreateAssetMenu(fileName = "NewItemFertilizer", menuName = "Inventory/Item Fertilizer")]
    public class ItemDataFertilizer : ItemData
    {
        public float timeUse;
        public int recipeCountPopNormal;
        public int recipeCountPopRare;

        //for fertilizer
        [Header("Fertilizer")]
        [Range(1, 100)]
        public int boost = 0;
    }
}