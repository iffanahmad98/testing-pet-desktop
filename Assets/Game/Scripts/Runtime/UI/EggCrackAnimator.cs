using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
[Serializable]
public class EggCrackGachaConfig {
    public MonsterType monsterType;
    public int chanceGet;
}

public class EggCrackAnimator : MonoBehaviour
{
    [Header ("Egg Crack Reference")]
    Animator eggAnimator;
    [Header("Gacha Configuration")]
    public MonsterDatabaseSO monsterDatabase;
    public List<RarityWeight> rarityWeights;
    [SerializeField] EggCrackGachaConfig [] eggCrackGachaConfigs;
    [Header("Allowed Rarities")]
    [SerializeField]
    private List<MonsterType> allowedRarities = new List<MonsterType>
    {
        MonsterType.Rare,
        MonsterType.Mythic,
        MonsterType.Legend
    };



    [Header("UI References")]
    public GachaResultPanelByEggs gachaResultPanelByEggs;

    public Action doneConfirmEvent;
    public void RollGacha()
    {
        eggAnimator = GetComponentInChildren <Animator> (true);
        eggAnimator.gameObject.SetActive (true);
        StartCoroutine (nHideEggs ());
       
        MonsterType monsterRarity = GetRandomRarity (eggCrackGachaConfigs);
        MonsterDataSO selectedMonster = SelectRandomMonster (monsterRarity);

        ShowGachaResult(selectedMonster, () => SellMonster(selectedMonster), () => SpawnMonster(selectedMonster));
    }

    public MonsterType GetRandomRarity(EggCrackGachaConfig[] configs)
    {
        int totalWeight = 0;

        // 1. Hitung total chance
        foreach (EggCrackGachaConfig c in configs)
            totalWeight += c.chanceGet;

        // 2. Ambil angka random
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // 3. Cari rarity berdasarkan weight
        int currentWeight = 0;
        foreach (EggCrackGachaConfig c in configs)
        {
            currentWeight += c.chanceGet;
            if (randomValue < currentWeight)
                return c.monsterType;
        }

        // fallback (harusnya tidak pernah kena)
        return configs[0].monsterType;
    }

    
    private MonsterDataSO SelectRandomMonster(MonsterType rarity)
    {
        List<MonsterDataSO> candidates = monsterDatabase.monsters
            .Where(m => allowedRarities.Contains(m.monType))
            .Where(m => m.monType == rarity)
            .ToList();

        return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
    }

    
    

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
        yield return new WaitForSeconds (1.5f); // animator HideEggs
        eggAnimator.gameObject.SetActive (false);
    }

    #region HotelEggsCollectionMenu
    public void AddDoneConfirmEvent (Action value) {
        doneConfirmEvent += value;
    }
    #endregion
}
