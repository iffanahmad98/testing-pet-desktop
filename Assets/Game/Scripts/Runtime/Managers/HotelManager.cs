using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using System.Linq;
using MagicalGarden.Farm;
using MagicalGarden.AI;

namespace MagicalGarden.Manager
{
    public class HotelManager : MonoBehaviour
    {
        public static HotelManager Instance;
        [Header("Hotel")]
        public Transform poolHotelRoom;
        public Vector2Int targetCheckOut;
        public List<HotelController> hotelControllers = new List<HotelController>();
        public List<HotelRoom> hotelRooms = new();
        public HotelTile cleanTile;
        public HotelTile dirtyTile;
        public List<GameObject> guestPrefab;
        public List<GuestStageGroup> guestStageGroup;
        public NPCHotel npcHotel;
        [Header("Guest Queue")]
        public Transform guestSpawnPoint;
        public List<GuestRequest> todayGuestRequests = new List<GuestRequest>();
        [Header("UI")]
        [SerializeField] private GameObject prefabGuestItem;
        public GameObject roomHotelPrefab;
        [SerializeField] private Transform content;
        [SerializeField] public Transform objectGuestPool;
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
            yield return new WaitForSeconds(1f);

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            FindAllHotelRoom();
            LoadLastDate();
            // LoadHotelRooms();
            LoadGuestRequests();
            CheckGenerateGuestList();
        }
        [ContextMenu("Debug: save room hotel")]
        private void testroom()
        {
            SaveHotelRooms();
        }
        [ContextMenu("Debug: delete hotel room")]
        private void DeleteRoomHotel()
        {
            PlayerPrefs.DeleteKey("SavedHotelRooms");
            PlayerPrefs.Save();
        }

        public GameObject GetRandomGuestPrefab()
        {
            int randomIndex = UnityEngine.Random.Range(0, guestPrefab.Count);
            return guestPrefab[randomIndex];
        }

        public GuestStageGroup GetRandomGuestStagePrefab()
        {
            return guestStageGroup[UnityEngine.Random.Range(0, guestStageGroup.Count)];
        }
        GuestRarity GetRandomRarity()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 70) return GuestRarity.Normal;
            if (roll < 85) return GuestRarity.Rare;
            if (roll < 95) return GuestRarity.Mythic;
            return GuestRarity.Legend;
        }
        // public HotelRoom AssignGuestToAvailableRoom_backup(GuestRequest guest)
        // {
            
        //     SaveHotelRooms();
        // }
        public void AssignGuestToAvailableRoom(GuestRequest guest)
        {
            List<HotelController> availableRooms = new List<HotelController>();

            foreach (var room in hotelControllers)
            {
                if (!room.IsOccupied)
                    availableRooms.Add(room);
            }

            if (availableRooms.Count == 0)
            {
                Debug.LogWarning("No available rooms for guest: " + guest.guestName);
            }
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            HotelController hotelController = availableRooms[randomIndex];
            hotelController.CheckInToRoom(guest);
            Vector3 basePos = guestSpawnPoint.position;

            List<int> stageList = GetStageDistribution(guest.party);
            foreach (var stage in stageList)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    0
                );
                Vector3 spawnPos = basePos + randomOffset;

                var prefab = guest.GuestGroup.GetPrefabByStage(stage);
                var guestObject = Instantiate(prefab, spawnPos, Quaternion.identity);

                var pet = guestObject.GetComponent<PetMonsterHotel>();
                hotelController.AddPet(pet);
                pet.destinationTile.x = hotelController.hotelPositionTile.x;
                pet.destinationTile.y = hotelController.hotelPositionTile.y;
                pet.hotelContrRef = hotelController;
                pet.SetupPetHotel();
            }
        }

        private List<int> GetStageDistribution(int partySize)
        {
            List<int> stages = new List<int>();

            switch (partySize)
            {
                case 1:
                    stages.Add(1);
                    break;
                case 2:
                    stages.AddRange(new int[] { 1, 2 });
                    break;
                case 3:
                    stages.AddRange(new int[] { 1, 2, 3 });
                    break;
                case 4:
                    stages.AddRange(new int[] { 1, 2, 2, 3 });
                    break;
                case 5:
                    stages.AddRange(new int[] { 1, 2, 2, 3, 3 });
                    break;
                default:
                    // fallback: isi dengan stage 1
                    for (int i = 0; i < partySize; i++)
                        stages.Add(1);
                    break;
            }

            return stages;
        }
        public void GenerateGuestRequestsForToday()
        {
            todayGuestRequests.Clear();
            int requestCount = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < requestCount; i++)
            {
                string type = types[UnityEngine.Random.Range(0, types.Length)];
                int party = UnityEngine.Random.Range(1, 5);
                int price = UnityEngine.Random.Range(100, 301);

                //random
                // int days = UnityEngine.Random.Range(2, 6);
                // int minutes = UnityEngine.Random.Range(0, 60);
                // TimeSpan stayDuration = new TimeSpan(days, 0, minutes, 0);
                TimeSpan stayDuration = new TimeSpan(0, 0, 3, 0); // 3 menit
                var guestTemp = GetRandomGuestStagePrefab();
                GuestRequest newRequest = new GuestRequest(guestTemp.name,guestTemp.icon, type, party, price, stayDuration, GetRandomRarity());
                newRequest.GuestGroup = guestTemp;
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
                // string desc = $"Nama: {request.guestName}\nTipe: {request.type}\nDurasi: {request.stayDurationDays} hari\nRarity: {rarityText}";
                guestItem.GetComponent<GuestItem>().Setup(request);
            }
        }

        private void FindAllHotelRoom()
        {
            hotelControllers.Clear();
            foreach (Transform child in poolHotelRoom)
            {
                HotelController controller = child.GetComponent<HotelController>();
                if (controller != null)
                {
                    hotelControllers.Add(controller);
                }
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
                DisplayGuestRequests();
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
                TimeSpan stayedDuration = today - checkInDate;
                if (stayedDuration >= saved.guest.stayDurationDays)
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
                    // var guestObj = Instantiate(occupiedIconPrefab, objectGuestPool);
                    var guestController = room.guest;
                    guestController.SetupFromRequest(saved.guest);
                    guestController.happiness = (int)saved.happiness;
                    guestController.SetHappiness(saved.happiness);
                    room.AssignGuestLoad(guestController);

                    Vector3 worldPos = TileManager.Instance.tilemapHotel.GetCellCenterWorld(pos) + new Vector3(0, 3f, 0);
                    room.transform.position = worldPos;
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
    
    [System.Serializable]
    public class GuestStageGroup
    {
        public string name;
        public string nameInfo;
        public Sprite icon;
        public GameObject stage1;
        public GameObject stage2;
        public GameObject stage3;

        public GameObject GetPrefabByStage(int stage)
        {
            switch (stage)
            {
                case 3:
                    if (stage3 != null) return stage3;
                    goto case 2;
                case 2:
                    if (stage2 != null) return stage2;
                    goto case 1;
                case 1:
                    return stage1; // bisa null, fallback terakhir
                default:
                    return stage1;
            }
        }
    }
    
}

