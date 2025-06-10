using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Image highlightBorder; // Optional: For showing selection

    [Header("Data")]
    public ItemDataSO itemData;

    private bool isSelected = false;

    public System.Action<ItemCardUI> OnSelected; // Callback to ShopManager or controller

    private void Start()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(HandleClick);
        }

        SetSelected(false);
    }

    public void Setup(ItemDataSO data)
    {
        itemData = data;

        if (itemIcon != null)
            itemIcon.sprite = data.icon;

        if (itemNameText != null)
            itemNameText.text = data.itemName;

        if (priceText != null)
            priceText.text = data.price.ToString();

        SetSelected(false);
    }

    private void HandleClick()
    {
        OnSelected?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Visual feedback
        if (highlightBorder != null)
            highlightBorder.enabled = selected;
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
