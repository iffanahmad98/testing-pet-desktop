using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class DecorationShopManager : MonoBehaviour
{
    public static DecorationShopManager instance;

    [Header("UI References")]
    public Transform cardParent;
    public GameObject decorationCardPrefab;
    List<string> listTreeDecoration1 = new List<string>();
    DecorationCardUI treeDecoration1;
    string lastLoadTreeDecoration1;

    [Header("Info Panel")]
    public TMP_Text decorationNameText;
    public TMP_Text decorationPriceText;
    public TMP_Text decorationDescText;

    [Header("Data")]
    public DecorationDatabaseSO decorations; // List of all available decorations

    private DecorationCardUI selectedCard;
    private bool canBuyItem = false;

    private List<DecorationCardUI> activeCards = new List<DecorationCardUI>();

    private readonly WaitForEndOfFrame waitEndOfFrame = new();

    private void Awake()
    {
        instance = this;
        ServiceLocator.Register(this);
        
        LoadListTreeDecoration1();
        
        StartCoroutine( InitializeCardPool());

    }

    private void Start()
    {
        RefreshDecorationCards();
    }

    private IEnumerator InitializeCardPool()
    {
        //int initialPoolSize = Mathf.Max(10, decorations?.allDecorations?.Count ?? 10);

        yield return new WaitUntil(()=> SaveSystem.IsLoadFinished);

        for (int i = 0; i < decorations.allDecorations.Count; i++)
        {
            int temp = i;
            GameObject cardObj = Instantiate(decorationCardPrefab, cardParent);
            DecorationCardUI card = cardObj.GetComponent<DecorationCardUI>();

            card.Setup(decorations.allDecorations[temp]);
            card.OnSelected = OnDecorationSelected;
            card.OnApplyClicked = OnDecorationApply;
            card.OnCancelApplied = OnDecorationCancel;
            card.OnBuyClicked = OnDecorationBuy;

            if (SaveSystem.UiSaveData.DecorationShopCards.Count < decorations.allDecorations.Count)
                SaveSystem.UiSaveData.DecorationShopCards.Add(new()
                {
                    Id = card.DecorationData.decorationID
                });

            card.gameObject.SetActive(false);
            activeCards.Add(card);

            //cardPool.Enqueue(card);
        }
    }

    public void RefreshItem()
    {
        RefreshDecorationCards(true);
    }

    IEnumerator WaitRefreshDecorationCards(bool eligibleBuyVfx = false)
    {
        foreach (var deco in activeCards)
        {
            deco.gameObject.SetActive(true);
            deco.UpdateState();
            bool canBuy = CheckBuyingRequirement(deco);
            deco.SetCanBuy(canBuy);
        }

        // Sort: BUYABLE → TOP
        activeCards = activeCards
            .OrderByDescending(card => !card.grayscaleObj.activeInHierarchy)
            .ToList();

        yield return waitEndOfFrame;    //Wait set dirty UI at the end of frame

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            int temp = i;

            var currentCard = activeCards[temp];
            currentCard.transform.SetSiblingIndex(temp);

            // If it's already grayscaled, we know it's not buyable.
            if (!currentCard.IsCanBuy) continue;

            yield return waitEndOfFrame;    //Wait set dirty UI at the end of frame

            if (eligibleBuyVfx)
            {
                if (!SaveSystem.UiSaveData.GetShopCardData(ShopType.DecorationShop, currentCard.DecorationData.decorationID).IsOpened)
                {
                    ServiceLocator.Get<UIManager>().InitUnlockedMenuVfx(currentCard.GetComponent<RectTransform>());

                    SaveSystem.UiSaveData.SetShopCardsOpenState(ShopType.DecorationShop, currentCard.DecorationData.decorationID, true);
                }
            }
        }

        // memberikan tombol terakhir treeDecoration1 (Pas awal load data + nampilkan menu)
        if (!treeDecoration1 && lastLoadTreeDecoration1 != "") { Debug.Log("Decoration Tree : " + lastLoadTreeDecoration1); treeDecoration1 = GetDecorationCardById(lastLoadTreeDecoration1); lastLoadTreeDecoration1 = ""; }
        OnDecorationSelected(activeCards[0]);
        //ClearInfo();
    }

    private void RefreshDecorationCards(bool eligibleBuyVfx = false)
    {
        StartCoroutine(WaitRefreshDecorationCards(eligibleBuyVfx));
    }


    private void OnDecorationSelected(DecorationCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        MonsterManager.instance.audio.PlaySFX("button_click");

        selectedCard = card;
        selectedCard.SetSelected(true);
        ShowDecorationInfo(card.DecorationData);


    }

    private void OnDecorationApply(DecorationCardUI card)
    {
        var decoID = card.DecorationData.decorationID;

        SaveSystem.ToggleDecorationActiveState(card.DecorationData.decorationID, true);
        ServiceLocator.Get<DecorationManager>()?.ApplyDecorationByID(decoID);
        ServiceLocator.Get<UIManager>()?.ShowMessage($"Applied '{card.DecorationData.decorationName}' decoration!");
        DecorationUIFixHandler.SetDecorationStats(card.DecorationData.decorationID);

        if (decoID == "rainbowPot")
        {
            MonsterManager.instance.audio.PlaySFX("rainbow_pot");
        }

        RefreshDecorationCards();
        OnDecorationSelected(card);


        ReplaceDecoration(card.DecorationData.decorationID);

    }

    private void OnDecorationCancel(DecorationCardUI card)
    {
        SaveSystem.ToggleDecorationActiveState(card.DecorationData.decorationID, false);
        SaveSystem.SaveAll();

        ServiceLocator.Get<DecorationManager>()?.RemoveActiveDecoration(card.DecorationData.decorationID);

        ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled '{card.DecorationData.decorationName}' decoration.");
        DecorationUIFixHandler.SetDecorationStats(card.DecorationData.decorationID);

        // Debug.Log ("Decoration Replace Canceling " + card.DecorationData.decorationID);

        card.UpdateState();
        selectedCard = null;
        ClearInfo();

        RefreshDecorationCards(); // harus paling belakang, untuk melakukan refresh kartu replace.
    }

    private void OnDecorationBuy(DecorationCardUI card)
    {
        var deco = card.DecorationData;
        
        if (CheckBuyingRequirement(card))
        {
            if (SaveSystem.TryPurchaseDecoration(card.DecorationData))
            {

                SaveSystem.ToggleDecorationActiveState(card.DecorationData.decorationID, true);
                ServiceLocator.Get<DecorationManager>()?.ApplyDecorationByID(card.DecorationData.decorationID);
                DecorationUIFixHandler.SetDecorationStats(card.DecorationData.decorationID);
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought and applied '{deco.decorationName}'!");

                // Update UI Coin Text
                ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();
                MonsterManager.instance.audio.PlaySFX("buy");
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy decoration!");
            }
        }
        else
        {
            Debug.Log("Minimum Requirement Monster not enough!");
        }

        SaveSystem.SaveAll();
        RefreshDecorationCards();
        OnDecorationSelected(card);

        ReplaceDecoration(card.DecorationData.decorationID);



    }

    private bool CheckBuyingRequirement(DecorationCardUI card)
    {
        var itemData = card.DecorationData;

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

    private void ShowDecorationInfo(DecorationDataSO deco)
    {
        
        decorationNameText.text = deco.decorationName;
        // decorationPriceText.text = $"Price: {deco.price}";
        decorationPriceText.text = $"Price:\t<color=orange>{deco.price}</color>";
        decorationDescText.text = deco.description;
    }

    private void ClearInfo()
    {
        decorationNameText.text = "";
        decorationPriceText.text = "";
        decorationDescText.text = "";
    }

    #region ReplaceDecoration

    void LoadListTreeDecoration1()
    {
        listTreeDecoration1.Add("banyanTree");
        listTreeDecoration1.Add("blossomTree");
    }

    // this:
    public void ReplaceDecoration(string id)
    {
        if (listTreeDecoration1.Contains(id))
        {
            if (treeDecoration1)
            {
                OnDecorationCancel(treeDecoration1);
            }

            treeDecoration1 = GetDecorationCardById(id);
        }
    }

    // DecorationCardUI (Start)
    public void SetLastLoadTreeDecoration1(string id)
    {
        if (listTreeDecoration1.Contains(id))
        {
            lastLoadTreeDecoration1 = id;
        }
    }

    #endregion
    #region Utility
    DecorationCardUI GetDecorationCardById(string id)
    {
        foreach (DecorationCardUI deco in activeCards)
        {
            if (deco.DecorationData.decorationID == id)
            {
                return deco;
            }
        }
        return null;
    }
    #endregion

}
