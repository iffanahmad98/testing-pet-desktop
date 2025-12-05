using UnityEngine;
using TMPro;

public class HotelGiftHandler : MonoBehaviour
{
    public static HotelGiftHandler instance;
     
    [SerializeField] GameObject hotelGiftDisplayPrefab;
    
    [Header ("UI")]
    [SerializeField] GameObject lootTargetUI;
    [SerializeField] TMP_Text currentText;
    public DOTweenScaleBounce uiIconBounce;
    public DOTweenScaleBounce uiIconTabBounce;
    bool onceRefresh = false;
    void Awake () {
        instance = this;
    }

    public void SpawnHotelGiftDisplay () { // HotelGiftSpawner
        if (!onceRefresh) {
            onceRefresh = true;
            currentText.text = HotelGift.instance.GetCurrency().ToString ();
        }
        GameObject clone = GameObject.Instantiate(hotelGiftDisplayPrefab);
        clone.SetActive (true);
        clone.transform.SetParent (lootTargetUI.transform.parent);
        clone.GetComponent<HotelLootDisplay> ().OnClearTransitionFinished += PlayLootBounce;
        clone.GetComponent <HotelLootDisplay> ().StartPlay (lootTargetUI.transform); 
        ShowTabLoot ();
    }

    void ShowTabLoot () {
        
        uiIconTabBounce.gameObject.SetActive (true);
        uiIconTabBounce.Play ();
    }

    void PlayLootBounce () {
        Debug.Log ("Refresh Bounce " + HotelGift.instance.GetCurrency());
        currentText.text = HotelGift.instance.GetCurrency().ToString ();
        uiIconBounce.Play ();
    }

    
}
