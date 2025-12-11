using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
public class RewardAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private int minGoldReward = 20;
    [SerializeField] private int maxGoldReward = 100;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject vfxShiny;
    Animator animator;
    public Action closeBoxEvent;
    void Start () {
        animator = GetComponentInChildren <Animator> ();
        closeButton.onClick.AddListener (CloseBox);
        closeButton.gameObject.SetActive (false);
        vfxShiny.gameObject.SetActive (false);
    }

   // HotelGiftExchangeMenu 
    public void OpenBox () {
        vfxShiny.gameObject.SetActive (true);
        closeButton.interactable = false;
        closeButton.gameObject.SetActive (true);
        animator.SetTrigger ("Open");
        int goldAmount = UnityEngine.Random.Range (minGoldReward, maxGoldReward);
        if (rewardAmountText != null)
        {
            rewardAmountText.text = goldAmount.ToString() + " GOLD";
        }
        SaveReward (goldAmount);
        StartCoroutine (nCanClose ());
    }

    IEnumerator nCanClose () {
        yield return new WaitForSeconds (2.0f); // tunggu animasi buka.
        closeButton.interactable = true;
    }

    public void CloseBox () {
        vfxShiny.gameObject.SetActive (false);
        closeButton.gameObject.SetActive (false);
        Debug.Log ("Close Box");
        animator.SetTrigger("Hide");
        closeBoxEvent?.Invoke ();
    }

    // HotelGiftExchangeMenu
    public void AddClosedEvent (Action action) { 
        closeBoxEvent += action;
    }

    #region Data
    public void SaveReward (int coinAmount) {
        CoinManager.AddCoins (coinAmount);
    }
    #endregion
}
