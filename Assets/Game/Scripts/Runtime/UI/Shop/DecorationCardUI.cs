using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
public class DecorationCardUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    public GameObject grayscaleObj;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Button applyButton;
    public Button cancelButton;
    public Image highlightImage;

    public Image thumbnail;

    [Header("Grayscaleable Components")]
    public Material grayscaleMat;
    public Image[] grayscaleImage;

    public Action<DecorationCardUI> OnSelected;
    public Action<DecorationCardUI> OnApplyClicked;
    public Action<DecorationCardUI> OnCancelApplied;
    public Action<DecorationCardUI> OnBuyClicked;

    public DecorationDataSO DecorationData { get; private set; }

    public bool IsCanBuy { get; private set; }

public void Setup(DecorationDataSO data)
    {
        DecorationData = data;
        nameText.text = data.decorationName;
        thumbnail.sprite = data.thumbnail;
        priceText.text = data.price.ToString();
        
        
        string currentDecorationID = DecorationUIFixHandler.GetDecorationStats();
        bool isOwned = SaveSystem.IsDecorationOwned(data.decorationID);
        bool isApplied = SaveSystem.GetDecorationActiveStatus (DecorationData.decorationID);
        // Debug.Log ($"id {currentDecorationID}, isOwned {isOwned}, isApplied {isApplied}");
        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied && currentDecorationID !="");
        buyButton.gameObject.SetActive(!isOwned);
        

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
        string currentDecorationID = DecorationUIFixHandler.GetDecorationStats();
        bool isOwned = SaveSystem.IsDecorationOwned(DecorationData.decorationID);
        bool isApplied = SaveSystem.GetDecorationActiveStatus (DecorationData.decorationID);
      //  Debug.Log ($"id {currentDecorationID}, isOwned {isOwned}, isApplied {isApplied}");
        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied && currentDecorationID !="");
        buyButton.gameObject.SetActive(!isOwned);
    }
   

    public void SetSelected(bool selected)
    {
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
    public void SetCanBuy (bool value) // MonsterShopManager.cs
    { 
        IsCanBuy = value;
        SetGrayscale(!value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RequirementTipManager.Instance.StartClick(DecorationData.requirementTipDataSO.GetInfoData ());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
    #endregion
}
