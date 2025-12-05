using UnityEngine;
using UnityEngine.UI;
using System;
using MagicalGarden.Manager;
using System.Collections.Generic;
using MagicalGarden.AI;
using MagicalGarden.Farm;

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

        [Header("Request UI Buttons")]
        public GameObject giftBtn;
        public GameObject roomServiceBtn;
        public GameObject foodBtn;
        public Image fillExpired;  // Progress bar untuk countdown

        [Header("Request Timing")]
        public float requestInterval = 60f;  // Request setiap 60 detik
        private float requestTimer = 0f;
        private bool hasRequest = false;
        private Coroutine currentRequestCountdown;

        // [Header ("Hotel Gift")]
        HotelGiftSpawner hotelGiftSpawner;
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
                    requestTimer += Time.deltaTime;
                    if (requestTimer >= requestInterval && !hasRequest)
                    {
                        GenerateRoomServiceRequest();
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
            // SetHappiness(0);
            checkInDate = TimeManager.Instance.currentTime;
        }

        public void CheckOutRoom()
        {
            IsOccupied = false;
            Debug.LogError("guest keluar room");
            // if (happiness >= 50)
                // {
                //     Debug.Log($"💰 {nameGuest} membayar karena puas!");
                // }
                // else
                // {
                //     Debug.Log($"😡 {nameGuest} kecewa dan tidak membayar.");
                // }
                Farm.CoinManager.Instance.AddCoins(price);

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
            requestTimer = 0f;
            ResetRequestButtons();

            foreach (var pet in listPet)
            {
                pet.RunToTargetAndDisappear(HotelManager.Instance.targetCheckOut);
            }

            if (hotelGiftSpawner) {
                hotelGiftSpawner.OnSpawnGift ();
            }
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
            var randomType = GuestRequestType.RoomService;

            SetDirty();  // Room jadi kotor
            hasRequest = true;

            // Aktifkan button
            if (roomServiceBtn) roomServiceBtn.SetActive(true);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(true);

            string roomName = gameObject.name;
            Debug.Log($"🛎️ [ROOM SERVICE REQUEST] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness}/100 | Timer: 30 detik | Rarity: {rarity}");

            // Start countdown timer (30 detik)
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
            }
            currentRequestCountdown = StartCoroutine(RequestCountdown(30f));
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
            happiness = Mathf.Max(happiness - 15, 0);
            hasRequest = false;

            string roomName = gameObject.name;
            Debug.LogWarning($"❌ [ROOM SERVICE EXPIRED] Tamu: {nameGuest} | Kamar: {roomName} | Happiness: {happiness} (-15) | Request tidak dipenuhi dalam 30 detik!");

            // Hide semua button
            ResetRequestButtons();
        }

        void ResetRequestButtons()
        {
            if (roomServiceBtn) roomServiceBtn.SetActive(false);
            if (foodBtn) foodBtn.SetActive(false);
            if (giftBtn) giftBtn.SetActive(false);
            if (fillExpired) fillExpired.transform.parent.gameObject.SetActive(false);
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
        private void FulfillRequest(GuestRequestType type)
        {
            Debug.Log($"VALID {hasRequest}");
            if (!hasRequest) return;

            // Stop countdown coroutine
            if (currentRequestCountdown != null)
            {
                StopCoroutine(currentRequestCountdown);
                currentRequestCountdown = null;
            }

            hasRequest = false;
            happiness = Mathf.Min(happiness + 20, 100);

            Debug.Log($"✅ {nameGuest} puas dengan {type}! Happiness: {happiness} (+20)");

            // Hide buttons
            ResetRequestButtons();

            if (type == GuestRequestType.RoomService)
            {
                // Log NPC cleaning dimulai
                string roomName = gameObject.name;
                Debug.Log($"🧹 [NPC CLEANING] Mengirim NPC untuk membersihkan kamar {roomName} milik {nameGuest}");

                HotelManager.Instance.npcHotel.hotelControlRef = this;
                StartCoroutine(HotelManager.Instance.npcHotel.NPCHotelCleaning());
            }
        }
        public void FulfillRequestByString(string typeStr)
        {
            Debug.Log("TEST Valid request type: " + typeStr);

            if (System.Enum.TryParse(typeStr, out GuestRequestType type))
            {
                Debug.Log("Valid request type: " + type);
                FulfillRequest(type);
            }
            else
            {
                Debug.LogWarning("Invalid request type: " + typeStr);
            }
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

    
    }

}