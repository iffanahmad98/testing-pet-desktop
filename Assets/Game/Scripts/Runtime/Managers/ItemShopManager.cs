using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    private void Start()
    {
        if (tabController != null)
        {
            tabController.OnTabChanged += OnTabChanged;
            tabController.OnTabSelected(0); // Default tab
        }
    }

    private void OnTabChanged(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabCategories.Count) return;

        ItemType category = (ItemType)System.Enum.Parse(typeof(ItemType), tabCategories[tabIndex]);
        ShowItemsByCategory(category);
    }
    private void ShowItemsByCategory(ItemType category)
    {
        ClearItemGrid();

        var filteredItems = itemDatabase.allItems.FindAll(i => i.category == category);

        foreach (var item in filteredItems)
        {
            GameObject obj = Instantiate(itemCardPrefab, itemGridParent);
            ItemCardUI card = obj.GetComponent<ItemCardUI>();
            card.Setup(item);
            card.OnSelected = OnItemSelected;
            card.OnBuy = OnItemBuy;
        }

        ClearItemInfo(); // Reset info panel
    }

    private void OnItemSelected(ItemCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);

        ShowItemInfo(card.itemData);
    }

    private void OnItemBuy(ItemCardUI card)
    {
        var item = card.itemData;

        // Reference All Monster Player Have
        var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;

        bool canBuyItem = false;

        foreach (var required in item.monsterRequirements)
        {
            if (!required.anyTypeMonster)
            {
                for (int i = 0; i < required.minimumRequirements; i++)
                {
                    if (monsters.Count >= required.minimumRequirements)
                    {
                        if (required.monsterType != monsters[i].MonsterData.monType)
                        {
                            canBuyItem = false;
                            break;
                        }
                        else
                        {
                            canBuyItem = true;
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
                    canBuyItem = true;
                else
                    canBuyItem = false;
            }
        }

        if (canBuyItem)
        {
            if (SaveSystem.TryBuyItem(item))
            {
                OnItemSelected(card);

                // Refresh all inventory views when item is bought
                ServiceLocator.Get<ItemInventoryUI>().StartPopulateAllInventories();
                // Success message
                ServiceLocator.Get<UIManager>().ShowMessage($"Bought {item.itemName}!", 2f);
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
