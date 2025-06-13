using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterShopManager : MonoBehaviour
{
    [Header("Rarity Tab Controller")]
    public TabController rarityTabController;
    [Header("UI References")]
    public Transform monsterCardParent;
    public GameObject monsterCardPrefab;
    public MonsterDetailPanel detailPanel;

    [Header("Monster Data")]
    public List<MonsterDataSO> allMonsters;


    private void Start()
    {
        rarityTabController.OnTabChanged += OnRarityTabChanged;
        OnRarityTabChanged(0); // Default to "All"
        detailPanel.SetVisible(false);
    }

    private void OnRarityTabChanged(int index)
    {
        switch (index)
        {
            case 0:
                ShowAllMonsters();
                break;
            case 1:
                FilterByRarity(MonsterType.Common);
                break;
            case 2:
                FilterByRarity(MonsterType.Uncommon);
                break;
            case 3:
                FilterByRarity(MonsterType.Rare);
                break;
                // Add more cases if you support more rarities
        }
    }

    private void ShowAllMonsters()
    {
        Populate(allMonsters);
    }

    private void FilterByRarity(MonsterType rarity)
    {
        var filtered = allMonsters.Where(m => m.monType == rarity).ToList();
        Populate(filtered);
    }

    private void Populate(List<MonsterDataSO> list)
    {
        foreach (Transform child in monsterCardParent)
            Destroy(child.gameObject);

        foreach (var mon in list)
        {
            var card = Instantiate(monsterCardPrefab, monsterCardParent);
            card.GetComponent<MonsterCardUI>().Setup(mon, this);
        }
    }

    public void ShowMonsterDetail(MonsterDataSO data)
    {
        detailPanel.SetVisible(true);
        detailPanel.SetData(data);
    }
}
