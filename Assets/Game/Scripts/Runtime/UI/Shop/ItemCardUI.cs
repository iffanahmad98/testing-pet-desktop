using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject grayscaleObj;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Image highlightBorder; // Optional: For showing selection

    [Header("Grayscaleable Components")]
    public Material grayscaleMat;
    public Image[] grayscaleImage;

    [Header("Data")]
    public ItemDataSO itemData;

    private bool isSelected = false;

    public System.Action<ItemCardUI> OnSelected; // Called when select button clicked
    public System.Action<ItemCardUI> OnBuy;      // Called when buy button clicked

    private void Start()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(HandleSelect);

        if (buyButton != null)
            buyButton.onClick.AddListener(HandleBuy);

        //SetSelected(false);
    }

    public void Setup(ItemDataSO data)
    {
        itemData = data;

        if (itemIcon != null)
            itemIcon.sprite = data.itemImgs[0]; // Use first sprite as default icon

        if (itemNameText != null)
            itemNameText.text = data.itemName;

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
            highlightBorder.enabled = selected;
    }

    public bool IsSelected()
    {
        return isSelected;
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
}
