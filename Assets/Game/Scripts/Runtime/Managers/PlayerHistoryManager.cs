using UnityEngine;
using System;

public class PlayerHistoryManager : MonoBehaviour, IPlayerHistory
{
    public static PlayerHistoryManager instance;
    PlayerConfig playerConfig;

    // IEligibilityRule
    public int hotelRoomCompleted =0;
    public int harvestFruit = 0;
    public int harvestEggMonsters = 0;

    public event Action OnHotelRoomCompletedChanged;
    public event Action OnHarvestFruitChanged;
    public event Action OnHarvestEggMonstersChanged;

    void Awake () {
        instance = this;
    }

    void Start () {
        playerConfig = SaveSystem.PlayerConfig;
    }
    #region Event
    public void AddHotelRoomCompletedChanged (Action actionValue) {
        OnHotelRoomCompletedChanged += actionValue;
    }
    public void AddHarvestFruitChanged (Action actionValue) { // UnlockBubbleUI
        OnHarvestFruitChanged += actionValue;
    }
    public void AddHarvestEggMonstersChanged(Action actionValue) {
        OnHarvestEggMonstersChanged += actionValue;
    }
    #endregion
    #region Load
    // PlayerConfig.cs
    public void GetLoadPlayerConfig (
        int hotelRoomCompletedVal,
        int harvestFruitVal,
        int harvestEggMonstersVal
    ) {
        hotelRoomCompleted = hotelRoomCompletedVal;
        harvestFruit = harvestFruitVal;
        harvestEggMonsters = harvestEggMonstersVal;
    }

    #endregion
    #region Save
    public void SetHotelRoomCompleted (int value) { // HotelController.cs
        hotelRoomCompleted += value;
        playerConfig.hotelRoomCompleted = hotelRoomCompleted;
        Debug.Log ("Hotel Room Completed " + playerConfig.hotelRoomCompleted);
        SaveSystem.SaveAll ();
    }
    public void SetHarvestFruit (int value) { // InventoryManager.cs
        harvestFruit += value;
        playerConfig.harvestFruit = harvestFruit;
        Debug.Log ("Hotel Room Completed " + playerConfig.harvestFruit);
        SaveSystem.SaveAll ();
    }

    public void SetHarvestEggMonsters (int value) { // InventoryManager.cs
        harvestEggMonsters += value;
        playerConfig.harvestEggMonsters = harvestEggMonsters;
        Debug.Log ("Hotel Room Completed " + playerConfig.harvestEggMonsters);
        SaveSystem.SaveAll ();
    }
    #endregion
}
