using UnityEngine;
using MagicalGarden.Manager;
using MagicalGarden.Hotel;
using System.Collections;
using System.Collections.Generic;
// attach and become child to npc that has feature "Hotel Service"
public class HotelRequestDetector : MonoBehaviour {

    // NPCRoboShroom
    public HotelController GetRandomHotelRequest () {
        return HotelManager.Instance.GetRandomHotelRequestDetector ();
    }  

    public bool IsHasHotelRequest () {
        return HotelManager.Instance.IsHasHotelRequest ();
    }

    public List <HotelController> GetListHotelController () {
        return HotelManager.Instance.GetListHotelController ();
    }

    public void RemoveSpecificHotelControllerHasRequest (HotelController hotelController) {
        HotelManager.Instance.RemoveHotelControllerHasRequest (hotelController);
    }
}
