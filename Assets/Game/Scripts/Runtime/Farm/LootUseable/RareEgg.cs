using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class RareEgg : LootUseable
{
    public static RareEgg instance = new RareEgg();

    static int totalValue = 0;
    public override int TotalValue => totalValue;

    public override void GetLoot(int value)
    {
        totalValue += value;
        SaveSystem.PlayerConfig.rareEgg = totalValue;
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

    public override void SaveListDecorationIds (List <int> listIds, DateTime dateTimeNow) {
        SaveSystem.PlayerConfig.listHotelRareEggs = listIds;
        SaveSystem.PlayerConfig.lastRefreshTimeRareEggs = dateTimeNow;
        SaveSystem.SaveAll ();
    }

    public override List <int> LoadListDecorationIds () {
        return SaveSystem.PlayerConfig.listHotelRareEggs;
    }

    public override DateTime LoadLastRefreshTime () {
        return SaveSystem.PlayerConfig.lastRefreshTimeRareEggs;
    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.rareEgg;
    }
}
