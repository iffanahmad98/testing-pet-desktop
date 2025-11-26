using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DecorationShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform cardParent;
    public GameObject decorationCardPrefab;

    [Header("Info Panel")]
    public TMP_Text decorationNameText;
    public TMP_Text decorationPriceText;
    public TMP_Text decorationDescText;

    [Header("Data")]
    public DecorationDatabaseSO decorations; // List of all available decorations

    private DecorationCardUI selectedCard;

    private Queue<DecorationCardUI> cardPool = new Queue<DecorationCardUI>();
    private List<DecorationCardUI> activeCards = new List<DecorationCardUI>();
    private void Awake()
    {
        InitializeCardPool();
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

    private void RefreshDecorationCards()
    {
        /*
        foreach (var card in activeCards)
        {
            ReturnCardToPool(card);
        }
        activeCards.Clear();

        foreach (var deco in decorations.allDecorations)
        {
            var card = GetPooledCard();
            card.Setup(deco);
            card.OnSelected = OnDecorationSelected;
            card.OnApplyClicked = OnDecorationApply;
            card.OnCancelApplied = OnDecorationCancel;
            card.OnBuyClicked = OnDecorationBuy;

            activeCards.Add(card);
        }
        */
        // Destroy all existing cards
        
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

                activeCards.Add(card);
                totalCount++;
            }
       

        ClearInfo();
    }


    private void OnDecorationSelected(DecorationCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

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
        DecorationUIFixHandler.SetDecorationStats (card.DecorationData.decorationID);

        RefreshDecorationCards();
        OnDecorationSelected(card);
    }

    private void OnDecorationCancel(DecorationCardUI card)
    {
        SaveSystem.ToggleDecorationActiveState(card.DecorationData.decorationID, false);
        SaveSystem.SaveAll();

        ServiceLocator.Get<DecorationManager>()?.RemoveActiveDecoration(card.DecorationData.decorationID);

        ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled '{card.DecorationData.decorationName}' decoration.");
        DecorationUIFixHandler.SetDecorationStats (card.DecorationData.decorationID);

        card.UpdateState();
        selectedCard = null;
        ClearInfo();
    }

    private void OnDecorationBuy(DecorationCardUI card)
    {
        var deco = card.DecorationData;

        if (SaveSystem.TryPurchaseDecoration(card.DecorationData))
        {
            
            SaveSystem.ToggleDecorationActiveState(card.DecorationData.decorationID, true);
            ServiceLocator.Get<DecorationManager>()?.ApplyDecorationByID(card.DecorationData.decorationID);
            DecorationUIFixHandler.SetDecorationStats (card.DecorationData.decorationID);
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought and applied '{deco.decorationName}'!");
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy decoration!");
        }

        SaveSystem.SaveAll();
        RefreshDecorationCards();
        OnDecorationSelected(card);
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

    
}
