using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using MagicalGarden.Manager;
public class NormalEgg : LootUseable
{
    public static NormalEgg instance = new NormalEgg();

    static int totalValue = 0;
    public override int TotalValue => totalValue;

    bool firstTime = false;
    
    public override void GetLoot(int value)
    {
        totalValue += value;
        SaveSystem.PlayerConfig.normalEgg = totalValue;
        SaveLoot();
    }

    public override void UsingLoot(int value)
    {
        totalValue -= value;
        SaveLoot();
    }

    public override void SaveLoot()
    {
        SaveSystem.SaveAll();
    }

    public override void LoadLoot (int value)
    {
        totalValue = value;
    }

    public override void SaveListDecorationIds (List <int> listIds , DateTime dateTimeNow) {
        SaveSystem.PlayerConfig.listHotelNormalEggs = listIds;
        SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs = dateTimeNow;
        SaveSystem.SaveAll ();
    }

    public override List <int> LoadListDecorationIds () {
        return SaveSystem.PlayerConfig.listHotelNormalEggs;
    }

    public override DateTime LoadLastRefreshTime () {
        /*
        if (TimeManager.Instance.IsTimeInFuture(SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs)) {
           // SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs = TimeManager.Instance.currentTime;
            return TimeManager.Instance.realCurrentTime;
        } else {
            return SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs;
        }
        */
        if (TimeManager.Instance.IsTimeInFuture(SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs)) {
           if (!firstTime) {
                firstTime = true;
                SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets = TimeManager.Instance.currentTime;
                return TimeManager.Instance.realCurrentTime;
            } else {
                return SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets;
            }
        } else {
            return SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs;
        }

        
    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.normalEgg;
    }
}
