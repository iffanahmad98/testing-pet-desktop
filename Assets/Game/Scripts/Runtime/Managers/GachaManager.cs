using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class RarityWeight
{
    public MonsterType type;
    [Range(0f, 100f)]
    public float weight;
}

public class GachaManager : MonoBehaviour
{
    [Header("Gacha Configuration")]
    public MonsterDatabaseSO monsterDatabase;
    public List<RarityWeight> rarityWeights;
    public int gachaCost = 1000;

    [Header("Allowed Rarities")]
    [SerializeField]
    private List<MonsterType> allowedRarities = new List<MonsterType>
    {
        MonsterType.Rare,
        MonsterType.Mythic,
        MonsterType.Legend
    };
    [Header("UI References")]
    public GachaResultPanel gachaResultPanel;

    private void Awake()
    {
        ServiceLocator.Register(this);
        ValidateConfiguration();
    }


    private void OnDestroy()
    {
        ServiceLocator.Unregister<GachaManager>();
    }

    private void ValidateConfiguration()
    {
        if (monsterDatabase == null)
        {
            return;
        }

        var invalidWeights = rarityWeights.Where(w => !allowedRarities.Contains(w.type)).ToList();

        foreach (var rarity in allowedRarities)
        {
            var monstersOfRarity = monsterDatabase.monsters.Where(m => m.monType == rarity).Count();
        }
    }

    public void RollGacha()
    {
        if (!CoinManager.SpendCoins(gachaCost))
        {
            ServiceLocator.Get<UIManager>().ShowMessage("Not enough coins for gacha!", 1f);
            return;
        }
        Debug.Log($"Rolling gacha for {gachaCost} coins...");

        MonsterType chosenRarity = GetRandomRarity();
        MonsterDataSO selectedMonster = SelectRandomMonster(chosenRarity);

        if (selectedMonster == null)
        {
            ServiceLocator.Get<UIManager>().ShowMessage("No monsters available!", 1f);
            // Coins are already deducted here!
            return;
        }

        ShowGachaResult(selectedMonster, () => SellMonster(selectedMonster), () => SpawnMonster(selectedMonster));
    }

    private MonsterDataSO SelectRandomMonster(MonsterType rarity)
    {
        List<MonsterDataSO> candidates = monsterDatabase.monsters
            .Where(m => allowedRarities.Contains(m.monType))
            .Where(m => m.monType == rarity)
            .ToList();

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    private MonsterType GetRandomRarity()
    {
        // Filter weights to only include allowed rarities
        var validWeights = rarityWeights.Where(w => allowedRarities.Contains(w.type)).ToList();
        if (validWeights.Count == 0)
        {
            return allowedRarities[0];
        }

        float totalWeight = validWeights.Sum(r => r.weight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var rarity in validWeights)
        {
            cumulative += rarity.weight;
            if (roll <= cumulative)
                return rarity.type;
        }

        return validWeights[0].type; // fallback
    }

    private void SpawnMonster(MonsterDataSO monsterData)
    {
        ServiceLocator.Get<MonsterManager>().SpawnMonster(monsterData);
    }

    private void SellMonster(MonsterDataSO monsterData)
    {
        ServiceLocator.Get<MonsterManager>().SellMonster(monsterData);
    }

    private void ShowGachaResult(MonsterDataSO monster, System.Action onSellComplete, System.Action onSpawnComplete)
    {
        if (gachaResultPanel != null)
        {
            gachaResultPanel.Show(monster, onSellComplete, onSpawnComplete);
        }
    }
}