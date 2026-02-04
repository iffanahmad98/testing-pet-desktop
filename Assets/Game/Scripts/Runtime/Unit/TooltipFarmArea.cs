using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TooltipFarmArea : MonoBehaviour {
    [SerializeField] GameObject [] fakeFarmAreaOpened;
    [SerializeField] GameObject [] fakeFarmAreaLocked;    

    
    void Start () {
        MagicalGarden.Farm.PlantManager.Instance.AddLoadEventData (RefreshFakeFarmArea);
    }
    
    void RefreshFakeFarmArea () {
        List <int> idFarms = MagicalGarden.Farm.PlantManager.Instance.GetFarmAreaIdsPurchased ();
        Debug.Log ("id Farm " + idFarms.Count);
        int x = 0;
        for (x=0; x < fakeFarmAreaLocked.Length; x++) {
            if (idFarms.Contains (x)) {
                if (fakeFarmAreaLocked[x]) {
                    fakeFarmAreaLocked[x].SetActive (false);
                }
                if (fakeFarmAreaOpened[x]) {
                    fakeFarmAreaOpened[x].SetActive (true);
                }
            } else {
                if (fakeFarmAreaLocked[x]) {
                    fakeFarmAreaLocked[x].SetActive (true);
                }
                if (fakeFarmAreaOpened[x]) {
                    fakeFarmAreaOpened[x].SetActive (false);
                }
            }
        }
    }
}
