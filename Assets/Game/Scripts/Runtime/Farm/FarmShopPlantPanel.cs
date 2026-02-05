using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FarmShopPlantPanel : FarmShopPanelBase
{
    [SerializeField] Transform parentPanel;
    [SerializeField] Image panel, idlePanel;
    [SerializeField] Sprite onPanel, offPanel;
    [SerializeField] FarmItemDatabaseSO itemDatabaseSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardParent;
    List<ItemCard> listCardClone = new();
    bool allItemsLoaded = false;

    [Header("Selected Item")]
    [SerializeField] Image selectedItemPanel;
    [SerializeField] Image infoIcon;
    [SerializeField] TMP_Text infoPriceText, infoWaterText, infoHarvestText;
    [SerializeField] TMP_Text infoDetailText;
    ItemCard selectedItemCard;
    [SerializeField] ToggleGroup cardGroup;

    [Header("Data")]
    PlayerConfig playerConfig;

    [System.Serializable]
    public class ItemCard
    {
        public GameObject cloneCard;
        public TMP_Text title;
        public Image icon;
        public Button buyButton;
        public TMP_Text priceText;
        public MagicalGarden.Inventory.ItemData itemDataSO;
    }

    void Start()
    {
        playerConfig = SaveSystem.PlayerConfig;
        CoinManager.AddCoinChangedRefreshEvent(CheckEligibleAllCards);
    }

    public override void ShowPanel()
    {
        panel.transform.SetSiblingIndex(parentPanel.childCount - 1);
        InstantiateAllItems();
        selectedItemPanel.gameObject.SetActive(true);
        idlePanel.gameObject.SetActive(true);
    }

    public override void HidePanel()
    {
        HideAllInstantiatedAllItems();
        selectedItemPanel.gameObject.SetActive(false);
        idlePanel.gameObject.SetActive(false);
    }

    void InstantiateAllItems()
    {
        if (!allItemsLoaded)
        {

            allItemsLoaded = true;
            bool isStarter = false;
            foreach (MagicalGarden.Inventory.ItemData itemDataSO in itemDatabaseSO.GetListItemData())
            {
                GameObject clone = GameObject.Instantiate(cardPrefab);
                ItemCard newItemCard = new ItemCard();
                newItemCard.cloneCard = clone;
                newItemCard.title = clone.transform.Find("Title").gameObject.GetComponent<TMP_Text>();
                newItemCard.icon = clone.transform.Find("Icon").gameObject.GetComponent<Image>();
                newItemCard.buyButton = clone.transform.Find("BuyButton").gameObject.GetComponent<Button>();
                newItemCard.priceText = newItemCard.buyButton.gameObject.transform.Find("PriceText").gameObject.GetComponent<TMP_Text>();
                newItemCard.itemDataSO = itemDataSO;
                Toggle toggle = clone.GetComponent<Toggle>();
                toggle.group = cardGroup;
                toggle.onValueChanged.AddListener(
                value => SetSelectedItem(newItemCard, value)
                );

                listCardClone.Add(newItemCard);

                if (!isStarter)
                {
                    isStarter = true;
                    // StartCoroutine (nToggleStarter (toggle));
                    toggle.isOn = true;
                    SetSelectedItem(newItemCard, true);
                }
            }
            Debug.Log("Total clone" + listCardClone.Count);
            foreach (ItemCard itemCard in listCardClone)
            {
                FillupItemCard(itemCard);
            }
        }
        else
        {
            ShowAllInstantiatedAllItems();
        }

        CheckEligibleAllCards();
    }

    void FillupItemCard(ItemCard itemCard)
    {
        itemCard.cloneCard.gameObject.transform.SetParent(cardParent);
        itemCard.title.text = itemCard.itemDataSO.ItemName;
        itemCard.icon.sprite = itemCard.itemDataSO.RewardSprite;
        itemCard.priceText.text = itemCard.itemDataSO.price.ToString();
        itemCard.cloneCard.gameObject.SetActive(true);
        itemCard.buyButton.onClick.AddListener(() => OnBuyItem(itemCard));
    }

    void ShowAllInstantiatedAllItems()
    {
        foreach (ItemCard item in listCardClone)
        {
            item.cloneCard.SetActive(true);
        }
    }

    void HideAllInstantiatedAllItems()
    {
        foreach (ItemCard item in listCardClone)
        {
            item.cloneCard.SetActive(false);
        }
    }

    #region SelectedItem
    public void SetSelectedItem(ItemCard itemCard, bool isOn)
    {
        if (isOn)
        {
            selectedItemCard = itemCard;
            ShowInformationSelectedItem();
        }
        else
        {

        }


    }

    void ShowInformationSelectedItem()
    {
        selectedItemPanel.gameObject.SetActive(true);
        MagicalGarden.Inventory.ItemData itemData = selectedItemCard.itemDataSO;
        infoIcon.sprite = itemData.RewardSprite;

        infoPriceText.text = itemData.price.ToString();
        infoWaterText.text = itemData.needHourWatering.ToString() + " hours";
        infoHarvestText.text = itemData.needHourGrow.ToString() + " hours";
        infoDetailText.text = itemData.description;
    }
    #endregion
    #region Buy Features
    public void OnBuyItem(ItemCard itemCard)
    {
        MagicalGarden.Inventory.ItemData dataSO = itemCard.itemDataSO;
        if (dataSO.IsEligible())
        {
            playerConfig.AddItemFarm(dataSO.itemId, 1);
            CoinManager.SpendCoins(dataSO.price);
            SaveSystem.SaveAll();
        }

        itemCard.cloneCard.transform.DOKill();
        itemCard.cloneCard.transform.localScale = Vector3.one;
        itemCard.cloneCard.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 10, 1);

        ShowInformationSelectedItem();
    }

    #endregion
    #region Eligible
    void CheckEligibleAllCards()
    {

        foreach (ItemCard itemCard in listCardClone)
        {
            MagicalGarden.Inventory.ItemData dataSO = itemCard.itemDataSO;
            if (dataSO.IsEligible())
            {
                itemCard.buyButton.interactable = true;
            }
            else
            {
                itemCard.buyButton.interactable = false;
            }
        }
    }
    #endregion
}
