using System;

public static class CoinManager
{
    public static event Action<int> OnCoinChanged;

    public static int Coins
    {
        get => SaveSystem.PlayerConfig != null 
            ? SaveSystem.PlayerConfig.coins 
            : 0;
        set
        {
            if (SaveSystem.PlayerConfig == null)
            {
                return;
            }

            if (SaveSystem.PlayerConfig.coins != value)
            {
                SaveSystem.PlayerConfig.coins = value;
                SaveSystem.SaveAll();
                OnCoinChanged?.Invoke(value);
            }
        }
    }

    public static void AddCoins(int amount)
    {
        Coins += amount;
    }

    public static bool SpendCoins(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            return true;
        }
        return false; // Not enough coins
    }

    public static bool CheckCoins (int amount) { // HotelFacilitiesMenu
        return Coins >= amount;
    }
}