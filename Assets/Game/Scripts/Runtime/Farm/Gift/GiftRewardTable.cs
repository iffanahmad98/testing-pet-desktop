using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MagicalGarden.Gift
{
    /// <summary>
    /// ScriptableObject untuk menyimpan reward configuration per gift size
    /// </summary>
    [CreateAssetMenu(fileName = "GiftRewardTable", menuName = "Gift/Reward Table")]
    public class GiftRewardTable : ScriptableObject
    {
        [Header("Gift Size")]
        public GiftSize giftSize;

        [Header("Reward List")]
        [Tooltip("List semua possible rewards untuk gift size ini")]
        public List<GiftRewardData> rewards = new List<GiftRewardData>();

        /// <summary>
        /// Get random reward berdasarkan probability
        /// </summary>
        public GiftRewardData GetRandomReward()
        {
            // Generate random value 0-100
            float randomValue = Random.Range(0f, 100f);
            float cumulativeProbability = 0f;

            // Sort rewards by drop chance (descending) untuk fairness
            var sortedRewards = rewards.OrderByDescending(r => r.dropChance).ToList();

            foreach (var reward in sortedRewards)
            {
                cumulativeProbability += reward.dropChance;

                if (randomValue <= cumulativeProbability)
                {
                    return reward;
                }
            }

            // Fallback: return first reward (shouldn't happen if probabilities sum to 100%)
            return rewards.Count > 0 ? rewards[0] : null;
        }

        /// <summary>
        /// Validate total probability (should be 100%)
        /// </summary>
        public float GetTotalProbability()
        {
            return rewards.Sum(r => r.dropChance);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            float total = GetTotalProbability();
            if (Mathf.Abs(total - 100f) > 0.01f)
            {
                Debug.LogWarning($"[{name}] Total probability is {total}%, should be 100%!");
            }
        }
#endif
    }
}
