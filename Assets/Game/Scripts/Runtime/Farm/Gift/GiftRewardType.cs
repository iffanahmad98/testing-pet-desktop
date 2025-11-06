namespace MagicalGarden.Gift
{
    /// <summary>
    /// Jenis reward yang bisa didapat dari gift
    /// </summary>
    public enum GiftRewardType
    {
        Coin,           // Coin biasa
        FoodPack,       // Feed item
        Medicine,       // Medicine item
        GoldenTicket,   // Golden ticket item
        Decoration,     // Decoration rare item
        DoubleCoin,     // Bonus: double coins (2x coin amount)
        Empty           // Empty → akan dikonversi ke coin
    }
}
