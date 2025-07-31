using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;

public class MonsterShopManager : MonoBehaviour
{
    [Header("Rarity Tab Controller")]
    public TabController rarityTabController;
    [Header("UI References")]
    public Transform monsterCardParent;
    public GameObject monsterCardPrefab;
    [Header("Detail Panel References")]
    [SerializeField] public GameObject detailPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Monster Data")]
    [SerializeField] private MonsterDatabaseSO monsterDatabase;

    public MonsterCardUI selectedCard;

    private void Start()
    {
        rarityTabController.OnTabChanged += OnRarityTabChanged;
        OnRarityTabChanged(0); // Default to "All"
        detailPanel.SetActive(false);
        ClearMonsterInfo();
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
        if (monsterDatabase != null && monsterDatabase.monsters != null)
        {
            Populate(monsterDatabase.monsters);
        }
    }

    private void FilterByRarity(MonsterType rarity)
    {
        if (monsterDatabase != null && monsterDatabase.monsters != null)
        {
            var filtered = monsterDatabase.monsters.Where(m => m.monType == rarity).ToList();
            Populate(filtered);
        }
    }

    private void Populate(List<MonsterDataSO> list)
    {
        ClearMonsterGrid();

        foreach (var monster in list)
        {
            GameObject obj = Instantiate(monsterCardPrefab, monsterCardParent);
            MonsterCardUI card = obj.GetComponent<MonsterCardUI>();
            card.Setup(monster);
            card.OnSelected = OnMonsterSelected;
            card.OnBuy = OnMonsterBuy;
        }

        ClearMonsterInfo(); // Reset info panel
    }

    private void OnMonsterSelected(MonsterCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        Debug.Log($"Selected Monster: {card.monsterData.monsterName}");
        detailPanel.SetActive(true);
        ShowMonsterInfo(card.monsterData);
    }

    private void OnMonsterBuy(MonsterCardUI card)
    {
        var monster = card.monsterData;

        if (SaveSystem.TryBuyMonster(monster)) // You'll need to implement this method
        {
            OnMonsterSelected(card);

            // Refresh monster inventory if you have one
            // ServiceLocator.Get<MonsterInventoryUI>().StartPopulateAllInventories();

            // Success message
            ServiceLocator.Get<UIManager>().ShowMessage($"Bought {monster.monsterName}!", 2f);
            ServiceLocator.Get<MonsterManager>().SpawnMonster(monster);
        }
        else
        {
            // Failure message
            ServiceLocator.Get<UIManager>().ShowMessage($"Not enough coins to buy {monster.monsterName}!", 2f);
        }
    }

    private void ShowMonsterInfo(MonsterDataSO monster)
    {

        titleText.text = monster.monsterName;
        priceText.text = $"Price: {monster.monsterPrice}";
        descriptionText.text = monster.description; // Assuming you have this field

    }

    private void ClearMonsterGrid()
    {
        foreach (Transform child in monsterCardParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearMonsterInfo()
    {
        titleText.text = "";
        priceText.text = "";
        descriptionText.text = "";
    }

    // Keep this method for backward compatibility with the detail panel
    public void ShowMonsterDetail(MonsterDataSO data)
    {
        detailPanel.SetActive(true);
    }
}