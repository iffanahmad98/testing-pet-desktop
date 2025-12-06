using UnityEngine;
using TMPro;

public class HotelGiftExchangeMenu : HotelShopMenuBase
{
   int totalGift;
   [SerializeField] TMP_Text remainingGiftText;
   
   public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay ();
   }

   public override void HideMenu () {
    base.HideMenu ();
   }

   void RefreshDisplay () {
      totalGift = SaveSystem.PlayerConfig.hotelGift;
      remainingGiftText.text = "Remaining : " + totalGift.ToString ();

   }
}
