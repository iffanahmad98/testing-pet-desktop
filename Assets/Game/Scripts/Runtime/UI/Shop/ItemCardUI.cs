using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class ItemCardUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IUIButtonSource
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

    [Header("Tutorial Integration")]
    public string tutorialItemIdForBuyButton;

    private bool isSelected = false;

    public System.Action<ItemCardUI> OnSelected; // Called when select button clicked
    public System.Action<ItemCardUI> OnBuy;      // Called when buy button clicked
    public bool IsCanBuy { get; private set; }

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

        // preserve aspect is different per category
        if (data.category == ItemType.Food)
        {
            Debug.Log($"{itemNameText.text} width and height: {itemIcon.sprite.texture.width} & {itemIcon.sprite.texture.height}");
            if (itemIcon.sprite.texture.width > itemIcon.sprite.texture.height)
                itemIcon.preserveAspect = false;
            else
                itemIcon.preserveAspect = true;
        }
        else if (data.category == ItemType.Medicine)
            itemIcon.preserveAspect = true;

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
            foreach (var img in grayscaleImage)
            {
                img.material = grayscaleMat;
            }
        }
        else
        {
            foreach (var img in grayscaleImage)
            {
                img.material = null;
            }
        }
    }

    #region Requirement
    public void SetCanBuy(bool value)
    { // MonsterShopManager.cs
        IsCanBuy = value;
        SetGrayscale(!value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RequirementTipManager.Instance.StartClick(itemData.requirementTipDataSO.GetInfoData());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
    #endregion

    // IUIButtonSource: expose buyButton ke TutorialManager jika card ini adalah item yang dipakai tutorial
    public void CollectButtons(System.Collections.Generic.List<Button> target)
    {
        if (target == null || buyButton == null)
            return;

        if (string.IsNullOrEmpty(tutorialItemIdForBuyButton))
        {
            // Tidak dikonfigurasi untuk tutorial, abaikan saja.
            return;
        }

        if (itemData == null)
        {
            Debug.Log($"[ItemCardUI/Tutorial] Skip CollectButtons untuk '{name}' karena itemData null. Setup sudah dipanggil?");
            return;
        }

        var itemId = itemData.itemID;
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.Log($"[ItemCardUI/Tutorial] Skip CollectButtons untuk '{name}' karena itemID kosong pada itemData '{itemData.name}'.");
            return;
        }

        if (!string.Equals(itemId, tutorialItemIdForBuyButton, System.StringComparison.OrdinalIgnoreCase))
        {
            // Bukan item yang diincar tutorial, abaikan.
            return;
        }

        if (!target.Contains(buyButton))
        {
            target.Add(buyButton);
            Debug.Log($"[ItemCardUI/Tutorial] BUY BUTTON terdaftar ke UI cache untuk '{name}' (itemId='{itemId}').");
        }
    }
}
