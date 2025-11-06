using UnityEngine;
using MagicalGarden.Inventory;
using System.Collections.Generic;

namespace MagicalGarden.Gift
{
    /// <summary>
    /// Helper class untuk create preset reward configurations
    /// Berdasarkan requirement:
    ///
    /// Item             Small               Medium              Large
    /// Coin             60% (50–150)        55% (150–500)       45% (500–2000)
    /// Food pack        20% (Feed×1)        22% (Feed×3)        22% (Feed×5)
    /// Medicine         6%                  8%                  12%
    /// Golden Ticket    0.50%               1.00%               2.00%
    /// Decoration       1.00%               3.00%               6.00%
    /// Bonus 2x coins   4%                  6%                  8%
    /// Empty→coins      8.50%               5%                  5%
    /// </summary>
    public static class GiftRewardPresets
    {
        /// <summary>
        /// Create Small Gift reward list
        /// </summary>
        public static List<GiftRewardData> CreateSmallGiftRewards(
            ItemData feedItem = null,
            ItemData medicineItem = null,
            ItemData goldenTicketItem = null,
            ItemData decorationItem = null)
        {
            var rewards = new List<GiftRewardData>();

            // Coin - 60%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Coin,
                dropChance = 60f,
                minAmount = 50,
                maxAmount = 150,
                itemData = null
            });

            // Food Pack - 20%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.FoodPack,
                dropChance = 20f,
                minAmount = 1,
                maxAmount = 1,
                itemData = feedItem
            });

            // Medicine - 6%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Medicine,
                dropChance = 6f,
                minAmount = 1,
                maxAmount = 1,
                itemData = medicineItem
            });

            // Bonus Double Coins - 4%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.DoubleCoin,
                dropChance = 4f,
                minAmount = 50,
                maxAmount = 150,
                itemData = null
            });

            // Decoration - 1%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Decoration,
                dropChance = 1f,
                minAmount = 1,
                maxAmount = 1,
                itemData = decorationItem
            });

            // Empty (convert to coins) - 8.5%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Empty,
                dropChance = 8.5f,
                minAmount = 25,
                maxAmount = 75,
                itemData = null
            });

            // Golden Ticket - 0.5%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.GoldenTicket,
                dropChance = 0.5f,
                minAmount = 1,
                maxAmount = 1,
                itemData = goldenTicketItem
            });

            return rewards;
        }

        /// <summary>
        /// Create Medium Gift reward list
        /// </summary>
        public static List<GiftRewardData> CreateMediumGiftRewards(
            ItemData feedItem = null,
            ItemData medicineItem = null,
            ItemData goldenTicketItem = null,
            ItemData decorationItem = null)
        {
            var rewards = new List<GiftRewardData>();

            // Coin - 55%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Coin,
                dropChance = 55f,
                minAmount = 150,
                maxAmount = 500,
                itemData = null
            });

            // Food Pack - 22%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.FoodPack,
                dropChance = 22f,
                minAmount = 3,
                maxAmount = 3,
                itemData = feedItem
            });

            // Medicine - 8%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Medicine,
                dropChance = 8f,
                minAmount = 1,
                maxAmount = 1,
                itemData = medicineItem
            });

            // Bonus Double Coins - 6%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.DoubleCoin,
                dropChance = 6f,
                minAmount = 150,
                maxAmount = 500,
                itemData = null
            });

            // Empty (convert to coins) - 5%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Empty,
                dropChance = 5f,
                minAmount = 75,
                maxAmount = 250,
                itemData = null
            });

            // Decoration - 3%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Decoration,
                dropChance = 3f,
                minAmount = 1,
                maxAmount = 1,
                itemData = decorationItem
            });

            // Golden Ticket - 1%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.GoldenTicket,
                dropChance = 1f,
                minAmount = 1,
                maxAmount = 1,
                itemData = goldenTicketItem
            });

            return rewards;
        }

        /// <summary>
        /// Create Large Gift reward list
        /// </summary>
        public static List<GiftRewardData> CreateLargeGiftRewards(
            ItemData feedItem = null,
            ItemData medicineItem = null,
            ItemData goldenTicketItem = null,
            ItemData decorationItem = null)
        {
            var rewards = new List<GiftRewardData>();

            // Coin - 45%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Coin,
                dropChance = 45f,
                minAmount = 500,
                maxAmount = 2000,
                itemData = null
            });

            // Food Pack - 22%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.FoodPack,
                dropChance = 22f,
                minAmount = 5,
                maxAmount = 5,
                itemData = feedItem
            });

            // Medicine - 12%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Medicine,
                dropChance = 12f,
                minAmount = 1,
                maxAmount = 1,
                itemData = medicineItem
            });

            // Bonus Double Coins - 8%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.DoubleCoin,
                dropChance = 8f,
                minAmount = 500,
                maxAmount = 2000,
                itemData = null
            });

            // Decoration - 6%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Decoration,
                dropChance = 6f,
                minAmount = 1,
                maxAmount = 1,
                itemData = decorationItem
            });

            // Empty (convert to coins) - 5%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.Empty,
                dropChance = 5f,
                minAmount = 250,
                maxAmount = 1000,
                itemData = null
            });

            // Golden Ticket - 2%
            rewards.Add(new GiftRewardData
            {
                rewardType = GiftRewardType.GoldenTicket,
                dropChance = 2f,
                minAmount = 1,
                maxAmount = 1,
                itemData = goldenTicketItem
            });

            return rewards;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validate bahwa total probability = 100%
        /// </summary>
        public static void ValidateRewards(List<GiftRewardData> rewards, string sizeName)
        {
            float total = 0f;
            foreach (var reward in rewards)
            {
                total += reward.dropChance;
            }

            if (Mathf.Abs(total - 100f) > 0.01f)
            {
                Debug.LogWarning($"[{sizeName}] Total probability is {total}%, expected 100%!");
            }
            else
            {
                Debug.Log($"[{sizeName}] Reward table validated successfully! Total: {total}%");
            }
        }
#endif
    }
}
