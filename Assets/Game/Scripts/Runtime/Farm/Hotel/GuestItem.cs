using UnityEngine;
using TMPro;
using MagicalGarden.Inventory;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MagicalGarden.Manager;
using MagicalGarden.Farm;
using System;
using MagicalGarden.AI;

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
                HotelRoom room = HotelManager.Instance.AssignGuestToAvailableRoom(guest);
                if (room != null)
                {
                    var guest = Instantiate(HotelManager.Instance.guestPrefab, HotelManager.Instance.guestSpawnPoint.position, Quaternion.identity);
                    guest.GetComponent<PetMonsterHotel>().destinationTile.x = room.hotelPosition.x;
                    guest.GetComponent<PetMonsterHotel>().destinationTile.y = room.hotelPosition.y;
                    guest.GetComponent<PetMonsterHotel>().hotelRoomRef = room;
                    guest.GetComponent<PetMonsterHotel>().SetupPetHotel();
                    Destroy(gameObject);
                }
            });
        }
    }
}
