using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class MonsterCatalogueDetailUI : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup canvasGroup;
    public LayoutElement layoutElement;
    public Image monsterImage;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI monsterTypeText;
    public TextMeshProUGUI monsterEvolutionText;
    public Slider monsterFullnessSlider;
    public Slider monsterHappinessSlider;
    public Slider monsterEvolutionProgressSlider;
    public TextMeshProUGUI monsterSellPriceText;
    public TextMeshProUGUI monsterEarningText;
    public GameObject[] monsterEvolutionProgressImg;
    public Button closeButton;
    public Button markFavoriteButton;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false; 
        layoutElement.ignoreLayout = true; 
    }

    public void SetDetails(MonsterController monsterController = null)
    {
        if (canvasGroup == null || monsterImage == null || monsterNameText == null ||
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
            canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => 
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                layoutElement.ignoreLayout = true; 
            });
            return;
        }
        else
        {
            // Ensure the detail panel is active before setting details
            canvasGroup.alpha = 0f; // Reset alpha to 0 before fading in
            canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => 
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                layoutElement.ignoreLayout = false; // Allow layout updates
            });

            var _evolveLvl = monsterController.MonsterData.evolutionLevel;

            // Example setup, replace with actual data retrieval logic
            canvasGroup.alpha = 1f;
            monsterImage.sprite = monsterController.GetEvolutionIcon(MonsterIconType.Detail); // Set the sprite for the monster
            monsterNameText.text = monsterController.MonsterData.monsterName;
            monsterTypeText.text = monsterController.MonsterData.monType.ToString();
            monsterEvolutionText.text = $"Stage {monsterController.MonsterData.GetEvolutionStageName(_evolveLvl)}";
            monsterFullnessSlider.value = monsterController.StatsHandler.CurrentHunger; //hunger or fullness nutrition going to ask later
            monsterHappinessSlider.value = monsterController.StatsHandler.CurrentHappiness;
            monsterEvolutionProgressSlider.value = (_evolveLvl - 1f) / monsterController.MonsterData.evolutionRequirements.Length;
            monsterSellPriceText.text = $"{monsterController.MonsterData.GetSellPrice(_evolveLvl)}";
            monsterEarningText.text = $"{(1 / monsterController.MonsterData.GetGoldCoinDropRate(_evolveLvl) / 60).ToString("F2")} / MIN";
        }
    }
    
}
