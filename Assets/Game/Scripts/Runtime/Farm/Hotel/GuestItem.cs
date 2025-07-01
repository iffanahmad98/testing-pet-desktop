using UnityEngine;
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

        public void Setup(GuestRequest guest, Image image)
        {
            guestIcon = image;
            titleText.text = guest.guestName;
            descPartyText.text = guest.party.ToString();
            descPriceText.text = guest.price.ToString();
            descTimeText.text = guest.GetStayDurationString();

            checkInBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            checkInBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                HotelRoom room = HotelManager.Instance.AssignGuestToAvailableRoom(guest);
                if (room != null)
                {

                    var guestPrefab = HotelManager.Instance.GetRandomGuestPrefab();
                    var guest = Instantiate(guestPrefab, HotelManager.Instance.guestSpawnPoint.position, Quaternion.identity);
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
