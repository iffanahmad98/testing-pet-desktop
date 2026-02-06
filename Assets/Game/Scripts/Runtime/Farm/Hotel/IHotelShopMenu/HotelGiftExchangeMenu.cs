using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Spine.Unity;
public class HotelGiftExchangeMenu : HotelShopMenuBase
{
   
   [SerializeField] TMP_Text remainingGiftText;
   [SerializeField] RectTransform giftCollectionPanel;
   [SerializeField] GameObject hotelGiftExchangePrefab;
   [SerializeField] GameObject rewardAnimatorGift;
   

   List <GameObject> listHotelGiftExchange = new List <GameObject> ();
   int totalGift;
   int maxGiftDisplay = 6;
   
   [SerializeField] RewardAnimator rewardAnimator;
   [SerializeField] Button openBoxButton;
   [SerializeField] Sprite openBoxAvailableSprite;
   [SerializeField] Sprite openBoxNotAvailableSprite;

   [Header ("Audio")]
   public AudioClip exchangeSfx;

   [SerializeField] SkeletonGraphic npcSkeletonGrpahic;
   void Start () {
      openBoxButton.onClick.AddListener (OpenBox);
      rewardAnimator.AddClosedEvent (CloseBox);
   }
   
   public override void ShowMenu () {
        base.ShowMenu ();
        RefreshDisplay ();
        rewardAnimatorGift.gameObject.SetActive (true);
   }

   public override void HideMenu () {
    base.HideMenu ();
    
    rewardAnimatorGift.gameObject.SetActive (false);
   }

   void RefreshDisplay () {
      totalGift = SaveSystem.PlayerConfig.hotelGift;
      remainingGiftText.text = "Remaining : " + totalGift.ToString ();
      DestroyCloning ();

      for (int gift = 0; gift < maxGiftDisplay; gift++) {
         if (gift < totalGift) {
            GameObject cloneBox = GameObject.Instantiate (hotelGiftExchangePrefab);
            cloneBox.transform.SetParent (giftCollectionPanel);
            cloneBox.SetActive (true);
            if (listHotelGiftExchange.Count <6) {
               listHotelGiftExchange.Add (cloneBox);
            }
         }
      }

      IsOpenBoxAvailable ();

   }

   void DestroyCloning () {
      foreach (GameObject giftBox in listHotelGiftExchange) {
         Destroy (giftBox);
      }

      listHotelGiftExchange.Clear (); 
   }

   void OpenBox () {
      if (IsOpenBoxAvailable ()) {
         openBoxButton.gameObject.SetActive (false);
         rewardAnimator.OpenBox ();
         HotelGift.instance.UsingLoot (1);
         RefreshDisplay ();
         SfxExchange ();
         NpcThankyou ();
      }
   }

   void CloseBox () {
      openBoxButton.gameObject.SetActive (true);
   }

   bool IsOpenBoxAvailable () {
      if (totalGift >0) {
         openBoxButton.image.sprite = openBoxAvailableSprite;
         return true;
      } else {
         openBoxButton.image.sprite = openBoxNotAvailableSprite;
         return false;
      }
   }

   void NpcThankyou () {

        if (npcSkeletonGrpahic != null) {
            var state = npcSkeletonGrpahic.AnimationState;
            state.SetAnimation(0, "jumping", false);
            state.AddAnimation(0, "idle", true, 0f);
            npcSkeletonGrpahic.Update(0);
        }
   }

    #region Audio
   void SfxExchange () {
      ServiceLocator.Get<AudioManager> ().PlaySFX (exchangeSfx);
   }
   #endregion
}
