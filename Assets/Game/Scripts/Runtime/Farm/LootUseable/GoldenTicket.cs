using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public class GoldenTicket : LootUseable
{
    public static GoldenTicket instance = new GoldenTicket();

    static int totalValue = 0;
    public override int TotalValue => totalValue;

    public override void GetLoot(int value)
    {
        totalValue += value;
        SaveSystem.PlayerConfig.goldenTicket = totalValue;
        Debug.Log ($"Golden Tickets saved : " + SaveSystem.PlayerConfig.goldenTicket);
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

    public override void SaveListDecorationIds (List <int> listIds ,DateTime dateTimeNow) {
        SaveSystem.PlayerConfig.listHotelGoldenTickets = listIds;
        SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets = dateTimeNow;
        Debug.Log ($"Save List Decoration ids Golden Tickets");
        SaveSystem.SaveAll ();
    }

    public override List <int> LoadListDecorationIds () {
        return SaveSystem.PlayerConfig.listHotelGoldenTickets;
    }

    public override DateTime LoadLastRefreshTime () {
        return SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets;
    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.goldenTicket;
    }
}
