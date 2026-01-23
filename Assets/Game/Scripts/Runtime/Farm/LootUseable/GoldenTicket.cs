using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using MagicalGarden.Manager;
public class GoldenTicket : LootUseable
{
    public static GoldenTicket instance = new GoldenTicket();

    static int totalValue = 0;
    public override int TotalValue => totalValue;
    bool firstTime = false;
    public override void GetLoot(int value)
    {
        // Collect egg golden ticket is at index 14
        MonsterManager.instance.audio.PlayFarmSFX(14);
        totalValue += value;
        SaveSystem.PlayerConfig.goldenTicket = totalValue;
        Debug.Log ($"Golden Tickets saved : " + SaveSystem.PlayerConfig.goldenTicket);
        SaveLoot();
    }

    public override void UsingLoot(int value)
    {
        totalValue -= value;
        SaveSystem.PlayerConfig.goldenTicket = totalValue;
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
        GetLootOffline ();
        if (TimeManager.Instance.IsTimeInFuture(SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets)) {
          //  SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets = TimeManager.Instance.currentTime;
            if (!firstTime) {
                firstTime = true;
                 SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets = TimeManager.Instance.currentTime;
                return TimeManager.Instance.realCurrentTime;
            } else {
                return SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets;
            }
        } else {
            firstTime = true;
            return SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets;
        }

    }

    public override int GetCurrency () {
        return SaveSystem.PlayerConfig.goldenTicket;
    }

    public override void GetLootOffline()
    {
        if (SaveSystem.PlayerConfig.HasHotelFacilityAndIsActive ("nerd_shroom")) {
            TimeSpan diff = TimeManager.Instance.currentTime 
                            - SaveSystem.PlayerConfig.lastRefreshTimeHotelGoldenTickets;

            double hours = diff.TotalHours;

            Debug.Log("Total Hours GoldenTickets : " + hours);

            if (hours > 0)
            {
                int cycles = (int)(hours / 2.0); // 1 cycle setiap 2 jam
                if (cycles > 84) { // 24 jam * 7 Hari 
                    cycles = 84;
                } 
                int totalLoot = 0;
                for (int x=0; x< cycles; x++ ) {
                    totalLoot += UnityEngine.Random.Range (5,10 +1); // +1 karena max tidak terhitung.
                }

                if (totalLoot > 0)
                    GetLoot(totalLoot);
            }
        }
    }

}
