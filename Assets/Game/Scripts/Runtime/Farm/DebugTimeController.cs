using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class DebugTimeController : MonoBehaviour
{
    public Action preSimulateEvent;
    public Action simulateEvent;
    public Action lateEvent;
    public static DebugTimeController instance;

    void Awake () {
        instance = this;
    }

    public void SimulateDay () {
        preSimulateEvent?.Invoke ();
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (24);
        simulateEvent?.Invoke ();
         Debug.Log ("Normal Egg 2");
         StartCoroutine (nLateEvent ());
    }

    IEnumerator nLateEvent () {
        yield return new WaitForSeconds (1.0f);
        lateEvent?.Invoke ();
    }

    public void SimulateHour () {
        preSimulateEvent?.Invoke ();
        MagicalGarden.Manager.TimeManager.Instance.AddTimeDebug (1);
        simulateEvent?.Invoke ();
        StartCoroutine (nLateEvent ());
    }

    // HotelRandomLoot, HotelController
    public void AddDebuggingEvent (Action value) { 
        simulateEvent += value;
    }

    public void AddLateDebuggingEvent (Action value) {
        lateEvent += value;
    }

    public void AddPreDebuggingEvent(Action value) {
        preSimulateEvent += value;
    }
}
