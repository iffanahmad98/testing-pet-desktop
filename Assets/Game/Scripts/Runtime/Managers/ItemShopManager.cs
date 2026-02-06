using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    List<ItemCardUI> activeCards = new();
    private ItemCardUI selectedCard;
    private bool canBuyItem = false;
    private int indexTab = 0;

    private readonly WaitForEndOfFrame waitEndOfFrame = new ();

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    void Start()
    {
        StartCoroutine(InitializeCardPool());

        if (tabController != null)
        {
            tabController.OnTabChanged += OnTabChanged;
            //tabController.OnTabSelected(indexTab); // Default tab
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
        StartCoroutine(ShowItemsByCategory(category));
    }

    private IEnumerator InitializeCardPool()
    {
        yield return new WaitUntil(() => SaveSystem.IsLoadFinished);

        for (int i = 0; i < itemDatabase.allItems.Count; i++)
        {
            int temp = i;
            GameObject cardObj = Instantiate(itemCardPrefab, itemGridParent);
            ItemCardUI card = cardObj.GetComponent<ItemCardUI>();
            card.Setup(itemDatabase.allItems[temp]);

            card.OnSelected = OnItemSelected;
            card.OnBuy = OnItemBuy;

            if (SaveSystem.UiSaveData.ItemShopCards.Count < itemDatabase.allItems.Count)
                SaveSystem.UiSaveData.ItemShopCards.Add(new()
                {
                    Id = card.itemData.ItemId
                });

            card.gameObject.SetActive(false);
            activeCards.Add(card);
        }
    }

    private IEnumerator ShowItemsByCategory(ItemType category)
    {
        ClearItemGrid();

        foreach (var card in activeCards)
        {
            //Debug.Log($"{card.itemData.name} is {card.itemData.category}");

            if (card.itemData.category != category)
                continue;

            card.gameObject.SetActive(true);
            bool canBuy = CheckBuyingRequirement(card);
            card.SetCanBuy(canBuy);
        }

        yield return waitEndOfFrame;

        // sort by price
        activeCards = activeCards.OrderByDescending(c => c.isCanBuy).ThenBy(c => c.itemData.price).ToList();

        //yield return waitEndOfFrame;

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            int temp = i;

            var currentCard = activeCards[temp];
            currentCard.transform.SetSiblingIndex(temp);

            // If it's already grayscaled, we know it's not buyable.
            if (!currentCard.isCanBuy) continue;

            yield return waitEndOfFrame;

            if (!SaveSystem.UiSaveData.GetShopCardData(ShopType.ItemShop, currentCard.itemData.itemID).IsOpened)
            {
                ServiceLocator.Get<UIManager>().InitUnlockedMenuVfx(currentCard.GetComponent<RectTransform>());

                SaveSystem.UiSaveData.SetShopCardsOpenState(ShopType.ItemShop, currentCard.itemData.itemID, true);
            }
        }

        //OnItemSelected(activeCards[0]);
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

        if (CheckBuyingRequirement(card))
        // if (true) // DEBUG ONLY
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
        // var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;
        var monsters = MonsterManagerEligible.Instance.GetListMonsterDataSO ();
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
                    if (required.monsterType == monsters[i].monType)
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
       // itemPriceText.text = $"Price: {item.price}";
        itemPriceText.text = $"Price:\t<color=orange>{item.price}</color>";
        itemDescText.text = item.description;
        // itemFullnessText.text = $"Fullness: {item.nutritionValue}";
        itemFullnessText.text = $"Fullness:\t<color=orange>{item.price}</color>";
        if (itemInfoIcon != null)
        {
            itemInfoIcon.sprite = item.itemImgs[0];
            itemInfoIcon.enabled = item.itemImgs[0] != null;
        }
    }

    private void ClearItemGrid()
    {
        foreach (var child in activeCards)
        {
            child.gameObject.SetActive(false);
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
