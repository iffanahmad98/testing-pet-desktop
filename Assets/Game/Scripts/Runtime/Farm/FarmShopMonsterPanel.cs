using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FarmShopMonsterPanel : FarmShopPanelBase
{
    [SerializeField] Transform parentPanel;
    [SerializeField] Image panel;
    [SerializeField] Image idlePanel;
    [SerializeField] Sprite onPanel, offPanel;
    [SerializeField] FarmFacilitiesDatabaseSO itemDatabaseSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardParent;
    List<ItemCard> listCardClone = new();
    bool allItemsLoaded = false;
    bool firstSetup = false;

    [Header("Selected Item")]
    [SerializeField] Image selectedItemPanel;
    [SerializeField] Image infoIcon;
    [SerializeField] Image smallPodium;
    [SerializeField] TMP_Text infoPriceText;
    [SerializeField] TMP_Text infoHiredText;
    [SerializeField] TMP_Text infoDetailText;
    GameObject cloneMotionPodium;
    ItemCard selectedItemCard;
    [SerializeField] ToggleGroup cardGroup;

    [Header("Data")]
    PlayerConfig playerConfig;
    List<HiredFarmFacilityData> listHiredFarmFacilityData = new List<HiredFarmFacilityData>();

    [System.Serializable]
    public class ItemCard
    {
        public GameObject cloneCard;
        public GameObject cloneMotion;
        public TMP_Text title;
        public Image icon;
        public Image smallPodium;
        public Button buyButton;
        public TMP_Text priceText;
        public FarmFacilitiesDataSO dataSO;
        public RequirementTipClick requirementTipClick;
    }

    void Start()
    {
        playerConfig = SaveSystem.PlayerConfig;
        LoadAllDatas();
    }

    public override void ShowPanel()
    {
        if (!firstSetup)
        {
            firstSetup = true;
            CoinManager.AddCoinChangedRefreshEvent(CheckEligibleAllCards);
        }

        panel.enabled = true;
        idlePanel.gameObject.SetActive(false);
        panel.transform.SetSiblingIndex(parentPanel.childCount - 1);
        selectedItemPanel.gameObject.SetActive(true);
        InstantiateAllItems();
    }

    public override void HidePanel()
    {
        panel.enabled = false;
        idlePanel.gameObject.SetActive(true);
        selectedItemPanel.gameObject.SetActive(false);
    }

    void InstantiateAllItems()
    {
        if (!allItemsLoaded)
        {
            allItemsLoaded = true;
            bool isStarter = false;
            foreach (FarmFacilitiesDataSO dataSO in itemDatabaseSO.GetListFarmFacilitiesDataSO())
            {
                GameObject clone = GameObject.Instantiate(cardPrefab);
                GameObject cloneMotionSample = GameObject.Instantiate(dataSO.facilityUIPrefab);
                ItemCard newItemCard = new ItemCard();
                newItemCard.cloneCard = clone;
                newItemCard.cloneMotion = cloneMotionSample;
                newItemCard.title = clone.transform.Find("Title").gameObject.GetComponent<TMP_Text>();
                newItemCard.icon = clone.transform.Find("Icon").gameObject.GetComponent<Image>();
                newItemCard.smallPodium = clone.transform.Find("SmallPodium").gameObject.GetComponent<Image>();
                newItemCard.buyButton = clone.transform.Find("BuyButton").gameObject.GetComponent<Button>();
                newItemCard.priceText = newItemCard.buyButton.gameObject.transform.Find("PriceText").gameObject.GetComponent<TMP_Text>();
                newItemCard.requirementTipClick = clone.transform.Find ("RequirementTipClick").gameObject.GetComponent <RequirementTipClick> ();
                newItemCard.dataSO = dataSO;

                cloneMotionSample.transform.SetParent(newItemCard.smallPodium.gameObject.transform);
                cloneMotionSample.transform.localPosition = dataSO.facilityCardUILocalPosition;
                cloneMotionSample.transform.localScale = dataSO.facilityCardUILocalScale;
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
        // itemCard.icon.sprite = itemCard.dataSO.RewardSprite;
        itemCard.title.text = itemCard.dataSO.facilityName;
        // itemCard.priceText.text = itemCard.dataSO.price.ToString ();
        HiredFarmFacilityData hiredFarmFacilityData = playerConfig.GetHiredFarmFacilityData(itemCard.dataSO.id);

        if (hiredFarmFacilityData != null)
        {
            if (hiredFarmFacilityData.hired < itemCard.dataSO.maxHired)
            {
                itemCard.priceText.text = itemCard.dataSO.GetHiredPrice(hiredFarmFacilityData.hired).ToString();
            }
            else
            {
                itemCard.priceText.text = itemCard.dataSO.GetHiredPrice(hiredFarmFacilityData.hired - 1).ToString();
            }
        }
        else
        {
            itemCard.priceText.text = itemCard.dataSO.GetHiredPrice(0).ToString();
        }

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
            ShowInformationSelectedItem(false);
        }
        else
        {

        }


    }

    void ShowInformationSelectedItem(bool playJump = false)
    {
        selectedItemPanel.gameObject.SetActive(true);
        FarmFacilitiesDataSO itemData = selectedItemCard.dataSO;
        // infoIcon.sprite = itemData.RewardSprite;
        HiredFarmFacilityData hiredFarmFacilityData = playerConfig.GetHiredFarmFacilityData(itemData.id);
        if (hiredFarmFacilityData != null)
        {
            if (hiredFarmFacilityData.hired < itemData.maxHired)
            {
                infoPriceText.text = itemData.GetHiredPrice(hiredFarmFacilityData.hired).ToString();
            }
            else
            {
                infoPriceText.text = itemData.GetHiredPrice(hiredFarmFacilityData.hired - 1).ToString();
            }
        }
        else
        {
            infoPriceText.text = itemData.GetHiredPrice(0).ToString();
        }


        if (playerConfig.GetHiredFarmFacilityData(itemData.id) != null)
        {
            int hired = playerConfig.GetHiredFarmFacilityData(itemData.id).hired;
            infoHiredText.text = hired.ToString() + " / " + itemData.maxHired.ToString();

        }
        else
        {
            infoHiredText.text = "0" + " / " + itemData.maxHired.ToString();
        }
        infoDetailText.text = itemData.detailText;
        if (cloneMotionPodium) { Destroy(cloneMotionPodium); }
        cloneMotionPodium = GameObject.Instantiate(itemData.facilityUIPrefab);
        cloneMotionPodium.transform.SetParent(smallPodium.gameObject.transform);
        cloneMotionPodium.transform.localScale = itemData.facilityPodiumUILocalScale;
        cloneMotionPodium.transform.localPosition = itemData.facilityPodiumUILocalPosition;
        cloneMotionPodium.SetActive(true);

        if (playJump)
        {
            SkeletonGraphic npcSkeletonGraphicCard = selectedItemCard.cloneMotion.GetComponent<SkeletonGraphic>();
            var stateCard = npcSkeletonGraphicCard.AnimationState;
            stateCard.SetAnimation(0, "jumping", false);
            stateCard.AddAnimation(0, "idle", true, 0f);
            npcSkeletonGraphicCard.Update(0);
            // Info Podium :
            SkeletonGraphic npcSkeletonGraphic = cloneMotionPodium.GetComponent<SkeletonGraphic>();
            var state = npcSkeletonGraphic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            npcSkeletonGraphic.Update(0);
        }
    }
    #endregion
    #region Buy Features
    public void OnBuyItem(ItemCard itemCard)
    {
        FarmFacilitiesDataSO dataSO = itemCard.dataSO;
        if (playerConfig.GetHiredFarmFacilityData(dataSO.id) != null)
        {
            int totalHired = playerConfig.GetHiredFarmFacilityData(dataSO.id).hired;
            if (dataSO.IsHiredEligible(totalHired))
            {
                MonsterManager.instance.audio.PlaySFX("buy");

                playerConfig.AddHiredFarmFacilityData(dataSO.id, 1);
                CoinManager.SpendCoins(dataSO.price);
                SaveSystem.SaveAll();
                SpawnFarmFacility(dataSO.id, totalHired);
            }
        }
        else
        {
            if (dataSO.IsHiredEligible(0))
            {
                playerConfig.AddHiredFarmFacilityData(dataSO.id, 1);
                CoinManager.SpendCoins(dataSO.price);
                SaveSystem.SaveAll();
                SpawnFarmFacility(dataSO.id, 0);
            }
        }

        itemCard.cloneCard.transform.DOKill();
        itemCard.cloneCard.transform.localScale = Vector3.one;
        itemCard.cloneCard.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 10, 1);
        ShowInformationSelectedItem(true);
    }

    #endregion
    #region Eligible
    void CheckEligibleAllCards()
    {
        foreach (ItemCard itemCard in listCardClone)
        {
            FarmFacilitiesDataSO dataSO = itemCard.dataSO;
            if (playerConfig.GetHiredFarmFacilityData(dataSO.id) != null)
            {
                int totalHired = playerConfig.GetHiredFarmFacilityData(dataSO.id).hired;
                Debug.Log($"Hired Total : {totalHired} max hired {dataSO.maxHired}");
                if (dataSO.IsHiredEligible(totalHired) && totalHired < dataSO.maxHired)
                {
                    itemCard.buyButton.interactable = true;

                    itemCard.requirementTipClick.gameObject.SetActive (false);
                    
                }
                else
                {
                    itemCard.buyButton.interactable = false;

                    itemCard.requirementTipClick.gameObject.SetActive (true);
                    itemCard.requirementTipClick.dataSO = dataSO.GetRequirementTipData (totalHired);
                }
            }
            else
            {
                if (dataSO.IsHiredEligible(0))
                {
                    itemCard.buyButton.interactable = true;

                    itemCard.requirementTipClick.gameObject.SetActive (false);
                }
                else
                {
                    itemCard.buyButton.interactable = false;

                    itemCard.requirementTipClick.gameObject.SetActive (true);
                    itemCard.requirementTipClick.dataSO = dataSO.GetRequirementTipData (0);
                }
            }

        }
    }
    #endregion
    #region World Spawner
    void SpawnFarmFacility(string id, int hiredElement)
    {
        FarmFacilitiesDataSO data = itemDatabaseSO.GetFarmFacilitiesDataSO(id);
        GameObject worldClone = GameObject.Instantiate(data.facilityPrefab);
        worldClone.transform.position = data.facilitySpawnPositions[hiredElement];
        worldClone.SetActive(true);
    }

    #endregion
    #region Data
    void LoadAllDatas()
    {
        listHiredFarmFacilityData = playerConfig.hiredFarmFacilitiesData;
        foreach (HiredFarmFacilityData data in listHiredFarmFacilityData)
        {
            for (int x = 0; x < data.hired; x++)
            {
                SpawnFarmFacility(data.id, x);
            }
        }
    }
    #endregion
}
