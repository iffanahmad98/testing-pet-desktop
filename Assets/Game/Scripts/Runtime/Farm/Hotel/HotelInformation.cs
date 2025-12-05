using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MagicalGarden.Manager;
using MagicalGarden.AI;

namespace MagicalGarden.Hotel
{
    public class HotelInformation : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleGuest;
        public Image guestImage;
        public TextMeshProUGUI partyCount;
        public TextMeshProUGUI timeStay;

        [Header("Hotel Reference")]
        public HotelController currentHotelController;

        public void Setup(HotelController hotelController)
        {
            Debug.Log ("Testing");
            currentHotelController = hotelController;
            titleGuest.text = hotelController.nameGuest;
            guestImage.sprite = hotelController.iconGuest;
            partyCount.text = hotelController.party.ToString();
            timeStay.text = hotelController.GetFormattedRemainingTime();
        }

        public void FulfillRequestByString(string typeStr)
        {
            if (currentHotelController != null)
            {
                currentHotelController.FulfillRequestByString(typeStr);
            }
            else
            {
                Debug.LogWarning("HotelInformation: currentHotelController is null, cannot fulfill request");
            }
        }
    }
}
