using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BiomeShopManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform biomeParent;
    public Transform cardParent;
    public GameObject biomeCardPrefab;

    [Header("Info Panel")]
    public TMP_Text biomeNameText;
    public TMP_Text biomePriceText;
    public TMP_Text biomeDescText;

    [Header("Data")]
    public List<BiomeDataSO> biomes; // List of all available biomes

    private BiomeCardUI selectedCard;
    private float originalBiomeParentHeight;
    private RectTransform biomeParentRect;
    private void Awake()
    {
        biomeParentRect = biomeParent.GetComponent<RectTransform>();
        originalBiomeParentHeight = biomeParentRect.sizeDelta.y;
    }



    private void Start()
    {
        RefreshBiomeCards();
    }

    private void RefreshBiomeCards()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        foreach (var biome in biomes)
        {
            GameObject cardObj = Instantiate(biomeCardPrefab, cardParent);
            BiomeCardUI card = cardObj.GetComponent<BiomeCardUI>();
            card.Setup(biome);
            card.OnSelected = OnBiomeSelected;
            card.OnApplyClicked = OnBiomeApply;
            card.OnCancelApplied = OnBiomeCancel;
            card.OnBuyClicked = OnBiomeBuy;
        }
        AdjustBiomeParentHeight(biomes.Count);
        ClearInfo();
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
        SaveSystem.SetActiveBiome(card.BiomeData.biomeID);
        SaveSystem.SaveAll();

        ServiceLocator.Get<UIManager>().ShowMessage($"Applied '{card.BiomeData.biomeName}' biome!");

        RefreshBiomeCards();
        OnBiomeSelected(card); // Reselect to update info panel
    }

    private void OnBiomeCancel(BiomeCardUI card)
    {
        SaveSystem.SetActiveBiome(""); // Clear active biome (default)
        SaveSystem.SaveAll();

        ServiceLocator.Get<UIManager>().ShowMessage($"Cancelled '{card.BiomeData.biomeName}' biome.");

        RefreshBiomeCards();
        ClearInfo();
    }

    private void OnBiomeBuy(BiomeCardUI card)
    {
        var biome = card.BiomeData;
        if (SaveSystem.TryBuyBiome(biome.biomeID, biome.price))
        {
            SaveSystem.SetActiveBiome(biome.biomeID);
            ServiceLocator.Get<UIManager>().ShowMessage($"Bought and applied '{biome.biomeName}'!");
        }
        else
        {
            ServiceLocator.Get<UIManager>().ShowMessage("Not enough coins to buy biome!");
        }

        SaveSystem.SaveAll();
        RefreshBiomeCards();
        OnBiomeSelected(card); // Auto-select after buying
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
}
