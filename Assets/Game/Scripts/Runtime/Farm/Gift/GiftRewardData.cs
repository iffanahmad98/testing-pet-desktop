using UnityEngine;
using MagicalGarden.Inventory;

namespace MagicalGarden.Gift
{
    /// <summary>
    /// Data konfigurasi untuk satu jenis reward
    /// </summary>
    [System.Serializable]
    public class GiftRewardData
    {
        [Header("Reward Info")]
        public GiftRewardType rewardType;

        [Header("Probability (%)")]
        [Tooltip("Chance reward ini muncul (0-100%)")]
        [Range(0f, 100f)]
        public float dropChance;

        [Header("Reward Amount")]
        [Tooltip("Minimum jumlah reward")]
        public int minAmount = 1;
        [Tooltip("Maximum jumlah reward")]
        public int maxAmount = 1;

        [Header("Item Reference (untuk FoodPack, Medicine, dll)")]
        [Tooltip("ItemData yang akan diberikan (jika reward berupa item)")]
        public ItemData itemData;

        /// <summary>
        /// Get random amount between min and max
        /// </summary>
        public int GetRandomAmount()
        {
            return Random.Range(minAmount, maxAmount + 1);
        }

        /// <summary>
        /// Check if this reward should drop based on probability
        /// </summary>
        public bool ShouldDrop(float randomValue)
        {
            return randomValue <= dropChance;
        }
    }
}
