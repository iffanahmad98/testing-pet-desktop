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
        void Start()
        {
            CalculateWanderingArea();
            spriteRenderer = GetComponent<SpriteRenderer>();
            selectedIndex = UnityEngine.Random.Range(0, cleanSprites.Length);
            SetClean();
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
            foreach (var pet in listPet)
            {
                pet.RunToTargetAndDisappear(HotelManager.Instance.targetCheckOut);
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
            // var request = currentRoom.roomRequests.Find(r => r.requestType == type && !r.isFulfilled);
            // if (request != null)
            // {
            //     request.isFulfilled = true;
            //     happiness = Mathf.Min(happiness + 20, 100);
            //     SetHappiness(happiness);
            //     currentRoom.roomRequests.Remove(request);
            //     hasRequest = false;
            //     ResetBtn();
            //     fillExpired.transform.parent.gameObject.SetActive(false);

            //     Debug.Log($"✅ {guestName} puas dengan {type}! Happiness: {happiness}");
            // }
            if (type == GuestRequestType.RoomService)
            {
                HotelManager.Instance.npcHotel.hotelControlRef = this;
                StartCoroutine(HotelManager.Instance.npcHotel.NPCHotelCleaning());
                // currentRoom.SetHotelTileDirty(false);
            }
        }
        public void FulfillRequestByString(string typeStr)
        {
            if (System.Enum.TryParse(typeStr, out GuestRequestType type))
            {
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

                    if (TileManager.Instance.tilemapSoil.HasTile(pos))
                    {
                        wanderingTiles.Add(pos);
                    }
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