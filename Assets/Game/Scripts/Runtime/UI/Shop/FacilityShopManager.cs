using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FacilityShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform facilityParent;
    public Transform cardParent;
    public GameObject facilityCardPrefab;

    [Header("Info Panel")]
    public TMP_Text facilityNameText;
    public TMP_Text facilityPriceText;
    public TMP_Text facilityDescText;
    public TMP_Text facilityCooldownText;

    [Header("Data")]
    public FacilityDatabaseSO facilities; // List of all available facilities
    public MonsterDatabaseSO npcDatabases; // List of all NPCs

    private FacilityCardUI selectedCard;
    private float originalFacilityParentHeight;
    private RectTransform facilityParentRect;
    private FacilityManager facilityManager;

    // Object pool for facility cards
    private List<FacilityCardUI> cardPool = new List<FacilityCardUI>();
    private List<FacilityCardUI> activeCards = new List<FacilityCardUI>();

    private void Awake()
    {
        facilityParentRect = facilityParent.GetComponent<RectTransform>();
        originalFacilityParentHeight = facilityParentRect.sizeDelta.y;
    }

    private void Start()
    {
        facilityManager = ServiceLocator.Get<FacilityManager>();
        RefreshFacilityCards();
    }

    private void RefreshFacilityCards()
    {
        // Return all active cards to pool
        ReturnCardsToPool();

        // Get or create cards for each facility
        foreach (var facility in facilities.allFacilities)
        {
            FacilityCardUI card = GetCardFromPool();
            card.SetupFacility(facility);
            card.OnSelected = OnFacilitySelected;
            card.OnUseClicked = OnFacilityUse;
            card.OnBuyClicked = OnFacilityBuy;
            card.OnCancelClicked = OnFacilityCancel; // Add cancel handler
            card.gameObject.SetActive(true);
            activeCards.Add(card);
        }

        // Get or create cards for each NPC
        if (npcDatabases != null && npcDatabases.monsters != null)
        {
            foreach (var npc in npcDatabases.monsters)
            {
                FacilityCardUI card = GetCardFromPool();
                card.SetupNPC(npc);
                card.OnSelected = OnNPCSelected;
                card.OnUseClicked = OnNPCUse;
                card.OnBuyClicked = OnNPCBuy;
                card.OnCancelClicked = OnNPCCancel; // Add cancel handler
                card.gameObject.SetActive(true);
                activeCards.Add(card);
            }
        }

        int totalCount = facilities.allFacilities.Count + (npcDatabases.monsters?.Count ?? 0);
        AdjustFacilityParentHeight(totalCount);
        ClearInfo();
    }

    private FacilityCardUI GetCardFromPool()
    {
        // Try to find an inactive card in the pool
        for (int i = 0; i < cardPool.Count; i++)
        {
            if (!cardPool[i].gameObject.activeInHierarchy)
            {
                return cardPool[i];
            }
        }

        // If no inactive card found, create a new one
        GameObject cardObj = Instantiate(facilityCardPrefab, cardParent);
        FacilityCardUI card = cardObj.GetComponent<FacilityCardUI>();
        cardPool.Add(card);
        return card;
    }

    private void ReturnCardsToPool()
    {
        // Deactivate all active cards
        foreach (var card in activeCards)
        {
            card.gameObject.SetActive(false);
        }
        activeCards.Clear();

        // Clear selection
        selectedCard = null;
    }

    private void AdjustFacilityParentHeight(int facilityCount)
    {
        int rows = Mathf.CeilToInt(facilityCount / 2f);
        int baseRows = 1; // first 2 items (1 row) doesn't need extra height
        int extraRows = Mathf.Max(0, rows - baseRows);

        float newHeight = originalFacilityParentHeight + (extraRows * 248.8f);

        if (facilityParentRect != null)
        {
            Vector2 size = facilityParentRect.sizeDelta;
            size.y = newHeight;
            facilityParentRect.sizeDelta = size;
        }
    }

    private void OnFacilitySelected(FacilityCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        ShowFacilityInfo(card.FacilityData);
    }

    private void OnFacilityUse(FacilityCardUI card)
    {
        if (facilityManager != null)
        {
            bool success = facilityManager.UseFacility(card.FacilityData.facilityID);
            
            if (success)
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Used '{card.FacilityData.name}'!");
                
                // Update all cards to reflect cooldown changes
                foreach (var activeCard in activeCards)
                {
                    activeCard.UpdateState();
                }
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"'{card.FacilityData.name}' is on cooldown!");
            }
        }
    }

    private void OnFacilityBuy(FacilityCardUI card)
    {
        var facility = card.FacilityData;

        if (SaveSystem.TryPurchaseFacility(facility))
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{facility.name}'!");
            // Update all cards to reflect ownership
            foreach (var activeCard in activeCards)
            {
                activeCard.UpdateState();
            }
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy facility!");
        }

        SaveSystem.SaveAll();
        RefreshFacilityCards();
        OnFacilitySelected(card);
    }

    private void ShowFacilityInfo(FacilityDataSO facility)
    {
        facilityNameText.text = facility.name;
        facilityPriceText.text = $"Price: {facility.price}";
        facilityDescText.text = facility.description;
        facilityCooldownText.text = $"Cooldown: {facility.cooldownSeconds}s";
    }

    private void ClearInfo()
    {
        facilityNameText.text = "";
        facilityPriceText.text = "";
        facilityDescText.text = "";
        facilityCooldownText.text = "";
    }

    private void Update()
    {
        // Update all active cards every frame to handle cooldown displays
        foreach (var card in activeCards)
        {
            if (card.FacilityData != null)
            {
                card.UpdateState();
            }
            else
            {
                // For NPC cards, you'll need to pass the NPC ID
                string npcID = GetNPCIDFromCard(card);
                card.UpdateStateNPC(npcID);
            }
        }
    }

    // Add new handlers for NPCs
    private void OnNPCSelected(FacilityCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        // You'll need to create ShowNPCInfo method or modify ShowFacilityInfo to handle NPCs
    }

    private void OnNPCUse(FacilityCardUI card)
    {
        // Get NPC data from the card (you might need to store this in the card)
        // For now, assuming the card has a way to get NPC ID
        string npcID = GetNPCIDFromCard(card); // You'll need to implement this
        
        if (facilityManager != null)
        {
            bool success = facilityManager.UseFacility(npcID);
            
            if (success)
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Used NPC!");
                
                // Update all cards to reflect cooldown changes
                foreach (var activeCard in activeCards)
                {
                    if (activeCard.FacilityData != null)
                        activeCard.UpdateState();
                    else
                        activeCard.UpdateStateNPC(npcID); // Update NPC state
                }
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("NPC is on cooldown!");
            }
        }
    }

    private void OnNPCBuy(FacilityCardUI card)
    {
        string npcID = GetNPCIDFromCard(card);
        var npcData = npcDatabases.GetMonsterByID(npcID);

        if (npcData != null && SaveSystem.TryBuyMonster(npcData)) // You might need this method
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{npcData.monsterName}'!");
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy NPC!");
        }

        SaveSystem.SaveAll();
        RefreshFacilityCards();
        OnNPCSelected(card);
    }

    // Add cancel handlers
    private void OnFacilityCancel(FacilityCardUI card)
    {
        if (facilityManager != null)
        {
            facilityManager.CancelFacilityCooldown(card.FacilityData.facilityID);
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled cooldown for '{card.FacilityData.facilityName}'!");
            
            // Update all cards
            foreach (var activeCard in activeCards)
            {
                activeCard.UpdateState();
            }
        }
    }

    private void OnNPCCancel(FacilityCardUI card)
    {
        string npcID = GetNPCIDFromCard(card);
        
        if (facilityManager != null)
        {
            facilityManager.CancelFacilityCooldown(npcID);
            ServiceLocator.Get<UIManager>()?.ShowMessage("Cancelled NPC cooldown!");
            
            // Update all cards
            foreach (var activeCard in activeCards)
            {
                if (activeCard.FacilityData != null)
                    activeCard.UpdateState();
                else
                    activeCard.UpdateStateNPC(npcID);
            }
        }
    }

    // Helper method - you'll need to implement this based on how you store NPC data in the card
    private string GetNPCIDFromCard(FacilityCardUI card)
    {

        // This depends on how you want to store/retrieve NPC ID from the card
        // You might need to add a property to FacilityCardUI to store NPC data
        // For now, returning placeholder
        return card.npc?.id ?? ""; // Replace with actual implementation
    }
}
