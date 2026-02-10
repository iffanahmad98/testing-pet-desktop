using UnityEngine;
using MagicalGarden.Manager;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;

public class HotelLocker : MonoBehaviour
{  // semua setup data tetap disini, karena PlayerConfig sudah kebanyakan
 IPlayerHistory iPlayerHistory;
 PlayerConfig playerConfig;
 public event System.Action RefreshHotelController;
 public void StartSystem () { // HotelManager.cs
    iPlayerHistory = PlayerHistoryManager.instance;
    playerConfig = SaveSystem.PlayerConfig;
    CheckFirstTime ();
    RefreshLock ();
    iPlayerHistory.AddHotelRoomCompletedChanged (RefreshGiveOptionBuy);
    CoinManager.AddCoinChangedRefreshEvent (RefreshGiveOptionBuy);
    MonsterManager.instance.AddEventPetMonsterChanged (RefreshGiveOptionBuy);
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

   void RefreshGiveOptionBuy () {
        List<HotelController> hotelControllers = HotelManager.Instance.GetHotelControllers ();
        if (playerConfig.listIdHotelOpen.Count < hotelControllers.Count) {
            foreach (HotelController hotel in hotelControllers) {
                if (hotel.GetIsLocked ()) {
                    hotel.GiveOptionBuy ();
                }
            }
        }
   } 

  public void BuyHotelController (HotelController hotelController) { // HotelController.cs
    if (!playerConfig.listIdHotelOpen.Contains (hotelController.idHotel)) {
        playerConfig.listIdHotelOpen.Add (hotelController.idHotel);
    }
    RefreshLock ();
    RefreshHotelController?.Invoke ();
  }

  #region Event
  public void AddEventHotelRoom (System.Action actionValue) { // HotelMainUI
    RefreshHotelController += actionValue;
  }  
  #endregion

}
