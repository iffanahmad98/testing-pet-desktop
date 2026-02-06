using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
   public event System.Action ShowEvent;
   public event System.Action HideEvent;
   [SerializeField] Image hotelShopUI;
   [SerializeField] Button closeShopButton;
   [SerializeField] Canvas worldCanvas;

   
   void Start () {
    AddListeners ();
    LoadDictionaries ();
    AddEvents ();
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
      
      ShowEvent?.Invoke ();
      iCameraDragLocker.AddLockedBy (this.gameObject);
      UIWorldManager.Instance.OnBackgroundOutside ();
      hotelShopUI.gameObject.SetActive (true);
      hotelShopUI.transform.localScale = new Vector3 (1,1,1);
      OnShopMenu ("GiftExchangeMenu");

      RectTransform worldRect = worldCanvas.GetComponent<RectTransform>();
   RectTransform shopRect = hotelShopUI.GetComponent<RectTransform>();

      HotelMainUI.instance.Hide ();
   /*
   Debug.LogError(
      "UI SHOP Scale: " + hotelShopUI.transform.localScale +
      " | Canvas Scale: " + worldCanvas.transform.localScale +
      " | Canvas Size: " + worldRect.rect.width + "x" + worldRect.rect.height +
      " | UI Size: " + shopRect.rect.width + "x" + shopRect.rect.height
   );
   */
   }

   public void OffShopUI () {
      HideEvent?.Invoke ();
      iCameraDragLocker.RemoveLockedBy (this.gameObject);
      UIWorldManager.Instance.OffBackgroundOutside ();
      hotelShopUI.gameObject.SetActive (false);

      HotelMainUI.instance.Show ();
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
   
   #region Event
   void AddEvents () {
      AddShowEvent (RequirementTipManager.Instance.HideAllRequirementTipClick2d);
      AddHideEvent (RequirementTipManager.Instance.ShowAllRequirementTipClick2d);

      AddShowEvent (TooltipManager.Instance.HideAllRequirementTipClick2d);
      AddHideEvent (TooltipManager.Instance.ShowAllRequirementTipClick2d);
   }

   void AddShowEvent (System.Action value) { 
      ShowEvent += value;
   }

   void AddHideEvent (System.Action value) { 
      HideEvent += value;
   }
   #endregion
}
