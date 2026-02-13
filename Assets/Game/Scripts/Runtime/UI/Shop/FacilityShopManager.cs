using MagicalGarden.Farm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    private bool canBuyItem = false;

    private List<FacilityCardUI> activeCards = new List<FacilityCardUI>();

    private readonly WaitForEndOfFrame waitEndOfFrame = new();

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

        StartCoroutine(InitializeCardPool());
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        if (facilityManager != null)
        {
            facilityManager.OnTimeKeeperStateChanged -= OnTimeKeeperStateChanged;
        }
    }

    private IEnumerator InitializeCardPool()
    {
        //int initialPoolSize = Mathf.Max(10, decorations?.allDecorations?.Count ?? 10);

        yield return new WaitUntil(() => SaveSystem.IsLoadFinished);

        for (int i = 0; i < facilities.allFacilities.Count; i++)
        {
            int temp = i;
            GameObject cardObj = Instantiate(facilityCardPrefab, cardParent);
            FacilityCardUI card = cardObj.GetComponent<FacilityCardUI>();

            card.SetupFacility(facilities.allFacilities[temp]);
            card.OnSelected = OnFacilitySelected;
            card.OnUseClicked = OnFacilityUse;
            card.OnBuyClicked = OnFacilityBuy;
            card.OnCancelClicked = OnFacilityCancel;

            if (SaveSystem.UiSaveData.FacilityShopCards.Count < facilities.allFacilities.Count + npcDatabases.monsters.Count)
                SaveSystem.UiSaveData.FacilityShopCards.Add(new()
                {
                    Id = card.FacilityData.facilityID
                });

            activeCards.Add(card);
        }

        for (int i = 0; i < npcDatabases.monsters.Count; i++)
        {
            int temp = i;
            GameObject cardObj = Instantiate(facilityCardPrefab, cardParent);
            FacilityCardUI card = cardObj.GetComponent<FacilityCardUI>();

            card.SetupNPC(npcDatabases.monsters[temp]);
            card.OnSelected = OnNPCSelected;
            card.OnUseClicked = OnNPCUse;
            card.OnBuyClicked = OnNPCBuy;
            card.OnCancelClicked = OnNPCCancel;

            if (SaveSystem.UiSaveData.FacilityShopCards.Count < facilities.allFacilities.Count + npcDatabases.monsters.Count)
                SaveSystem.UiSaveData.FacilityShopCards.Add(new()
                {
                    Id = card.npc.id
                });

            activeCards.Add(card);
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

        MonsterManager.instance.audio.PlaySFX("time_keeper");
    }

    public void RefreshItem()
    {
        RefreshFacilityCards(true);
    }

    public void RefreshFacilityCards(bool eligibleBuyVfx = false)
    {
        WaitRefreshFacilityCards(eligibleBuyVfx);
        //ClearInfo();
    }

    private void WaitRefreshFacilityCards(bool eligibleBuyVfx = false)
    {
        //StopAllCoroutines();

        foreach (var card in activeCards)
        {
            card.UpdateState();
            bool canBuy = CheckBuyingRequirement(card, card.IsNpc);
            card.SetGrayscale(!canBuy);

        }

        StartCoroutine(SortByBuyRequirement(eligibleBuyVfx));
    }


    private IEnumerator SortByBuyRequirement(bool eligibleBuyVfx = false)
    {
       
        // Sort: BUYABLE → TOP
        activeCards = activeCards
            .OrderByDescending(card => !card.grayscaleObj.activeInHierarchy)
            .ToList();

        yield return waitEndOfFrame;

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            int temp = i;

            var currentCard = activeCards[temp];
            currentCard.transform.SetSiblingIndex(temp);

            if (currentCard.grayscaleObj.activeInHierarchy) continue;

            yield return waitEndOfFrame; //Wait set dirty UI at the end of frame

            string idToCheck = currentCard.IsNpc ? currentCard.npc.id : currentCard.FacilityData.facilityID;

            if (!SaveSystem.UiSaveData.GetShopCardData(ShopType.FacilityShop, idToCheck).IsOpened)
            {
                ServiceLocator.Get<UIManager>().InitUnlockedMenuVfx(currentCard.GetComponent<RectTransform>());

                SaveSystem.UiSaveData.SetShopCardsOpenState(ShopType.FacilityShop, idToCheck, true);
            }
        }

        if(selectedCard == null)
        {
            if (activeCards[0].FacilityData != null)
            {
                OnFacilitySelected(activeCards[0]);
            }
            else if (activeCards[0].npc != null)
            {
                OnNPCSelected(activeCards[0]);
            }
        }
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

        // Show "FREE" for toggle facilities, otherwise show price
        // facilityPriceText.text = facility.isFreeToggleFacility ? "Price: FREE" : $"Price: {facility.price}";
        facilityPriceText.text = facility.isFreeToggleFacility ? "Price:\t<color=orange>FREE</color>" : $"Price:\t<color=orange>{facility.price}</color>";
        facilityDescText.text = facility.description;

        // Don't show cooldown for free toggle facilities
       // facilityCooldownText.text = facility.isFreeToggleFacility ? "" : $"Cooldown: {facility.cooldownSeconds}s";
       facilityCooldownText.text = facility.isFreeToggleFacility ? "" : $"Cooldown:\t<color=orange>{facility.cooldownSeconds}</color>s";
    }

    private void OnFacilitySelected(FacilityCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        MonsterManager.instance.audio.PlaySFX("button_click");
        
        selectedCard = card;
        selectedCard.SetSelected(true);
        ShowFacilityInfo(card.FacilityData);
    }

    private void OnFacilityUse(FacilityCardUI card)
    {
        if (facilityManager != null)
        {
            var facility = card.FacilityData;

            // Handle free toggle facilities (Pumpkin Facility)
            if (facility.isFreeToggleFacility)
            {
                // Mark as owned (active state)
                SaveSystem.MarkFacilityOwned(facility.facilityID);

                // Apply Pumpkin Facility effects (enable Pumpkin Mini, disable Pumpkin Car)
                facilityManager.ApplyPumpkinFacility();

                SaveSystem.SaveAll();

                ServiceLocator.Get<UIManager>()?.ShowMessage($"Applied '{facility.facilityName}'!");
                RefreshFacilityCards();
                OnFacilitySelected(card);

                return;
            }

            // Normal facility logic
            bool success = facilityManager.UseFacility(facility.facilityID);

            if (success)
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Used '{facility.name}'!");
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"'{facility.name}' is on cooldown!");
            }

            MonsterManager.instance.audio.PlaySFX("placing_facility");

            RefreshFacilityCards();

            OnFacilitySelected(card);
        }
    }

    private void OnFacilityBuy(FacilityCardUI card)
    {
        var facility = card.FacilityData;

        if (CheckBuyingRequirement(card, false))
        {
            if (SaveSystem.TryPurchaseFacility(facility))
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{facility.name}'!");
                MonsterManager.instance.audio.PlaySFX("buy");
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy facility!");
            }
        }
        else
        {
            Debug.Log("Minimum Requirement Monster not enough!");
        }

        SaveSystem.SaveAll();
        RefreshFacilityCards();
        OnFacilitySelected(card);
    }

    private bool CheckBuyingRequirement(FacilityCardUI card, bool monsterData)
    {
        var facilityData = card.FacilityData;
        var npcData = card.npc;

        // if (facilityData == null)
        //     return false;

        // Reference All Monster Player Have
        // var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;
        var monsters = MonsterManagerEligible.Instance.GetListMonsterDataSO ();
        // value to check if every index of Array/List is Eligible
        int valid = 0;

        if (!monsterData)
        {
            // check every single current active monster to meet minimum requirement
            foreach (var required in facilityData.monsterRequirements)
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

            if (valid == facilityData.monsterRequirements.Length)
            {
                canBuyItem = true;
            }
            else
            {
                canBuyItem = false;
            }
        }
        else if (monsterData)
        {
            // Reference All Monster Player Have
            var npcs = ServiceLocator.Get<MonsterManager>().npcMonsters;
            // check every single current active monster to meet minimum requirement
            foreach (var required in npcData.monsterRequirements)
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

                    for (int i = 0; i < npcs.Count; i++)
                    {
                        if (required.monsterType == npcs[i].MonsterData.monType)
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
                        string npcID = GetNPCIDFromCard(card);

                        Debug.Log(npcID);

                        // Check if already owned
                        if (SaveSystem.HasNPC(npcID))
                        {
                            ServiceLocator.Get<UIManager>()?.ShowMessage($"You already own '{npcData.monsterName}'!");
                            valid++;
                        }
                        
                        Debug.Log($"{required.monsterType} Failed required value = {requiredValue}/{required.minimumRequirements}");
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

            if (valid == npcData.monsterRequirements.Length)
            {
                canBuyItem = true;
            }
            else
            {
                canBuyItem = false;
            }
        }

        return canBuyItem;
    }

    private void OnFacilityCancel(FacilityCardUI card)
    {
        if (facilityManager != null)
        {
            var facility = card.FacilityData;

            // Handle free toggle facilities (Pumpkin Facility)
            if (facility.isFreeToggleFacility)
            {
                // Remove ownership (inactive state)
                SaveSystem.RemoveFacilityOwnership(facility.facilityID);

                // Unapply Pumpkin Facility effects (disable both, reset pomodoro)
                facilityManager.UnapplyPumpkinFacility();

                SaveSystem.SaveAll();

                ServiceLocator.Get<UIManager>()?.ShowMessage($"Unapplied '{facility.facilityName}'!");
                RefreshFacilityCards();
                return;
            }

            // Normal facility logic
            facilityManager.CancelFacilityCooldown(facility.facilityID);
            ServiceLocator.Get<UIManager>()?.ShowMessage($"Cancelled cooldown for '{facility.facilityName}'!");
            RefreshFacilityCards();

            OnFacilitySelected(card);
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
           // facilityPriceText.text = $"Price: {npcData.monsterPrice}";
            facilityPriceText.text = $"Price:\t<color=orange>{npcData.monsterPrice}</color>";
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
        RefreshNPCIdleFlower();
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

        if (CheckBuyingRequirement(card, true))
        {
            // Try to buy
            if (SaveSystem.TryBuyMonster(npcData))
            {
                SaveSystem.AddNPC(npcID);
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{npcData.monsterName}'!");

                OnNPCUse(card);

                // Refresh NPCIdleFlower ownership check (untuk update idle station availability)
                RefreshNPCIdleFlower();

                // Refresh ALL facility cards (untuk update prerequisite check di cards lain)
                RefreshFacilityCards();
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy NPC!");
            }
        }

        RefreshItem();

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
        RefreshNPCIdleFlower();
        RefreshFacilityCards();
        SaveSystem.SaveAll();

        OnNPCSelected(card);
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

                RefreshItem();
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
