using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image monsterIcon;
    public TMP_Text monsterNameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Image highlightBorder; // Optional: For showing selection

    [Header("Data")]
    public ItemDataSO monsterItemData;

    private bool isSelected = false;

    public System.Action<MonsterCardUI> OnSelected; // Called when select button clicked
    public System.Action<MonsterCardUI> OnBuy;      // Called when buy button clicked

    private void Start()
    {
        selectButton.onClick.RemoveAllListeners();
        buyButton.onClick.RemoveAllListeners();
        if (selectButton != null)
            selectButton.onClick.AddListener(HandleSelect);

        if (buyButton != null)
            buyButton.onClick.AddListener(HandleBuy);

        SetSelected(false);
    }

    public void Setup(ItemDataSO data)
    {
        monsterItemData = data;
        if (monsterIcon != null && data != null && data.itemImgs.Length > 0)
        {
            monsterIcon.sprite = data.itemImgs[0];
        }
        if (monsterNameText != null)
            monsterNameText.text = data.itemName;
        if (priceText != null)
            priceText.text = data.price.ToString();
        SetSelected(false);
    }

    private void HandleSelect()
    {
        OnSelected?.Invoke(this);
    }

    private void HandleBuy()
    {
        OnBuy?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (highlightBorder != null)
        {
            // Solution 1: Use SetActive (most reliable)
            highlightBorder.gameObject.SetActive(selected);

            // Alternative Solution 2: Use color with alpha
            // Color borderColor = highlightBorder.color;
            // borderColor.a = selected ? 1f : 0f;
            // highlightBorder.color = borderColor;

            // Alternative Solution 3: Use different colors
            // highlightBorder.color = selected ? selectedBorderColor : normalBorderColor;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}