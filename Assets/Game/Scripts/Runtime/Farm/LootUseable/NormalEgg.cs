using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class NormalEgg : LootUseable
{
    public static NormalEgg instance = new NormalEgg();

    static int totalValue = 0;
    public override int TotalValue => totalValue;

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
        return SaveSystem.PlayerConfig.lastRefreshTimeNormalEggs;
    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.normalEgg;
    }
}
