using UnityEngine;
using UnityEngine.UI;
using System;
using MagicalGarden.Manager;
using System.Collections.Generic;
using MagicalGarden.AI;
using MagicalGarden.Farm;
using TMPro;

namespace MagicalGarden.Hotel
{
    public class HotelController : MonoBehaviour
    {
        [Header("Position VFX")]
        public Transform dustPos;
        public Transform rayPos;
        [Header("Visual")]
        [Header("Clean Variants")]
        public Sprite[] cleanSprites;
        [Header("Dirty Variants (Same Index as Clean)")]
        public Sprite[] dirtySprites;
        private SpriteRenderer spriteRenderer;
        private int selectedIndex;
        private bool isDirty = false;

        [Space(20)]
        public bool IsOccupied = false;
        public Vector2Int hotelPositionTile;
        public int offsetDistance;
        public AreaOffsetDirection offsetDirection;
        public List<Vector3Int> wanderingTiles = new List<Vector3Int>();
        [HideInInspector] public string nameGuest;
        [HideInInspector] public Sprite iconGuest;
        [HideInInspector] public string typeGuest;
        [HideInInspector] public int party;
        [HideInInspector] public int price;
        [Range(0, 100)]
        public int happiness = 100;
        DateTime checkInDate;
        public TimeSpan stayDurationDays;
        public GuestRarity rarity = GuestRarity.Common;
        private float logTimer = 0f;
        public List<PetMonsterHotel> listPet = new List<PetMonsterHotel>();
        [SerializeField] Vector2Int [] checkOutTiles;

        [Header("Request UI Buttons (Ini kemungkinan akan dihapus, karena sistem sedikit berbeda)")]
        public GameObject giftBtn;
        public GameObject roomServiceBtn;
        public GameObject foodBtn;
        public Image fillExpired;  // Progress bar untuk countdown

        [Header("Request Timing")]
        // public float requestInterval = 60f;  // Request setiap 60 detik
        public float minRequestInterval = 30f;
        public float maxRequestInterval = 60f; 
        public float durationRequestExpired = 120f;
        private float requestTimer = 0f;
        private bool hasRequest = false;
        private bool onProgressingRequest = false;
        public  bool isPetReachedTarget = false;
        private Coroutine currentRequestCountdown;
        

        [Header ("Request Bubbles")]
        [HideInInspector] public Canvas worldCanvas;
        public GameObject giftBubblePrefab;
        public GameObject roomServiceBubblePrefab;
        public GameObject foodBubblePrefab;
        GameObject currentRequestBubble;
        GuestRequestType currentGuestRequestType;
        
        [Header ("Request Config")]
        public GuestRequestConfig [] guestRequestConfigs;
        [Header ("Hotel Gift")]
        HotelGiftSpawner hotelGiftSpawner;
        
