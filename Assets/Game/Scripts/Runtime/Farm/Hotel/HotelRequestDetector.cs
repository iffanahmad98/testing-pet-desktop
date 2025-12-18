using UnityEngine;
using MagicalGarden.Manager;
using MagicalGarden.Hotel;
// attach and become child to npc that has feature "Hotel Service"
public class HotelRequestDetector : MonoBehaviour {

    // NPCRoboShroom
    public HotelController GetRandomHotelRequest () {
        return HotelManager.Instance.GetRandomHotelRequestDetector ();
    }  

    public bool IsHasHotelRequest () {
        return HotelManager.Instance.IsHasHotelRequest ();
    }

}
