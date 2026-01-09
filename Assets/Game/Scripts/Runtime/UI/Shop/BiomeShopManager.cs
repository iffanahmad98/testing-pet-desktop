using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class BiomeShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform biomeParent;
    public Transform cardParent;
    public GameObject biomeCardPrefab;

    [Header("Toggle References")]
    public ToggleIconButton skyToggleButton;
    public ToggleIconButton cloudToggleButton;

    [Header("Info Panel")]
    public TMP_Text biomeNameText;
    public TMP_Text biomePriceText;
    public TMP_Text biomeDescText;

    [Header("Data")]
    public BiomeDatabaseSO biomes; // List of all available biomes

    private BiomeCardUI selectedCard;
    private float originalBiomeParentHeight;
    private RectTransform biomeParentRect;
    BiomeManager biomeManager;

    // Object pooling
    private Queue<BiomeCardUI> cardPool = new Queue<BiomeCardUI>();
    private List<BiomeCardUI> activeCards = new List<BiomeCardUI>();

    private bool canBuyMonster;

    private void Awake()
    {
        ServiceLocator.Register(this);

        biomeParentRect = biomeParent.GetComponent<RectTransform>();
        originalBiomeParentHeight = biomeParentRect.sizeDelta.y;

        // Pre-populate pool with initial cards
        InitializeCardPool();
    }

    private void InitializeCardPool()
    {
        // Create initial pool size based on expected biome count
        int initialPoolSize = Mathf.Max(10, biomes?.allBiomes?.Count ?? 10);

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject cardObj = Instantiate(biomeCardPrefab, cardParent);
            BiomeCardUI card = cardObj.GetComponent<BiomeCardUI>();
            card.gameObject.SetActive(false);
            cardPool.Enqueue(card);
        }
    }

    private BiomeCardUI GetPooledCard()
    {
        if (cardPool.Count > 0)
        {
            BiomeCardUI card = cardPool.Dequeue();
            card.gameObject.SetActive(true);
            return card;
        }
        else
        {
            // Pool is empty, create new card
            GameObject cardObj = Instantiate(biomeCardPrefab, cardParent);
            return cardObj.GetComponent<BiomeCardUI>();
        }
    }

    private void ReturnCardToPool(BiomeCardUI card)
    {
        card.gameObject.SetActive(false);
        card.transform.SetAsLastSibling(); // Move to end to keep pool organized
        cardPool.Enqueue(card);
    }

    private void Start()
    {
        RefreshBiomeCards();

        biomeManager = ServiceLocator.Get<BiomeManager>();

        // Load saved state and apply to toggles
        bool isSkyEnabled = SaveSystem.IsSkyEnabled();
        bool isCloudEnabled = SaveSystem.IsCloudEnabled();

        skyToggleButton.SetState(isSkyEnabled);
        cloudToggleButton.SetState(isCloudEnabled);

        // Subscribe to UI toggle changes
        skyToggleButton.OnToggleChanged += ToggleSkyLayer;
        cloudToggleButton.OnToggleChanged += ToggleCloudLayer;

        // Apply immediately to BiomeManager (if needed)
        if (biomeManager != null)
        {
            biomeManager.SetSkyLayerActive(isSkyEnabled);
            biomeManager.ToggleClouds(isCloudEnabled); // Ensure this accepts bool
        }
    }

    public void RefreshItem()
    {
        RefreshBiomeCards();
    }

    private void RefreshBiomeCards()
    {
        // Return all active cards to pool
        foreach (var card in activeCards)
        {
            ReturnCardToPool(card);
        }
        activeCards.Clear();

        // Get cards from pool and setup
        foreach (var biome in biomes.allBiomes)
        {
            BiomeCardUI card = GetPooledCard();
            card.Setup(biome);
            card.OnSelected = OnBiomeSelected;
            card.OnApplyClicked = OnBiomeApply;
            card.OnCancelApplied = OnBiomeCancel;
            card.OnBuyClicked = OnBiomeBuy;

            activeCards.Add(card);
        }

        // Check requirement & grayscale
        foreach (var card in activeCards)
        {
            if (card.BiomeData != null)
            {
                bool canBuy = CheckBuyingRequirement(card);
                card.SetGrayscale(!canBuy);
            }
        }

        // Sort: BUYABLE → TOP
        var filtered = activeCards
            .OrderByDescending(card => !card.grayscaleObj.activeInHierarchy)
            .ToList();

        // Apply order to activeCards
        activeCards.Clear();
        activeCards.AddRange(filtered);

        // Apply order to UI
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].transform.SetSiblingIndex(i);
        }

        AdjustBiomeParentHeight(biomes.allBiomes.Count);
        
        OnBiomeSelected(activeCards[0]);
        //ClearInfo();
    }
    private void AdjustBiomeParentHeight(int biomeCount)
    {
        int rows = Mathf.CeilToInt(biomeCount / 2f);
        int baseRows = 1; // first 2 items (1 row) doesn't need extra height
        int extraRows = Mathf.Max(0, rows - baseRows);

        float newHeight = originalBiomeParentHeight + (extraRows * 248.8f);

        if (biomeParentRect != null)
        {
            Vector2 size = biomeParentRect.sizeDelta;
            size.y = newHeight;
            biomeParentRect.sizeDelta = size;
        }
    }

    private void OnBiomeSelected(BiomeCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedCard.SetSelected(true);
        ShowBiomeInfo(card.BiomeData);
    }

    private void OnBiomeApply(BiomeCardUI card)
    {
        var biomeID = card.BiomeData.biomeID;

        SaveSystem.SetActiveBiome(biomeID);

        // Enable all layers when applying
        SaveSystem.SetSkyEnabled(true);
        SaveSystem.SetAmbientEnabled(true);
        SaveSystem.SetCloudEnabled(true);

        SaveSystem.SaveAll();

        if (biomeManager != null)
        {
            biomeManager.ChangeBiomeByID(biomeID);
            biomeManager.SetSkyLayerActive(true);
            biomeManager.SetAmbientLayerActive(true);
            biomeManager.ToggleClouds(true);
        }

        // Also sync the UI toggle states
        skyToggleButton.SetState(true);
        cloudToggleButton.SetState(true);

        ServiceLocator.Get<UIManager>().ShowMessage($"Applied '{card.BiomeData.biomeName}' biome!");

        RefreshBiomeCards();
        OnBiomeSelected(card); // Reselect to update info panel
    }


    private void OnBiomeCancel(BiomeCardUI card)
    {
        // Turn off toggles (UI)
        skyToggleButton.SetState(false);
        cloudToggleButton.SetState(false);

        // Deactivate biome visuals
        biomeManager?.DeactiveBiome();

        // Update SaveSystem values accordingly
        SaveSystem.SetSkyEnabled(false);
        SaveSystem.SetCloudEnabled(false);
        SaveSystem.SetAmbientEnabled(false);
        SaveSystem.SetActiveBiome("");

        // Persist changes
        SaveSystem.SaveAll();

        ServiceLocator.Get<UIManager>().ShowMessage($"Cancelled '{card.BiomeData.biomeName}' biome.");

        // Refresh all cards to update button states
        RefreshBiomeCards();
    }


    private void OnBiomeBuy(BiomeCardUI card)
    {
        var biome = card.BiomeData;

        if (CheckBuyingRequirement(card))
        {
            if (SaveSystem.TryBuyBiome(biome.biomeID, biome.price))
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage($"Bought '{biome.biomeName}'!");

                // Update UI Coin Text
                ServiceLocator.Get<CoinDisplayUI>().UpdateCoinText();

                // Update Shop Item
                //ServiceLocator.Get<RefreshShopItems>().RefreshAllShop();
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("Not enough coins to buy biome!");
            }
        }
        else
        {
            Debug.Log("Minimum Requirement Monster not enough!");
        }

        SaveSystem.SaveAll();
        RefreshBiomeCards();
        OnBiomeSelected(card);
    }

    private bool CheckBuyingRequirement(BiomeCardUI card)
    {
        var biome = card.BiomeData;

        if (biome == null)
            return false;

        // Reference All Monster Player Have
        var monsters = ServiceLocator.Get<MonsterManager>().activeMonsters;

        // value to check if every index of Array/List is Eligible
        int valid = 0;

        // check every single current active monster to meet minimum requirement
        foreach (var required in biome.monsterRequirements)
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

        if (valid == biome.monsterRequirements.Length)
        {
            canBuyMonster = true;
        }
        else
        {
            canBuyMonster = false;
        }

        Debug.Log(canBuyMonster);

        return canBuyMonster;
    }

    private void ShowBiomeInfo(BiomeDataSO biome)
    {
        biomeNameText.text = biome.biomeName;
        biomePriceText.text = $"Price: {biome.price}";
        biomeDescText.text = biome.description;
    }

    private void ClearInfo()
    {
        biomeNameText.text = "";
        biomePriceText.text = "";
        biomeDescText.text = "";
    }
    private void ToggleSkyLayer(bool isOn)
    {
        biomeManager.ToggleLayer(ref biomeManager.skyLayer, isOn);
        ServiceLocator.Get<UIManager>()?.ShowMessage("Sky: " + (isOn ? "ON" : "OFF"));
    }

    private void ToggleCloudLayer(bool isOn)
    {
        biomeManager.ToggleClouds(isOn);
        ServiceLocator.Get<UIManager>()?.ShowMessage("Clouds: " + (isOn ? "ON" : "OFF"));
    }


}
