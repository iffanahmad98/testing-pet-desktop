using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TooltipFarmArea : MonoBehaviour {
    [SerializeField] GameObject [] fakeFarmAreaOpened;
    [SerializeField] GameObject [] fakeFarmAreaLocked;    

    void RefreshFakeFarmArea () {
        List <int> idFarms = MagicalGarden.Farm.PlantManager.Instance.GetFarmAreaIdsPurchased ();

        int x = 0;
        for (x=0; x < fakeFarmAreaLocked.Length; x++) {
            if (idFarms.Contains (x)) {
                if (fakeFarmAreaLocked[x]) {
                    
                }
                if (fakeFarmAreaOpened[x]) {

                }
            } else {

            }
        }
    }
}
