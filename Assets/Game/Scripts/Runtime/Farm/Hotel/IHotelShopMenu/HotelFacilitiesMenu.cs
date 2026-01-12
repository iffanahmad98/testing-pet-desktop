using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using MagicalGarden.AI;
[System.Serializable]
public class HotelFacilitiesPodiumCard {
    public Button hireButtonOnce; // Dulu ini sebelumnya sistem buy 1x saja.
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text detailText;
    public GameObject podium;
    public Image coinTypeImage;
    public Button appliedButton;
    public Button applyButton;
    public Button hireButton;
    public TMP_Text hiredText;
    public HotelFacilitiesDataSO facilityData;
    public GameObject cloneAI;
}

public class HotelFacilitiesMenu : HotelShopMenuBase {
    [SerializeField] HotelFacilitiesDatabaseSO database;

    bool hotelFacilitiesPodium = false;
    [SerializeField] GameObject podiumCardPrefab;
    [SerializeField] Transform podiumCardPanel;
    List <HotelFacilitiesPodiumCard> listPodiumCard = new List <HotelFacilitiesPodiumCard> ();
    
    [Header ("UI")]
    public Sprite onBuySprite;
    public Sprite offBuySprite;
    public Color onHireColor, offHireColor;
    [Header ("Handler")]
    public List<OwnedHotelFacilityData> ownedHotelFacilitiesData = new ();
    public Dictionary <string, GameObject> dictionaryHotelFacilities = new Dictionary <string, GameObject> ();
    public List <HiredHotelFacilityData> hiredHotelFacilityData = new ();

    [Header ("Start Dictionary")]
    public Dictionary <string, int> dictionaryHiredMaxFacility = new Dictionary <string, int> ();
    public Dictionary <string, int> dictionaryCurrentFacility = new Dictionary <string, int> ();
    
    public enum HotelFacilitiesType {
        Single, Multiple    
    }

