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
    [SerializeField] private ItemDatabaseSO monsterItemDatabase;

    public MonsterCardUI selectedCard;

    // Object pool for monster cards
    private List<MonsterCardUI> cardPool = new List<MonsterCardUI>();
    private List<MonsterCardUI> activeCards = new List<MonsterCardUI>();
    private bool canBuyMonster = false;

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
                FilterByRarity(ItemType.CommonMonster);
                break;
            case 2:
                FilterByRarity(ItemType.UncommonMonster);
                break;
                // Add more cases if you support more rarities
        }
    }

    private void ShowAllMonsters()
    {
        if (monsterItemDatabase != null && monsterItemDatabase.allItems != null)
        {
            Populate(monsterItemDatabase.allItems);
        }
    }

    private void FilterByRarity(ItemType rarity)
    {
        if (monsterItemDatabase != null && monsterItemDatabase.allItems != null)
        {
            var filtered = monsterItemDatabase.allItems.Where(m => m.category == rarity).ToList();
            Populate(filtered);
        }
    }

    private void Populate(List<ItemDataSO> list)
    {
        // Return all active cards to pool
        ReturnCardsToPool();

        // Get or create cards for the list
        for (int i = 0; i < list.Count; i++)
        {
            MonsterCardUI card = GetCardFromPool();
            card.Setup(list[i]);
            card.OnSelected = OnMonsterSelected;
            card.OnBuy = OnMonsterBuy;
            card.gameObject.SetActive(true);
            activeCards.Add(card);
        }

        foreach (var card in activeCards)
        {
            if (card.monsterItemData.monsterRequirements != null)
            {
                CheckBuyingRequirement(card);
                card.SetGrayscale(!canBuyMonster);
            }
        }

        ClearMonsterInfo(); // Reset info panel
    }

    private MonsterCardUI GetCardFromPool()
    {
        // Try to find an inactive card in the pool
        for (int i = 0; i < cardPool.Count; i++)
        {
            if (!cardPool[i].gameObject.activeInHierarchy)
            {
                return cardPool[i];
            }
        }

        // If no inactive card found, create a new one
        GameObject obj = Instantiate(monsterCardPrefab, monsterCardParent);
        MonsterCardUI card = obj.GetComponent<MonsterCardUI>();
        cardPool.Add(card);
        return card;
    }

    private void ReturnCardsToPool()
    {
        // Deactivate all active cards
        foreach (var card in activeCards)
        {
            card.gameObject.SetActive(false);
        }
        activeCards.Clear();

        // Clear selection
        selectedCard = null;
    }

    private void OnMonsterSelected(MonsterCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        Debug.Log($"Selected Monster: {card.monsterItemData.itemName}");
        detailPanel.SetActive(true);
        ShowMonsterInfo(card.monsterItemData);
    }

    private void OnMonsterBuy(MonsterCardUI card)
    {
        MonsterDataSO monsterItem = CheckBuyingRequirement(card);

        if (canBuyMonster)
        {
            if (SaveSystem.TryBuyMonster(monsterItem))
            {
                OnMonsterSelected(card);

                // Success message
                ServiceLocator.Get<UIManager>().ShowMessage($"Bought {monsterItem.name}!", 2f);
                ServiceLocator.Get<MonsterManager>().SpawnMonster(monsterItem);
            }
            else
            {
                // Failure message
                ServiceLocator.Get<UIManager>().ShowMessage($"Not enough coins to buy!", 2f);
            }
        }
        else
        {
            Debug.Log("Minimum Requirement Monster not enough!");
        }
    }

    private MonsterDataSO CheckBuyingRequirement(MonsterCardUI card)
    {
        var monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        if (monsterItem == null)
            return null;

        // Reference All Monster Player Have
        var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;

        foreach (var required in monsterItem.monsterRequirements)
        {
            if (!required.anyTypeMonster)
            {
                for (int i = 0; i < required.minimumRequirements; i++)
                {
                    if (monsters.Count >= required.minimumRequirements)
                    {
                        if (required.monsterType != monsters[i].MonsterData.monType)
                        {
                            canBuyMonster = false;
                            break;
                        }
                        else
                        {
                            canBuyMonster = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (monsters.Count >= required.minimumRequirements)
                    canBuyMonster = true;
                else
                    canBuyMonster = false;
            }
        }

        return monsterItem;
    }

    private void ShowMonsterInfo(ItemDataSO monster)
    {

        titleText.text = monster.itemName;
        priceText.text = $"Price: {monster.price}";
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