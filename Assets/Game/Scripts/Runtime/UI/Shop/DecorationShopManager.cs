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

    private Queue<DecorationCardUI> cardPool = new Queue<DecorationCardUI>();
    private List<DecorationCardUI> activeCards = new List<DecorationCardUI>();

    private WaitForEndOfFrame waitEndOfFrame = new();

    private void Awake()
    {
        instance = this;
        LoadListTreeDecoration1();
        InitializeCardPool();

        ServiceLocator.Register(this);
    }

    private void Start()
    {
        RefreshDecorationCards();

    }

    private void InitializeCardPool()
    {
        int initialPoolSize = Mathf.Max(10, decorations?.allDecorations?.Count ?? 10);

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject cardObj = Instantiate(decorationCardPrefab, cardParent);
            DecorationCardUI card = cardObj.GetComponent<DecorationCardUI>();
            card.gameObject.SetActive(false);
            cardPool.Enqueue(card);
        }
    }

    private DecorationCardUI GetPooledCard()
    {
        if (cardPool.Count > 0)
        {
            var card = cardPool.Dequeue();
            card.gameObject.SetActive(true);
            return card;
        }
        else
        {
            var cardObj = Instantiate(decorationCardPrefab, cardParent);
            return cardObj.GetComponent<DecorationCardUI>();
        }
    }

    private void ReturnCardToPool(DecorationCardUI card)
    {
        card.gameObject.SetActive(false);
        card.transform.SetAsLastSibling();
        cardPool.Enqueue(card);
    }

    public void RefreshItem()
    {
        RefreshDecorationCards(true);
    }

    IEnumerator WaitRefreshDecorationCards(bool eligibleBuyVfx = false)
    {
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        activeCards.Clear();
        selectedCard = null;

        int totalCount = 0;

        foreach (var deco in decorations.allDecorations)
        {
            GameObject cardObj = Instantiate(decorationCardPrefab, cardParent);
            DecorationCardUI card = cardObj.GetComponent<DecorationCardUI>();

            card.Setup(deco);
            card.OnSelected = OnDecorationSelected;
            card.OnApplyClicked = OnDecorationApply;
            card.OnCancelApplied = OnDecorationCancel;
            card.OnBuyClicked = OnDecorationBuy;

            bool canBuy = CheckBuyingRequirement(card);
            card.SetCanBuy(canBuy);

            activeCards.Add(card);
            totalCount++;
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
            if (currentCard.grayscaleObj.activeInHierarchy) continue;

            yield return waitEndOfFrame;    //Wait set dirty UI at the end of frame

            if (eligibleBuyVfx)
            {
                ServiceLocator.Get<UIManager>().InitUnlockedMenuVfx(currentCard.GetComponent<RectTransform>());
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

    private void ShowDecorationInfo(DecorationDataSO deco)
    {
        decorationNameText.text = deco.decorationName;
        decorationPriceText.text = $"Price: {deco.price}";
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
