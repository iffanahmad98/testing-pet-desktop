using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

public class MonsterCardUI : MonoBehaviour
{
    [Header("UI References")] public Image monsterIcon;
    public TMP_Text monsterNameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Image highlightBorder; // Optional: For showing selection
    public SkeletonGraphic monsterGraphic;
    public Material monsterMaterial; // Optional: For custom material effects

    [Header("Data")] public ItemDataSO monsterItemData;

    private bool isSelected = false;

    public System.Action<MonsterCardUI> OnSelected; // Called when select button clicked
    public System.Action<MonsterCardUI> OnBuy; // Called when buy button clicked

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
        if (data.skeletonDataAsset != null)
        {
            monsterIcon.gameObject.SetActive(false);
            monsterGraphic.gameObject.SetActive(true);
            monsterGraphic.skeletonDataAsset = data.skeletonDataAsset;
            monsterGraphic.Initialize(true);
            monsterGraphic.AnimationState.SetAnimation(0, "idle", true);
        }
        else if (data != null && data.itemImgs.Length > 0)
        {
            monsterGraphic.gameObject.SetActive(false);
            monsterIcon.gameObject.SetActive(true);
            monsterIcon.sprite = data.itemImgs[0];
        }

        monsterNameText.text = data.itemName;
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
            highlightBorder.gameObject.SetActive(selected);
        }

        if (monsterGraphic != null && monsterGraphic.skeletonDataAsset != null)
        {
            if (selected)
            {
                int _random = Random.Range(0, 2);
                string randomAnim = _random == 0 ? "itching" : "jumping";

                if (!HasAnimation(randomAnim))
                {
                    randomAnim = "jumping";
                }

                // Play random animation once, then queue idle to loop
                monsterGraphic.AnimationState.SetAnimation(0, randomAnim, false);
                monsterGraphic.AnimationState.AddAnimation(0, "idle", true, 0f);
            }
            else
            {
                // When deselected, just play idle
                monsterGraphic.AnimationState.SetAnimation(0, "idle", true);
            }
        }
    }

    bool HasAnimation(string animName)
    {
        return monsterGraphic.Skeleton.Data.FindAnimation(animName) != null;
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}