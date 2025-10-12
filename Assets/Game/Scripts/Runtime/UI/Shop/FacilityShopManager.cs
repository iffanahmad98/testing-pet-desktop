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
    public FacilityDatabaseSO facilities;
    public MonsterDatabaseSO npcDatabases;

    private FacilityCardUI selectedCard;
    private float originalFacilityParentHeight;
    private RectTransform facilityParentRect;
    private FacilityManager facilityManager;

    private List<FacilityCardUI> activeCards = new List<FacilityCardUI>();

    private void Awake()
    {
        facilityParentRect = facilityParent.GetComponent<RectTransform>();
        originalFacilityParentHeight = facilityParentRect.sizeDelta.y;
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        facilityManager = ServiceLocator.Get<FacilityManager>();

        // Subscribe to Time Keeper state changes
        if (facilityManager != null)
        {
            facilityManager.OnTimeKeeperStateChanged += OnTimeKeeperStateChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        if (facilityManager != null)
        {
            facilityManager.OnTimeKeeperStateChanged -= OnTimeKeeperStateChanged;
        }
    }

    private void OnTimeKeeperStateChanged()
    {
        // Update all Time Keeper cards when state changes
        foreach (var card in activeCards)
        {
            if (card.FacilityData != null &&
                (card.FacilityData.facilityID == "F2" || card.FacilityData.facilityID == "F3"))
            {
                card.UpdateState();
            }
        }
    }

    public void RefreshFacilityCards()
    {
        // Destroy all existing cards
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        activeCards.Clear();
        selectedCard = null;

        int totalCount = 0;


        // NPCs
        if (npcDatabases != null && npcDatabases.monsters != null)
        {
            foreach (var npc in npcDatabases.monsters)
            {
                GameObject cardObj = Instantiate(facilityCardPrefab, cardParent);
                FacilityCardUI card = cardObj.GetComponent<FacilityCardUI>();

                card.SetupNPC(npc);
                card.OnSelected = OnNPCSelected;
                card.OnUseClicked = OnNPCUse;
                card.OnBuyClicked = OnNPCBuy;
                card.OnCancelClicked = OnNPCCancel;

                activeCards.Add(card);
                totalCount++;
            }
        }
        if (facilities != null && facilities.allFacilities != null)
        {
            foreach (var facility in facilities.allFacilities)
            {
                GameObject cardObj = Instantiate(facilityCardPrefab, cardParent);
                FacilityCardUI card = cardObj.GetComponent<FacilityCardUI>();

                card.SetupFacility(facility);
                card.OnSelected = OnFacilitySelected;
                card.OnUseClicked = OnFacilityUse;
                card.OnBuyClicked = OnFacilityBuy;
                card.OnCancelClicked = OnFacilityCancel;

                activeCards.Add(card);
                totalCount++;
            }
        }

        ClearInfo();
    }



    private void ClearInfo()
    {
        facilityNameText.text = "";
        facilityPriceText.text = "";
        facilityDescText.text = "";
        facilityCooldownText.text = "";
    }

    private void ShowFacilityInfo(FacilityDataSO facility)
    {
        facilityNameText.text = facility.name;
        facilityPriceText.text = $"Price: {facility.price}";
        facilityDescText.text = facility.description;
        facilityCooldownText.text = $"Cooldown: {facility.cooldownSeconds}s";
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
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"'{card.FacilityData.name}' is on cooldown!");
            }

            RefreshFacilityCards();
        }
    }

    private void OnFacilityBuy(FacilityCardUI card)
    {
        var facility = card.FacilityData;

        if (SaveSystem.TryPurchaseFacility(facility))
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{facility.name}'!");
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy facility!");
        }

        SaveSystem.SaveAll();
        RefreshFacilityCards();
        OnFacilitySelected(card);
    }

    private void OnFacilityCancel(FacilityCardUI card)
    {
        if (facilityManager != null)
        {
            facilityManager.CancelFacilityCooldown(card.FacilityData.facilityID);
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled cooldown for '{card.FacilityData.facilityName}'!");
            RefreshFacilityCards();
        }
    }

    // ---- NPC Methods ----

    private void OnNPCSelected(FacilityCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        var npcData = npcDatabases.GetMonsterByID(card.npc.id);
        ShowNPCInfo(npcData);

        // You can show custom NPC info here if needed
    }
    public void ShowNPCInfo(MonsterDataSO npcData)
    {
        if (npcData != null)
        {
            facilityNameText.text = npcData.monsterName;
            facilityPriceText.text = $"Price: {npcData.monsterPrice}";
            facilityDescText.text = npcData.description;
            facilityCooldownText.text = "";
        }
        else
        {
            ClearInfo();
        }
    }

    private void OnNPCUse(FacilityCardUI card)
    {
        if (card == null || card.npc == null)
        {
            Debug.LogWarning("OnNPCUse called with null card or npc.");
            return;
        }
        string npcID = GetNPCIDFromCard(card);
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning("NPC ID is null or empty from card.");
            return;
        }
        var npcData = npcDatabases.GetMonsterByID(npcID);
        if (npcData == null)
        {
            Debug.LogWarning($"NPC data not found for ID: {npcID}");
            return;
        }
        SaveSystem.ToggleNPCActiveState(npcID, true);
        ServiceLocator.Get<MonsterManager>()?.SpawnNPCMonster(npcData);
        ServiceLocator.Get<UIManager>()?.ShowMessage($"Activated NPC '{npcData.monsterName}'!");
        card.UpdateStateNPC(npcID);
        RefreshFacilityCards();
        SaveSystem.SaveAll();
    }


    private void OnNPCBuy(FacilityCardUI card)
    {
        // Safety check
        if (card == null)
        {
            Debug.LogWarning("OnNPCBuy called with null card.");
            return;
        }

        // Get NPC ID and data
        string npcID = GetNPCIDFromCard(card);
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning("NPC ID is null or empty from card.");
            return;
        }

        var npcData = npcDatabases.GetMonsterByID(npcID);
        if (npcData == null)
        {
            Debug.LogWarning($"NPC data not found for ID: {npcID}");
            return;
        }

        // Check if already owned
        if (SaveSystem.HasNPC(npcID))
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage($"You already own '{npcData.monsterName}'!");
            return;
        }

        // Check prerequisite NPC requirement
        if (!string.IsNullOrEmpty(npcData.prerequisiteNPCId))
        {
            if (!SaveSystem.HasNPC(npcData.prerequisiteNPCId))
            {
                var prerequisiteNPC = npcDatabases.GetMonsterByID(npcData.prerequisiteNPCId);
                string prerequisiteName = prerequisiteNPC != null ? prerequisiteNPC.monsterName : npcData.prerequisiteNPCId;
                ServiceLocator.Get<UIManager>()?.ShowMessage($"You need to own '{prerequisiteName}' first!");
                return;
            }
        }

        // Try to buy
        if (SaveSystem.TryBuyMonster(npcData))
        {
            SaveSystem.AddNPC(npcID);
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{npcData.monsterName}'!");
            ServiceLocator.Get<MonsterManager>()?.SpawnNPCMonster(npcData);

            // Refresh NPCIdleFlower ownership check (untuk update idle station availability)
            RefreshNPCIdleFlower();

            // Refresh ALL facility cards (untuk update prerequisite check di cards lain)
            RefreshFacilityCards();
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy NPC!");
        }

        // Finalize
        SaveSystem.SaveAll();
    }



    private void OnNPCCancel(FacilityCardUI card)
    {
        string npcID = GetNPCIDFromCard(card);
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogWarning("NPC ID is null or empty from card.");
            return;
        }
        ServiceLocator.Get<MonsterManager>()?.DespawnNPC(npcID);
        SaveSystem.ToggleNPCActiveState(npcID, false);
        ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled NPC '{npcID}'!");
        RefreshFacilityCards();
        SaveSystem.SaveAll();
    }

    private void Update()
    {
        foreach (var card in activeCards)
        {
            if (card.FacilityData != null)
            {
                card.UpdateState(); // cooldown timer update
            }
        }
    }


    private string GetNPCIDFromCard(FacilityCardUI card)
    {
        return card.npc?.id ?? "";
    }

    /// <summary>
    /// Refresh NPCIdleFlower untuk update ownership check dan image reference
    /// Dipanggil setelah player membeli NPC facility
    /// </summary>
    private void RefreshNPCIdleFlower()
    {
        var npcManager = ServiceLocator.Get<NPCManager>();
        if (npcManager != null && npcManager.NPCIdleFlower != null)
        {
            var npcIdleFlower = npcManager.NPCIdleFlower.GetComponent<NPCIdleFlower>();
            if (npcIdleFlower != null)
            {
                npcIdleFlower.RefreshNPCOwnership();
                Debug.Log("NPCIdleFlower ownership refreshed after NPC purchase");
            }
            else
            {
                Debug.LogWarning("NPCIdleFlower component not found on NPCIdleFlower GameObject");
            }
        }
        else
        {
            Debug.LogWarning("NPCManager or NPCIdleFlower GameObject not found");
        }
    }
}
