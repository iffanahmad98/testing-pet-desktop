using System;
using MagicalGarden.Manager;
using UnityEngine;

namespace MagicalGarden.Hotel
{
    [System.Serializable]
    public class GuestRequest
    {
        public string guestName;
        public Sprite icon;
        public int party;
        public int price;
        public string type;
        public TimeSpan stayDurationDays;
        public GuestRarity rarity = GuestRarity.Common;
        public GuestStageGroup GuestGroup { get; set; }
        public GuestRequest(string name, Sprite icon, string type, int party, int price, TimeSpan stayDuration, GuestRarity rarity = GuestRarity.Common)
        {
            guestName = name;
            this.icon = icon;
            this.party = party;
            this.price = price;
            this.type = type;
            this.stayDurationDays = stayDuration;
            this.rarity = rarity;
        }

        public bool IsVIPGuest()
        {
            return rarity == GuestRarity.Rare || rarity == GuestRarity.Mythic || rarity == GuestRarity.Legend;
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
        Common,
        Uncommon,
        Rare,
        Mythic,
        Legend
    }
}