using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class HotelShop : MonoBehaviour
{
   
   [Header ("IHotelShopMenu")] 
   public MonoBehaviour hotelGiftExchangeMenu;
   public MonoBehaviour hotelFacilitiesMenu;
   public MonoBehaviour hotelEggsCollectionMenu;
   public MonoBehaviour hotelGoldenTicketMenu;
 
   public MonoBehaviour cameraDragLocker;
   public ILockedBy iCameraDragLocker => cameraDragLocker as ILockedBy;

   public Button [] categoryButtons;    
   public Dictionary <string, IHotelShopMenu> dictionaryIHotelShopMenu = new Dictionary <string, IHotelShopMenu> ();
   [SerializeField] Image hotelShopUI;
   [SerializeField] Button closeShopButton;
   void Start () {
    AddListeners ();
    LoadDictionaries ();
   }

   void AddListeners() {
    categoryButtons[0].onClick.AddListener(() => OnShopMenu("GiftExchangeMenu"));
    categoryButtons[1].onClick.AddListener(() => OnShopMenu("FacilitiesMenu"));
    categoryButtons[2].onClick.AddListener(() => OnShopMenu("EggsCollectionMenu"));
    categoryButtons[3].onClick.AddListener(() => OnShopMenu("GoldenTicketMenu"));

    closeShopButton.onClick.AddListener (OffShopUI);
}

   void LoadDictionaries() {
        dictionaryIHotelShopMenu = new Dictionary<string, IHotelShopMenu>() {
            { "GiftExchangeMenu", hotelGiftExchangeMenu as IHotelShopMenu },
            { "FacilitiesMenu", hotelFacilitiesMenu as IHotelShopMenu },
            { "EggsCollectionMenu", hotelEggsCollectionMenu as IHotelShopMenu },
            { "GoldenTicketMenu", hotelGoldenTicketMenu as IHotelShopMenu }
        };

        HideAllMenu ();
    }

   // ClickableShopHotel :
   public void OnShopUI () {
      iCameraDragLocker.AddLockedBy (this.gameObject);
      UIWorldManager.Instance.OnBackgroundOutside ();
      hotelShopUI.gameObject.SetActive (true);
      OnShopMenu ("GiftExchangeMenu");
   }

   public void OffShopUI () {
      iCameraDragLocker.RemoveLockedBy (this.gameObject);
      UIWorldManager.Instance.OffBackgroundOutside ();
      hotelShopUI.gameObject.SetActive (false);
   }

   
   public void OnShopMenu (string code) {
      HideAllMenu ();
      dictionaryIHotelShopMenu [code].ShowMenu (); 
   }

   public void OffShopMenu (string code) {
    dictionaryIHotelShopMenu [code].HideMenu ();
   } 
   
   void HideAllMenu () {
      foreach (var kvp in dictionaryIHotelShopMenu)
         {
            kvp.Value.HideMenu();
         }
   }
   
}