        [Header ("Coin Reward")]
        [SerializeField] GameObject coinBubblePrefab;
        [SerializeField] GameObject coinClaimDisplay;
        GameObject coinBubbleClone;
        int holdCoin = 0;
        bool holdReward = false;
        
        
        void Start()
        {
            CalculateWanderingArea();
            spriteRenderer = GetComponent<SpriteRenderer>();
            selectedIndex = UnityEngine.Random.Range(0, cleanSprites.Length);
            SetClean();

            // Hide semua button di awal
            if (giftBtn) giftBtn.SetActive(false);
            if (roomServiceBtn) roomServiceBtn.SetActive(false);
            if (foodBtn) foodBtn.SetActive(false);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(false);

            hotelGiftSpawner = GetComponent <HotelGiftSpawner> ();
            worldCanvas = MagicalGarden.Farm.UIManager.Instance.uIWorldNonScaleable; 
        }
        void Update()
        {
            if (IsOccupied && checkInDate != DateTime.MinValue)
            {
                TimeSpan elapsed = TimeManager.Instance.currentTime - checkInDate;
                TimeSpan remaining = stayDurationDays - elapsed;

                if (remaining.TotalSeconds <= 0)
                {
                    CheckOutRoom();
                    SetDirty();
                    Debug.Log($"🚪 {nameGuest} telah check-out karena waktu habis.");
                }
                else
                {
                    // Hanya log setiap 1 detik
                    logTimer += Time.deltaTime;
                    if (logTimer >= 1f)
                    {
                        logTimer = 0f;
                        string formattedTime = FormatRemainingTime(remaining);
                        Debug.Log($"⏳ Sisa waktu menginap: {formattedTime}");
                    }

                    // Request Timer - Generate request setiap interval
                    if (!hasRequest && isPetReachedTarget) {
                        requestTimer += Time.deltaTime;
                    }

                    if (requestTimer >= GetRandomTimeRequest () && !hasRequest)
                    {
                      //  GenerateRoomServiceRequest();
                        RandomGuestRequestType ();
                        requestTimer = 0f;
                    }
                }
            }
        }
        string FormatRemainingTime(TimeSpan time)
        {
            int hours = Mathf.FloorToInt((float)time.TotalHours);
            int minutes = time.Minutes;
            return $"{hours:D2}h {minutes:D2}m";
        }
        public string GetFormattedRemainingTime()
        {
            if (!IsOccupied || checkInDate == DateTime.MinValue)
                return "00h 00m";

            TimeSpan elapsed = TimeManager.Instance.currentTime - checkInDate;
            TimeSpan remaining = stayDurationDays - elapsed;

            if (remaining.TotalSeconds <= 0)
                return "00h 00m";

            int hours = Mathf.FloorToInt((float)remaining.TotalHours);
            int minutes = remaining.Minutes;

            return $"{hours:D2}h {minutes:D2}m";
        }
        public void SetClean()
        {
            isDirty = false;
            spriteRenderer.sprite = cleanSprites[selectedIndex];
            Debug.Log ("Hotel Clean");
            if (hotelGiftSpawner) {
                hotelGiftSpawner.OnSpawnGift (listPet);
            }
        }
        public void SetDirty()
        {
            isDirty = true;
            spriteRenderer.sprite = dirtySprites[selectedIndex];
        }
        public void ToggleCleanDirty()
        {
            if (isDirty)
                SetClean();
            else
                SetDirty();
        }
        public void CheckInToRoom(GuestRequest guest)
        {
            IsOccupied = true;
            nameGuest = guest.guestName;
            iconGuest = guest.icon;
            typeGuest = guest.type;
            party = guest.party;
            stayDurationDays = guest.stayDurationDays;
            rarity = guest.rarity;
            price = guest.price;
            happiness = 50;
            // SetHappiness(0);
            checkInDate = TimeManager.Instance.currentTime;
        }

        public void CheckOutRoom()
        {
            IsOccupied = false;
            Debug.Log("guest keluar room");
            // if (happiness >= 50)
                // {
                //     Debug.Log($"💰 {nameGuest} membayar karena puas!");
                // }
                // else
                // {
                //     Debug.Log($"😡 {nameGuest} kecewa dan tidak membayar.");
                // }
             //   Farm.CoinManager.Instance.AddCoins(price);
            if (price > 0) { 
                
                SetHoldReward (price);
            }
            //reset All
            nameGuest = "";
            iconGuest = null;
            typeGuest = "";
            party = 0;
            price = 0;
            happiness = 0;
            rarity = GuestRarity.Common;
            
            // Reset request state
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
                currentRequestCountdown = null;
            }
            hasRequest = false;
            onProgressingRequest = false;
            HotelManager.Instance.RemoveHotelControllerHasRequest (this, false);
            SetIsPetReachedTarget (false);
            requestTimer = 0f;
            ResetRequestButtons();

            foreach (PetMonsterHotel pet in listPet)
            {
               // pet.RunToTargetAndDisappear(HotelManager.Instance.targetCheckOut);
               pet.MoveToTargetWithEvent (checkOutTiles [0], pet.DestroyPet);
            }

            listPet.Clear ();
        }
        public void AddPet(PetMonsterHotel pet)
        {
            if (pet != null)
            {
                listPet.Add(pet);
                Debug.Log($"✅ Pet {pet.name} ditambahkan ke hotel.");
            }
        }

        public void ClearAllPets()
        {
            listPet.Clear();
            Debug.Log("🧹 Semua pet dihapus dari hotel.");
        }

