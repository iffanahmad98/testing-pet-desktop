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
        public float requestTimer = 0f;
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
        public bool holdReward = false; // this, HotelManager.cs
        
        [Header ("Eligible Data")]
        [SerializeField] HotelControllerEligibleDataSO eligibleDataSO;
        [SerializeField] GameObject hotelPurchasePrefab;
        [SerializeField] Color colorHotelUnlocked, colorHotelLocked;
        GameObject hotelPurchase;
        [Header ("Vfx")]
        public GameObject buyHotelRay;
        [SerializeField] ParticleSystem bubbleVfx;
        GameObject vfxRay;
        
        [Header ("Data")]
        public int idHotel = 0; // HotelManager.cs
        public bool isLocked = false;
        public HotelControllerData hotelControllerData;
        int offlineHappiness = 0;
        PlayerConfig playerConfig;
        Dictionary <string, Action> dictionaryGenerateRequest = new Dictionary <string, Action> ();
        

        [Header ("History")]
        IPlayerHistory iPlayerHistory;
        [Header ("Debug")]
        bool timePaused = false;
        void Start()
        {
            CalculateWanderingArea();
            spriteRenderer = GetComponent<SpriteRenderer>();
            selectedIndex = UnityEngine.Random.Range(0, cleanSprites.Length);
           // SetClean(); tidak membutuhkan ini lagi, karena sudah ada sistem load.

            // Hide semua button di awal
            if (giftBtn) giftBtn.SetActive(false);
            if (roomServiceBtn) roomServiceBtn.SetActive(false);
            if (foodBtn) foodBtn.SetActive(false);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(false);

            hotelGiftSpawner = GetComponent <HotelGiftSpawner> ();
            worldCanvas = MagicalGarden.Farm.UIManager.Instance.uIWorldNonScaleable; 

            playerConfig = SaveSystem.PlayerConfig;
            iPlayerHistory = PlayerHistoryManager.instance as IPlayerHistory;
        }
        void Update()
        {
            if (IsOccupied && checkInDate != DateTime.MinValue && !timePaused && !holdReward)
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
            Debug.Log ($"hours : {hours} elapsed {elapsed} remaining {remaining}");
            return $"{hours:D2}h {minutes:D2}m";
        }
        public void SetClean()
        {
           // isDirty = false;
           // spriteRenderer.sprite = cleanSprites[selectedIndex];
           InstantiateVfxClean ();
           SetCleanOnly ();
            Debug.Log ("Hotel Clean");
            if (hotelGiftSpawner) {
                hotelGiftSpawner.OnSpawnGift (listPet);
            }
        }

        void SetCleanOnly()
        {
            isDirty = false;
            spriteRenderer.sprite = cleanSprites[selectedIndex];
            Debug.Log ("Hotel Clean");
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

            // PlayerConfig
            HotelControllerData data = new HotelControllerData {
                idHotel =idHotel,
                isDirty = isDirty,
                isOccupied = IsOccupied,
                nameGuest = nameGuest,
                typeGuest = typeGuest,
                party = party,
                price = price,
                stayDurationDays = stayDurationDays,
                happiness = happiness,
                checkInDate = checkInDate,
                rarity = rarity.ToString (),
                hasRequest = hasRequest,
                codeRequest = ""
            };

            playerConfig.AddHotelControllerData (data);
            SaveSystem.SaveAll ();

            HotelMainUI.instance.RefreshHotelRoom ();
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
            } else {
                IsOccupied = false;
                // langsung hapus data jika tidak ada reward : (Customer marah)
                ClearData ();

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
            HotelManager.Instance.AddGuestRequestAfterCheckOut ();
            SetIsPetReachedTarget (false);
           
            requestTimer = 0f;
            ResetRequestButtons();

            foreach (PetMonsterHotel pet in listPet)
            {
               pet.hotelContrRef = null; 
               // pet.RunToTargetAndDisappear(HotelManager.Instance.targetCheckOut);
               pet.MoveToTargetWithEvent (checkOutTiles [0], true, pet.DestroyPet);
               pet.ClearData ();
            }

           
            listPet.Clear ();
            HotelMainUI.instance.RefreshHotelRoom ();
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
            Invoke ("SetDirty", 3.0f);  // Room jadi kotor
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
            playerConfig.HotelControllerDataChangeCodeRequest (idHotel, "RoomService");
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
            playerConfig.HotelControllerDataChangeCodeRequest (idHotel, "Food");
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
            playerConfig.HotelControllerDataChangeCodeRequest (idHotel, "Gift");
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
            SetHappinessData ();
            hasRequest = false;
            onProgressingRequest = false;
            HotelManager.Instance.RemoveHotelControllerHasRequest (this, false);

            string roomName = gameObject.name;
            Debug.LogWarning($"❌ [ROOM SERVICE EXPIRED] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness} (-15) | Request tidak dipenuhi dalam 30 detik!");

            // Hide semua button
            ResetRequestButtons();

            SetBubbleVfxState(true);

            if (happiness == 0) {
                // Debug.LogError ("Customer langsung keluar !");
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
            if (currentRequestBubble) 
            {    
                HotelManager.Instance.RemoveBubbleRequest (currentRequestBubble.GetComponentInChildren <Button> ());
                Destroy (currentRequestBubble);
            }

            playerConfig.HotelControllerDataChangeCodeRequest (idHotel, "");
        }

        void GenerateRequestBubble (GuestRequestType guestRequestType) 
        {
            if (currentRequestBubble) 
            {
                HotelManager.Instance.RemoveBubbleRequest (currentRequestBubble.GetComponentInChildren <Button> ());
                Destroy (currentRequestBubble);
            }

            GameObject prefabTarget = null;
            switch (guestRequestType) 
            {
                case GuestRequestType.RoomService :
                prefabTarget = roomServiceBubblePrefab;
                    // Hotel service is at index 18
                    MonsterManager.instance.audio.PlayFarmSFX(18);
                break;
                case GuestRequestType.Food :
                prefabTarget = foodBubblePrefab;
                    // hotel notification for hotel quest is at index 19
                    MonsterManager.instance.audio.PlayFarmSFX(19);
                break;
                case GuestRequestType.Gift :
                prefabTarget = giftBubblePrefab;
                    // hotel notification for hotel quest is at index 19
                    MonsterManager.instance.audio.PlayFarmSFX(19);
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
        public void ClickableFulFillRequest () 
        {
            if (holdReward) 
            {
                ClaimCoin ();
            } else 
            {
                FulfillRequest (currentGuestRequestType, NPCService.NPCHotel);
            }
        }

        private void FulfillRequest(GuestRequestType type, NPCService npcService, INPCHotelService autoNpcHotel = null)
        {
            Debug.Log($"VALID {idHotel} {hasRequest}");
            if (!hasRequest) return;
            if (onProgressingRequest) return;
            if (npcService == NPCService.NPCHotel) {if (!HotelManager.Instance.CheckNPCHotelAvailable ()) return;}
            Debug.Log ("VALID 2");
            onProgressingRequest = true;
            
            // Stop countdown coroutine
            INPCHotelService npcHotelService = null;
            
            if (npcService == NPCService.NPCHotel) 
            {
                npcHotelService = HotelManager.Instance.npcHotel;
            } else if (npcService == NPCService.NPCAutoService) 
            {
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

            // Klik bubble button hotel is at index 20
            MonsterManager.instance.audio.PlayFarmSFX(20);
            SetBubbleVfxState(false);

            if (type == GuestRequestType.RoomService)
            {
                Debug.Log ("VALID 3");
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
                Debug.Log ("VALID 4");
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($" [NPC Food] Mengirim NPC untuk mengantar makanan kamar {roomName} milik {nameGuest}");

                npcHotelService.hotelControlRef = this;
                npcHotelService.AddFinishEventHappiness (IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine (npcHotelService.NPCHotelCleaning());
            }
            else if (type == GuestRequestType.Gift)
            {
                Debug.Log ("VALID 5");
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($"[NPC Gift] Mengirim NPC untuk memberikan hadiah ke kamar {roomName} milik {nameGuest}");

                npcHotelService.hotelControlRef = this;
                npcHotelService.AddFinishEventHappiness (IncreaseHappiness, GetGuestRequest(currentGuestRequestType).increaseHappiness);
                StartCoroutine (npcHotelService.NPCHotelCleaning());
            }
            Debug.Log ("VALID 6");
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
            SetHappinessData ();
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

        #region Vfx
        public void InstantiateVfxDust () { // NPCHotel.cs
            if (currentGuestRequestType == GuestRequestType.RoomService) {
                GameObject vfx  = GameObject.Instantiate (HotelManager.Instance.GetCleaningVfx ());
                vfx.transform.SetParent (this.transform);
                vfx.transform.localPosition = new Vector3 (0,2,0);
            }
            
        }

        void InstantiateVfxClean () {
            
            Debug.Log ("Vfx Clean");
            vfxRay = GameObject.Instantiate (HotelManager.Instance.GetRayCleaningVfx ());
            vfxRay.transform.SetParent (this.transform);
            vfxRay.transform.localPosition = new Vector3 (0,0.7f,0);
            vfxRay.transform.localEulerAngles = new Vector3 (240,0,180);
            Invoke ("DestroyVfxClean", 3.0f);
            
        }

        void InstantiateVfxBuy () {
            Debug.Log ("Vfx Buy");
            GameObject buyRay = GameObject.Instantiate (buyHotelRay);
            buyRay.transform.SetParent (this.transform);
            buyRay.transform.localPosition = new Vector3 (0,1.5f,0);
            buyRay.transform.localEulerAngles = new Vector3 (-90,0,0);
            buyRay.SetActive (true);
        }

        void DestroyVfxClean () {
            Destroy (vfxRay);
        }

        private void SetBubbleVfxState(bool state)
        {
            if (state)
            {
                bubbleVfx.gameObject.SetActive(true);
                bubbleVfx.Play();
            }
            else
            {
                if (bubbleVfx) {
                    bubbleVfx.Stop();
                    bubbleVfx.gameObject.SetActive(false);
                }
            }
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

        public void NPCAutoClaimReward (INPCHotelService npc) { // NPCBellboyShroom.cs
            if (holdReward) {
               // ClaimCoin ();
                string roomName = gameObject.name;
                Debug.Log($" [NPC Claim Reward] mengirim pemain untuk mengambil reward");
                
                npc.hotelControlRef = this;
                npc.AddRewardEvent (ClaimCoin);
                StartCoroutine (npc.NPCHotelCleaning());
            
                Destroy (coinBubbleClone);
                coinBubbleClone = null;
                holdReward = false;
            }
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
            if (!holdReward) {
                holdReward = true;
                playerConfig.SetHotelReward (idHotel, holdReward) ;
                SaveSystem.SaveAll ();

                holdCoin = coinValue;

                GameObject coinBubble = GameObject.Instantiate (coinBubblePrefab);
                coinBubble.SetActive (true);
                coinBubble.transform.position = this.transform.position;
                coinBubble.transform.SetParent (worldCanvas.GetComponent <RectTransform> ());
                coinBubble.transform.localPosition += new Vector3 (0,10,0);
                coinBubble.GetComponentInChildren <Button> ().onClick.AddListener (() => ClaimCoin ());
                coinBubbleClone = coinBubble;

                HotelManager.Instance.AddListHotelControllerHasReward (this);
            }
            
        }

        void ClaimCoin () { 
            holdReward = false;
            IsOccupied = false;
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

            if (isDirty) {
                SetCleanOnly ();
            }
            HotelManager.Instance.RemoveListHotelControllerHasReward (this);
            iPlayerHistory.SetHotelRoomCompleted (1);
            
            playerConfig.RemoveHotelControllerData (hotelControllerData);
            SaveSystem.SaveAll ();
        }
        #endregion
        #region Data
        public void ChangeHappinessOffline (bool isIncrease) { // HotelManager.cs
            if (isIncrease) {
                Debug.Log ("Increase Happiness");
                offlineHappiness += UnityEngine.Random.Range (4,10 +1); 
            } else {
                Debug.Log ("Decrease Happiness");
                offlineHappiness -= UnityEngine.Random.Range (2,5);
            }
        }

        public void LoadData (HotelControllerData data) { // HotelManager.cs (Debugging)
            hotelControllerData = data;

            isDirty = data.isDirty;
            IsOccupied = data.isOccupied;
            nameGuest = data.nameGuest;
            typeGuest = data.typeGuest;
            iconGuest = HotelManager.Instance.GetSpecificGuestStagePrefab (data.nameGuest).icon;
            party = data.party;
            price = data.price;
            stayDurationDays = data.stayDurationDays;
            happiness = data.happiness;
            checkInDate = data.checkInDate;
            //rarity = rarity.ToString (),
            hasRequest = data.hasRequest;
           // codeRequest = ""
           if (!hotelControllerData.holdReward) {
                LoadListenerGuestRequest ();

                LoadEventHappinessOffline (data.codeRequest);
           }

           LoadEventHoldReward ();
        }
        
        void LoadListenerGuestRequest () {
            dictionaryGenerateRequest = new Dictionary <string, Action> ();
            dictionaryGenerateRequest.Add ("Food", GenerateFoodRequest);
            dictionaryGenerateRequest.Add ("Gift", GenerateGiftRequest);
            dictionaryGenerateRequest.Add ("RoomService", GenerateRoomServiceRequest);
        }

        
        // this
        public void LoadEventHappinessOffline (string codeRequest) {
            if (offlineHappiness == 0) { // kurang dari 1 jam.
                LoadEventCodeRequest (codeRequest);
                
            }
            else {
                happiness = Mathf.Clamp(happiness + offlineHappiness, 0, 100);
                offlineHappiness = 0;
                SetHappinessData ();
                hotelControllerData.codeRequest = "";
                if (happiness >0) {
                    SaveSystem.SaveAll ();
                } else {
                    // Guest hotel runaway is at index 22
                    MonsterManager.instance.audio.PlayFarmSFX(22);

                    price = 0;
                    CheckOutRoom ();
                }
            }

            SetTimePaused (false);
            /*
            TimeSpan diff = TimeManager.Instance.currentTime 
                            - SaveSystem.PlayerConfig.lastRefreshTimeHotel;

            double hours = diff.TotalHours;

            Debug.Log("Total Hours Happiness : " + hours.ToString () + TimeManager.Instance.currentTime.ToString ());

            int totalNPCService = playerConfig.GetTotalHiredService ();
            if (totalNPCService > 0) {
                if (hours >= 1)
                {
                    int cycles = (int)(hours / 1.0); // 1 cycle setiap 1 jam
                    int decreaseHappiness = 0;
                    for (int x=0; x< cycles; x++ ) {
                        decreaseHappiness += UnityEngine.Random.Range (2,5 +1); // +1 karena max tidak terhitung.
                    }

                    if (happiness > 0) {
                        happiness = Mathf.Max ( happiness - decreaseHappiness,0);
                        SetHappinessData ();
                        hotelControllerData.codeRequest = "";
                        if (happiness >0) {
                            SaveSystem.SaveAll ();
                        } else {
                            price = 0;
                            CheckOutRoom ();
                        }
                    }
                        
                } else {
                    // jika masih dibawah 1 jam :
                    LoadEventCodeRequest (codeRequest);
                    
                }
            } else {
                if (hours >= 1)
                {
                    int cycles = (int)(hours / 1.0); // 1 cycle setiap 1 jam
                    int increaseHappiness = 0;
                    for (int x=0; x< cycles; x++ ) {
                        increaseHappiness += 5 * UnityEngine.Random.Range (2, 8 +1); // +1 karena max tidak terhitung.
                    }

                        happiness = Mathf.Min (happiness +  increaseHappiness,100);
                        SetHappinessData ();
                        hotelControllerData.codeRequest = "";

                        if (happiness >0) {
                            SaveSystem.SaveAll ();
                        } else {
                            price = 0;
                            CheckOutRoom ();
                        }
                        
                } else {
                    // jika masih dibawah 1 jam :
                    LoadEventCodeRequest (codeRequest);
                    
                }
            }
            SetTimePaused (false);
            */
        }

        void LoadEventCodeRequest (string value) {
            if (value != "")
            dictionaryGenerateRequest[value] ();
        }

        void LoadEventHoldReward () {
            if (hotelControllerData.holdReward) {
                SetHoldReward (hotelControllerData.price);
            }
        }

        void ClearData () {
            playerConfig.RemoveHotelControllerData (hotelControllerData);
            SaveSystem.SaveAll ();
        }

        void SetHappinessData () {
            playerConfig.HotelControllerDataChangeHappiness (idHotel, happiness);
        }
        #endregion
        #region Locked
        public void HotelLocked () { // HotelLocker.cs
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = colorHotelLocked;
            isLocked = true;
            GetComponent <RequirementTipClick2D> ().requirementData = eligibleDataSO.requirementDataSO;
        }

        public void HotelUnlocked () {
            spriteRenderer.color = colorHotelUnlocked;
            isLocked = false;
            GetComponent <RequirementTipClick2D> ().requirementData = null;
        }

        public void GiveOptionBuy () { // HotelLocker.cs
            CheckAvailableToBuy ();
        }

        void CheckAvailableToBuy () {
            if (isLocked && !hotelPurchase) {
                    
                if (eligibleDataSO.IsEligibleWithoutCoin ()) {
                    GameObject purchaseParent = GameObject.Instantiate (hotelPurchasePrefab);
                    Button purchaseButton = purchaseParent.transform.Find ("PurchaseButton").GetComponent <Button> ();
                    if (eligibleDataSO.IsEligible ()) {
                        purchaseButton.image.color = new Color (1,1,1,1);
                        purchaseButton.onClick.AddListener (BuyHotelController);
                    } else {
                        purchaseButton.image.color = new Color (0.5f,0.5f,0.5f,1);
                    }
                    TMP_Text priceText = purchaseButton.transform.Find ("PriceText").GetComponent <TMP_Text> ();
                    priceText.text = eligibleDataSO.GetPrice ().ToString ();
                    purchaseParent.transform.SetParent (this.gameObject.transform);
                    purchaseParent.transform.localPosition = new Vector3 (0, 1.5f,0);
                    purchaseParent.transform.SetParent (worldCanvas.GetComponent <RectTransform> ());
                    hotelPurchase = purchaseParent;
                    hotelPurchase.SetActive (true);
                }

            } else {
                if (hotelPurchase) { // had Hotel Purchase button, but player dont has enough coin.
                    Button purchaseButton = hotelPurchase.transform.Find ("PurchaseButton").GetComponent <Button> ();
                    if (eligibleDataSO.IsEligible ()) {
                        purchaseButton.image.color = new Color (1,1,1,1);
                        purchaseButton.onClick.AddListener (BuyHotelController);
                    } else {
                        purchaseButton.image.color = new Color (0.5f,0.5f,0.5f,1);
                    }
                }
            }
        }
        
        public void BuyHotelController () {
            HotelManager.Instance.hotelLocker.BuyHotelController (this);
            CoinManager.SpendCoins (eligibleDataSO.GetPrice ());
            Destroy (hotelPurchase);
            hotelPurchase = null;
            InstantiateVfxBuy ();

            // Unlocked hotel room is at index 24
            MonsterManager.instance.audio.PlayFarmSFX(24);
        }
        
        public bool GetIsLocked () { // hotelLocker
            return isLocked;
        }
        #endregion
        #region Debug
        public void SetTimePaused (bool value) { // HotelManager.cs = true, this = false.
            timePaused = value;
        }
        #endregion
        
    }

}