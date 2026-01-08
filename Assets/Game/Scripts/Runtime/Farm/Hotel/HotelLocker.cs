using UnityEngine;
using MagicalGarden.Manager;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;

public class HotelLocker : MonoBehaviour
{  // semua setup data tetap disini, karena PlayerConfig sudah kebanyakan
 PlayerConfig playerConfig;
 public void StartSystem () {
    playerConfig = SaveSystem.PlayerConfig;
    CheckFirstTime ();
    RefreshLock ();
 }
 #region FirstTime
 void CheckFirstTime () {
    if (playerConfig.listIdHotelOpen.Count == 0) {
        playerConfig.listIdHotelOpen.Add (0);
        SaveSystem.SaveAll ();
    }
 }  
 #endregion
 void RefreshLock () {
    // Let's make all hotels locked first
    List<HotelController> hotelControllers = HotelManager.Instance.GetHotelControllers ();
    foreach (HotelController hotel in hotelControllers) {
        hotel.HotelLocked ();
    }

    // then Unlock equals id
    foreach (int id in playerConfig.listIdHotelOpen) {
        hotelControllers[id].HotelUnlocked ();
    }

    foreach (HotelController hotel in hotelControllers) {
        hotel.GiveOptionBuy ();
    }

  }

  public void BuyHotelController (HotelController hotelController) { // HotelController.cs
    if (!playerConfig.listIdHotelOpen.Contains (hotelController.idHotel)) {
        playerConfig.listIdHotelOpen.Add (hotelController.idHotel);
    }
    RefreshLock ();
  }

}
