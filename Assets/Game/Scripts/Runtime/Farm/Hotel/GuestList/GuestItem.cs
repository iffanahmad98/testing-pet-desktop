using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using MagicalGarden.Manager;
using MagicalGarden.AI;

namespace MagicalGarden.Hotel
{
    public class GuestItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image guestIcon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descPartyText;
        public TextMeshProUGUI descPriceText;
        public TextMeshProUGUI descTimeText;
        private GuestRequest guest;

        [Header("Type")]
        public GameObject vipObject;
        public GameObject regObject;
        public GameObject checkInBtn;
        public GameObject confirmBtn;
        public GameObject declineBtn;

        public void Setup(GuestRequest guest)
        {
            this.guest = guest;
            guestIcon.sprite = guest.icon;
            titleText.text = guest.guestName;
            descPartyText.text = guest.party.ToString();
            descPriceText.text = guest.price.ToString();
            descTimeText.text = guest.GetStayDurationString();

            checkInBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            confirmBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            declineBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            checkInBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                HandleGuestCheckIn();
            });
            confirmBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                HandleGuestCheckIn();
            });

            declineBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                HandleGuestDecline ();
               // Destroy(gameObject);
            });
            if (guest.IsVIPGuest())
            {
                checkInBtn.SetActive(false);
                confirmBtn.SetActive(true);
                vipObject.SetActive(true);
                regObject.SetActive(false);
            }else
            {
                checkInBtn.SetActive(true);
                confirmBtn.SetActive(false);
                vipObject.SetActive(false);
                regObject.SetActive(true);
            }
        }


        private void HandleGuestCheckIn()
        {
            HotelManager.Instance.AssignGuestToAvailableRoom(guest);
            Destroy(gameObject);
        }

        private void HandleGuestDecline () {
             HotelManager.Instance.DeclineGuest(guest);
            Destroy (gameObject);
        }
    }
}
