using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using MagicalGarden.AI;
[System.Serializable]
public class HotelFacilitiesPodiumCard {
    public Button buyButton;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text detailText;
    public GameObject podium;
    public Image coinTypeImage;
    public Button appliedButton;
    public Button applyButton;
    public HotelFacilitiesDataSO facilityData;
    public BaseEntityAI baseEntityAI;
}

public class HotelFacilitiesMenu : HotelShopMenuBase {
    [SerializeField] HotelFacilitiesDatabaseSO database;

    bool hotelFacilitiesPodium = false;
    [SerializeField] GameObject podiumCardPrefab;
    [SerializeField] Transform podiumCardPanel;
    List <HotelFacilitiesPodiumCard> listPodiumCard = new List <HotelFacilitiesPodiumCard> ();

    [Header ("Handler")]
    public List<OwnedHotelFacilityData> ownedHotelFacilitiesData = new ();
    public Dictionary <string, GameObject> dictionaryHotelFacilities = new Dictionary <string, GameObject> ();
    
    void Start () {
        LoadAllDatas ();
        foreach (OwnedHotelFacilityData data in ownedHotelFacilitiesData) {
            if (data.isActive) {
                SpawnHotelFacilities (data.id, false);
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

    
   void InstantiateHotelFacilities () {
    if (!hotelFacilitiesPodium) {
        hotelFacilitiesPodium = true;
        foreach (HotelFacilitiesDataSO data in database.allDatas) {
            GameObject podiumClone = GameObject.Instantiate (podiumCardPrefab);
            HotelFacilitiesPodiumCard newPodiumCard = new HotelFacilitiesPodiumCard {
                buyButton = podiumClone.transform.Find ("BuyButton").GetComponent <Button> (),
                nameText = podiumClone.transform.Find ("Name").GetComponent <TMP_Text> (),
                priceText = podiumClone.transform.Find ("Price").GetComponent <TMP_Text> (),
                detailText = podiumClone.transform.Find ("DetailTab/DetailText").GetComponent <TMP_Text> (),
                podium = podiumClone.transform.Find ("Podium").gameObject,
                coinTypeImage = podiumClone.transform.Find ("CoinTypeImage").GetComponent <Image> (),
                appliedButton = podiumClone.transform.Find ("AppliedButton").GetComponent <Button> (),
                applyButton = podiumClone.transform.Find ("ApplyButton").GetComponent <Button> (),
                facilityData = data
            };

            podiumClone.SetActive (true);
            podiumClone.transform.SetParent (podiumCardPanel);
           // podiumClone.transform.localScale = new Vector3  (1,1,1);
          //  Debug.LogError ("Podium " + podiumClone.transform.localScale);
            listPodiumCard.Add (newPodiumCard);
        }

        for (int i = 0; i <listPodiumCard.Count; i++) {
             int index = i; // penting!
            if (SaveSystem.PlayerConfig.HasHotelFacility (listPodiumCard[index].facilityData.id)) {
                listPodiumCard[index].buyButton.gameObject.SetActive (false);
                listPodiumCard[index].priceText.gameObject.SetActive (false);
                listPodiumCard[index].coinTypeImage.gameObject.SetActive (false);
                listPodiumCard[index].appliedButton.gameObject.SetActive (true);
            } else {
                listPodiumCard[index].buyButton.onClick.AddListener(() => 
                    BuyFacilities(listPodiumCard[index], listPodiumCard[index].facilityData)
                );
                listPodiumCard[index].appliedButton.gameObject.SetActive (false);
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
            
            GameObject facilityClone = GameObject.Instantiate (listPodiumCard[i].facilityData.facilityPrefab);
            facilityClone.transform.SetParent (listPodiumCard[i].podium.transform);
            facilityClone.transform.localPosition = listPodiumCard[i].facilityData.facilityLocalPosition;
            facilityClone.transform.localScale = listPodiumCard[i].facilityData.facilityLocalScale;
            facilityClone.SetActive (true);
            
            listPodiumCard[index].baseEntityAI = facilityClone.GetComponent <BaseEntityAI> ();
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
        
    }
   // podiumLayout.OnRebuild ();
   }

   public void BuyFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
    Debug.Log ("Facilities Price : " + data.price);
    if (CoinManager.CheckCoins (data.price)) {
        CoinManager.SpendCoins (data.price);

        SaveSystem.PlayerConfig.AddHotelFacilityData (data.id);
        SaveSystem.SaveAll ();
        
        var state = podiumCard.baseEntityAI.skeleton.AnimationState;
        // play jump sekali
        state.SetAnimation(0, "jumping", false);
        // setelah selesai, lanjut idle
        state.AddAnimation(0, "idle", true, 0f);

        SpawnHotelFacilities (data.id, true);
        
    }
   }

   public void CancelFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
        SaveSystem.PlayerConfig.ChangeHotelFacilityData (data.id, false);
        SaveSystem.SaveAll ();
        
        DestroyHotelFacilities (data.id);
   }

   public void ApplyFacilities (HotelFacilitiesPodiumCard podiumCard, HotelFacilitiesDataSO data) {
        SaveSystem.PlayerConfig.ChangeHotelFacilityData (data.id, true);
        SaveSystem.SaveAll ();
        var state = podiumCard.baseEntityAI.skeleton.AnimationState;
        // play jump sekali
        state.SetAnimation(0, "jumping", false);
        // setelah selesai, lanjut idle
        state.AddAnimation(0, "idle", true, 0f);

        SpawnHotelFacilities (data.id, true);
   }

   void RefreshBuyButton (HotelFacilitiesPodiumCard podiumCard, string buttonCode) {
        if (buttonCode == "Applied") {
            podiumCard.buyButton.gameObject.SetActive (false);
            podiumCard.appliedButton.gameObject.SetActive (true);
            podiumCard.applyButton.gameObject.SetActive (false);
        } else if (buttonCode == "Apply") {
            podiumCard.buyButton.gameObject.SetActive (false);
            podiumCard.appliedButton.gameObject.SetActive (false);
            podiumCard.applyButton.gameObject.SetActive (true);
        } else if (buttonCode == "Buy") {
            podiumCard.buyButton.gameObject.SetActive (true);
            podiumCard.appliedButton.gameObject.SetActive (false);
            podiumCard.applyButton.gameObject.SetActive (false);
        }
   }

    #region Handler
    void LoadAllDatas () {
        ownedHotelFacilitiesData = SaveSystem.PlayerConfig.ownedHotelFacilitiesData;
    }

    void SpawnHotelFacilities (string facilityId, bool refreshButton) {
        HotelFacilitiesDataSO data = database.GetHotelFacilitiesDataSO (facilityId);
        GameObject facilityClone = GameObject.Instantiate (data.facilityPrefab);
        facilityClone.transform.position = data.facilitySpawnPosition;
        facilityClone.SetActive (true);
        dictionaryHotelFacilities.Add (facilityId, facilityClone);
        if (refreshButton) {
            RefreshBuyButton (GetHotelFacilitiesPodiumCard (data.id), "Applied");
        }
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
