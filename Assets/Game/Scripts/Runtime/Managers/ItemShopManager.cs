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

    [Header("Item Info Panel")]
    public TMP_Text itemNameText;
    public TMP_Text itemPriceText;
    public TMP_Text itemDescText;
    public TMP_Text itemFullnessText;
    public Image itemInfoIcon;

    [Header("Data")]
    public List<ItemDataSO> allItems; // Replace with your actual item source

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

        var filteredItems = allItems.FindAll(i => i.category == category);

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

        if (SaveSystem.TryBuyItem(item))
        {
            OnItemSelected(card);

            // Success message
            ServiceLocator.Get<UIManager>().ShowMessage($"Bought {item.itemName}!", 2f);
        }
        else
        {
            // Failure message
            ServiceLocator.Get<UIManager>().ShowMessage($"Not enough coins to buy {item.itemName}!", 2f);
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
