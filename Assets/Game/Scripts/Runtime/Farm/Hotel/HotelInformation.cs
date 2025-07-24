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

        public void Setup(HotelController hotelController)
        {
            titleGuest.text = hotelController.nameGuest;
            guestImage.sprite = hotelController.iconGuest;
            partyCount.text = hotelController.party.ToString();
            timeStay.text = hotelController.GetFormattedRemainingTime();
        }
    }
}
