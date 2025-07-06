using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class MonsterCatalogueDetailUI : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup monsterDetailPanel;
    public Image monsterImage;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI monsterTypeText;
    public TextMeshProUGUI monsterEvolutionText;
    public Slider monsterFullnessSlider;
    public Slider monsterHappinessSlider;
    public Slider monsterEvolutionProgressSlider;
    public TextMeshProUGUI monsterSellPriceText;
    public TextMeshProUGUI monsterEarningText;

    public void SetDetails(MonsterController monsterController = null)
    {
        if (monsterDetailPanel == null || monsterImage == null || monsterNameText == null ||
            monsterTypeText == null || monsterEvolutionText == null || monsterFullnessSlider == null ||
            monsterHappinessSlider == null || monsterEvolutionProgressSlider == null ||
            monsterSellPriceText == null || monsterEarningText == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the MonsterCatalogueDetailUI.");
            return;
        }

        if (monsterController == null)
        {
            // Hide the detail panel if no monster is provided
            return;
        }
        else
        {
            var _evolveLvl = monsterController.MonsterData.evolutionLevel;

            // Example setup, replace with actual data retrieval logic
            monsterDetailPanel.alpha = 1f;
            monsterImage.sprite = monsterController.GetEvolutionIcon(MonsterIconType.Detail); // Set the sprite for the monster
            monsterNameText.text = monsterController.MonsterData.monsterName;
            monsterTypeText.text = monsterController.MonsterData.monType.ToString();
            monsterEvolutionText.text = $"Evolution Level: {_evolveLvl}";
            monsterFullnessSlider.value = monsterController.StatsHandler.CurrentHunger; //hunger or fullness nutrition going to ask later
            monsterHappinessSlider.value = monsterController.StatsHandler.CurrentHappiness;
            monsterEvolutionProgressSlider.value = (float)_evolveLvl / (monsterController.MonsterData.evolutionRequirements.Length + 1);
            monsterSellPriceText.text = $"{monsterController.MonsterData.GetSellPrice(_evolveLvl)}";
            monsterEarningText.text = $"{1 / monsterController.MonsterData.GetGoldCoinDropRate(_evolveLvl)} hourly";
        }
    }
    
}
