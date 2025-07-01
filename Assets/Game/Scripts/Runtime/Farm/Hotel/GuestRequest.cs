using System;

namespace MagicalGarden.Hotel
{
    [System.Serializable]
    public class GuestRequest
    {
        public string guestName;
        public int party;
        public int price;
        public string type;
        public TimeSpan stayDurationDays;
        public GuestRarity rarity = GuestRarity.Normal;
        public GuestRequest(string name, string type, int party, int price, TimeSpan stayDuration, GuestRarity rarity = GuestRarity.Normal)
        {
            guestName = name;
            this.party = party;
            this.price = price;
            this.type = type;
            this.stayDurationDays = stayDuration;
            this.rarity = rarity;
        }

        public string GetStayDurationString()
        {
            return FormatTimeShort(stayDurationDays);
        }
        public static string FormatTimeShort(TimeSpan timeSpan)
        {
            int totalHours = (int)timeSpan.TotalHours;
            int minutes = timeSpan.Minutes;

            string result = "";
            if (totalHours > 0)
                result += $"{totalHours}hr ";
            if (minutes > 0 || totalHours == 0)
                result += $"{minutes}m";

            return result.Trim();
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