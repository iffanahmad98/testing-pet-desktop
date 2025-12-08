using System;

namespace MagicalGarden.Hotel
{
    [Serializable]
    public class HotelRoomRequest
    {
        public GuestRequestType requestType;
        public TimeSpan timeRemaining;
        public bool isFulfilled;

        public HotelRoomRequest(GuestRequestType type, TimeSpan duration)
        {
            requestType = type;
            timeRemaining = duration;
            isFulfilled = false;
        }
    }

    /*
    public enum GuestRequestType
    {
        RoomService
        // ,
        // Food,
        // Gift
    }
    */
}