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
    public List <GameObject> listDecorationObject = new List <GameObject> ();
    public DateTime lastRefreshTime;
    public Transform hotelPropertyLayer;

    

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
        listDecorationObject.Clear ();

        int max = lootRefreshTotals;
        for (int i = 0; i < max; i++) {
            List<int> keys = new List<int>(dict.Keys);
            int randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];

            listDecorationId.Add(randomKey);
            listDecorationObject.Add (GetChildGameObjectFromId (randomKey));
            dict.Remove (randomKey);
        }
        
        lootCurrent = max;
        SaveGenerateData ();
    }

    public void AddDecorationId (int id) {
        listDecorationId.Add (id);
        listDecorationObject.Add (GetChildGameObjectFromId (id));
    }

    public void RemoveDecorationId (int id) {
        listDecorationId.Remove (id);
        listDecorationObject.Remove (GetChildGameObjectFromId (id));
    }

    void SaveGenerateData () {
        lootUseable.SaveListDecorationIds (listDecorationId, TimeManager.Instance.currentTime);
    }

    public void LoadGenerateData () {
        listDecorationId = lootUseable.LoadListDecorationIds ();
        lastRefreshTime = lootUseable.LoadLastRefreshTime (); 

        listDecorationObject.Clear ();
        foreach (int value in listDecorationId) {
            listDecorationObject.Add (GetChildGameObjectFromId (value));
        }
      //  Debug.Log ($"time Useable : " + lastRefreshTime.ToString ());
    }

    #region Utility
    GameObject GetChildGameObjectFromId (int value) {
        int idDecoration = 0;
        foreach (Transform child in hotelPropertyLayer) {
            if (child.gameObject.tag == "ClickableDecoration") {
                if (value == idDecoration) {
                    return child.gameObject;
                } else {
                    idDecoration ++;
                }
            }
        }

        return null;
    }

    public bool IsContainsSameGameObject (GameObject checkObject) {
        foreach (GameObject theObject in listDecorationObject) {
            if (theObject == checkObject) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

[System.Serializable]
public class HotelRandomLootObject {
    public LootType lootType;
    public GameObject displayPrefab;
    public DOTweenScaleBounce uiIconBounce;
    public DOTweenScaleBounce uiIconTabBounce;
    public TMP_Text currentText;
    public GameObject displayWorldPrefab;
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

    [Header("Shake Settings")]
    public float duration = 0.15f;
    public float strength = 0.3f;
    public int vibrato = 1;
    
    Dictionary <LootType, HotelRandomLootConfig> dictionaryHotelRandomLootConfig = new Dictionary <LootType, HotelRandomLootConfig> ();
    void Start () {
        hotelClickableHandler.OnShakedObject += GetClickedGameObject;
        hotelRandomLootConfigs = new HotelRandomLootConfig [] {
            new HotelRandomLootConfig {
                lootType = LootType.GoldenTicket,
                lootRefreshTotals = 15,
                lootRefreshHours = 2,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.GoldenTicket),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.GoldenTicket),
                hotelPropertyLayer = hotelPropertyLayer
            },
            new HotelRandomLootConfig {
                lootType = LootType.NormalEgg,
                lootRefreshTotals = 1,
                lootRefreshHours = 24,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.NormalEgg),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.NormalEgg),
                hotelPropertyLayer = hotelPropertyLayer
            },
            new HotelRandomLootConfig {
                lootType = LootType.RareEgg,
                lootRefreshTotals = 1,
                lootRefreshHours = 24 * 7,
                lootCurrent = 0,
                lootUseable = GetLootUsable (LootType.RareEgg),
                hotelRandomLootObject = GetHotelRandomLootObject (LootType.RareEgg),
                hotelPropertyLayer = hotelPropertyLayer
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

        dictionaryHotelRandomLootConfig = new Dictionary <LootType, HotelRandomLootConfig> ();
        dictionaryHotelRandomLootConfig.Add (LootType.GoldenTicket, hotelRandomLootConfigs [0]);
        dictionaryHotelRandomLootConfig.Add (LootType.NormalEgg, hotelRandomLootConfigs [1]);
        dictionaryHotelRandomLootConfig.Add (LootType.RareEgg, hotelRandomLootConfigs [2]);

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
                        /*
                        Debug.Log ("This stuff has a loot !");
                        config.listDecorationId.Remove (id);
                        config.lootUseable.GetLoot (1);

                        if (showLootAreas) {
                           Destroy (dictionaryLootBaloon[id]) ;
                           dictionaryLootBaloon.Remove (id);
                        }
                        currentLootObject = config.hotelRandomLootObject;
                        */
                        if (!config.IsContainsSameGameObject (clickedObject)) {
                            return;
                        }
                        GetLoot (config, id);
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
        dictionaryLootBaloon = new Dictionary <int, GameObject> ();
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
        Debug.Log (config.lootUseable.GetCurrency().ToString ());
        configObject.currentText.text = config.lootUseable.GetCurrency().ToString ();
        configObject.uiIconBounce.Play ();
    }

    void ShowTabLoot () {
        
        currentLootObject.uiIconTabBounce.gameObject.SetActive (true);
        currentLootObject.uiIconTabBounce.Play ();
    }
    
    #endregion
    #region NPCLootHunter

    public void GetTicketFromNPC (GameObject npcFrom, GameObject lootObject) {
        ObjectShakeUtility.ShakeSingle (lootObject, lootObject.transform.position, duration, strength, vibrato);

        foreach (HotelRandomLootConfig config in hotelRandomLootConfigs) {
            List <int> ids = config.listDecorationId;
            if (ids.Count > 0) {
                foreach (int id in ids) {
                    if (dictionaryDecorations[id] == lootObject) {
                        Debug.Log ("This stuff has a loot (By NPC) !");
                        if (!config.IsContainsSameGameObject (lootObject)) {
                            return;
                        }
                        GetLoot (config, id);
                        ShowTabLoot ();
                        SpawnLootDisplayUpperNPC (config, npcFrom);
                        return;
                    }
                }
            }
        }

    }

    void SpawnLootDisplayUpperNPC (HotelRandomLootConfig config, GameObject npcTarget) {
        GameObject clone = GameObject.Instantiate(currentLootObject.displayWorldPrefab);
        clone.transform.position = npcTarget.transform.position + new Vector3 (0,1.5f,0);
        clone.SetActive (true);
        clone.GetComponent<DOTweenBounceWorld> ().OnTransitionFinished += PlayLootBounce;
        clone.GetComponent <DOTweenBounceWorld> ().StartPlay (config, GetHotelRandomLootObject (config.lootType));
        clone.GetComponent <DOTweenColorWorld> ().Play ();
        
       // clone.transform.SetParent (lootTargetUI.transform.parent);
       /*
        clone.GetComponent<HotelLootDisplay> ().OnTransitionFinished += PlayLootBounce;
        
        clone.GetComponent <HotelLootDisplay> ().StartPlay (lootTargetUI.transform, config, GetHotelRandomLootObject (config.lootType)); 
        */
    }
    #endregion
    // HotelFacilitiesLootDetector :
    public bool IsHasLoot (LootType lootType, GameObject checkObject) {
        return dictionaryHotelRandomLootConfig[lootType].IsContainsSameGameObject (checkObject);
    }

    void GetLoot (HotelRandomLootConfig config, int id) {
        
        config.RemoveDecorationId (id);
        config.lootUseable.GetLoot (1);
        if (showLootAreas) {
            Destroy (dictionaryLootBaloon[id]) ;
            dictionaryLootBaloon.Remove (id);
        }
        currentLootObject = config.hotelRandomLootObject;
    }
    
}   
