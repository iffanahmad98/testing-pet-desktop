using UnityEngine;
using System;
public class DebugTimeController : MonoBehaviour
{
    public Action preSimulateEvent;
    public Action simulateEvent;
    public static DebugTimeController instance;

    void Awake () {
        instance = this;
    }

    public void SimulateDay () {
        preSimulateEvent?.Invoke ();
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (24);
        simulateEvent?.Invoke ();
    }

    public void SimulateHour () {
        preSimulateEvent?.Invoke ();
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (1);
        simulateEvent?.Invoke ();
    }

    // HotelRandomLoot, HotelController
    public void AddDebuggingEvent (Action value) { 
        simulateEvent += value;
    }

    public void AddPreDebuggingEvent(Action value) {
        preSimulateEvent += value;
    }
}
