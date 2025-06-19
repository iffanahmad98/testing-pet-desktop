using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using System.Linq;
using MagicalGarden.Farm;

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
        public DateTime lastGeneratedDate;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            StartCoroutine(InitializeAfterDelay());
        }

        IEnumerator InitializeAfterDelay()
        {
            yield return new WaitForSeconds(1f); // ⏱ Delay 1 detik

            // PlayerPrefs.DeleteAll();
            // PlayerPrefs.Save();
            // Debug.Log("✅ Semua PlayerPrefs dihapus.");

            FindHotelTiles();
            LoadLastDate();
            LoadHotelRooms();
            LoadGuestRequests();
            CheckGenerateGuestList();
        }
        [ContextMenu("Debug: save room hotel")]
        private void testroom() {
            SaveHotelRooms();
        }
        [ContextMenu("Debug: delete hotel room")]
        private void DeleteRoomHotel() {
            PlayerPrefs.DeleteKey("SavedHotelRooms");
            PlayerPrefs.Save();
        }
        GuestRarity GetRandomRarity()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 70) return GuestRarity.Normal;
            if (roll < 85) return GuestRarity.Rare;
            if (roll < 95) return GuestRarity.Mythic;
            return GuestRarity.Legend;
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
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            HotelRoom selectedRoom = availableRooms[randomIndex];
            var guestObj = Instantiate(occupiedIconPrefab, objectGuestPool);
            var guestController = guestObj.GetComponent<GuestController>();
            guestController.SetupFromRequest(guest);

            selectedRoom.AssignGuest(guestController);
            Vector3 worldPos = TileManager.Instance.tilemapHotel.GetCellCenterWorld(selectedRoom.hotelPosition) + new Vector3(0, 3f, 0);
            guestObj.transform.position = worldPos;
            SaveHotelRooms();
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
                GuestRequest newRequest = new GuestRequest(name, type, stayDuration, GetRandomRarity());
                todayGuestRequests.Add(newRequest);
            }
            DisplayGuestRequests();
            Debug.Log("Generated " + requestCount + " guest requests for today.");
        }
        private void DisplayGuestRequests()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);

            foreach (var request in todayGuestRequests)
            {
                var guestItem = Instantiate(prefabGuestItem, content);
                string rarityText = request.rarity.ToString().ToUpper(); // Tambahan
                string desc = $"Nama: {request.guestName}\nTipe: {request.type}\nDurasi: {request.stayDurationDays} hari\nRarity: {rarityText}";
                guestItem.GetComponent<GuestItem>().Setup(request, null, desc);
            }
        }
        private void FindHotelTiles()
        {
            hotelRooms.Clear();
            var mapHotel = TileManager.Instance.tilemapHotel;
            foreach (var pos in mapHotel.cellBounds.allPositionsWithin)
            {
                if (!mapHotel.HasTile(pos)) continue;
                GameObject roomObj = new GameObject("HotelRoom");
                HotelRoom room = roomObj.AddComponent<HotelRoom>();
                room.hotelPosition = pos;
                hotelRooms.Add(room);
            }
        }
        private void CheckGenerateGuestList()
        {
            DateTime today = TimeManager.Instance.currentTime.Date;
            if (today > lastGeneratedDate.Date || today < lastGeneratedDate.Date)
            {
                GenerateGuestRequestsForToday();
                lastGeneratedDate = today;
                SaveLastDate();
                SaveGuestRequests();
            }
            else
            {
                Debug.Log("✅ Guest request hari ini sudah ada, pakai data tersimpan.");
                DisplayGuestRequests(); // tetap tampilkan ke UI
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
        public void SaveLastDate()
        {
            PlayerPrefs.SetString("LastGuestGenDate", lastGeneratedDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }
        public void SaveGuestRequests()
        {
            GuestRequestSaveWrapper wrapper = new GuestRequestSaveWrapper();
            wrapper.guestRequests = todayGuestRequests;

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString("GuestRequestList", json);
            PlayerPrefs.Save();
        }
        private void LoadGuestRequests()
        {
            if (PlayerPrefs.HasKey("GuestRequestList"))
            {
                string json = PlayerPrefs.GetString("GuestRequestList");
                GuestRequestSaveWrapper wrapper = JsonUtility.FromJson<GuestRequestSaveWrapper>(json);
                todayGuestRequests = wrapper.guestRequests ?? new List<GuestRequest>();
            }
        }
#region Save Hotel Room
        public void SaveHotelRooms()
        {
            var savedRooms = new List<SavedHotelRoom>();

            foreach (var room in hotelRooms)
            {
                if (!room.IsOccupied || room.guest == null)
                    continue;

                var guest = room.guest;

                var saved = new SavedHotelRoom
                {
                    x = room.hotelPosition.x,
                    y = room.hotelPosition.y,
                    z = room.hotelPosition.z,
                    guest = guest.ToRequest(),
                    checkInDate = guest.checkInDate.ToString("yyyy-MM-dd"),
                    happiness = guest.happiness
                };

                savedRooms.Add(saved);
            }

            string json = JsonUtility.ToJson(new Wrapper<List<SavedHotelRoom>> { data = savedRooms });
            Debug.LogError(json);
            PlayerPrefs.SetString("SavedHotelRooms", json);
            PlayerPrefs.Save();
        }
        public void LoadHotelRooms()
        {
            if (!PlayerPrefs.HasKey("SavedHotelRooms")) return;

            string json = PlayerPrefs.GetString("SavedHotelRooms");
            Debug.LogError(json);
            var wrapper = JsonUtility.FromJson<Wrapper<List<SavedHotelRoom>>>(json);
            if (wrapper == null || wrapper.data == null) return;
            DateTime today = TimeManager.Instance.currentTime;
            foreach (var saved in wrapper.data)
            {
                if (saved.guest == null) continue;

                DateTime checkInDate = DateTime.Parse(saved.checkInDate);
                int stayedDays = (today - checkInDate).Days;
                Debug.LogError(stayedDays);
                if (stayedDays >= saved.guest.stayDurationDays)
                {
                    int coinReward = 100;
                    if (saved.guest.rarity == GuestRarity.Rare) coinReward = 200;
                    if (saved.guest.rarity == GuestRarity.Mythic) coinReward = 250;
                    if (saved.guest.rarity == GuestRarity.Legend) coinReward = 300;

                    if (saved.happiness > 80)
                    {
                        CoinManager.Instance.AddCoins(coinReward);
                        Debug.Log($"🎉 {saved.guest.guestName} happy, +{coinReward} coins!");
                    }
                    else
                    {
                        Debug.Log($"{saved.guest.guestName} sudah pergi, happiness: {saved.happiness}");
                    }

                    continue; // skip loading
                }

                // Tamu masih menginap
                Vector3Int pos = new Vector3Int(saved.x, saved.y, saved.z);
                var room = hotelRooms.FirstOrDefault(r => r.hotelPosition == pos);
                if (room != null)
                {
                    var guestObj = Instantiate(occupiedIconPrefab, objectGuestPool);
                    var guestController = guestObj.GetComponent<GuestController>();
                    guestController.SetupFromRequest(saved.guest);
                    guestController.happiness = (int)saved.happiness;
                    guestController.SetHappiness(saved.happiness);
                    room.AssignGuestLoad(guestController);

                    Vector3 worldPos = TileManager.Instance.tilemapHotel.GetCellCenterWorld(pos) + new Vector3(0, 3f, 0);
                    guestObj.transform.position = worldPos;
                }
            }
        }
#endregion
    }
    [Serializable]
    public class GuestRequestSaveWrapper
    {
        public List<GuestRequest> guestRequests = new();
    }
    [Serializable]
    public class SavedHotelRoom
    {
        public int x, y, z;
        public GuestRequest guest;
        public string checkInDate; // ex "2025-06-17"
        public float happiness;
    }
    [Serializable]
    public class Wrapper<T>
    {
        public T data;
    }
    
}

