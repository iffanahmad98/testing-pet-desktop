using UnityEngine;
using TMPro;
using MagicalGarden.Inventory;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MagicalGarden.Manager;
using MagicalGarden.Farm;
using System;

namespace MagicalGarden.Hotel
{
    public class GuestItem : MonoBehaviour
    {
        [Header("UI References")]
        public Image guestIcon;
        public TextMeshProUGUI descText;
        public Button acceptButton;
        private GuestRequest guest;

        public void Setup(GuestRequest guest, Image image, string desc)
        {
            guestIcon = image;
            descText.text = desc;

            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(() =>
            {
                bool assigned = HotelManager.Instance.AssignGuestToAvailableRoom(guest);
                if (assigned)
                {
                    Destroy(gameObject);
                }
            });
        }
    }
}
