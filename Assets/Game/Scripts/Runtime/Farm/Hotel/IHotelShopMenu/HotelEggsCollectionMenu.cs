using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class HotelMenuEggCard {
   public LootType lootType;
   public TMP_Text remainText;
   int remain;

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
}

public class HotelEggsCollectionMenu : HotelShopMenuBase
{
   [SerializeField] HotelMenuEggCard [] hotelMenuEggCards;

   public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay (); 
   }

   public override void HideMenu () {
    base.HideMenu ();
   }
   
   void RefreshDisplay () {
      
      foreach (HotelMenuEggCard eggCard in hotelMenuEggCards) {
         eggCard.RefreshRemain ();
      }
   }
}
