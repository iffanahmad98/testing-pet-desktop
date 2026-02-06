using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
public class BiomeCardUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    public GameObject grayscaleObj;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Button applyButton;
    public Button cancelButton;
    public Image highlightImage; // Optional highlight for selection

    public Image thumbnail;

    [Header("Grayscaleable Components")]
    public Material grayscaleMat;
    public Image[] grayscaleImage;

    public Action<BiomeCardUI> OnSelected;
    public Action<BiomeCardUI> OnApplyClicked;
    public Action<BiomeCardUI> OnCancelApplied;
    public Action<BiomeCardUI> OnBuyClicked;

    public BiomeDataSO BiomeData { get; private set; }
    public bool IsCanBuy { get; private set; }
    public void Setup(BiomeDataSO data)
    {
        BiomeData = data;
        nameText.text = data.biomeName;
        thumbnail.sprite = data.thumbnail;
        priceText.text = data.price.ToString();

        string currentBiomeID = SaveSystem.GetActiveBiome();
        bool isOwned = SaveSystem.IsBiomeOwned(data.biomeID);
        bool isApplied = currentBiomeID == data.biomeID;


        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied); // Show cancel when applied
        buyButton.gameObject.SetActive(!isOwned);

        // Set up button listeners
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickCard());
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(this));
        applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(() => OnApplyClicked?.Invoke(this));
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => OnCancelApplied?.Invoke(this));
    }
    public void UpdateState()
    {
        string currentBiomeID = SaveSystem.GetActiveBiome();
        bool isOwned = SaveSystem.IsBiomeOwned(BiomeData.biomeID);
        bool isApplied = currentBiomeID == BiomeData.biomeID;

        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied);
        buyButton.gameObject.SetActive(!isOwned);
    }


    public void SetSelected(bool selected)
    {
        // Optional: highlight visual
        // For now, we just ensure selection works
        highlightImage.gameObject.SetActive(selected);
    }

    public void OnClickCard()
    {
        OnSelected?.Invoke(this);
    }

    public void SetCancelActive(bool isActive)
    {
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(isActive);
    }

    public void SetGrayscale(bool grayscale)
    {
        grayscaleObj.SetActive(grayscale);

        if (grayscale)
        {
            foreach(var img in grayscaleImage)
            {
                img.material = grayscaleMat;
            }
        }
        else
        {
            foreach(var img in grayscaleImage)
            {
                img.material = null;
            }
        }
    }

    #region Requirement
    public void SetCanBuy (bool value) { // MonsterShopManager.cs
        IsCanBuy = value;
        SetGrayscale(!value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RequirementTipManager.Instance.StartClick(BiomeData.requirementTipDataSO.GetInfoData ());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
    #endregion
}
