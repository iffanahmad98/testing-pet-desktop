using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
public class HotelGoldenTicketRedeemMenu : HotelShopMenuBase
{
  int totalRemainGoldenTicket = 0;
  [SerializeField] TMP_Text goldenTicketText;
  bool onListenerLoaded = false;
  
  [Header ("Exchange Slot")]
  [SerializeField] GameObject goldenTicketCardPrefab;
  [SerializeField] Transform goldenTicketCardParent; 
  [SerializeField] ToggleGroup goldenTicketGroup;
  [SerializeField] GoldenTicketCard curGoldenTicketCard;
  bool isAllExchangeEntries = false;
  public GoldenTicketsExchangeEntry [] goldenTicketsExchangeEntries;
  public List <GoldenTicketCard> listGoldenTicketCards = new ();
  public class GoldenTicketCard {
      public GameObject clone;
      public GoldenTicketsExchangeEntry data;
  } 

  [Header ("Exchange Display")]
  [SerializeField] Image exchangeImage;
  [SerializeField] TMP_Text exchangeNameText;
  [SerializeField] TMP_Text exchangeText;
  
   
  [Header ("Exchange Redeem")]
  [SerializeField] Sprite redeemOn, redeemOff;
  [SerializeField] Button redeemButton;
  public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay ();
        OnLoadListener ();
   }

   public override void HideMenu () {
    base.HideMenu ();
   }

   void RefreshDisplay () {
      totalRemainGoldenTicket = SaveSystem.PlayerConfig.goldenTicket;
      goldenTicketText.text = totalRemainGoldenTicket.ToString ();
      InstantiateAllExchangeEntries ();
   }

   #region Exchange Slot

   void InstantiateAllExchangeEntries () {
      if (!isAllExchangeEntries) {
         isAllExchangeEntries = true;
         bool isStarter = false;
         foreach (GoldenTicketsExchangeEntry entry in goldenTicketsExchangeEntries) {
            GameObject card = GameObject.Instantiate (goldenTicketCardPrefab);
            card.transform.SetParent (goldenTicketCardParent);
            card.SetActive (true);
            Image iconCard = card.transform.Find ("Icon").GetComponent<Image> ();
            TMP_Text nameCard = card.transform.Find ("Name").GetComponent <TMP_Text> ();
            TMP_Text goldenTicketText = card.transform.Find ("GoldenTicketText").GetComponent <TMP_Text> ();

            iconCard.sprite = entry.rewardable.RewardSprite;
            nameCard.text = entry.rewardable.ItemName;
            goldenTicketText.text = entry.ticketCost.ToString ();

            Vector3 scaleAverage = new Vector3 (
               entry.rewardable.RewardScale.x - (entry.rewardable.RewardScale.x * 10/100),
               entry.rewardable.RewardScale.y - (entry.rewardable.RewardScale.y * 10/100),
               entry.rewardable.RewardScale.z - (entry.rewardable.RewardScale.z * 10/100)
            );
            Toggle toggle = card.GetComponent<Toggle>();
            toggle.group = goldenTicketGroup;
            toggle.onValueChanged.AddListener(
               (bool value) => ToggleGoldenTicketCard(toggle, value)
            );
            
            iconCard.transform.localScale = scaleAverage;

            GoldenTicketCard goldenTicketCard = new GoldenTicketCard ();
            goldenTicketCard.clone = card;
            goldenTicketCard.data = entry;
            listGoldenTicketCards.Add (goldenTicketCard);

            if (!isStarter) {
               isStarter = true;
               toggle.isOn = true;
               ToggleGoldenTicketCard (toggle, true);
            }
         }
      }
   }

   void ToggleGoldenTicketCard(Toggle toggle, bool value)
   {
      if (value)
      {
         Debug.Log("Toggle aktif: " + toggle.name);
         SelectedGoldenTicketCard (GetGoldenTicketCardByClone (toggle.gameObject));

      }
   }

   #endregion
   #region Exchange Display
   void SelectedGoldenTicketCard (GoldenTicketCard card) {
      curGoldenTicketCard = card;
      exchangeNameText.text = card.data.rewardable.ItemName;
      exchangeImage.sprite = card.data.rewardable.RewardSprite;
      exchangeImage.transform.localScale = card.data.rewardable.RewardScale;
      // ini untuk nama exchangeText.text = card.data.rewardable.ItemName;
      exchangeText.text = card.data.ticketCost.ToString ();
      RefreshRedeemEligible (card);
   }

   #endregion
   #region Exchange Redeem
   public void Redeem () {
      if (curGoldenTicketCard.data.IsEligible ()) {
         GoldenTicket.instance.UsingLoot (curGoldenTicketCard.data.ticketCost);
         curGoldenTicketCard.data.rewardable.RewardGotItem (1);
         RefreshDisplay ();
         RefreshRedeemEligible (curGoldenTicketCard);
      }
      
   }

   void RefreshRedeemEligible (GoldenTicketCard card) {
      if (card.data.IsEligible ()) {
         redeemButton.image.sprite = redeemOn;
         redeemButton.interactable = true;
      } else {
         redeemButton.image.sprite = redeemOff;
         redeemButton.interactable = false;
      }
   }
   #endregion
   #region GoldenTicketCard
   GoldenTicketCard GetGoldenTicketCardByClone(GameObject cloneTarget)
   {
      return listGoldenTicketCards.FirstOrDefault(gc => gc.clone == cloneTarget);
   }

   #endregion
   #region Listener
   void OnLoadListener () {
      if (!onListenerLoaded) {
         onListenerLoaded = true;
         redeemButton.onClick.AddListener (Redeem);
      }
   }
   #endregion
}
