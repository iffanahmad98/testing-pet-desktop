using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipFarmArea : MonoBehaviour {
    [SerializeField] GameObject [] fakeFarmAreaOpened;
    [SerializeField] GameObject [] fakeFarmAreaLocked;    
    
    void Start () {
        playerConfig = SaveSystem.PlayerConfig;
    }

    void RefreshFakeFarmArea () {
        List <int> idFarms = PlantManager.Instance.GetFarmAreaIdsPurchased ();

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
