using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Spine.Unity;
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

  [System.Serializable]
  public class GoldenTicketCard {
      public GameObject clone;
      public GoldenTicketsExchangeEntry data;
  } 

  [Header ("Exchange Display")]
  [SerializeField] Image exchangeImage;
  [SerializeField] TMP_Text exchangeNameText;
  [SerializeField] TMP_Text exchangeText;
  [SerializeField] SkeletonGraphic npcSkeletonGrpahic;
  [SerializeField] NumberingUtility remainGoldenTicketUtility;
  [SerializeField] GotItemMotion gotItemMotion;
  [SerializeField] Transform gotItemParent;
  [SerializeField] Animator motionSelectedPrize;
   
  [Header ("Exchange Redeem")]
  [SerializeField] Sprite redeemOn, redeemOff;
  [SerializeField] Sprite redeemRed;
  [SerializeField] Button redeemButton;
  Coroutine cnRedeemButton; 
  [Header ("Data")]
  PlayerConfig playerConfig;

  public override void ShowMenu () {
        base.ShowMenu ();
        playerConfig = SaveSystem.PlayerConfig;

        RefreshDisplay (true);
        OnLoadListener ();
   }

   public override void HideMenu () {
    base.HideMenu ();
   }

   void RefreshDisplay (bool refreshGoldenTicketRemain = true) {
      if (refreshGoldenTicketRemain) {
      totalRemainGoldenTicket = SaveSystem.PlayerConfig.goldenTicket;
      remainGoldenTicketUtility.SetImmediate (totalRemainGoldenTicket);
      }
      goldenTicketText.text = totalRemainGoldenTicket.ToString ();
      InstantiateAllExchangeEntries ();
   }

   #region Exchange Slot

   void InstantiateAllExchangeEntries () {
      if (!isAllExchangeEntries) {
         isAllExchangeEntries = true;
         bool isStarter = false;
         foreach (GoldenTicketsExchangeEntry entry in goldenTicketsExchangeEntries)
         {
            // Skip decoration yang sudah dimiliki player
            if (entry.rewardable is DecorationDataSO &&
               playerConfig.HasDecoration(entry.rewardable.ItemId))
            {
               continue;
            }

            GameObject card = Instantiate(goldenTicketCardPrefab, goldenTicketCardParent);
            card.SetActive(true);

            Image iconCard = card.transform.Find("Icon").GetComponent<Image>();
            TMP_Text nameCard = card.transform.Find("Name").GetComponent<TMP_Text>();
            TMP_Text goldenTicketText = card.transform.Find("GoldenTicketText").GetComponent<TMP_Text>();

            iconCard.sprite = entry.rewardable.RewardSprite;
            nameCard.text = entry.rewardable.ItemName;
            goldenTicketText.text = entry.ticketCost.ToString();

            Vector3 scaleAverage = entry.rewardable.RewardScale * 0.9f;
            iconCard.transform.localScale = scaleAverage;

            Toggle toggle = card.GetComponent<Toggle>();
            toggle.group = goldenTicketGroup;
            toggle.onValueChanged.AddListener(
               value => ToggleGoldenTicketCard(toggle, value)
            );

            GoldenTicketCard goldenTicketCard = new GoldenTicketCard
            {
               clone = card,
               data = entry
            };
            listGoldenTicketCards.Add(goldenTicketCard);

            if (!isStarter)
            {
               isStarter = true;
               toggle.isOn = true;
               ToggleGoldenTicketCard(toggle, true);
            }
         }
      }
   }

   void RefreshToggleCard () {
      for (int i = listGoldenTicketCards.Count-1; i >= 0; i--) {
        
         if (listGoldenTicketCards[i].data.rewardable is DecorationDataSO &&
               playerConfig.HasDecoration(listGoldenTicketCards[i].data.rewardable.ItemId))
         {
            Destroy (listGoldenTicketCards[i].clone);
            listGoldenTicketCards.RemoveAt(i);
            // if Destroyed, target next card.
            int targetElement = i;
            if (targetElement >= listGoldenTicketCards.Count-1) {targetElement = 0;}
            Toggle toggle = listGoldenTicketCards[targetElement].clone.GetComponent <Toggle> ();
            toggle.isOn = true;
            ToggleGoldenTicketCard(toggle, true);
         }

               
      }
   }

   void ToggleGoldenTicketCard(Toggle toggle, bool value)
   {
      if (value)
      {
         Debug.Log("Toggle aktif: " + toggle.name);
         SelectedGoldenTicketCard (GetGoldenTicketCardByClone (toggle.gameObject));
         toggle.gameObject.transform.Find ("SelectedArea").gameObject.SetActive (true);
      } else {
         toggle.gameObject.transform.Find ("SelectedArea").gameObject.SetActive (false);
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

   void NpcThankyou () {

        if (npcSkeletonGrpahic != null) {
            var state = npcSkeletonGrpahic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            npcSkeletonGrpahic.Update(0);
        }
   }

   void RefreshMotionRedeem (int target) {
      // Decrease Number
      remainGoldenTicketUtility.AnimateTo (target);

   }

   void RefreshMotionAddInventory (int quantities) {
      
      // Motion Add Inventory
      Rewardable rewardable = curGoldenTicketCard.data.rewardable;
      GameObject clone = GameObject.Instantiate (gotItemMotion.gameObject);
      clone.transform.SetParent (gotItemParent);
      clone.transform.localPosition = new Vector3 (0,0,0);
      clone.GetComponent <GotItemMotion> ().ChangeDisplay (rewardable.RewardSprite, rewardable.RewardScale, quantities);
   }

   void RefreshMotionPrize () {
      motionSelectedPrize.SetTrigger ("OnMotion");
   }

   void RefreshRedeemColorButton () {
      if (cnRedeemButton == null)
      cnRedeemButton = StartCoroutine (nRefreshRedeemColorButton ());
   }

   IEnumerator nRefreshRedeemColorButton () {
      redeemButton.image.sprite = redeemRed;
      yield return new WaitForSeconds (0.2f);
      RefreshRedeemEligible (curGoldenTicketCard);
      cnRedeemButton = null;
   }
   #endregion
   #region Exchange Redeem
   public void Redeem () {
      if (curGoldenTicketCard.data.IsEligible ()) {
         GoldenTicket.instance.UsingLoot (curGoldenTicketCard.data.ticketCost);
         curGoldenTicketCard.data.rewardable.RewardGotItem (1);
         RefreshDisplay (false);
        // RefreshRedeemEligible (curGoldenTicketCard);
         // Invoke ("RefreshToggleCard",0.5f);
         RefreshToggleCard ();
         NpcThankyou ();
         RefreshMotionRedeem (GoldenTicket.instance.TotalValue);
         RefreshMotionAddInventory (1);
         RefreshMotionPrize ();
         RefreshRedeemColorButton ();
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
