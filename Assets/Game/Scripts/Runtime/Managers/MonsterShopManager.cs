using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MonsterShopManager : MonoBehaviour
{
    [Header("Rarity Tab Controller")]
    public TabController rarityTabController;
    [Header("UI References")]
    public Transform monsterCardParent;
    public GameObject monsterCardPrefab;
    [Header("Detail Panel References")]
    [SerializeField] public GameObject detailUpPanel;
    [SerializeField] public GameObject detailPanel;
    [SerializeField] SkeletonGraphic monsterGraphic;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] Button buyButton;
    [SerializeField] TextMeshProUGUI rarityText;
    [Header("Monster Data")]
    [SerializeField] private ItemDatabaseSO monsterItemDatabase;

    public MonsterCardUI selectedCard;

    // Object pool for monster cards
    private List<MonsterCardUI> activeCards = new List<MonsterCardUI>();
    private bool canBuyMonster = false;
    private int indexTab = 0;

    private readonly WaitForEndOfFrame waitEndOfFrame = new();

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        /*
        rarityTabController.OnTabChanged += OnRarityTabChanged;
        OnRarityTabChanged(indexTab); // Default to "All"
        detailPanel.SetActive(false);
        ClearMonsterInfo();
        */
        buyButton.onClick.AddListener (ClickBuyPreviewButton);
        //Invoke ("nStart", 0.5f);
        nStart();
    }

    void nStart () {
        rarityTabController.OnTabChanged += OnRarityTabChanged;
        //OnRarityTabChanged(indexTab); // Default to "All"
        detailPanel.SetActive(false);
        detailUpPanel.SetActive (false);
        //ClearMonsterInfo();

        StartCoroutine(InitializeCardPool());
    }

    private IEnumerator InitializeCardPool()
    {
        yield return new WaitUntil(() => SaveSystem.IsLoadFinished);

        for (int i = 0; i < monsterItemDatabase.allItems.Count; i++)
        {
            int temp = i;
            GameObject cardObj = Instantiate(monsterCardPrefab, monsterCardParent);
            MonsterCardUI card = cardObj.GetComponent<MonsterCardUI>();
            card.Setup(monsterItemDatabase.allItems[temp]);

            card.OnSelected = OnMonsterSelected;
            card.OnBuy = OnMonsterBuy;

            if (SaveSystem.UiSaveData.MonsterShopCards.Count < monsterItemDatabase.allItems.Count)
                SaveSystem.UiSaveData.MonsterShopCards.Add(new()
                {
                    Id = card.monsterItemData.ItemId
                });

            card.gameObject.SetActive(true);
            activeCards.Add(card);
        }

        OnRarityTabChanged(0);
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
        rarityTabController.OnTabChanged?.Invoke(indexTab);
    }

    public void ShowAllMonsters()
    {
        StartCoroutine(Populate(ItemType.Medicine, true));
    }

    private void FilterByRarity(ItemType rarity)
    {

        StartCoroutine(Populate(rarity));
    }

    private IEnumerator Populate(ItemType category, bool ignoreCategory = false)
    {
        // Check requirement & grayscale
        foreach (var card in activeCards)
        {
            if (card.monsterItemData.category != category && !ignoreCategory)
            {
                card.gameObject.SetActive(false);
                continue;
            }

            card.gameObject.SetActive(true);
            bool canBuy = CheckBuyingRequirementNewVersion(card);
            card.SetCanBuy (canBuy);
        }

        yield return waitEndOfFrame;

        // sort by price
        activeCards = activeCards.OrderByDescending(c => c.IsCanBuy).ThenBy(c => c.monsterItemData.price).ToList();

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            int temp = i;

            var currentCard = activeCards[temp];
            currentCard.transform.SetSiblingIndex(temp);

            // If it's already grayscaled, we know it's not buyable.
            if (!currentCard.IsCanBuy) continue;

            yield return waitEndOfFrame;

            if (!SaveSystem.UiSaveData.GetShopCardData(ShopType.MonsterShop, currentCard.monsterItemData.itemID).IsOpened)
            {
                ServiceLocator.Get<UIManager>().InitUnlockedMenuVfx(currentCard.GetComponent<RectTransform>());

                SaveSystem.UiSaveData.SetShopCardsOpenState(ShopType.MonsterShop, currentCard.monsterItemData.itemID, true);
            }
        }

        //OnMonsterSelected(activeCards[0]);
        //ClearMonsterInfo();
    }

    private void OnMonsterSelected(MonsterCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        MonsterManager.instance.audio.PlaySFX("button_click");
        selectedCard = card;
        selectedCard.SetSelected(true);
        Debug.Log($"Selected Monster: {card.monsterItemData.itemName}");
        detailPanel.SetActive(true);
        detailUpPanel.SetActive (true);
        ShowMonsterInfo(card.monsterItemData);
    }

    private void OnMonsterBuy(MonsterCardUI card)
    {
        MonsterDataSO monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        //if (CheckBuyingRequirement(card))
        
        
        if (true) // DEBUG ONLY
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

                MonsterManager.instance.audio.PlaySFX("buy");
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

    private void ClickBuyPreviewButton () {
        OnMonsterBuy (selectedCard);
    }
    
    private bool CheckBuyingRequirementNewVersion (MonsterCardUI card) {
        var monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        if (monsterItem == null)
            return false;

        // Reference All Monster Player Have
        // var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;
        var monsters = MonsterManagerEligible.Instance.GetListMonsterDataSO ();
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

        Debug.Log ("Total valid "+monsterItem.name + valid + monsters.Count);
        if (valid == monsterItem.monsterRequirements.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckBuyingRequirement(MonsterCardUI card)
    {
        var monsterItem = ServiceLocator.Get<MonsterManager>().monsterDatabase.GetMonsterByID(card.monsterItemData.itemName);

        if (monsterItem == null)
            return false;

        // Reference All Monster Player Have
        // var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;
        var monsters = MonsterManagerEligible.Instance.GetListMonsterDataSO ();
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
        string rarity = monster.category.ToString ().Replace("Monster", "");
        monsterGraphic.skeletonDataAsset = monster.skeletonDataAsset;
        monsterGraphic.Initialize(true);
        monsterGraphic.AnimationState.SetAnimation(0, "idle", true);

        titleText.text = monster.itemName;
       // priceText.text = $"Price: {monster.price}";
        priceText.text = monster.price.ToString ();
        descriptionText.text = monster.description; // Assuming you have this field
        
        rarityText.text = rarity;

        bool canBuy = CheckBuyingRequirementNewVersion(selectedCard);
        if (canBuy) {
            buyButton.interactable = true;
        } else {
            buyButton.interactable = false;
        }
    }

    private void ClearMonsterInfo()
    {
        titleText.text = "";
        priceText.text = "";
        descriptionText.text = "";
        rarityText.text = "";
    }

    // Keep this method for backward compatibility with the detail panel
    public void ShowMonsterDetail(MonsterDataSO data)
    {
        detailPanel.SetActive(true);
        detailUpPanel.SetActive (true);
    }
}