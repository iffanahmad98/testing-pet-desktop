using UnityEngine;
using System;
public interface  IPlayerHistory
{
    void SetHotelRoomCompleted (int value);
    void AddHotelRoomCompletedChanged (Action actionValue);
}