        #region Request System
        void GenerateRoomServiceRequest()
        {
            // Random pilih tipe request (untuk sekarang cuma RoomService yang aktif)
           // var randomType = GuestRequestType.RoomService;
            currentGuestRequestType = GuestRequestType.RoomService;
            SetDirty();  // Room jadi kotor
            hasRequest = true;

            // Aktifkan button
            if (roomServiceBtn) roomServiceBtn.SetActive(true);
            GenerateRequestBubble (currentGuestRequestType);
            HotelManager.Instance.AddHotelControllerHasRequest (this);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(true);

            string roomName = gameObject.name;
            Debug.Log($"🛎️ [ROOM SERVICE REQUEST] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness}/100 | Timer: 30 detik | Rarity: {rarity}");

            // Start countdown timer (30 detik)
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
            }
            currentRequestCountdown = StartCoroutine(RequestCountdown(durationRequestExpired));
        }

        void GenerateFoodRequest()
        {
            // Random pilih tipe request (untuk sekarang cuma RoomService yang aktif)
            hasRequest = true;

            // Aktifkan button
           // if (roomServiceBtn) roomServiceBtn.SetActive(true);
           currentGuestRequestType = GuestRequestType.Food;

            GenerateRequestBubble (currentGuestRequestType);
            HotelManager.Instance.AddHotelControllerHasRequest (this);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(true);

            string roomName = gameObject.name;
            Debug.Log($"🛎️ [ROOM SERVICE REQUEST] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness}/100 | Timer: 30 detik | Rarity: {rarity}");

            // Start countdown timer (30 detik)
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
            }
            currentRequestCountdown = StartCoroutine(RequestCountdown(durationRequestExpired));
        }

        void GenerateGiftRequest()
        {

            hasRequest = true;

            // Aktifkan button
            currentGuestRequestType = GuestRequestType.Gift;
           // if (roomServiceBtn) roomServiceBtn.SetActive(true);
            GenerateRequestBubble (currentGuestRequestType);
            HotelManager.Instance.AddHotelControllerHasRequest (this);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(true);

            string roomName = gameObject.name;
            Debug.Log($"🛎️ [ROOM SERVICE REQUEST] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness}/100 | Timer: 30 detik | Rarity: {rarity}");

            // Start countdown timer (30 detik)
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
            }
            currentRequestCountdown = StartCoroutine(RequestCountdown(durationRequestExpired));
        }

        System.Collections.IEnumerator RequestCountdown(float duration)
        {
            float timeRemaining = duration;

            while (timeRemaining > 0 && hasRequest)
            {
                timeRemaining -= Time.deltaTime;

                // Update progress bar
                if (fillExpired != null)
                {
                    float percent = Mathf.Clamp01(timeRemaining / duration);
                    fillExpired.fillAmount = percent;
                }

                yield return null;
            }

            // Kalau timeout (request tidak dipenuhi)
            if (hasRequest)
            {
                HandleRequestExpired();
            }
        }

        void HandleRequestExpired()
        {
           // happiness = Mathf.Max(happiness - 15, 0);
           happiness = Mathf.Max ( happiness + GetGuestRequest (currentGuestRequestType).decreaseHappiness,0);
            hasRequest = false;
            onProgressingRequest = false;
            HotelManager.Instance.RemoveHotelControllerHasRequest (this, false);

            string roomName = gameObject.name;
            Debug.LogWarning($"❌ [ROOM SERVICE EXPIRED] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness} (-15) | Request tidak dipenuhi dalam 30 detik!");

            // Hide semua button
            ResetRequestButtons();

            if (happiness == 0) {
                Debug.LogError ("Customer langsung keluar !");
                price = 0;
                CheckOutRoom();
            }
        }

        void ResetRequestButtons()
        {
            if (roomServiceBtn) roomServiceBtn.SetActive(false);
            if (foodBtn) foodBtn.SetActive(false);
            if (giftBtn) giftBtn.SetActive(false);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(false);
            if (currentRequestBubble) {
                
                HotelManager.Instance.RemoveBubbleRequest (currentRequestBubble.GetComponentInChildren <Button> ());
                Destroy (currentRequestBubble);
            }
        }

        void GenerateRequestBubble (GuestRequestType guestRequestType) {
            if (currentRequestBubble) {
                HotelManager.Instance.RemoveBubbleRequest (currentRequestBubble.GetComponentInChildren <Button> ());
                Destroy (currentRequestBubble);
            }

            GameObject prefabTarget = null;
            switch (guestRequestType) {
                case GuestRequestType.RoomService :
                prefabTarget = roomServiceBubblePrefab;
                break;
                case GuestRequestType.Food :
                prefabTarget = foodBubblePrefab;
                break;
                case GuestRequestType.Gift :
                prefabTarget = giftBubblePrefab;
                break;
            }

            GameObject clone = GameObject.Instantiate (prefabTarget);
            clone.SetActive (true);
            clone.transform.position = this.transform.position;
            clone.transform.SetParent (worldCanvas.GetComponent <RectTransform> ());
            clone.transform.localPosition += new Vector3 (0,10,0);

            RectTransform rect = clone.GetComponent<RectTransform>();
            Vector3 pos = rect.localPosition;
            pos.z = 0f;
            rect.localPosition = pos;

            currentRequestBubble = clone;

            currentRequestBubble.GetComponentInChildren <Button> ().onClick.AddListener (() => FulfillRequest (currentGuestRequestType, NPCService.NPCHotel));
            HotelManager.Instance.AddBubbleRequest (currentRequestBubble.GetComponentInChildren <Button> ());
        }
        #endregion

        #region FullFill Service
        #if UNITY_EDITOR
        [ContextMenu("Request/✅ Fulfill Clean")]
        private void Context_Fulfill_Clean()
        {
            SetDirty();
            FulfillRequestByString("RoomService");
        }
        #endif

        // CickableObjectHotel
        public void ClickableFulFillRequest () {
            if (holdReward) {
                ClaimCoin ();
            } else {
                FulfillRequest (currentGuestRequestType, NPCService.NPCHotel);
            }
        }

        private void FulfillRequest(GuestRequestType type, NPCService npcService, INPCHotelService autoNpcHotel = null)
        {
            Debug.Log($"VALID {hasRequest}");
            if (!hasRequest) return;
            if (onProgressingRequest) return;
            if (npcService == NPCService.NPCHotel) {if (!HotelManager.Instance.CheckNPCHotelAvailable ()) return;}
            onProgressingRequest = true;
            // Stop countdown coroutine
            INPCHotelService npcHotelService = null;
            if (npcService == NPCService.NPCHotel) {
                npcHotelService = HotelManager.Instance.npcHotel;
            } else if (npcService == NPCService.NPCAutoService) {
                npcHotelService = autoNpcHotel;
            }

            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
                currentRequestCountdown = null;
            }

            // hasRequest = false; (Pindah ke IncreaseHappiness)
            HotelManager.Instance.RemoveHotelControllerHasRequest (this, false);
            // happiness = Mathf.Min(happiness + 20, 100);
           // happiness = Mathf.Min (happiness + GetGuestRequest (currentGuestRequestType).increaseHappiness,100);

           // Debug.Log($"✅ {nameGuest} puas dengan {type}! Happiness: {happiness} (+20)");

            

            if (type == GuestRequestType.RoomService)
            {
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($"🧹 [NPC CLEANING] Mengirim NPC untuk membersihkan kamar {roomName} milik {nameGuest}");

                /*
                HotelManager.Instance.npcHotel.hotelControlRef = this;
                HotelManager.Instance.npcHotel.AddFinishEventHappiness(IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine(HotelManager.Instance.npcHotel.NPCHotelCleaning());
                */
                npcHotelService.hotelControlRef = this;
                npcHotelService.AddFinishEventHappiness (IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine (npcHotelService.NPCHotelCleaning());
            } else if (type == GuestRequestType.Food)
            {
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($" [NPC Food] Mengirim NPC untuk mengantar makanan kamar {roomName} milik {nameGuest}");

                npcHotelService.hotelControlRef = this;
                npcHotelService.AddFinishEventHappiness (IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine (npcHotelService.NPCHotelCleaning());
            }
            else if (type == GuestRequestType.Gift)
            {
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($"[NPC Gift] Mengirim NPC untuk memberikan hadiah ke kamar {roomName} milik {nameGuest}");

                npcHotelService.hotelControlRef = this;
                npcHotelService.AddFinishEventHappiness (IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine (npcHotelService.NPCHotelCleaning());
            }

            // Hide buttons
            ResetRequestButtons();
            
        }
        
        public void FulfillRequestByString(string typeStr)
        {
            Debug.Log("TEST Valid request type: " + typeStr);

            if (System.Enum.TryParse(typeStr, out GuestRequestType type))
            {
                Debug.Log("Valid request type: " + type);
                FulfillRequest(type, NPCService.NPCHotel);
            }
            else
            {
                Debug.LogWarning("Invalid request type: " + typeStr);
            }
        }

        void IncreaseHappiness (int happinessValue) {
            happiness = Mathf.Min (happiness +  happinessValue,100);
            hasRequest = false;
            onProgressingRequest = false;
        }

        #endregion

        #region Tile Wandering
        public void CalculateWanderingArea()
        {
            Vector3Int globalOffset = Vector3Int.zero;
            wanderingTiles.Clear();

            switch (offsetDirection)
            {
                case AreaOffsetDirection.Right:
                    globalOffset = new Vector3Int(offsetDistance, 0, 0);
                    break;
                case AreaOffsetDirection.Left:
                    globalOffset = new Vector3Int(-offsetDistance, 0, 0);
                    break;
                case AreaOffsetDirection.Up:
                    globalOffset = new Vector3Int(0, offsetDistance, 0);
                    break;
                case AreaOffsetDirection.Down:
                    globalOffset = new Vector3Int(0, -offsetDistance, 0);
                    break;
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int localOffset = new Vector3Int(x, y, 0);
                    Vector3Int pos = new Vector3Int(hotelPositionTile.x, hotelPositionTile.y, 0) + globalOffset + localOffset;

                    
                    wanderingTiles.Add(pos);
                    
                }
            }
        }
        public Vector2Int? GetRandomWanderingTile2D()
        {
            if (wanderingTiles == null || wanderingTiles.Count == 0)
                return null;

            var tile = wanderingTiles[UnityEngine.Random.Range(0, wanderingTiles.Count)];
            return new Vector2Int(tile.x, tile.y);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            if (TileManager.Instance.tilemapSoil == null || wanderingTiles == null) return;

            foreach (var pos in wanderingTiles)
            {
                Vector3 worldPos = TileManager.Instance.tilemapSoil.GetCellCenterWorld(pos);
                Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);
            }
        }
        public enum AreaOffsetDirection
        {
            None,
            Right,
            Left,
            Up,
            Down
        }
        #endregion
        #region NPCAutoService
        public enum NPCService {
            NPCHotel,
            NPCAutoService
        }

        public void NPCAutoService (INPCHotelService npc) { // NPCRoboShroom.cs
            FulfillRequest (currentGuestRequestType, NPCService.NPCAutoService, npc);
        }
        #endregion
        #region Random Guest Request Type

        public void RandomGuestRequestType()
        {
            int total = 0;

            for (int i = 0; i < guestRequestConfigs.Length; i++)
                total += guestRequestConfigs[i].chanceActive;

            if (total <= 0)
            {
                Debug.LogError("Total chanceActive = 0");
                return;
            }

            int roll = UnityEngine.Random.Range(0, total);
            int cumulative = 0;

            for (int i = 0; i < guestRequestConfigs.Length; i++)
            {
                cumulative += guestRequestConfigs[i].chanceActive;
                if (roll < cumulative)
                {
                    ExecuteRequest(guestRequestConfigs[i].guestRequestType);
                    return;
                }
            }
        }


        void ExecuteRequest(GuestRequestType type)
        {
            switch (type)
            {
                case GuestRequestType.Gift:
                    GenerateGiftRequest();
                    break;

                case GuestRequestType.Food:
                    GenerateFoodRequest();
                    break;

                case GuestRequestType.RoomService:
                    GenerateRoomServiceRequest();
                    break;
            }
        }

        GuestRequestConfig GetGuestRequest (GuestRequestType guestRequestType) {
            foreach (GuestRequestConfig config in guestRequestConfigs) {
                if (config.guestRequestType == guestRequestType) {
                    return config;
                }
            }
            return null;
        }

        #endregion
        #region PetMonsterHotel

        public void SetIsPetReachedTarget (bool value) { // this, PetMonsterHotel
            isPetReachedTarget = value;
        }
        #endregion
        #region GetRandomTimeRequest
        float GetRandomTimeRequest () {
            return UnityEngine.Random.Range (minRequestInterval, maxRequestInterval);
        }
        #endregion 
        #region CoinReward
        void SetHoldReward (int coinValue) {
            holdReward = true;
            holdCoin = coinValue;

            GameObject coinBubble = GameObject.Instantiate (coinBubblePrefab);
            coinBubble.SetActive (true);
            coinBubble.transform.position = this.transform.position;
            coinBubble.transform.SetParent (worldCanvas.GetComponent <RectTransform> ());
            coinBubble.transform.localPosition += new Vector3 (0,10,0);
            coinBubble.GetComponentInChildren <Button> ().onClick.AddListener (() => ClaimCoin ());
            coinBubbleClone = coinBubble;
        }

        void ClaimCoin () { 
            holdReward = false;
            
            GameObject coinClaimDisplayClone = GameObject.Instantiate (coinClaimDisplay);
            coinClaimDisplayClone.SetActive (true);
            coinClaimDisplayClone.transform.position = this.transform.position;
            coinClaimDisplayClone.transform.SetParent (worldCanvas.GetComponent <RectTransform> ());
            coinClaimDisplayClone.transform.localPosition += new Vector3 (0,10,0);
            TMP_Text coinDisplayText = coinClaimDisplayClone.transform.Find ("Canvas").GetComponentInChildren <TMP_Text> ();
            coinDisplayText.text = "+ " + holdCoin.ToString () ;
            CoinManager.AddCoins (holdCoin);

            holdCoin = 0;
            Destroy (coinBubbleClone);
            coinBubbleClone = null;
        }
        #endregion
    }

}