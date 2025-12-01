using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class HotelFacilitiesPodiumCard {
    public Button buyButton;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text detailText;
    public GameObject podium;
    public Image coinTypeImage;
    public Button appliedButton;
    public HotelFacilitiesDataSO facilityData;
}

public class HotelFacilitiesMenu : HotelShopMenuBase {
    [SerializeField] HotelFacilitiesDatabaseSO database;

    bool hotelFacilitiesPodium = false;
    [SerializeField] GameObject podiumCardPrefab;
    [SerializeField] Transform podiumCardPanel;
    List <HotelFacilitiesPodiumCard> listPodiumCard = new List <HotelFacilitiesPodiumCard> ();
    
    public override void ShowMenu () {
        base.ShowMenu ();
        InstantiateHotelFacilities ();
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
                facilityData = data
            };

            podiumClone.transform.SetParent (podiumCardPanel);
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
                    BuyFacilities(listPodiumCard[index].facilityData)
                );
                listPodiumCard[index].appliedButton.gameObject.SetActive (false);
            }

            listPodiumCard[i].nameText.text = listPodiumCard[i].facilityData.facilityName;
            listPodiumCard[i].priceText.text = listPodiumCard[i].facilityData.price.ToString ();
            listPodiumCard[i].detailText.text = listPodiumCard[i].facilityData.detailText;
            GameObject facilityClone = GameObject.Instantiate (listPodiumCard[i].facilityData.facilityPrefab);
            facilityClone.transform.SetParent (listPodiumCard[i].podium.transform);
            facilityClone.transform.localPosition = listPodiumCard[i].facilityData.facilityLocalPosition;
            facilityClone.transform.localScale = listPodiumCard[i].facilityData.facilityLocalScale;
            facilityClone.SetActive (true);
            
        }
        
    }
   }

   public void BuyFacilities (HotelFacilitiesDataSO data) {
    Debug.Log ("Facilities Price : " + data.price);
    if (CoinManager.CheckCoins (data.price)) {
        CoinManager.SpendCoins (data.price);

        SaveSystem.PlayerConfig.AddHotelFacilityData (data.id);
        SaveSystem.SaveAll ();
    }
   }

}
