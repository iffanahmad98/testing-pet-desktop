using System;

namespace MagicalGarden.Hotel
{
    [Serializable]
    public class HotelRoomRequest
    {
        public GuestRequestType requestType;
        public float timeRemaining;
        public bool isFulfilled;

        public HotelRoomRequest(GuestRequestType type, float duration)
        {
            requestType = type;
            timeRemaining = duration;
            isFulfilled = false;
        }
    }

    public enum GuestRequestType
    {
        RoomService,
        Food,
        Gift
    }
}