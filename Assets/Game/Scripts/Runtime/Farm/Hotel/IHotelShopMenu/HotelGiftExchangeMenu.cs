using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HotelGiftExchangeMenu : HotelShopMenuBase
{
   
   [SerializeField] TMP_Text remainingGiftText;
   [SerializeField] RectTransform giftCollectionPanel;
   [SerializeField] GameObject hotelGiftExchangePrefab;
   
   List <GameObject> listHotelGiftExchange = new List <GameObject> ();
   int totalGift;
   int maxGiftDisplay = 6;
   
   public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay ();
   }

   public override void HideMenu () {
    base.HideMenu ();
    DestroyCloning ();
   }

   void RefreshDisplay () {
      totalGift = SaveSystem.PlayerConfig.hotelGift;
      remainingGiftText.text = "Remaining : " + totalGift.ToString ();

      for (int gift = 0; gift < maxGiftDisplay; gift++) {
         GameObject cloneBox = GameObject.Instantiate (hotelGiftExchangePrefab);
         cloneBox.transform.SetParent (giftCollectionPanel);
         cloneBox.SetActive (true);
         if (listHotelGiftExchange.Count <6) {
            listHotelGiftExchange.Add (cloneBox);
         }
      }

   }

   void DestroyCloning () {
      foreach (GameObject giftBox in listHotelGiftExchange) {
         Destroy (giftBox);
      }

      listHotelGiftExchange.Clear (); 
   }


}
