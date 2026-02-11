using UnityEngine;
using System.Collections.Generic;
using MagicalGarden.Hotel;

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

        public static GuestItem FindGuestItem(string guestName, string guestType)
        {
            if (string.IsNullOrEmpty(guestName))
                return null;

            var allGuestItems = Object.FindObjectsOfType<GuestItem>(true);
            Debug.Log($"[HotelTutorial] Searching for GuestItem with guestName='{guestName}' | guestType='{guestType}' | Total GuestItems found: {allGuestItems.Length}");

            var candidates = FilterGuestItems(allGuestItems, guestName, guestType);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[HotelTutorial] No matching GuestItem found with guestName='{guestName}' and guestType='{guestType}'");
                return null;
            }

            return SelectRandomOrFirst(candidates, string.IsNullOrEmpty(guestType), "GuestItem", guestName, guestType);
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

        private static List<GuestItem> FilterGuestItems(GuestItem[] allItems, string guestName, string guestType)
        {
            var candidates = new List<GuestItem>();

            foreach (var item in allItems)
            {
                if (!IsValidGuestItem(item, guestName))
                    continue;

                if (!string.IsNullOrEmpty(guestType) && item.GuestType != guestType)
                {
                    Debug.Log($"[HotelTutorial] Skipping GuestItem: name matches but type mismatch (expected={guestType}, actual={item.GuestType})");
                    continue;
                }

                candidates.Add(item);
            }

            return candidates;
        }

        private static bool IsValidGuestItem(GuestItem item, string guestName)
        {
            if (item == null || !item.gameObject.activeInHierarchy)
                return false;

            if (item.titleText == null)
                return false;

            return item.titleText.text == guestName;
        }

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

        private static T SelectRandomOrFirst<T>(List<T> candidates, bool selectRandom, string typeName, string name, string type)
        {
            if (selectRandom)
            {
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                Debug.Log($"[HotelTutorial] Found {candidates.Count} {typeName} candidates, selecting random index {randomIndex}");
                return candidates[randomIndex];
            }

            Debug.Log($"[HotelTutorial] Found matching {typeName}: '{name}' (type={type})");
            return candidates[0];
        }

        #endregion
    }
}
