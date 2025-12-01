using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using MagicalGarden.Manager;
using TMPro;
using UnityEngine.UI;
[System.Serializable]
public class HotelRandomLootConfig {
    public LootType lootType;
    public int lootRefreshTotals = 0; // Total setiap waktu di refresh.
    public int lootRefreshHours = 0; // tiap berapa jam loot di refresh.
    public int lootCurrent = 0; // loot yang tersisa.
    public LootUseable lootUseable;
    public HotelRandomLootObject hotelRandomLootObject;
    public List <int> listDecorationId = new List <int> ();
    public DateTime lastRefreshTime;
    public bool CanRefresh () {

        // kalau belum pernah refresh sebelumnya
        if (lastRefreshTime == default)
            return true;

        TimeSpan diff = TimeManager.Instance.currentTime - lastRefreshTime;
        if (diff.TotalHours >= lootRefreshHours) {
          //  Debug.Log ("Sudah waktunya refresh");
        } else {
          //  Debug.Log ("Belum waktunya refresh");
        }
        return diff.TotalHours >= lootRefreshHours;
    }


    public void GenerateNewDecorationId (Dictionary <int, GameObject> dict) {
        listDecorationId.Clear();

        int max = lootRefreshTotals;
        for (int i = 0; i < max; i++) {
            List<int> keys = new List<int>(dict.Keys);
            int randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];

            listDecorationId.Add(randomKey);
            dict.Remove (randomKey);
        }
        
        lootCurrent = max;
        SaveGenerateData ();
    }

    public void AddDecorationId (int id) {
        listDecorationId.Add (id);
    }

    public void RemoveDecorationId (int id) {
        listDecorationId.Remove (id);
    }

    void SaveGenerateData () {
        lootUseable.SaveListDecorationIds (listDecorationId, TimeManager.Instance.currentTime);
    }

    public void LoadGenerateData () {
        listDecorationId = lootUseable.LoadListDecorationIds ();
        lastRefreshTime = lootUseable.LoadLastRefreshTime (); 
      //  Debug.Log ($"time Useable : " + lastRefreshTime.ToString ());
    }
}

[System.Serializable]
public class HotelRandomLootObject {
    public LootType lootType;
    public GameObject displayPrefab;
    public DOTweenScaleBounce uiIconBounce;
    public DOTweenScaleBounce uiIconTabBounce;
    public TMP_Text currentText;
}

public class HotelRandomLoot : MonoBehaviour 
{
    [Header ("Debug")]
    [SerializeField] bool debugSpawnAll = false;
    [SerializeField] bool showLootAreas = false;
    [SerializeField] GameObject debugLootSample;
    Dictionary <int,GameObject> dictionaryLootBaloon = new Dictionary <int, GameObject> ();
    [Header ("Main")]
    [SerializeField] HotelRandomLootConfig [] hotelRandomLootConfigs;
    
    Dictionary <int, GameObject> dictionaryDecorations = new Dictionary <int, GameObject> ();
    Dictionary <int, GameObject> dictionaryDecorationsOptions = new Dictionary <int, GameObject> ();
    [SerializeField] Transform hotelPropertyLayer;
    DateTime lastTimeHotelRandomLoot;
    [SerializeField] HotelClickableHandler hotelClickableHandler;
    

