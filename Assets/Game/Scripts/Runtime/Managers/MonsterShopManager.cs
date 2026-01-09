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
    private int indexTab = 0;

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        rarityTabController.OnTabChanged += OnRarityTabChanged;
        OnRarityTabChanged(indexTab); // Default to "All"
        detailPanel.SetActive(false);
        ClearMonsterInfo();
    }

    private void OnRarityTabChanged(int index)
    {
        indexTab = index;
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

    public void RefreshItem()
    {
        rarityTabController.OnTabChanged.Invoke(indexTab);
    }

    public void ShowAllMonsters()
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
        ReturnCardsToPool();
        activeCards.Clear();

        // Create cards
        for (int i = 0; i < list.Count; i++)
        {
            MonsterCardUI card = GetCardFromPool();
            card.Setup(list[i]);
            card.OnSelected = OnMonsterSelected;
            card.OnBuy = OnMonsterBuy;
            card.gameObject.SetActive(true);
            activeCards.Add(card);
        }

        // Check requirement & grayscale
        foreach (var card in activeCards)
        {
            if (card.monsterItemData.monsterRequirements != null)
            {
                bool canBuy = CheckBuyingRequirement(card);
                card.SetGrayscale(!canBuy);
            }
        }

        // Sort: BUYABLE → TOP
        var filtered = activeCards
            .OrderByDescending(card => !card.grayscaleObj.activeInHierarchy)
            .ToList();

        // Apply order to activeCards
        activeCards.Clear();
        activeCards.AddRange(filtered);

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].transform.SetSiblingIndex(i);
        }

        OnMonsterSelected(activeCards[0]);
        //ClearMonsterInfo();
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
        MonsterDataSO monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        if (CheckBuyingRequirement(card))
        {
            if (SaveSystem.TryBuyMonster(monsterItem))
            {
                OnMonsterSelected(card);

                // Success message
                ServiceLocator.Get<UIManager>().ShowMessage($"Bought {monsterItem.name}!", 2f);
                ServiceLocator.Get<MonsterManager>().SpawnMonster(monsterItem);

                // Update UI Coin Text
                ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();

                // Update List Monster Catalogue
                if (ServiceLocator.Get<MonsterCatalogueListUI>() != null)
                    ServiceLocator.Get<MonsterCatalogueListUI>().RefreshCatalogue();

                // Update Shop Item
                RefreshItem();
                OnMonsterSelected(card);
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

    private bool CheckBuyingRequirement(MonsterCardUI card)
    {
        var monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        if (monsterItem == null)
            return false;

        // Reference All Monster Player Have
        var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;

        // value to check if every index of Array/List is Eligible
        int valid = 0;

        // check every single current active monster to meet minimum requirement
        foreach (var required in monsterItem.monsterRequirements)
        {
            if (!required.anyTypeMonster)
            {
                int requiredValue = 0;
                for (int i = 0; i < monsters.Count; i++)
                {
                    if (required.monsterType == monsters[i].MonsterData.monType)
                    {
                        requiredValue++;
                    }
                }

                if (requiredValue >= required.minimumRequirements)
                {
                    valid++;
                }
                else
                {
                    //Debug.Log($"Failed required value = {requiredValue}/{required.minimumRequirements}");
                }
            }
            else
            {
                if (monsters.Count >= required.minimumRequirements)
                {
                    valid++;
                }
            }
        }

        if (valid == monsterItem.monsterRequirements.Length)
        {
            canBuyMonster = true;
        }
        else
        {
            canBuyMonster = false;
        }

        return canBuyMonster;
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