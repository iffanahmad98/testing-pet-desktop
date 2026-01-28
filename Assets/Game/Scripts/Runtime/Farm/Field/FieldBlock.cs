using UnityEngine;

namespace MagicalGarden.Farm
{
    [System.Serializable]
    public class FieldBlock
    {
        public int numberId;
        public Vector2Int blockId;
        public bool isUnlocked = false;
        public int requiredHaveMonster = 0;
        public int requiredCoins = 0;
        public int requiredHarvest = 0;
        public int requiredHarvestEgg = 0;
        public GameObject bubbleUI;
        public FarmAreaEligibleDataSO farmAreaEligibleDataSO;
    }
}