    void Start () {
        LoadStartDictionary ();
        LoadAllDatas ();
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (data.isActive) {
                SpawnHotelFacilities (data.id, false, HotelFacilitiesType.Single);
            }
        }
        foreach (HiredHotelFacilityData data in hiredHotelFacilityData) {
            for (int i=0; i < data.hired; i++) {
                SpawnHotelFacilities (data.id, false, HotelFacilitiesType.Multiple);
            }
            
        }
    }

   // [SerializeField] UILayoutResetter podiumLayout;
    public override void ShowMenu () {
        base.ShowMenu ();
        InstantiateHotelFacilities ();
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (!data.isActive) {
                RefreshBuyButton (GetHotelFacilitiesPodiumCard (data.id), "Apply");
            }
        }
   }

   public override void HideMenu () {
    base.HideMenu ();
   }

   void LoadStartDictionary () {
    dictionaryHiredMaxFacility.Add ("robo_shroom", 3);
    dictionaryHiredMaxFacility.Add ("bellboy_shroom", 2);

    dictionaryCurrentFacility.Add ("wizard_shroom", 0);
    dictionaryCurrentFacility.Add ("nerd_shroom", 0);
    dictionaryCurrentFacility.Add ("robo_shroom", 0);
    dictionaryCurrentFacility.Add ("bellboy_shroom",0);
   } 
    
   void InstantiateHotelFacilities () {
    if (!hotelFacilitiesPodium) {
        hotelFacilitiesPodium = true;
        foreach (HotelFacilitiesDataSO data in database.allDatas) {
            GameObject podiumClone = GameObject.Instantiate (podiumCardPrefab);
            HotelFacilitiesPodiumCard newPodiumCard = new HotelFacilitiesPodiumCard {
                hireButtonOnce = podiumClone.transform.Find ("HireButton (Once)").GetComponent <Button> (),
                nameText = podiumClone.transform.Find ("Name").GetComponent <TMP_Text> (),
                priceText = podiumClone.transform.Find ("Price").GetComponent <TMP_Text> (),
                detailText = podiumClone.transform.Find ("DetailTab/DetailText").GetComponent <TMP_Text> (),
                podium = podiumClone.transform.Find ("Podium").gameObject,
                coinTypeImage = podiumClone.transform.Find ("CoinTypeImage").GetComponent <Image> (),
                appliedButton = podiumClone.transform.Find ("AppliedButton").GetComponent <Button> (),
                applyButton = podiumClone.transform.Find ("ApplyButton").GetComponent <Button> (),
                hireButton = podiumClone.transform.Find ("HireButton").GetComponent <Button> (),
                hiredText = podiumClone.transform.Find ("HiredText").GetComponent <TMP_Text> (),
                facilityData = data,
            };
            CheckEligibleCard (newPodiumCard, data);

            podiumClone.SetActive (true);
            podiumClone.transform.SetParent (podiumCardPanel);
           // podiumClone.transform.localScale = new Vector3  (1,1,1);
          //  Debug.LogError ("Podium " + podiumClone.transform.localScale);
            listPodiumCard.Add (newPodiumCard);
        }

        for (int i = 0; i <listPodiumCard.Count; i++) {
             int index = i; // penting!
             if (dictionaryHiredMaxFacility.ContainsKey (listPodiumCard[index].facilityData.id)) {
                // Hired System :
                var card = listPodiumCard[index];
                string facilityId = card.facilityData.id;

                // default state
                card.hireButtonOnce.onClick.RemoveAllListeners();
                card.appliedButton.gameObject.SetActive(false);

                bool isMaximum = false;
            
                if (SaveSystem.PlayerConfig.GetHiredHotelFacilityData(facilityId) !=null)
                {
                    HiredHotelFacilityData hiredData =
                        SaveSystem.PlayerConfig.GetHiredHotelFacilityData(facilityId);

                    if (hiredData != null)
                    {
                        int maxHire = dictionaryHiredMaxFacility[facilityId];
                        isMaximum = hiredData.hired == maxHire;
                    }

                    listPodiumCard[index].hiredText.text = "Hired: " + hiredData.hired;

                } else {
                    listPodiumCard[index].hiredText.text = "Hired: " + 0;
                }

                if (isMaximum)
                {
                    // APPLIED STATE
                  //  card.hireButtonOnce.gameObject.SetActive(false);
                  //  card.hireButton.gameObject.SetActive(true);
                 //   card.hireButton.interactable = false;
                 //   card.priceText.gameObject.SetActive(false);
                  //  card.coinTypeImage.gameObject.SetActive(false);
                    card.appliedButton.gameObject.SetActive(false);
                }
                else
                {
                    // HIRE / BUY STATE
                 //   card.hireButton.gameObject.SetActive(true);
                  //  card.hireButtonOnce.gameObject.SetActive(false);
                 //   card.priceText.gameObject.SetActive(true);
                //    card.coinTypeImage.gameObject.SetActive(true);

                    card.hireButton.onClick.AddListener(() =>
                        HireFacilities(card, card.facilityData)
                    );
                }
                listPodiumCard[index].hiredText.gameObject.SetActive (true);
                
             } else {
                if (SaveSystem.PlayerConfig.HasHotelFacility (listPodiumCard[index].facilityData.id)) {
                    listPodiumCard[index].hireButtonOnce.gameObject.SetActive (false);
                    listPodiumCard[index].priceText.gameObject.SetActive (false);
                    listPodiumCard[index].coinTypeImage.gameObject.SetActive (false);
                    listPodiumCard[index].appliedButton.gameObject.SetActive (true);
                } else {
                    listPodiumCard[index].hireButtonOnce.gameObject.SetActive (true);
                    listPodiumCard[index].hireButtonOnce.onClick.AddListener(() => 
                        BuyFacilities(listPodiumCard[index], listPodiumCard[index].facilityData)
                    );
                    listPodiumCard[index].appliedButton.gameObject.SetActive (false);
                }
                listPodiumCard[index].hiredText.gameObject.SetActive (false);
             }
            
            listPodiumCard[index].applyButton.onClick.AddListener(() => 
                    ApplyFacilities(listPodiumCard[index], listPodiumCard[index].facilityData)
            );
            listPodiumCard[index].appliedButton.onClick.AddListener(() => 
                    CancelFacilities(listPodiumCard[index], listPodiumCard[index].facilityData)
            );

            listPodiumCard[i].nameText.text = listPodiumCard[i].facilityData.facilityName;
            listPodiumCard[i].priceText.text = listPodiumCard[i].facilityData.price.ToString ();
            listPodiumCard[i].detailText.text = listPodiumCard[i].facilityData.detailText;
            
            GameObject facilityClone = GameObject.Instantiate (listPodiumCard[i].facilityData.facilityUIPrefab);
            facilityClone.transform.SetParent (listPodiumCard[i].podium.transform);
            facilityClone.transform.localPosition = listPodiumCard[i].facilityData.facilityUILocalPosition;
            facilityClone.transform.localScale = listPodiumCard[i].facilityData.facilityUILocalScale;
            facilityClone.SetActive (true);
            
            listPodiumCard[index].cloneAI = facilityClone;
            if (facilityClone.GetComponent <IsometricSpineSorting> ()) {
                // facilityClone.GetComponent <SkeletonAnimation> ().sortingLayerName  = "MotionUI";
               SkeletonRenderer skeletonRenderer = facilityClone.GetComponent<SkeletonRenderer>();
                Renderer renderer = skeletonRenderer.GetComponent<Renderer>();
                renderer.sortingLayerName = "Motion UI";
                renderer.sortingOrder = 10;
            }
            if (facilityClone.GetComponent <NPCHotel> ()) {
                facilityClone.GetComponent <NPCHotel> ().enabled = false;
            }

            if (facilityClone.GetComponent <NPCLootHunter> ()) {
                facilityClone.GetComponent <NPCLootHunter> ().enabled = false;
            }

            foreach (Transform children in facilityClone.transform) {
                children.gameObject.SetActive (false);
            }
            
            
        }
        
    } else {
        foreach (HotelFacilitiesPodiumCard podium in listPodiumCard) {
            CheckEligibleCard (podium, podium.facilityData);
        }
    }
   // podiumLayout.OnRebuild ();
   }

   public void BuyFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
        Debug.Log("Facilities Price : " + data.price);

        if (CoinManager.CheckCoins(data.price)) {
            CoinManager.SpendCoins(data.price);

            SaveSystem.PlayerConfig.AddHotelFacilityData(data.id);
            SaveSystem.SaveAll();

            SkeletonGraphic skeletonGraphic =
                podiumCard.cloneAI.GetComponent<SkeletonGraphic>();

            if (skeletonGraphic != null) {
                var state = skeletonGraphic.AnimationState;
                state.SetAnimation(0, "jumping", false);
                state.AddAnimation(0, "idle", true, 0f);
                skeletonGraphic.Update(0);
            }

            SpawnHotelFacilities(data.id, true, HotelFacilitiesType.Single);

            foreach (HotelFacilitiesPodiumCard podium in listPodiumCard) {
                CheckEligibleCard(podium, podium.facilityData);
            }
        }
    }


   public void HireFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
    Debug.Log ("Facilities Price : " + data.price);
    if (CoinManager.CheckCoins (data.price)) {
        CoinManager.SpendCoins (data.price);

        SaveSystem.PlayerConfig.AddHiredHotelFacilityData (data.id, 1);
        SaveSystem.SaveAll ();
        
        /*
        var state = podiumCard.baseEntityAI.skeleton.AnimationState;
        // play jump sekali
        state.SetAnimation(0, "jumping", false);
        // setelah selesai, lanjut idle
        state.AddAnimation(0, "idle", true, 0f);
        */

        SkeletonGraphic skeletonGraphic =
                podiumCard.cloneAI.GetComponent<SkeletonGraphic>();

        if (skeletonGraphic != null) {
            var state = skeletonGraphic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            skeletonGraphic.Update(0);
        }

        SpawnHotelFacilities (data.id, true, HotelFacilitiesType.Multiple);
        podiumCard.hiredText.text = "Hired: " + SaveSystem.PlayerConfig.GetHiredHotelFacilityData (data.id).hired.ToString ();
        
        bool isMaximum = false;

     
        int maxHire = dictionaryHiredMaxFacility[data.id];
        isMaximum = SaveSystem.PlayerConfig.GetHiredHotelFacilityData (data.id).hired == maxHire;
        

        if (isMaximum) {
            podiumCard.hireButton.interactable = false;
        }

        foreach (HotelFacilitiesPodiumCard podium in listPodiumCard) {
            CheckEligibleCard (podium, podium.facilityData);
        }
    }
   }

   public void CancelFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
        SaveSystem.PlayerConfig.ChangeHotelFacilityData (data.id, false);
        SaveSystem.SaveAll ();
        
        DestroyHotelFacilities (data.id);
        dictionaryCurrentFacility[data.id] --;
   }

   public void ApplyFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
        SaveSystem.PlayerConfig.ChangeHotelFacilityData (data.id, true);
        SaveSystem.SaveAll ();
        /*
        var state = podiumCard.baseEntityAI.skeleton.AnimationState;
        // play jump sekali
        state.SetAnimation(0, "jumping", false);
        // setelah selesai, lanjut idle
        state.AddAnimation(0, "idle", true, 0f);
        */
        SkeletonGraphic skeletonGraphic =
                podiumCard.cloneAI.GetComponent<SkeletonGraphic>();

        if (skeletonGraphic != null) {
            var state = skeletonGraphic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            skeletonGraphic.Update(0);
        }
        
        SpawnHotelFacilities (data.id, true, HotelFacilitiesType.Single);
   }

   void RefreshBuyButton (HotelFacilitiesPodiumCard podiumCard, string buttonCode) {
        if (buttonCode == "Applied") {
            podiumCard.hireButtonOnce.gameObject.SetActive (false);
            podiumCard.appliedButton.gameObject.SetActive (true);
            podiumCard.applyButton.gameObject.SetActive (false);
        } else if (buttonCode == "Apply") {
            podiumCard.hireButtonOnce.gameObject.SetActive (false);
            podiumCard.appliedButton.gameObject.SetActive (false);
            podiumCard.applyButton.gameObject.SetActive (true);
        } else if (buttonCode == "Buy") {
            podiumCard.hireButtonOnce.gameObject.SetActive (true);
            podiumCard.appliedButton.gameObject.SetActive (false);
            podiumCard.applyButton.gameObject.SetActive (false);
        }
   }
    #region Eligibility
    void CheckEligibleCard (HotelFacilitiesPodiumCard newPodiumCard, HotelFacilitiesDataSO data ) {
         if (dictionaryHiredMaxFacility.ContainsKey (data.id)) { // data.maxHired > 0
                newPodiumCard.hireButtonOnce.gameObject.SetActive (false);
                newPodiumCard.hireButton.gameObject.SetActive (true);
                HiredHotelFacilityData hiredHotelFacilityData = SaveSystem.PlayerConfig.GetHiredHotelFacilityData (data.id);
                int curHired = 0;
                if (hiredHotelFacilityData != null) {
                    curHired = hiredHotelFacilityData.hired;
                }
                if (curHired < dictionaryHiredMaxFacility[data.id]) {
                    if (data.IsHiredEligible (curHired)) {
                        newPodiumCard.hireButton.image.color = onHireColor;
                        newPodiumCard.hireButton.interactable = true;
                        newPodiumCard.priceText.gameObject.SetActive (true);
                        newPodiumCard.coinTypeImage.gameObject.SetActive (true);
                    } else {
                        newPodiumCard.hireButton.image.color = offHireColor;
                        newPodiumCard.hireButton.interactable = false;
                        newPodiumCard.priceText.gameObject.SetActive (false);
                        newPodiumCard.coinTypeImage.gameObject.SetActive (false);
                    }
                } else {
                        newPodiumCard.hireButton.image.color = offHireColor;
                        newPodiumCard.hireButton.interactable = false;
                        newPodiumCard.priceText.gameObject.SetActive (false);
                        newPodiumCard.coinTypeImage.gameObject.SetActive (false);
                }

            } else {
                newPodiumCard.hireButtonOnce.gameObject.SetActive (true);
                newPodiumCard.hireButton.gameObject.SetActive (false);

                if (data.IsEligible () && SaveSystem.PlayerConfig.GetHotelFacilityData (data.id) == null) {
                    newPodiumCard.hireButtonOnce.image.sprite = onBuySprite;
                    newPodiumCard.hireButtonOnce.interactable = true;
                    newPodiumCard.priceText.gameObject.SetActive (true);
                    newPodiumCard.coinTypeImage.gameObject.SetActive (true);
                } else {
                    if (SaveSystem.PlayerConfig.GetHotelFacilityData (data.id) != null) {
                        newPodiumCard.hireButtonOnce.gameObject.SetActive (false);
                        newPodiumCard.hireButtonOnce.image.sprite = offBuySprite;
                        newPodiumCard.hireButtonOnce.interactable = false;
                        newPodiumCard.priceText.gameObject.SetActive (false);
                        newPodiumCard.coinTypeImage.gameObject.SetActive (false);
                    } else {
                        newPodiumCard.hireButtonOnce.gameObject.SetActive (false);
                        newPodiumCard.hireButtonOnce.image.sprite = offBuySprite;
                        newPodiumCard.hireButtonOnce.interactable = false;
                        newPodiumCard.priceText.gameObject.SetActive (true);
                        newPodiumCard.coinTypeImage.gameObject.SetActive (true);
                    }
                    
                }
            }
    }
    #endregion
    #region Handler
    void LoadAllDatas () {
        ownedHotelFacilitiesData = SaveSystem.PlayerConfig.ownedHotelFacilitiesData;
        hiredHotelFacilityData = SaveSystem.PlayerConfig.hiredHotelFacilityData;

        ClearAllUnusedDatas ();
    }

    void ClearAllUnusedDatas ()
    {
        for (int i = ownedHotelFacilitiesData.Count - 1; i >= 0; i--)
        {
            if (ownedHotelFacilitiesData[i].id == "robo_shroom")
            {
                SaveSystem.PlayerConfig.RemoveHotelFacilityData(
                    ownedHotelFacilitiesData[i].id
                );
            }
        }

        SaveSystem.SaveAll();
    }

    void SpawnHotelFacilities (string facilityId, bool refreshButton, HotelFacilitiesType hotelFacilitiesType) {
        HotelFacilitiesDataSO data = database.GetHotelFacilitiesDataSO (facilityId);
        GameObject facilityClone = GameObject.Instantiate (data.facilityPrefab);
        facilityClone.transform.position = data.facilitySpawnPositions[dictionaryCurrentFacility[facilityId]];
        facilityClone.SetActive (true);
        if (hotelFacilitiesType == HotelFacilitiesType.Single) {
            dictionaryHotelFacilities.Add (facilityId, facilityClone);

            if (refreshButton) {
                RefreshBuyButton (GetHotelFacilitiesPodiumCard (data.id), "Applied");
            }
        }

        dictionaryCurrentFacility[facilityId] ++;


    }

    void DestroyHotelFacilities (string facilityId) {
        GameObject target = dictionaryHotelFacilities [facilityId];
        Destroy (target);
        dictionaryHotelFacilities.Remove (facilityId);

        RefreshBuyButton (GetHotelFacilitiesPodiumCard (facilityId), "Apply");
    }
    #endregion
    HotelFacilitiesPodiumCard GetHotelFacilitiesPodiumCard (string facilityId) {
        foreach (HotelFacilitiesPodiumCard card in listPodiumCard) {
            if (card.facilityData.id == facilityId) {
                return card;
            }
        }
        return null;
    }

    
}
