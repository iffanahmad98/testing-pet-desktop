using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

[System.Serializable]
public class HotelMenuEggCard {
   public HotelEggsCollectionMenu hotelEggsCollectionMenu;
   public LootType lootType;
   public GameObject hotelEggDisplay;
   public EggCrackAnimator eggCrackAnimator; 

   [Header ("Card Panel")]
   public Toggle cardToggle;
   public Image selectedCard;
   public TMP_Text remainText;
   
   [HideInInspector] public int remain;

   public void RefreshRemain () {
      remain = GetLootUsable (lootType).GetCurrency ();
      remainText.text = remain.ToString ();
   }

   LootUseable GetLootUsable (LootType lootType) {
        switch (lootType) {
            case LootType.NormalEgg :
            return NormalEgg.instance;
            break;
            case LootType.RareEgg :
            return RareEgg.instance;
            break;
  
        }
        return null;
    }

     public void LoadListener () {
         selectedCard.gameObject.SetActive (false);
         hotelEggDisplay.gameObject.SetActive (false);
         cardToggle.onValueChanged.AddListener(OnCardToggle);
         eggCrackAnimator.AddDoneConfirmEvent (hotelEggsCollectionMenu.CloseEgg);
     }

    public void OnCardToggle (bool isOn) {
      if (isOn) {
         selectedCard.gameObject.SetActive (true);
         hotelEggDisplay.gameObject.SetActive (true);
         hotelEggsCollectionMenu.SelectHotelMenuEgg (this);
      } else {
         selectedCard.gameObject.SetActive (false);
         hotelEggDisplay.gameObject.SetActive (false);
      }
    }  

    public void CloseEgg () {
         hotelEggDisplay.gameObject.SetActive (true);
    }

    public void UsingEgg () {
      GetLootUsable (lootType).UsingLoot (1);
    }
}

public class HotelEggsCollectionMenu : HotelShopMenuBase
{

   
   [SerializeField] HotelMenuEggCard [] hotelMenuEggCards;
   HotelMenuEggCard selectedHotelMenuEgg;

   [SerializeField] Button openEggButton;
   [SerializeField] Sprite openEggAvailable, openEggNotAvailable;
   [SerializeField] Image hotelShopBlocker;
   [SerializeField] SkeletonGraphic npcSkeletonGrpahic;
   bool listenerLoaded = false;

   public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay (); 
        LoadListener ();
   }

   public override void HideMenu () {
    base.HideMenu ();
   }
   
   void RefreshDisplay () {
      
      foreach (HotelMenuEggCard eggCard in hotelMenuEggCards) {
         eggCard.RefreshRemain ();
      }
   }

   void LoadListener () {
      if (!listenerLoaded) {
         listenerLoaded = true;
        foreach (HotelMenuEggCard eggCard in hotelMenuEggCards) {
         eggCard.LoadListener ();
        } 

        openEggButton.onClick.AddListener (OpenEgg);

      }
      hotelMenuEggCards[0].cardToggle.isOn = true;

   }

   public void SelectHotelMenuEgg (HotelMenuEggCard hotelMenuEggCard) {
      selectedHotelMenuEgg = hotelMenuEggCard;
      IsCanOpenEgg ();
      
   }

   public void OpenEgg () {
      if (IsCanOpenEgg ()) {
         selectedHotelMenuEgg.eggCrackAnimator.gameObject.SetActive (true);
         selectedHotelMenuEgg.eggCrackAnimator.RollGacha ();
         selectedHotelMenuEgg.hotelEggDisplay.gameObject.SetActive (false);
         selectedHotelMenuEgg.UsingEgg ();
         RefreshDisplay ();
         hotelShopBlocker.gameObject.SetActive (true);
         NpcThankyou ();
      }
   }

   public void CloseEgg () { // is called by EggCrackAnimator
      selectedHotelMenuEgg.CloseEgg ();
      hotelShopBlocker.gameObject.SetActive (false);
      
   }
   bool IsCanOpenEgg () {
      if (selectedHotelMenuEgg.remain > 0) {
         openEggButton.image.sprite = openEggAvailable;
         return true;
      } else {
         openEggButton.image.sprite = openEggNotAvailable;
         return false;
      }
   }

   void NpcThankyou () {

        if (npcSkeletonGrpahic != null) {
            var state = npcSkeletonGrpahic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            npcSkeletonGrpahic.Update(0);
        }
   }

}
