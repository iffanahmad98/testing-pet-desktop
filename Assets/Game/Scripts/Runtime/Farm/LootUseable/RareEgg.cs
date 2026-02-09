using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using MagicalGarden.Manager;
public class RareEgg : LootUseable
{
    public static RareEgg instance = new RareEgg();

    static int totalValue = 0;
    public override int TotalValue => totalValue;
    public static event Action OnCurrencyChangedRefreshEvent;

    bool firstTime = false;
    public override void GetLoot(int value)
    {
        totalValue += value;
        SaveSystem.PlayerConfig.rareEgg = totalValue;
        SaveLoot();
       // OnCurrencyChangedRefreshEvent?.Invoke (); (Gak dibutuhkan karena sudah refresh di HotelRandomLoot.cs)
    }

    public override void UsingLoot(int value)
    {
        totalValue -= value;
        SaveSystem.PlayerConfig.rareEgg = totalValue;
        SaveLoot();
        OnCurrencyChangedRefreshEvent?.Invoke ();
    }

    public override void SaveLoot()
    {
        SaveSystem.SaveAll();
    }

    public override void LoadLoot (int value)
    {
        totalValue = value;
    }

    public override void SaveListDecorationIds (List <int> listIds, DateTime dateTimeNow) {
        SaveSystem.PlayerConfig.listHotelRareEggs = listIds;
        SaveSystem.PlayerConfig.lastRefreshTimeRareEggs = dateTimeNow;
        SaveSystem.SaveAll ();
    }

    public override List <int> LoadListDecorationIds () {
        return SaveSystem.PlayerConfig.listHotelRareEggs;
    }

    public override DateTime LoadLastRefreshTime () {
        GetLootOffline();
        if (TimeManager.Instance.IsTimeInFuture(SaveSystem.PlayerConfig.lastRefreshTimeRareEggs)) {
           // SaveSystem.PlayerConfig.lastRefreshTimeRareEggs = TimeManager.Instance.currentTime;
            if (!firstTime) {
                firstTime = true;
                SaveSystem.PlayerConfig.lastRefreshTimeRareEggs = TimeManager.Instance.currentTime;
                return TimeManager.Instance.realCurrentTime;
            } else {
                return SaveSystem.PlayerConfig.lastRefreshTimeRareEggs;
            }
        } else {
            firstTime = true;
            return SaveSystem.PlayerConfig.lastRefreshTimeRareEggs;
        }
    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.rareEgg;
    }

    public override void GetLootOffline () {
        if (SaveSystem.PlayerConfig.HasHotelFacilityAndIsActive ("wizard_shroom")) {
            TimeSpan diff = TimeManager.Instance.currentTime 
                            - SaveSystem.PlayerConfig.lastRefreshTimeRareEggs;

            double hours = diff.TotalHours;

            Debug.Log("Total Hours NormalEgg : " + hours);

            if (hours > 0)
            {
                int cycles = (int)(hours / 168.0); // 1 cycle setiap 24 jam
                int totalLoot = 1 * cycles;

                if (totalLoot > 0)
                    GetLoot(totalLoot);
            }
        }
    }

    public static void AddCurrencyChangedRefreshEvent (Action value) { // HotelController.cs, FarmMainUI.cs
        OnCurrencyChangedRefreshEvent += value;
    }

    
}
