using UnityEngine;

[System.Serializable]
public class GuestRequestConfig 
{ // HotelController
    public GuestRequestType guestRequestType;
    [Range (0,100)]
    public int chanceActive = 0;
    public sbyte increaseHappiness = 40;
    public sbyte decreaseHappiness = -40;
}