    [Header ("UI")]
    [SerializeField] GameObject lootTargetUI;
    [SerializeField] HotelRandomLootObject [] hotelRandomLootObjects;
    HotelRandomLootObject currentLootObject;
    void Start () {
        hotelClickableHandler.OnShakedObject += GetClickedGameObject;
        hotelRandomLootConfigs = new HotelRandomLootConfig [] {
            new HotelRandomLootConfig {
                lootType = LootType.GoldenTicket,
                lootRefreshTotals = 15,
                lootRefreshHours = 2,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.GoldenTicket),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.GoldenTicket)
            },
            new HotelRandomLootConfig {
                lootType = LootType.NormalEgg,
                lootRefreshTotals = 1,
                lootRefreshHours = 24,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.NormalEgg),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.NormalEgg)
            },
            new HotelRandomLootConfig {
                lootType = LootType.RareEgg,
                lootRefreshTotals = 1,
                lootRefreshHours = 24 * 7,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.RareEgg),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.RareEgg)
            },
        };

        int idDecoration = 0;
        foreach (Transform child in hotelPropertyLayer) {
            if (child.gameObject.tag == "ClickableDecoration") {
                dictionaryDecorations.Add (idDecoration, child.gameObject);
                dictionaryDecorationsOptions.Add (idDecoration, child.gameObject);
                idDecoration ++;
            }
        }
        
        if (debugSpawnAll) {
            foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
                config.GenerateNewDecorationId (dictionaryDecorationsOptions);
            }
        } else {
            
        }

        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            config.LoadGenerateData ();
        }
        StartCoroutine (nCheckRefreshRealTime ());

        if (showLootAreas) {
            ShowLootAreas ();
        }

        hotelRandomLootObjects[0].currentText.text = SaveSystem.PlayerConfig.goldenTicket.ToString ();
        hotelRandomLootObjects[1].currentText.text = SaveSystem.PlayerConfig.normalEgg.ToString ();
        hotelRandomLootObjects[2].currentText.text = SaveSystem.PlayerConfig.rareEgg.ToString ();

        debugGenerateDirectly.onClick.AddListener (GenerateDirectly);
    }

    void Update () {
        DebugHandler ();
        
    }

    void GetClickedGameObject (GameObject clickedObject) {
        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            List <int> ids = config.listDecorationId;
            if (ids.Count > 0) {
                foreach (int id in ids) {
                    if (dictionaryDecorations[id] == clickedObject) {
                        Debug.Log ("This stuff has a loot !");
                        config.listDecorationId.Remove (id);
                        config.lootUseable.GetLoot (1);

                        if (showLootAreas) {
                           Destroy (dictionaryLootBaloon[id]) ;
                           dictionaryLootBaloon.Remove (id);
                        }
                        currentLootObject = config.hotelRandomLootObject;
                        SpawnHotelLootDisplay (config);
                        ShowTabLoot ();
                        return;
                    }
                }
            }
        }
        
    }

    IEnumerator nCheckRefreshRealTime () {
       
        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            if (config.CanRefresh ()) {
                config.GenerateNewDecorationId (dictionaryDecorationsOptions);
            } else {
                config.LoadGenerateData ();
            }
        }

        yield return new WaitForSeconds (120); // refresh tiap 2 menit supaya tidak berat.
        StartCoroutine (nCheckRefreshRealTime ());
    }
    
    LootUseable GetLootUsable (LootType lootType) {
        switch (lootType) {
            case LootType.GoldenTicket :
            return GoldenTicket.instance;
            break;
            case LootType.NormalEgg :
            return NormalEgg.instance;
            break;
            case LootType.RareEgg :
            return RareEgg.instance;
            break;
            
        }

        return null;
    }

    HotelRandomLootObject GetHotelRandomLootObject (LootType lootType) {
        foreach (HotelRandomLootObject hotelRandomLootObject in hotelRandomLootObjects) {
            if (hotelRandomLootObject.lootType == lootType) {
                return hotelRandomLootObject;
            }
        }
        return null;
    }
    
    #region HotelLootDisplay
    public void SpawnHotelLootDisplay (HotelRandomLootConfig config) {
        GameObject clone = GameObject.Instantiate(currentLootObject.displayPrefab);
        clone.SetActive (true);
        clone.transform.SetParent (lootTargetUI.transform.parent);
        clone.GetComponent<HotelLootDisplay> ().OnTransitionFinished += PlayLootBounce;
        clone.GetComponent <HotelLootDisplay> ().StartPlay (lootTargetUI.transform, config, GetHotelRandomLootObject (config.lootType)); 
    }
    #endregion
    #region Debug
    List <GameObject> listLootArea = new List <GameObject> ();
    void ShowLootAreas () {

        foreach (GameObject go in listLootArea) {
            Destroy (go);
        }
        listLootArea.Clear ();

        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            foreach (int id in config.listDecorationId) {
                GameObject target = dictionaryDecorations[id];
                GameObject clone = GameObject.Instantiate (debugLootSample);
                clone.transform.position = target.transform.position + new Vector3 (0,0.5f,0);
                listLootArea.Add (clone);
                dictionaryLootBaloon.Add (id, clone);
            }
        }
    }

    [SerializeField] Image debugMenu;
    [SerializeField] Button debugGenerateDirectly;
    bool onDebugHandler = false;
    
    void DebugHandler () {
        if (Input.GetKeyDown (KeyCode.H)) {
            if (onDebugHandler) {
                onDebugHandler = false;
                debugMenu.gameObject.SetActive (false);
            } else {
                onDebugHandler = true;
                debugMenu.gameObject.SetActive (true);
            }
        }
    }

    void GenerateDirectly () {
        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            config.GenerateNewDecorationId (dictionaryDecorationsOptions);
        }

        
        if (showLootAreas) {
            ShowLootAreas ();
        }
    }

    #endregion
    #region UI
    void PlayLootBounce (HotelRandomLootConfig config, HotelRandomLootObject configObject) {
        configObject.currentText.text = config.lootUseable.GetCurrency().ToString ();
        configObject.uiIconBounce.Play ();
    }

    void ShowTabLoot () {
        
        currentLootObject.uiIconTabBounce.gameObject.SetActive (true);
        currentLootObject.uiIconTabBounce.Play ();
    }
    
    #endregion
}
