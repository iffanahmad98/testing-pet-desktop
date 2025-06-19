using System;
namespace MagicalGarden.Hotel
{
    [System.Serializable]
    public class GuestRequest
    {
        public string guestName;
        public string type;
        public int stayDurationDays;
        public GuestRarity rarity = GuestRarity.Normal;

        public GuestRequest(string name, string type, int duration, GuestRarity rarity = GuestRarity.Normal)
        {
            guestName = name;
            this.type = type;
            stayDurationDays = duration;
            this.rarity = rarity;
        }
    }
    public enum GuestRarity
    {
        Normal,
        Rare,
        Mythic,
        Legend
    }
}