using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DecorationCardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Button applyButton;
    public Button cancelButton;
    public Image highlightImage;

    public Image thumbnail;

    public Action<DecorationCardUI> OnSelected;
    public Action<DecorationCardUI> OnApplyClicked;
    public Action<DecorationCardUI> OnCancelApplied;
    public Action<DecorationCardUI> OnBuyClicked;

    public DecorationDataSO DecorationData { get; private set; }

    public void Setup(DecorationDataSO data)
    {
        DecorationData = data;
        nameText.text = data.decorationName;
        thumbnail.sprite = data.thumbnail;
        priceText.text = data.price.ToString();

        string currentDecorationID = SaveSystem.GetActiveDecoration();
        bool isOwned = SaveSystem.IsDecorationOwned(data.decorationID);
        bool isApplied = currentDecorationID == data.decorationID;

        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied);
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
        string currentDecorationID = SaveSystem.GetActiveDecoration();
        bool isOwned = SaveSystem.IsDecorationOwned(DecorationData.decorationID);
        bool isApplied = currentDecorationID == DecorationData.decorationID;

        applyButton.gameObject.SetActive(isOwned && !isApplied);
        cancelButton.gameObject.SetActive(isOwned && isApplied);
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
}
