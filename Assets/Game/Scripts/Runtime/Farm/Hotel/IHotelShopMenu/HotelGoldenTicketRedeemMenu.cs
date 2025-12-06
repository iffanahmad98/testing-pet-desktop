using UnityEngine;
using TMPro;

public class HotelGoldenTicketRedeemMenu : HotelShopMenuBase
{
  int totalRemainGoldenTicket = 0;
  [SerializeField] TMP_Text goldenTicketText;
  public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay ();
   }

   public override void HideMenu () {
    base.HideMenu ();
   }

   void RefreshDisplay () {
      totalRemainGoldenTicket = SaveSystem.PlayerConfig.goldenTicket;
      goldenTicketText.text = totalRemainGoldenTicket.ToString ();
   }
}
