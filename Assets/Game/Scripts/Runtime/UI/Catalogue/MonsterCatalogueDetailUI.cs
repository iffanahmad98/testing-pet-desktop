using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class MonsterCatalogueDetailUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject CataloguePanel;
    public UISmoothFitter smoothFitter;
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
    public Button markFavoriteButton;
    public CatalogueMonsterData currentMonsterData;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        layoutElement.ignoreLayout = true;
        smoothFitter = CataloguePanel.GetComponent<UISmoothFitter>();
    }

    private void Start()
    {
        MonsterEvolutionHandler.OnMonsterEvolved += OnMonsterEvolved;
    }

    private void OnDestroy()
    {
        MonsterEvolutionHandler.OnMonsterEvolved -= OnMonsterEvolved;
    }

    public void SetDetails(CatalogueMonsterData catalogueMonsterData = null)
    {
        if (canvasGroup == null || monsterImage == null || monsterNameText == null ||
            monsterTypeText == null || monsterEvolutionText == null || monsterFullnessSlider == null ||
            monsterHappinessSlider == null || monsterEvolutionProgressSlider == null ||
            monsterSellPriceText == null || monsterEarningText == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the MonsterCatalogueDetailUI.");
            return;
        }

        if (catalogueMonsterData == null)
        {
            currentMonsterData = null;
            monsterImage.sprite = null;
            monsterNameText.text = string.Empty;
            // Hide the detail panel if no monster is provided
            smoothFitter.Kick();
            canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() => 
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                layoutElement.ignoreLayout = true; 
            });
            return;
        }
        else
        {
            currentMonsterData = catalogueMonsterData;
            // Ensure the detail panel is active before setting details
            canvasGroup.alpha = 0f; // Reset alpha to 0 before fading in
            smoothFitter.Kick();
            canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear).OnComplete(() => 
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                layoutElement.ignoreLayout = false; // Allow layout updates
            });

            // Set details using CatalogueMonsterData
            canvasGroup.alpha = 1f;
            monsterImage.sprite = catalogueMonsterData.GetMonsterIcon(MonsterIconType.Detail);
            monsterNameText.text = catalogueMonsterData.monsterData.monsterName;
            monsterTypeText.text = catalogueMonsterData.monsterData.monType.ToString();
            monsterEvolutionText.text = $"Stage {catalogueMonsterData.GetEvolutionStageName()}";
            monsterFullnessSlider.value = Mathf.Clamp01(catalogueMonsterData.currentHunger * 0.01f);
            monsterHappinessSlider.value = Mathf.Clamp01(catalogueMonsterData.currentHappiness * 0.01f);
            monsterEvolutionProgressSlider.value = (catalogueMonsterData.evolutionLevel - 1f) / 2f;
            monsterSellPriceText.text = $"{catalogueMonsterData.GetSellPrice()}";
            monsterEarningText.text = $"{(1 / catalogueMonsterData.GetGoldCoinDropRate() / 60).ToString("F2")} / MIN";
        }
    }

    private void OnMonsterEvolved(MonsterController evolvedMonster)
    {
        // If the evolved monster is currently displayed, refresh the details
        if (currentMonsterData != null && currentMonsterData.monsterID == evolvedMonster.monsterID)
        {
            SetDetails(new CatalogueMonsterData(evolvedMonster));
        }
    }
}
