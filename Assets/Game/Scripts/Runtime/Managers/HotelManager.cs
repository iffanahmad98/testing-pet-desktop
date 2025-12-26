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
        [Header("Visual FX")]
        public GameObject cleaningVfx;
        public GameObject rayCleaningVfx;
        public GameObject brokenHeartVfx;
        GameObject currentCleaningVFX;

        [Header("Hotel Setting")]
        public Transform poolHotelRoom;
        public Vector2Int targetCheckOut;
        public List<HotelController> hotelControllers = new List<HotelController>();
        public List<GuestStageGroup> guestStageGroup;
        public NPCHotel npcHotel;
        public List <NPCHotel> listNpcHotelAvailables = new List <NPCHotel> ();
        public bool npcHotelAvailable = false;

        [Header("Guest Queue")]
        public Transform guestSpawnPoint;
        public List<GuestRequest> todayGuestRequests = new List<GuestRequest>();
        [Header("UI")]
        [SerializeField] private GameObject prefabGuestItem;
        [SerializeField] private GameObject emptyGuest;
        [SerializeField] private Transform content;
        [SerializeField] public Transform objectGuestPool;
        private string[] types = { "Fire", "Water", "Earth", "Air", "Plant" };
        public DateTime lastGeneratedDate;
        public List <Button> listBubbleRequest = new List <Button> ();

        [Header("Debug")]
       // public int minRequestCount = 5;
      //  public int maxRequestCount = 7;

        [Header ("Management")]
        public List <HotelController> listHotelControllerHasRequest = new List <HotelController> ();

        [Tooltip ("NPC Robo Shroom")]
        public List <NPCRoboShroom> listNPCRoboShroom = new List <NPCRoboShroom> ();
        
        [Tooltip ("Data")]
        PlayerConfig playerConfig;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            playerConfig =  SaveSystem.PlayerConfig;
            StartCoroutine(InitializeAfterDelay());
        }

        IEnumerator InitializeAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            emptyGuest.SetActive(false);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            FindAllHotelRoom();
            LoadLastDate();
            LoadGuestRequests();
            CheckGenerateGuestList();
            LoadAllHotelControllerDatas ();
            LoadAllPetMonsterHotelDatas ();
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
            int randomIndex = UnityEngine.Random.Range(0, guestStageGroup.Count);
            return guestStageGroup[randomIndex].stage1;
        }

        public GuestStageGroup GetRandomGuestStagePrefab()
        {
            return guestStageGroup[UnityEngine.Random.Range(0, guestStageGroup.Count)];
        }

        // HotelController
        public GuestStageGroup GetSpecificGuestStagePrefab (string targetName)
        {
            foreach (GuestStageGroup guest in guestStageGroup) {
                if (guest.name == targetName) {
                    return guest;
                }
            }
            Debug.LogError ("Specific Guest Stage Prefab not found");
            return null;
        }
        GuestRarity GetRandomRarity()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < 70) return GuestRarity.Common;
            if (roll < 85) return GuestRarity.Rare;
            if (roll < 95) return GuestRarity.Mythic;
            return GuestRarity.Legend;
        }

        public void CallCleaningVFX(Transform pos)
        {
            currentCleaningVFX = Instantiate(cleaningVfx, pos.position, Quaternion.identity);
        }
        public void DestroyCleaningVFX(Transform pos)
        {
            Instantiate(rayCleaningVfx, pos.position, Quaternion.identity);
            Destroy(currentCleaningVFX);
        }
        public void AssignGuestToAvailableRoom(GuestRequest guest)
        {
            List<HotelController> availableRooms = new List<HotelController>();

            foreach (var room in hotelControllers)
            {
                if (!room.IsOccupied && !room.holdReward)
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

                PetMonsterHotelData petMonsterHotelData = new PetMonsterHotelData {
                    idHotel = hotelController.idHotel,
                    guestStageGroupName = guest.GuestGroup.name,
                    guestStage = stage
                };
                 pet.SetPetMonsterHotelData (petMonsterHotelData);
                 /*
                    playerConfig.AddPetMonsterHotelData (petMonsterHotelData);
                    SaveSystem.SaveAll ();
                */
            }

            bool removed = todayGuestRequests.Remove(guest);
            emptyGuest.SetActive(todayGuestRequests.Count == 0);

            playerConfig.RemoveGuestRequestData (guest.guestRequestData);
            SaveSystem.SaveAll ();
        }

        public void DeclineGuest (GuestRequest guest) { // GuestItem.cs

            bool removed = todayGuestRequests.Remove(guest);

            playerConfig.RemoveGuestRequestData (guest.guestRequestData);
            SaveSystem.SaveAll ();
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
        public void GenerateGuestRequestsForToday(DateTime generateTime, bool freeGenerate, int specificGenerate = 0)
        {
            
            int requestCount = 0;
            if (freeGenerate) {
                playerConfig.ClearAllGuestRequestData ();
                todayGuestRequests.Clear();
                requestCount = UnityEngine.Random.Range(GetTotalHotelControllerAvailable (), GetTotalHotelControllerAvailable () +4);
                if (requestCount < 5) {
                    requestCount =5;
                }
            } else {
                requestCount = specificGenerate;
            }
            

            for (int i = 0; i < requestCount; i++)
            {
                string type = types[UnityEngine.Random.Range(0, types.Length)];
                int party = UnityEngine.Random.Range(1, 5);
                //int price = UnityEngine.Random.Range(100, 301);
                
                //random
                // int days = UnityEngine.Random.Range(2, 6);
                // int minutes = UnityEngine.Random.Range(0, 60);
                // TimeSpan stayDuration = new TimeSpan(days, 0, minutes, 0);
                int day = UnityEngine.Random.Range (1,4);
                // day =0;
                TimeSpan stayDuration = new TimeSpan(day, 0, 0, 0); // 3 menit // 0,0,3,0
                int price = (party * 150) + (day * 400) + UnityEngine.Random.Range (1,100);
                var guestTemp = GetRandomGuestStagePrefab();

                GuestRequestData guestRequestData = new GuestRequestData ();
                guestRequestData.type = type;
                guestRequestData.party = party;
                guestRequestData.price = price;
                guestRequestData.stayDuration = stayDuration;
                guestRequestData.guestName = guestTemp.name;
                
                GuestRequest newRequest = new GuestRequest(guestRequestData, guestTemp.name,guestTemp.icon, type, party, price, stayDuration, guestTemp.guestType);
                newRequest.GuestGroup = guestTemp;
                todayGuestRequests.Add(newRequest);
                 playerConfig.AddGuestRequestData (guestRequestData);
                                 

            }
            DisplayGuestRequests();

            if (freeGenerate) {
                playerConfig.SetLastRefreshGenerateGuest (generateTime);
            }
            SaveSystem.SaveAll ();
            
            Debug.Log("Generated " + requestCount + " guest requests for this time.");
        }

        public void LoadLastestGuestRequest () {
            todayGuestRequests.Clear();
            List <GuestRequestData> listGuestRequestData = playerConfig.GetListGuestRequestData ();

            for (int i = 0; i < listGuestRequestData.Count; i++)
            {
                var guestRequestData = listGuestRequestData[i];

                string type = guestRequestData.type;
                int party = guestRequestData.party;
                int price = guestRequestData.price;
    
                TimeSpan stayDuration = guestRequestData.stayDuration;
                var guestTemp = GetSpecificGuestStagePrefab(guestRequestData.guestName);
                GuestRequest newRequest = new GuestRequest(guestRequestData, guestTemp.name,guestTemp.icon, type, party, price, stayDuration, guestTemp.guestType);
                newRequest.GuestGroup = guestTemp;

                todayGuestRequests.Add(newRequest);
                                 
            }
            DisplayGuestRequests();

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
            int curId= 0;
            foreach (Transform child in poolHotelRoom)
            {
                HotelController controller = child.GetComponent<HotelController>();
                if (controller != null)
                {
                    controller.idHotel = curId;
                    hotelControllers.Add(controller);
                    curId ++;
                }
            }
        }

        public void CheckGenerateGuestList() // this, HotelReciption (Listener)
        {
            DateTime today = TimeManager.Instance.currentTime;
            Debug.Log ("Generate hari ini : " + today + playerConfig.lastRefreshGenerateGuest);
            /*
            if (today > lastGeneratedDate.Date || today < lastGeneratedDate.Date)
            {
                GenerateGuestRequestsForToday(today);

              //  lastGeneratedDate = today;
             //   SaveLastDate();
             //   SaveGuestRequests();
            }
            else
            {
                Debug.Log("✅ Guest request hari ini sudah ada, pakai data tersimpan.");
               // DisplayGuestRequests();
            }
            */
            if (FirstTimeCheckGenerateQuest(SaveSystem.PlayerConfig.lastRefreshGenerateGuest)) {
                playerConfig.SetLastRefreshGenerateGuest (today);
                SaveSystem.SaveAll ();
            }

            TimeSpan diffTime = today - playerConfig.lastRefreshGenerateGuest;
            double hours = diffTime.TotalHours;
            if (hours > 8) {
                GenerateGuestRequestsForToday(today, true);
            } else {
                Debug.Log ("Generate belum expired");
                LoadLastestGuestRequest ();
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
            foreach (var room in hotelControllers)
            {
                if (!room.IsOccupied)
                    continue;
            }

            // foreach (var room in hotelRooms)
            // {
            //     if (!room.IsOccupied || room.guest == null)
            //         continue;

            //     var guest = room.guest;

            //     var saved = new SavedHotelRoom
            //     {
            //         x = room.hotelPosition.x,
            //         y = room.hotelPosition.y,
            //         z = room.hotelPosition.z,
            //         guest = guest.ToRequest(),
            //         checkInDate = guest.checkInDate.ToString("yyyy-MM-dd"),
            //         happiness = guest.happiness
            //     };

            //     savedRooms.Add(saved);
            // }

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
                        Farm.CoinManager.Instance.AddCoins(coinReward);
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
                // var room = hotelRooms.FirstOrDefault(r => r.hotelPosition == pos);
                // if (room != null)
                // {
                //     // var guestObj = Instantiate(occupiedIconPrefab, objectGuestPool);
                //     var guestController = room.guest;
                //     guestController.SetupFromRequest(saved.guest);
                //     guestController.happiness = (int)saved.happiness;
                //     guestController.SetHappiness(saved.happiness);
                //     room.AssignGuestLoad(guestController);

                //     Vector3 worldPos = TileManager.Instance.tilemapHotel.GetCellCenterWorld(pos) + new Vector3(0, 3f, 0);
                //     room.transform.position = worldPos;
                // }
            }
        }
        #endregion
        #region NPCHotel
        // NPCHotel
        public void AddNPCHotelAvailable (NPCHotel npc) {
            listNpcHotelAvailables.Add (npc);
            if (listNpcHotelAvailables.Count>0) {
                npcHotelAvailable = true;
            }
            RefreshBubbleRoomService ();
        }
        // NPCHotel
        public void RemoveNPCHotelAvailable (NPCHotel npc) {
            listNpcHotelAvailables.Remove (npc);
            if (listNpcHotelAvailables.Count==0) {
                npcHotelAvailable = false;
            }
            RefreshBubbleRoomService ();
        }

        // HotelController
        public bool CheckNPCHotelAvailable () {
            return npcHotelAvailable;
        }
        
        void RefreshBubbleRoomService () {
            if (npcHotelAvailable) {
                foreach (Button bubble in listBubbleRequest) {
                    bubble.interactable = true;
                }
            }
            else {
                foreach (Button bubble in listBubbleRequest) {
                    bubble.interactable = false;
                }
            }
        }
        #endregion

        #region HotelController
        // HotelController
        public void AddBubbleRequest (Button bubbleButton) {
            listBubbleRequest.Add (bubbleButton);
            RefreshBubbleRoomService ();
        }

        // HotelController
        public void RemoveBubbleRequest (Button bubbleButton) {
            listBubbleRequest.Remove (bubbleButton);
            RefreshBubbleRoomService ();
        }

        public void AddHotelControllerHasRequest (HotelController hotelController) {
            if (!listHotelControllerHasRequest.Contains (hotelController))
            Debug.Log ("Add Hotel Request");
            listHotelControllerHasRequest.Add (hotelController);
        }

        public void RemoveHotelControllerHasRequest (HotelController hotelController, bool refreshRobo = false) {
            if (listHotelControllerHasRequest.Contains (hotelController))
             Debug.Log ("Remove Hotel Request");
            listHotelControllerHasRequest.Remove (hotelController);
            if (refreshRobo) {
                RefreshAllMovementRoboShroom (hotelController); // mencegahBug (stuck isServing)
            }
        }

        public void AddGuestRequestAfterCheckOut () { // called at HotelController.cs when their check out.
            GenerateGuestRequestsForToday (new DateTime (), false,1);
        }
        #endregion

        #region HotelRequestDetector
        public HotelController GetRandomHotelRequestDetector () {
            int target = UnityEngine.Random.Range (0,listHotelControllerHasRequest.Count);
            HotelController targetHotel = listHotelControllerHasRequest[target];
            RemoveHotelControllerHasRequest (targetHotel, false);
            return targetHotel;
        }

        public bool IsHasHotelRequest () {
            return listHotelControllerHasRequest.Count > 0;
        }

        public List <HotelController> GetListHotelController () {
            return listHotelControllerHasRequest;
        }

        public void RemoveSpecificHotelControllerHasRequest (HotelController hotelController) {
            RemoveHotelControllerHasRequest (hotelController, true);
            
        }
        #endregion

        #region NPCRoboShroom
        // NPCRoboShroom.cs
        public void AddNPCRoboShroom (NPCRoboShroom npc) {
            listNPCRoboShroom.Add (npc);
        }

        // NPCRoboShroom.cs
        public void RemoveNPCRoboShroom (NPCRoboShroom npc) {
            listNPCRoboShroom.Remove (npc);
        }

        void RefreshAllMovementRoboShroom (HotelController hotelController) {
            foreach (NPCRoboShroom npc in listNPCRoboShroom) {
                npc.ResetMovementHotel (hotelController);    
            }
        }
        #endregion

        #region Data
        void LoadAllHotelControllerDatas () {
            List <HotelControllerData> listHotelControllerData = new List <HotelControllerData> ();
            listHotelControllerData = playerConfig.GetListHotelControllerData ();
            foreach (HotelControllerData data in listHotelControllerData) {
                hotelControllers [data.idHotel].LoadData (data);
            }

        }

        void LoadAllPetMonsterHotelDatas () {

        }

        #endregion
        #region Utility
        int GetTotalHotelControllerAvailable () {
            int total = 0;
            foreach (HotelController hotel in hotelControllers) {
                if (!hotel.IsOccupied) {
                    total++;
                }
            }
            return total;
        }
        #endregion

        #region Debug Only (IsTimeInFuture)
        bool firstTimeCheckGenerateQuest = false;
        bool FirstTimeCheckGenerateQuest (DateTime dateTime) {
            if (!firstTimeCheckGenerateQuest) {
                firstTimeCheckGenerateQuest = true;
                return TimeManager.Instance.IsTimeInFuture(dateTime);
            } else {
                return false;
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
        [Header("Guest Attributes")]
        public GuestRarity guestType;

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
        public bool IsVIPGuest()
        {
            return guestType == GuestRarity.Rare || guestType == GuestRarity.Mythic || guestType == GuestRarity.Legend;
        }
    }
    
}

