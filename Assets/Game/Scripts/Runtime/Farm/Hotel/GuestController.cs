using UnityEngine;
using UnityEngine.UI;
using System;
using MagicalGarden.Manager;
using System.Collections.Generic;
using TMPro;

namespace MagicalGarden.Hotel
{
    public class GuestController : MonoBehaviour
    {
        [Header("Guest Info")]
        public string guestName;
        public string type;
        public GuestRarity rarity = GuestRarity.Normal;
        [Range(0, 100)]
        public int happiness = 100;
        public DateTime checkInDate;

        [Header("Request Timing")]
        public float requestInterval = 60f;
        private float requestTimer = 0f;
        [Header("Room Reference")]
        public HotelRoom currentRoom;
        public TextMeshProUGUI dayRemaining;
        [Header("Happiness")]
        public Image fillHappiness;
        public Image fillExpired;
        public int stayDurationDays;
        [Header("Button")]
        public GameObject giftBtn;
        public GameObject roomServiceBtn;
        public GameObject foodBtn;

        [Header("Debug Data Guest")]
        public TextMeshProUGUI descGuest;
        private bool hasRequest = false;
        void Start()
        {
            giftBtn.SetActive(false);
            roomServiceBtn.SetActive(false);
            foodBtn.SetActive(false);
            fillExpired.transform.parent.gameObject.SetActive(false);
            if (currentRoom == null)
            {
                Debug.LogWarning($"{guestName} belum memiliki kamar!");
            }
        }

        [ContextMenu("cek checkin date")]
        private void CheckInDate()
        { 
            Debug.LogError(checkInDate.ToString("yyyy-MM-dd"));
        }

        public void SetupFromRequest(GuestRequest request)
        {
            guestName = request.guestName;
            type = request.type;
            stayDurationDays = request.stayDurationDays;
            rarity = request.rarity;
            dayRemaining.text = "Day " + stayDurationDays;
            happiness = 0;
            SetHappiness(happiness);
            string rarityText = request.rarity.ToString().ToUpper(); // Tambahan
            string desc = $"Nama: {request.guestName}\nTipe: {request.type}\nDurasi: {request.stayDurationDays} hari\nRarity: {rarityText}";
            descGuest.text = desc;
            if (checkInDate == DateTime.MinValue)
            {
                checkInDate = TimeManager.Instance.currentTime.Date;
            }
        }

        void Update()
        {
            if (currentRoom == null) return;

            requestTimer += Time.deltaTime;

            if (requestTimer >= requestInterval)
            {
                if(!hasRequest) GenerateRequest();
                requestTimer = 0f;
            }

            UpdateRoomRequests(Time.deltaTime);
        }
        void GenerateRequest()
        {
            // Buat permintaan acak: RoomService, Food, atau Gift
            var randomType = (GuestRequestType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GuestRequestType)).Length);
            var newRequest = new HotelRoomRequest(randomType, 30f); // 30 detik waktu fulfill

            currentRoom.AddRequest(newRequest);
            ActivateBtn(randomType);
            

            Debug.Log($"📦 {guestName} meminta: {randomType}");
            // Farm.UIManager.Instance?.ShowGuestRequest(this, newRequest);
        }
        public GuestRequest ToRequest()
        {
            return new GuestRequest(guestName, type, stayDurationDays, rarity);
        }
        void ResetBtn()
        {
            giftBtn.SetActive(false);
            roomServiceBtn.SetActive(false);
            foodBtn.SetActive(false);
            
        }
        private void ActivateBtn(GuestRequestType randomType)
        {
            ResetBtn();
            fillExpired.transform.parent.gameObject.SetActive(true);
            if (randomType == GuestRequestType.RoomService)
            {
                roomServiceBtn.SetActive(true);
                hasRequest = true;
            }
            else if (randomType == GuestRequestType.Food)
            {
                foodBtn.SetActive(true);
                hasRequest = true;
            }
            else if (randomType == GuestRequestType.Gift)
            {
                giftBtn.SetActive(true);
                hasRequest = true;
            }
            else
            {

            }
        }

        void UpdateRoomRequests(float deltaTime)
        {
            var expired = new List<HotelRoomRequest>();

            foreach (var request in currentRoom.roomRequests)
            {
                if (!request.isFulfilled)
                {
                    request.timeRemaining -= deltaTime;
                    if (fillExpired != null)
                    {
                        float percent = Mathf.Clamp01(request.timeRemaining / 30f);
                        fillExpired.fillAmount = percent;
                    }

                    if (request.timeRemaining <= 0f)
                        expired.Add(request);
                }
            }

            foreach (var request in expired)
            {
                HandleFailedRequest(request);
            }
        }

        void HandleFailedRequest(HotelRoomRequest request)
        {
            happiness = Mathf.Max(happiness - 15, 0);
            SetHappiness(happiness);
            Debug.Log($"❌ {guestName} tidak mendapat {request.requestType}. Happiness turun jadi {happiness}");

            currentRoom.roomRequests.Remove(request);
            hasRequest = false;
            ResetBtn();
            fillExpired.transform.parent.gameObject.SetActive(false);
        }

        private void FulfillRequest(GuestRequestType type)
        {
            var request = currentRoom.roomRequests.Find(r => r.requestType == type && !r.isFulfilled);
            if (request != null)
            {
                request.isFulfilled = true;
                happiness = Mathf.Min(happiness + 20, 100);
                SetHappiness(happiness);
                currentRoom.roomRequests.Remove(request);
                hasRequest = false;
                ResetBtn();
                fillExpired.transform.parent.gameObject.SetActive(false);

                Debug.Log($"✅ {guestName} puas dengan {type}! Happiness: {happiness}");
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
        public void SetHappiness(float value)
        {
            float percent = Mathf.Clamp01(value / 100f);
            fillHappiness.fillAmount = percent;
        }
    }
}