using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public interface INPCHotelService {
    MagicalGarden.Hotel.HotelController hotelControlRef {get;set;}
    void AddFinishEventHappiness(Action<int> callback, int value);
    IEnumerator NPCHotelCleaning ();
}
