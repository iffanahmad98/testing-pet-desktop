using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ItemShopManager : MonoBehaviour
{
    [Header("Tab Controller")]
    public TabController tabController;
    public List<string> tabCategories; // Must match TabController index order

    [Header("Item Display")]
    public GameObject itemCardPrefab;
    public Transform itemGridParent;
    [Header("Inventory Reference")]
    public ItemInventoryUI itemInventoryUI;

    [Header("Item Info Panel")]
    public TMP_Text itemNameText;
    public TMP_Text itemPriceText;
    public TMP_Text itemDescText;
    public TMP_Text itemFullnessText;
    public Image itemInfoIcon;


    [Header("Data")]
    public ItemDatabaseSO itemDatabase; // Optional, if you have a database scriptable object

    private ItemCardUI selectedCard;
    private bool canBuyItem = false;
    private int indexTab = 0;

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    void Start()
    {
        if (tabController != null)
        {
            tabController.OnTabChanged += OnTabChanged;
            tabController.OnTabSelected(indexTab); // Default tab
        }
    }

    public void RefreshItem()
    {
        tabController.OnTabChanged.Invoke(indexTab);
    }

    private void OnTabChanged(int tabIndex)
    {
        indexTab = tabIndex;

        if (tabIndex < 0 || tabIndex >= tabCategories.Count) return;

        ItemType category = (ItemType)System.Enum.Parse(typeof(ItemType), tabCategories[tabIndex]);
        ShowItemsByCategory(category);
    }
    private void ShowItemsByCategory(ItemType category)
    {
        ClearItemGrid();

        var filteredItems = itemDatabase.allItems.FindAll(i => i.category == category);
        List<ItemCardUI> activeCards = new List<ItemCardUI>();
        foreach (var item in filteredItems)
        {
            GameObject obj = Instantiate(itemCardPrefab, itemGridParent);
            ItemCardUI card = obj.GetComponent<ItemCardUI>();
            card.Setup(item);
            card.OnSelected = OnItemSelected;
            card.OnBuy = OnItemBuy;
            activeCards.Add(card);
        }

        // Check requirement & grayscale
        foreach (var card in activeCards)
        {
            if (card.itemData.monsterRequirements != null)
            {
                bool canBuy = CheckBuyingRequirement(card);
                //card.SetGrayscale(!canBuy);
                
                card.SetGrayscale(false); // DEBUG ONLY
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

        OnItemSelected(activeCards[0]);
        //ClearItemInfo(); // Reset info panel
    }

    private void OnItemSelected(ItemCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        Debug.Log("Selected Item");

        MonsterManager.instance.audio.PlaySFX("button_click");

        selectedCard = card;
        selectedCard.SetSelected(true);

        ShowItemInfo(card.itemData);
    }

    private void OnItemBuy(ItemCardUI card)
    {
        var item = card.itemData;

        //if (CheckBuyingRequirement(card))
        if (true) // DEBUG ONLY
        {
            if (SaveSystem.TryBuyItem(item))
            {
                OnItemSelected(card);

                MonsterManager.instance.audio.PlaySFX("buy");

                // Refresh all inventory views when item is bought
                ServiceLocator.Get<ItemInventoryUI>().StartPopulateAllInventories();
                // Success message
                ServiceLocator.Get<UIManager>().ShowMessage($"Bought {item.itemName}!", 2f);
                // Update UI Coin Text
                ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();
            }
            else
            {
                // Failure message
                ServiceLocator.Get<UIManager>().ShowMessage($"Not enough coins to buy {item.itemName}!", 2f);
            }
        }
        else
        {
            Debug.Log("Minimum Requirement Monster not enough!");
        }
    }

    private bool CheckBuyingRequirement(ItemCardUI card)
    {
        var itemData = card.itemData;

        if (itemData == null)
            return false;

        // Reference All Monster Player Have
        var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;

        // value to check if every index of Array/List is Eligible
        int valid = 0;

        // check every single current active monster to meet minimum requirement
        foreach (var required in itemData.monsterRequirements)
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
                    //Debug.Log($"{required.monsterType} Failed required value = {requiredValue}/{required.minimumRequirements}");
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

        if (valid == itemData.monsterRequirements.Length)
        {
            canBuyItem = true;
        }
        else
        {
            canBuyItem = false;
        }

        return canBuyItem;
    }

    private void ShowItemInfo(ItemDataSO item)
    {
        itemNameText.text = item.itemName;
        itemPriceText.text = $"Price: {item.price}";
        itemDescText.text = item.description;
        itemFullnessText.text = $"Fullness: {item.nutritionValue}";
        if (itemInfoIcon != null)
        {
            itemInfoIcon.sprite = item.itemImgs[0];
            itemInfoIcon.enabled = item.itemImgs[0] != null;
        }
    }

    private void ClearItemGrid()
    {
        foreach (Transform child in itemGridParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ClearItemInfo()
    {
        itemNameText.text = "";
        itemPriceText.text = "";
        itemDescText.text = "";
        itemFullnessText.text = "";
        if (itemInfoIcon != null)
        {
            itemInfoIcon.sprite = null;
            itemInfoIcon.enabled = false;
        }
    }
}
