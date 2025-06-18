using UnityEngine;
using System;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using System.Linq;

namespace MagicalGarden.Manager
{
    public class HotelManager : MonoBehaviour
    {
        public static HotelManager Instance;
        [Header("Hotel")]
        public List<HotelRoom> hotelRooms = new();
        public GameObject guestPrefab;
        [Header("Guest Queue")]
        public Transform guestSpawnPoint;
        public List<GuestRequest> todayGuestRequests = new List<GuestRequest>();
        [Header("UI")]
        [SerializeField] private GameObject prefabGuestItem;
        public GameObject occupiedIconPrefab;
        [SerializeField] private Transform content;
        [SerializeField] public Transform objectGuestPool;
        private string[] guestNames = { "Flufflin", "Grizzle", "Lumo", "Chompy", "Zibra" };
        private string[] types = { "Fire", "Water", "Earth", "Air", "Plant" };
        private DateTime lastGeneratedDate;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            FindHotelTiles();
            LoadLastDate();
            CheckGenerateGuestList();
        }
        public bool AssignGuestToAvailableRoom(GuestRequest guest)
        {
            List<HotelRoom> availableRooms = new List<HotelRoom>();

            foreach (var room in hotelRooms)
            {
                if (!room.IsOccupied)
                    availableRooms.Add(room);
            }

            if (availableRooms.Count == 0)
            {
                Debug.LogWarning("No available rooms for guest: " + guest.guestName);
                return false;
            }

            // Pilih satu kamar kosong secara acak
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            HotelRoom selectedRoom = availableRooms[randomIndex];
            var guestObj = Instantiate(occupiedIconPrefab, objectGuestPool);
            var guestController = guestObj.GetComponent<GuestController>();
            guestController.SetupFromRequest(guest);

            selectedRoom.AssignGuest(guestController);
            Vector3 worldPos = TileManager.Instance.tilemapHotel.GetCellCenterWorld(selectedRoom.hotelPosition) + new Vector3(0, 3f, 0);
            guestObj.transform.position = worldPos;

            return true;
        }
        public void GenerateGuestRequestsForToday()
        {
            todayGuestRequests.Clear();
            int requestCount = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < requestCount; i++)
            {
                string name = guestNames[UnityEngine.Random.Range(0, guestNames.Length)];
                string type = types[UnityEngine.Random.Range(0, types.Length)];
                int stayDuration = UnityEngine.Random.Range(1, 4);
                GuestRequest newRequest = new GuestRequest(name, type, stayDuration);
                todayGuestRequests.Add(newRequest);
                var guestItem = Instantiate(prefabGuestItem);
                string desc = $"Nama: {name}\nTipe: {type}\nDurasi: {stayDuration} hari";
                guestItem.GetComponent<GuestItem>().Setup(newRequest, null, desc);
                guestItem.transform.SetParent(content, false);
            }
            Debug.Log("Generated " + requestCount + " guest requests for today.");
        }
        private void FindHotelTiles()
        {
            hotelRooms.Clear();
            var mapHotel = TileManager.Instance.tilemapHotel;
            foreach (var pos in mapHotel.cellBounds.allPositionsWithin)
            {
                if (!mapHotel.HasTile(pos)) continue;
                var room = new HotelRoom();
                room.hotelPosition = pos;
                hotelRooms.Add(room);
            }
        }
        private void CheckGenerateGuestList()
        {
            DateTime now = DateTime.Now.Date;
            if (now > lastGeneratedDate)
            {
                GenerateGuestRequestsForToday();
                lastGeneratedDate = now;
                SaveLastDate();
            }
        }
        private void LoadLastDate()
        {
            if (PlayerPrefs.HasKey("LastGuestGenDate"))
            {
                string dateStr = PlayerPrefs.GetString("LastGuestGenDate");
                lastGeneratedDate = DateTime.Parse(dateStr);
            }
            else
            {
                lastGeneratedDate = DateTime.MinValue; // awal banget
            }
        }
        private void SaveLastDate()
        {
            PlayerPrefs.SetString("LastGuestGenDate", lastGeneratedDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }
        // public void EndDay()
        // {
        //     foreach (var room in allRooms)
        //     {
        //         if (room.IsOccupied)
        //         {
        //             room.CheckOut();
        //         }
        //     }

        //     GenerateGuestRequestsForToday();
        // }
    }

    
}

