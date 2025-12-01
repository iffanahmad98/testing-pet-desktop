using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraDragLocker : MonoBehaviour, ILockedBy {
    List <GameObject> listLockedBy = new List <GameObject> ();
    public void AddLockedBy (GameObject value) { // HotelShop
        listLockedBy.Add (value);
    }

    public void RemoveLockedBy (GameObject value) { // HotelShop
        listLockedBy.Remove (value);
    }

    public bool IsCan () {
        return listLockedBy.Count == 0;
    }

}
