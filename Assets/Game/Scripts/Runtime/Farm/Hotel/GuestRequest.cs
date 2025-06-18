using System;
namespace MagicalGarden.Hotel
{
    [System.Serializable]
    public class GuestRequest
    {
        public string guestName;
        public string type;
        public int stayDurationDays;

        public GuestRequest(string name, string type, int duration)
        {
            guestName = name;
            this.type = type;
            stayDurationDays = duration;
        }
    }
}