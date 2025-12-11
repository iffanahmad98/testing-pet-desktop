using UnityEngine;
using System;
public class DebugTimeController : MonoBehaviour
{
    public Action simulateEvent;
    public static DebugTimeController instance;

    void Awake () {
        instance = this;
    }

    public void SimulateDay () {
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (24);
        simulateEvent?.Invoke ();
    }

    public void SimulateHour () {
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (1);
        simulateEvent?.Invoke ();
    }

    // HotelRandomLoot
    public void AddDebuggingEvent (Action value) { 
        simulateEvent += value;
    }
}
