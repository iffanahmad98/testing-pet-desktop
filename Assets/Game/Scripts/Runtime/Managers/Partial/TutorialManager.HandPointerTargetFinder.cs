using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Hotel;
using MagicalGarden.Manager;
using MagicalGarden.Gift;

public partial class TutorialManager
{
    private class HandPointerTargetFinder
    {
        public static ClickableObject FindClickableObjectById(string clickableObjectId)
        {
            if (string.IsNullOrEmpty(clickableObjectId))
                return null;

            Debug.Log($"[HotelTutorial] Resolving ClickableObject for id '{clickableObjectId}'");

            var allClickables = Object.FindObjectsOfType<ClickableObject>(true);
            foreach (var clickable in allClickables)
            {
                if (clickable != null && string.Equals(clickable.tutorialId, clickableObjectId, System.StringComparison.Ordinal))
                    return clickable;
            }

            return null;
        }

        public static GuestItem FindGuestItem()
        {
            var hotelManager = HotelManager.Instance;
            if (hotelManager == null)
            {
                Debug.LogWarning("[HotelTutorial] HotelManager.Instance is null, cannot find GuestItem");
                return null;
            }

            var content = hotelManager.GetGuestListContent();
            if (content == null || content.childCount == 0)
            {
                Debug.LogWarning("[HotelTutorial] Guest list content is empty or null");
                return null;
            }

            var firstChild = content.GetChild(0);
            if (firstChild == null)
            {
                Debug.LogWarning("[HotelTutorial] First child of guest list content is null");
                return null;
            }

            var guestItem = firstChild.GetComponent<GuestItem>();
            if (guestItem == null)
            {
                Debug.LogWarning($"[HotelTutorial] First child '{firstChild.name}' does not have GuestItem component");
                return null;
            }

            Debug.Log($"[HotelTutorial] Found first GuestItem: '{guestItem.GuestName}' (type={guestItem.GuestType})");
            return guestItem;
        }

        public static GiftItem FindLatestHotelGiftItem()
        {
            var handler = HotelGiftHandler.instance;
            if (handler == null)
            {
                Debug.LogWarning("[HotelTutorial] FindLatestHotelGiftItem: HotelGiftHandler.instance is null");
                return null;
            }

            var list = handler.GetListHotelGift();
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning("[HotelTutorial] FindLatestHotelGiftItem: listHotelGift kosong, belum ada gift di dunia");
                return null;
            }

            var go = list[list.Count - 1];
            if (go == null)
            {
                Debug.LogWarning("[HotelTutorial] FindLatestHotelGiftItem: GameObject gift terakhir adalah null");
                return null;
            }

            var gift = go.GetComponent<GiftItem>();
            if (gift == null)
            {
                Debug.LogWarning($"[HotelTutorial] FindLatestHotelGiftItem: GameObject '{go.name}' tidak memiliki GiftItem component");
                return null;
            }

            Debug.Log($"[HotelTutorial] FindLatestHotelGiftItem: Menggunakan gift '{go.name}' sebagai target tutorial.");
            return gift;
        }

        public static HotelController FindLastAssignedHotelRoom()
        {
            var hotelManager = HotelManager.Instance;
            if (hotelManager == null)
            {
                Debug.LogWarning("[HotelTutorial] FindLastAssignedHotelRoom: HotelManager.Instance is null");
                return null;
            }

            var lastRoom = hotelManager.LastAssignedRoom;
            if (lastRoom == null)
            {
                Debug.LogWarning("[HotelTutorial] FindLastAssignedHotelRoom: LastAssignedRoom is null (belum ada guest yang check-in)");
                return null;
            }

            Debug.Log($"[HotelTutorial] FindLastAssignedHotelRoom: Menggunakan kamar terakhir untuk check-in guest (idHotel={lastRoom.idHotel}, nameGuest='{lastRoom.nameGuest}', typeGuest='{lastRoom.typeGuest}')");
            return lastRoom;
        }

        public static HotelController FindRandomOccupiedHotelRoom(string guestTypeFilter)
        {
            var allHotels = Object.FindObjectsOfType<HotelController>(true);
            Debug.Log($"[HotelTutorial] Searching for occupied HotelController | guestTypeFilter='{guestTypeFilter}' | Total HotelControllers found: {allHotels.Length}");

            var candidates = FilterOccupiedHotelRooms(allHotels, guestTypeFilter);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[HotelTutorial] No occupied HotelController found with guestTypeFilter='{guestTypeFilter}'");
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            var selected = candidates[randomIndex];
            Debug.Log($"[HotelTutorial] Found {candidates.Count} occupied hotel candidates, selecting random index {randomIndex} (guest={selected.nameGuest}, type={selected.typeGuest})");

            return selected;
        }

        #region Private Helper Methods

        private static List<HotelController> FilterOccupiedHotelRooms(HotelController[] allHotels, string guestTypeFilter)
        {
            var candidates = new List<HotelController>();

            foreach (var hotel in allHotels)
            {
                if (!IsValidOccupiedHotelRoom(hotel))
                    continue;

                if (!string.IsNullOrEmpty(guestTypeFilter) && hotel.typeGuest != guestTypeFilter)
                {
                    Debug.Log($"[HotelTutorial] Skipping HotelController: occupied but type mismatch (expected={guestTypeFilter}, actual={hotel.typeGuest})");
                    continue;
                }

                candidates.Add(hotel);
            }

            return candidates;
        }

        private static bool IsValidOccupiedHotelRoom(HotelController hotel)
        {
            if (hotel == null || !hotel.gameObject.activeInHierarchy)
                return false;

            return hotel.IsOccupied;
        }

        #endregion
    }
}
