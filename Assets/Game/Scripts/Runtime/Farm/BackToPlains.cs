using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BackToPlains : MonoBehaviour
{
   public static BackToPlains instance;
   // All services in hotel and farm must be registered here (If there's a bug) 
   [Header ("Service Hotel")]
   [SerializeField] MagicalGarden.AI.NPCHotel npcHotel;
   [SerializeField] List <MagicalGarden.Hotel.HotelController> listHotelControllerServiceProgress = new ();
   
   void Awake () {
      instance = this;
   }

   // Attach to back button
   public void StartConfig () {
    StopNPCHotel ();
    StopProgressHotelController ();
   }

   #region NPC Hotel
   void StopNPCHotel () {
    npcHotel.StopMovement ();
   }
   #endregion
   #region Hotel Controller
   void StopProgressHotelController () {
      for (int i = listHotelControllerServiceProgress.Count-1; i >= 0; i --) {
         listHotelControllerServiceProgress[i].BackToPlainsEvent ();
         RemoveHotelControllerServiceProgress (listHotelControllerServiceProgress[i]);
      }
   }

   public void AddHotelControllerServiceProgress (MagicalGarden.Hotel.HotelController hotelController) {
      if (!listHotelControllerServiceProgress.Contains (hotelController)) {
         listHotelControllerServiceProgress.Add (hotelController);
      }
   }

   public void RemoveHotelControllerServiceProgress (MagicalGarden.Hotel.HotelController hotelController) {
      if (listHotelControllerServiceProgress.Contains (hotelController)) {
         listHotelControllerServiceProgress.Remove (hotelController);
      }
   }

   #endregion

}
