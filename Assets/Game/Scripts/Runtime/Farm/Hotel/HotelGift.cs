using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class HotelGift 
{
    public static HotelGift instance = new HotelGift();

    static int totalValue = 0;
    public int TotalValue => totalValue;
    public static event Action OnCurrencyChangedRefreshEvent;

    public void GetLoot(int value)
    {
        totalValue += value;
        SaveSystem.PlayerConfig.hotelGift = totalValue;
        Debug.Log ($"Hotel Gift saved : " + SaveSystem.PlayerConfig.hotelGift);
        SaveLoot();
       // OnCurrencyChangedRefreshEvent?.Invoke (); HotelGiftHandler.cs
    }

    public void UsingLoot(int value)
    {
        totalValue -= value;
        SaveSystem.PlayerConfig.hotelGift = totalValue;
        SaveLoot();
        OnCurrencyChangedRefreshEvent?.Invoke ();
    }

    public void SaveLoot()
    {
        SaveSystem.SaveAll();
    }

    public void LoadLoot (int value)
    {
        totalValue = value;
        Debug.Log ("Load Hotel Gift : " + totalValue.ToString ());
    }

    /*
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
    */

    public int GetCurrency () {
        return SaveSystem.PlayerConfig.hotelGift;
    }

    public static void AddCurrencyChangedRefreshEvent (Action value) { // HotelController.cs, FarmMainUI.cs
        OnCurrencyChangedRefreshEvent += value;
    }
}
