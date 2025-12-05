using UnityEngine;
using UnityEngine.UI;

public class UIWorldManager : MonoBehaviour
{
   public static UIWorldManager Instance;

   [SerializeField] Image backgroundOutside;
   [SerializeField] Image backgroundDummy;
   [SerializeField] Image hotelShopUI;

   void Awake () {
    Instance = this; 
   }

   void Start () {
    OffBackgroundOutside ();
   } 

   public void OnBackgroundOutside () { // HotelShop
    backgroundOutside.gameObject.SetActive (true);
    backgroundDummy.gameObject.SetActive (true);
    hotelShopUI.gameObject.SetActive (true);
   } 

   public void OffBackgroundOutside () {
    backgroundOutside.gameObject.SetActive (false);
    backgroundDummy.gameObject.SetActive (false);
    hotelShopUI.gameObject.SetActive (false);
   } 

}
