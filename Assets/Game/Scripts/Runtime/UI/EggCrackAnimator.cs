using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
public class EggCrackAnimator : MonoBehaviour
{
    [Header ("Egg Crack Reference")]
    Animator eggAnimator;
    [Header("Gacha Configuration")]
    public MonsterDatabaseSO monsterDatabase;
    public List<RarityWeight> rarityWeights;

    // Internal counters
    private int totalPullCount = 0;
    private Dictionary<MonsterType, int> pityCounters = new Dictionary<MonsterType, int>();
    private const string TOTAL_PULL_COUNT_KEY = "GachaTotalPullCount";
    private const string PITY_COUNTER_PREFIX = "GachaPityCounter_";

    [Header("Allowed Rarities")]
    [SerializeField]
    private List<MonsterType> allowedRarities = new List<MonsterType>
    {
        MonsterType.Rare,
        MonsterType.Mythic,
        MonsterType.Legend
    };

    [Header("Gacha Counter Info (Read Only)")]
    [SerializeField, Tooltip("Total number of gacha pulls")]
    private int currentTotalPulls = 0;
    [SerializeField, Tooltip("Pull count since last获得 of each rarity type")]
    private List<PityCounterDisplay> pityCounterDisplay = new List<PityCounterDisplay>();

    [Header("UI References")]
    public GachaResultPanelByEggs gachaResultPanelByEggs;

    public Action doneConfirmEvent;
    public void RollGacha()
    {
        eggAnimator = GetComponentInChildren <Animator> ();
        eggAnimator.gameObject.SetActive (true);
        StartCoroutine (nHideEggs ());
        /*
        if (!CoinManager.SpendCoins(gachaCost))
        {
            ServiceLocator.Get<UIManager>().ShowMessage("Not enough coins for gacha!", 1f);
            return;
        }

        // Increment total pull count
        totalPullCount++;
        IncrementAllPityCounters();

        Debug.Log($"[Pull #{totalPullCount}] Rolling gacha for {gachaCost} coins...");

        MonsterType chosenRarity = GetRandomRarityWithPity(out float chosenRarityPercentage);
        MonsterDataSO selectedMonster = SelectRandomMonster(chosenRarity);

        if (selectedMonster == null)
        {
            ServiceLocator.Get<UIManager>().ShowMessage("No monsters available!", 1f);
            return;
        }

        Debug.Log($"[Pull #{totalPullCount}] Got {chosenRarity} - {selectedMonster.monsterName} ({chosenRarityPercentage:F2}% chance)");

        // Reset pity counter for the obtained rarity
        ResetPityCounter(chosenRarity);
        */

        MonsterDataSO selectedMonster = monsterDatabase.monsters[0];

        ShowGachaResult(selectedMonster, () => SellMonster(selectedMonster), () => SpawnMonster(selectedMonster));
    }


    /*
    private MonsterDataSO SelectRandomMonster(MonsterType rarity)
    {
        List<MonsterDataSO> candidates = monsterDatabase.monsters
            .Where(m => allowedRarities.Contains(m.monType))
            .Where(m => m.monType == rarity)
            .ToList();

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }
        
    private MonsterType GetRandomRarityWithPity(out float resultPercentage)
    {
        resultPercentage = 100f;

        // Check for Hard Pity activation for any rarity
        foreach (var rarityWeight in rarityWeights)
        {
            if (!allowedRarities.Contains(rarityWeight.type)) continue;
            if (!rarityWeight.hasHardPity) continue;

            int pulls = GetPityCounter(rarityWeight.type);
            if (pulls >= rarityWeight.hardPityAt)
            {
                Debug.Log($"Hard Pity activated for {rarityWeight.type} at pull {pulls}!");
                resultPercentage = 100f;
                return rarityWeight.type;
            }
        }

        // Filter weights to only include allowed rarities
        var validWeights = rarityWeights.Where(w => allowedRarities.Contains(w.type)).ToList();
        if (validWeights.Count == 0)
        {
            resultPercentage = 100f;
            return allowedRarities[0];
        }

        // Apply soft pity and hard pity rate adjustments
        var adjustedWeights = ApplyPityAdjustments(validWeights);

        float totalWeight = adjustedWeights.Sum(r => r.weight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        MonsterType selectedType = adjustedWeights[0].type;

        foreach (var rarity in adjustedWeights)
        {
            cumulative += rarity.weight;
            if (roll <= cumulative)
            {
                selectedType = rarity.type;
                resultPercentage = (rarity.weight / totalWeight) * 100f;
                break;
            }
        }

        return selectedType;
    }

    */

    private void SpawnMonster(MonsterDataSO monsterData)
    {
        ServiceLocator.Get<MonsterManager>().SpawnMonster(monsterData);
        doneConfirmEvent?.Invoke ();
    }

    private void SellMonster(MonsterDataSO monsterData)
    {
        ServiceLocator.Get<MonsterManager>().SellMonster(monsterData);
        doneConfirmEvent?.Invoke ();
    }

    private void ShowGachaResult(MonsterDataSO monster, System.Action onSellComplete, System.Action onSpawnComplete)
    {
        if (gachaResultPanelByEggs != null)
        {
            gachaResultPanelByEggs.Show(monster, onSellComplete, onSpawnComplete);
        }
    }

    IEnumerator nHideEggs () {
        yield return new WaitForSeconds (1.5f);
        eggAnimator.gameObject.SetActive (false);
    }

    #region HotelEggsCollectionMenu
    public void AddDoneConfirmEvent (Action value) {
        doneConfirmEvent += value;
    }
    #endregion
}
