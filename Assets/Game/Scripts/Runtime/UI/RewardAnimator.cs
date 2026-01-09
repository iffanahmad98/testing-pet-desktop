using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
public class RewardAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private int minGoldReward = 150;
    [SerializeField] private int maxGoldReward = 500;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject vfxShiny;
    [SerializeField] Image rewardImage;
    [SerializeField] Sprite coinSprite;
    [SerializeField] Sprite goldenTicketSprite;
    [SerializeField] Vector3 coinScale, goldenTicketScale;
    Animator animator;

    public Action closeBoxEvent;
    [Header ("Data")]
    PlayerConfig playerConfig;

    [Header ("RewardConfiguration")]
    public List <RewardTypeConfig> listRewardTypeConfig = new ();
    [SerializeField] DecorationDatabaseSO decorationDatabase;

    [Serializable]
    public class RewardTypeConfig {
        public RewardType rewardType;
        public int weight;
        public bool included = true;
        public Rewardable [] rewardables; 
       
        public Rewardable GetRandomRewardable () {
            return rewardables[UnityEngine.Random.Range (0,rewardables.Length)];
        }

        public Rewardable GetSpecificRewardableById (string targetId) {
            return rewardables.FirstOrDefault(r => r.ItemId == targetId);
        }
    }

    public enum RewardType {
        Coin,
        FoodPack,
        Medicine,
        GoldenTicket,
        Decoration,
        Fertilizer,
    }

    void Start () {
        animator = GetComponentInChildren <Animator> ();
        closeButton.onClick.AddListener (CloseBox);
        closeButton.gameObject.SetActive (false);
        vfxShiny.gameObject.SetActive (false);
    }

   // HotelGiftExchangeMenu 
    public void OpenBox () {
        playerConfig = SaveSystem.PlayerConfig;

        vfxShiny.gameObject.SetActive (true);
        closeButton.interactable = false;
        closeButton.gameObject.SetActive (true);
        animator.SetTrigger ("Open");
        

        RewardTypeConfig rewardTypeConfig = GetRandomRewardTypeConfig();

        switch (rewardTypeConfig.rewardType)
        {
            case RewardType.Coin:
                RewardCoin ();
                break;
            case RewardType.FoodPack:
                RewardRewardable (rewardTypeConfig);
                break;
            case RewardType.Medicine:
                RewardRewardable (rewardTypeConfig);
                break;
            case RewardType.GoldenTicket:
                RewardGoldenTicket ();
                break;
            case RewardType.Decoration:
                string targetId = decorationDatabase.GetRandomAvailableDecorationSO ().decorationID;
                Rewardable reward = rewardTypeConfig.GetSpecificRewardableById (targetId);
                RewardRewardable (rewardTypeConfig, reward);
                break;
            case RewardType.Fertilizer:
                RewardRewardable (rewardTypeConfig);
                break;
        }
        
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

    #region Reward Configuration
    public RewardTypeConfig GetRandomRewardTypeConfig()
{
    RefreshIncluded();

    Debug.Log("=== Reward Pool Debug ===");

    int totalWeight = 0;

    foreach (RewardTypeConfig c in listRewardTypeConfig)
    {
        Debug.Log($"{c} | included={c.included} | weight={c.weight}");

        if (!c.included) continue;
        if (c.weight <= 0) continue;

        totalWeight += c.weight;
    }

    Debug.Log($"TotalWeight = {totalWeight}");

    if (totalWeight <= 0)
    {
        Debug.LogError("No included reward with weight > 0");
        return null;
    }

    int randomValue = UnityEngine.Random.Range(0, totalWeight);
    int currentWeight = 0;

    foreach (RewardTypeConfig c in listRewardTypeConfig)
    {
        if (!c.included || c.weight <= 0) continue;

        currentWeight += c.weight;

        if (randomValue < currentWeight)
            return c;
    }

    return null;
}



    void RefreshIncluded () {
        // Element 4 : Decoration
        if (playerConfig.GetTotalOwnedDecorations () == decorationDatabase.GetTotalAllDecorations ()) {
            listRewardTypeConfig[4].included = false;
        } else {
            listRewardTypeConfig[4].included = true;
        }

    }

    #endregion
    #region Reward Saving
    void RewardCoin () {
        int goldAmount = UnityEngine.Random.Range (minGoldReward, maxGoldReward);
        if (rewardAmountText != null)
        {
            rewardAmountText.text = goldAmount.ToString() + " Gold";
        }
        rewardImage.sprite = coinSprite;
        rewardImage.transform.localScale = coinScale;
        SaveReward(goldAmount);
        StartCoroutine(nCanClose());
    }

    void RewardGoldenTicket () {
        int amount = 1;
        if (rewardAmountText != null)
        {
            rewardAmountText.text = amount.ToString() + " Golden Ticket";
        }
        rewardImage.sprite = goldenTicketSprite;
         rewardImage.transform.localScale = goldenTicketScale;
        GoldenTicket.instance.GetLoot (amount);
        StartCoroutine(nCanClose());
    }

    void RewardRewardable (RewardTypeConfig rewardTypeConfig, Rewardable rewardable = null) {
        Rewardable itemReward = rewardable;
        if (!rewardable) {
            itemReward = rewardTypeConfig.GetRandomRewardable ();
        } else {
            itemReward = rewardable;
        }
        int amount = 1;
        if (rewardAmountText != null)
        {
            rewardAmountText.text = amount.ToString() + " " + itemReward.ItemName;
        }
        rewardImage.sprite = itemReward.RewardSprite;
        rewardImage.transform.localScale = itemReward.RewardScale;
        itemReward.RewardGotItem (amount);
        StartCoroutine(nCanClose());
    }
    #endregion
    #region Data
    public void SaveReward (int coinAmount) {
        CoinManager.AddCoins (coinAmount);
    }
    #endregion
}
