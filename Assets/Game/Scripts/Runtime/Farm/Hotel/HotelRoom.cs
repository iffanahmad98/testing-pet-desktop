using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Manager;
using System;

namespace MagicalGarden.Hotel
{
    public class HotelRoom : MonoBehaviour
    {
        [Header("Room Info")]
        public string roomId;
        public GuestController guest;
        public Vector3Int hotelPosition;

        [Header("Guest Requests")]
        public List<HotelRoomRequest> roomRequests = new();

        public bool IsOccupied => guest != null;

        public void AssignGuest(GuestController newGuest)
        {
            guest = newGuest;
            roomRequests.Clear();
            int multiplier = 1;
            if (guest.rarity == GuestRarity.Rare || guest.rarity == GuestRarity.Mythic || guest.rarity == GuestRarity.Legend)
            {
                multiplier = 2;
            }

            int totalRequest = Mathf.Clamp(guest.stayDurationDays * multiplier, 1, 6);
            for (int i = 0; i < totalRequest; i++)
            {
                var type = (GuestRequestType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GuestRequestType)).Length);
                AddRequest(new HotelRoomRequest(type, guest.stayDurationDays));
            }
            guest.checkInDate = DateTime.Now.Date;
            
            HotelManager.Instance.SaveGuestRequests();
        }
        public void AssignGuestLoad(GuestController newGuest)
        {
            guest = newGuest;
            roomRequests.Clear();
            int totalRequest = Mathf.Clamp(guest.stayDurationDays, 1, 3); // max 3
            for (int i = 0; i < totalRequest; i++)
            {
                var type = (GuestRequestType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GuestRequestType)).Length);
                AddRequest(new HotelRoomRequest(type, guest.stayDurationDays));
            }
        }

        public void AddRequest(HotelRoomRequest request)
        {
            roomRequests.Add(request);
        }

        public void FulfillRequest(GuestRequestType type)
        {
            var request = roomRequests.Find(r => r.requestType == type && !r.isFulfilled);
            if (request != null)
            {
                request.isFulfilled = true;
                guest.happiness = Mathf.Min(guest.happiness + 20, 100);
                roomRequests.Remove(request);

                Debug.Log($"✅ Permintaan {type} dipenuhi di {roomId} untuk {guest.guestName}");
            }
        }
        public void CheckOut()
        {
            if (guest == null) return;

            if (guest.happiness >= 50)
            {
                Debug.Log($"💰 {guest.guestName} membayar karena puas!");
                // Tambahkan coin atau sistem pembayaran
            }
            else
            {
                Debug.Log($"😡 {guest.guestName} kecewa dan tidak membayar.");
            }

            guest = null;
            roomRequests.Clear();
        }
    }
}