using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class FarmShopPlantPanel : FarmShopPanelBase
{
    [SerializeField] Transform parentPanel;
    [SerializeField] Image panel;
    [SerializeField] Sprite onPanel, offPanel;
    [SerializeField] ItemDatabaseSO itemDatabaseSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardParent;
    List <ItemCard> listCardClone = new ();
    bool allItemsLoaded = false;

    [System.Serializable]
    public class ItemCard {
        public GameObject cloneCard;
        public TMP_Text title;
        public Image icon;
        public Button buyButton;
        public TMP_Text priceText;
        public ItemDataSO itemDataSO;
    }

    public override void ShowPanel() {
        panel.sprite = onPanel;
        panel.transform.SetSiblingIndex(parentPanel.childCount - 1);
        InstantiateAllItems ();
        
    }

    public override void HidePanel() {
        panel.sprite = offPanel;
        HideAllInstantiatedAllItems ();
    }

    void InstantiateAllItems () {
        if (!allItemsLoaded) {
            allItemsLoaded = true;
            foreach (ItemDataSO itemDataSO in itemDatabaseSO.GetAllItems ()) {
                GameObject clone = GameObject.Instantiate (cardPrefab);
                ItemCard newItemCard = new ItemCard ();
                newItemCard.cloneCard = clone;
                newItemCard.title = clone.transform.Find ("Title").gameObject.GetComponent <TMP_Text> ();
                newItemCard.icon = clone.transform.Find ("Icon").gameObject.GetComponent <Image> ();
                newItemCard.buyButton = clone.transform.Find ("BuyButton").gameObject.GetComponent <Button> ();
                newItemCard.priceText = newItemCard.buyButton.gameObject.transform.Find ("PriceText").gameObject.GetComponent <TMP_Text> ();
                newItemCard.itemDataSO  = itemDataSO;
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
        itemCard.title.text = itemCard.itemDataSO.itemName;
        itemCard.icon.sprite = itemCard.itemDataSO.RewardSprite;
        itemCard.priceText.text = itemCard.itemDataSO.price.ToString ();
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
}
