using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class FarmShopMonsterPanel : FarmShopPanelBase {
    [SerializeField] Transform parentPanel;
    [SerializeField] Image panel;
    [SerializeField] Sprite onPanel, offPanel;
    [SerializeField] FarmFacilitiesDatabaseSO itemDatabaseSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardParent;
    List <ItemCard> listCardClone = new ();
    bool allItemsLoaded = false;

    [Header ("Selected Item")]
    [SerializeField] Image selectedItemPanel;
    [SerializeField] Image infoIcon;
    [SerializeField] Image smallPodium;
    [SerializeField] TMP_Text infoPriceText;
    [SerializeField] TMP_Text infoDetailText;
    GameObject cloneMotionPodium;
    ItemCard selectedItemCard;
    [SerializeField] ToggleGroup cardGroup;
    

    [System.Serializable]
    public class ItemCard {
        public GameObject cloneCard;
        public GameObject cloneMotion;
        public TMP_Text title;
        public Image icon;
        public Image smallPodium;
        public Button buyButton;
        public TMP_Text priceText;
        public FarmFacilitiesDataSO dataSO;
    }
    public override void ShowPanel() {
        panel.sprite = onPanel;
        panel.transform.SetSiblingIndex(parentPanel.childCount - 1);
        selectedItemPanel.gameObject.SetActive (true);
        InstantiateAllItems ();
    }

    public override void HidePanel() {
        panel.sprite = offPanel;
        selectedItemPanel.gameObject.SetActive (false);
    }

    void InstantiateAllItems () {
        if (!allItemsLoaded) {
            allItemsLoaded = true;
            foreach (FarmFacilitiesDataSO dataSO in itemDatabaseSO.GetListFarmFacilitiesDataSO ()) {
                GameObject clone = GameObject.Instantiate (cardPrefab);
                GameObject cloneMotionSample = GameObject.Instantiate (dataSO.facilityUIPrefab);
                ItemCard newItemCard = new ItemCard ();
                newItemCard.cloneCard = clone;
                newItemCard.cloneMotion = cloneMotionSample;
                newItemCard.title = clone.transform.Find ("Title").gameObject.GetComponent <TMP_Text> ();
                newItemCard.icon = clone.transform.Find ("Icon").gameObject.GetComponent <Image> ();
                newItemCard.smallPodium = clone.transform.Find ("SmallPodium").gameObject.GetComponent <Image> ();
                newItemCard.buyButton = clone.transform.Find ("BuyButton").gameObject.GetComponent <Button> ();
                newItemCard.priceText = newItemCard.buyButton.gameObject.transform.Find ("PriceText").gameObject.GetComponent <TMP_Text> ();
                newItemCard.dataSO  = dataSO;

                cloneMotionSample.transform.SetParent (smallPodium.gameObject.transform);

                Toggle toggle = clone.GetComponent<Toggle>();
                toggle.group = cardGroup;
                toggle.onValueChanged.AddListener(
                value => SetSelectedItem(newItemCard, value)
                );

                listCardClone.Add (newItemCard);
            }
            Debug.Log ("Total clone" + listCardClone.Count);
            foreach (ItemCard itemCard in listCardClone) {
                FillupItemCard (itemCard);
            }
        } else {
            ShowAllInstantiatedAllItems ();
        }
    }

    void FillupItemCard (ItemCard itemCard) {
        itemCard.cloneCard.gameObject.transform.SetParent (cardParent);
       // itemCard.icon.sprite = itemCard.dataSO.RewardSprite;
        itemCard.title.text = itemCard.dataSO.facilityName;
        itemCard.priceText.text = itemCard.dataSO.price.ToString ();
        itemCard.cloneCard.gameObject.SetActive (true);
    }

    void ShowAllInstantiatedAllItems () {
        foreach (ItemCard item in listCardClone) {
            item.cloneCard.SetActive (true);
        }
    }

    void HideAllInstantiatedAllItems () {
        foreach (ItemCard item in listCardClone) {
            item.cloneCard.SetActive (false);
        }
    }

    #region SelectedItem
    public void SetSelectedItem (ItemCard itemCard, bool isOn) {
        if (isOn) {
            selectedItemCard = itemCard;
            ShowInformationSelectedItem ();
        } else {

        }

        
    }

    void ShowInformationSelectedItem () {
        selectedItemPanel.gameObject.SetActive (true);
        FarmFacilitiesDataSO itemData = selectedItemCard.dataSO;
       // infoIcon.sprite = itemData.RewardSprite;

        infoPriceText.text = itemData.price.ToString ();
        infoDetailText.text = itemData.detailText;

        if (cloneMotionPodium) {Destroy (cloneMotionPodium);}
        cloneMotionPodium = GameObject.Instantiate (itemData.facilityUIPrefab);
        cloneMotionPodium.transform.SetParent (smallPodium.gameObject.transform);
        cloneMotionPodium.transform.localScale = itemData.facilityPodiumUILocalScale;
        cloneMotionPodium.transform.localPosition = itemData.facilityPodiumUILocalPosition;
        cloneMotionPodium.SetActive (true);
    }
    #endregion
}
