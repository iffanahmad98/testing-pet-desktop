using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Image[] diamondImages;
    public Image[] evolStageImages;
    public CatalogueMonsterData currentMonsterData;
    public Button sellMonsterButton;
    public Button renameMonsterButton;

    public TMP_Text nameText;
    public Image innerBG;

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
    private void OnEnable()
    {
        sellMonsterButton.onClick.RemoveAllListeners();
        sellMonsterButton.onClick.AddListener(() => { SellMonster(); });

        renameMonsterButton.onClick.RemoveAllListeners();
        renameMonsterButton.onClick.AddListener(() => { RenameMonster(); });

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
            var playerConfig = SaveSystem.GetPlayerConfig();
            var found = playerConfig.ownedMonsters.Find(m => m.instanceId == currentMonsterData.monsterID);

            canvasGroup.alpha = 1f;
            monsterImage.sprite = catalogueMonsterData.GetMonsterIcon(MonsterIconType.Detail);
            monsterNameText.text = found.monsterId;
            monsterTypeText.text = catalogueMonsterData.monsterData.monType.ToString();
            monsterEvolutionText.text = $"Stage {catalogueMonsterData.GetEvolutionStageName()}";
            monsterFullnessSlider.value = Mathf.Clamp01(catalogueMonsterData.currentHunger * 0.01f);
            monsterHappinessSlider.value = Mathf.Clamp01(catalogueMonsterData.currentHappiness * 0.01f);
            monsterEvolutionProgressSlider.value = (catalogueMonsterData.evolutionLevel - 1f) / 2f;
            monsterSellPriceText.text = $"{catalogueMonsterData.GetSellPrice()}";
            monsterEarningText.text = $"{(1 / catalogueMonsterData.GetGoldCoinDropRate() / 60).ToString("F2")} / MIN";

            TMP_InputField inputField = renameMonsterButton.GetComponentInChildren<TMP_InputField>(true);
            inputField.text = monsterNameText.text;

            SetEvolutionDiamonds();
            SetEvolutionStages();
            SetEvolutionProgressBar();
        }
    }

    private void SetEvolutionDiamonds()
    {
        for (int i = 0; i < diamondImages.Length; i++)
        {
            // set all diamonds to unenabled first
            diamondImages[i].enabled = false;
        }

        Debug.Log("Current evolution level: " + currentMonsterData.evolutionLevel);
        for (int j = 0; j < diamondImages.Length; j++)
        {
            // then set the diamonds enabled based on evolution level
            if (j < currentMonsterData.evolutionLevel)
            {
                diamondImages[j].enabled = true;
            }
        }
    }

    private void SetEvolutionStages()
    {
        if (evolStageImages.Length < 4)
        {
            Debug.LogWarning("Evolution stage image is not enough.");
            return;
        }

        for (var i = 0; i < evolStageImages.Length; i++)
        {
            // pertama2 aktifkan semua imagenya
            evolStageImages[i].gameObject.SetActive(true);
        }

        MonsterType monsterType = currentMonsterData.GetMonsterType();

        switch (monsterType)
        {
            case MonsterType.Common:
            case MonsterType.Uncommon:
            case MonsterType.Rare:
                // nonaktifkan teks stage bloom di tengah
                evolStageImages[1].gameObject.SetActive(false);

                // nonaktifkan teks stage flourish
                evolStageImages[2].gameObject.SetActive(false);
                break;
            case MonsterType.Mythic:
            case MonsterType.Legend:
                // nonaktifkan teks stage bloom di kanan
                evolStageImages[3].gameObject.SetActive(false);
                break;
        }
    }

    private void SetEvolutionProgressBar()
    {
        MonsterType monsterType = currentMonsterData.GetMonsterType();

        switch (monsterType)
        {
            case MonsterType.Common:
            case MonsterType.Uncommon:
            case MonsterType.Rare:
                monsterEvolutionProgressSlider.value = (currentMonsterData.evolutionLevel - 1f) / 1f;
                break;
            case MonsterType.Mythic:
            case MonsterType.Legend:
                monsterEvolutionProgressSlider.value = (currentMonsterData.evolutionLevel - 1f) / 2f;
                break;
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

    private void RenameMonster()
    {
        if (currentMonsterData == null)
        {
            Debug.LogWarning("No monster to rename");
            return;
        }

        TMP_InputField inputField = renameMonsterButton.GetComponentInChildren<TMP_InputField>(true);
        
        if (inputField == null)
        {
            Debug.LogError("TMP_InputField not found as child of game area button.");
            return;
        }

        MonsterManager.instance.audio.PlaySFX("button_click");

        // aktifkan text dan inner background
        nameText.gameObject.SetActive(true);
        innerBG.gameObject.SetActive(true);

        // show the input field
        inputField.gameObject.SetActive(true);

        // Set the current text as the input field value
        string oldName = inputField.text;
        inputField.text = "Rename";

        // Focus the input field and select all text
        inputField.Select();
        inputField.ActivateInputField();

        // Remove any existing listeners to prevent duplicates
        inputField.onEndEdit.RemoveAllListeners();

        // Add listener for when editing is finished
        inputField.onEndEdit.AddListener((newName) => OnRenameComplete(renameMonsterButton, newName, inputField, oldName));

        // Add listener for when input field loses focus
        inputField.onDeselect.AddListener((value) => OnRenameComplete(renameMonsterButton, value, inputField, oldName));
    }

    private void OnRenameComplete(Button gameAreaButton, string newName, TMP_InputField inputField, string oldName)
    {
        // Validate the new name
        if (string.IsNullOrWhiteSpace(newName) || newName == "Rename")
        {
            newName = oldName; // Default name if empty
        }

        // Trim whitespace and limit length if needed
        newName = newName.Trim();
        if (newName.Length > 15) // Adjust max length as needed
        {
            newName = newName.Substring(0, 15);
        }

        monsterNameText.text = newName;

        inputField.gameObject.SetActive(false);
        // aktifkan text dan inner background
        nameText.gameObject.SetActive(false);
        innerBG.gameObject.SetActive(false);

        // save the game
        var playerConfig = SaveSystem.GetPlayerConfig();
        var found = playerConfig.ownedMonsters.Find(m => m.instanceId == currentMonsterData.monsterID);
        found.monsterId = newName;
        playerConfig.SaveMonsterData(found);
        
        // Remove listeners to prevent memory leaks
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onDeselect.RemoveAllListeners();
    }

    private void SellMonster()
    {
        if (currentMonsterData == null)
        {
            Debug.LogWarning("No monster data to sell!");
            return;
        }

        MonsterManager monsterManager = ServiceLocator.Get<MonsterManager>();
        MonsterController monsterToSell = monsterManager.activeMonsters
            .Find(m => m.monsterID == currentMonsterData.monsterID);

        if (monsterToSell != null)
        {
            monsterManager.SellMonster(currentMonsterData.monsterData);
            monsterManager.DespawnToPool(monsterToSell.gameObject);
            monsterManager.RemoveSavedMonsterID(currentMonsterData.monsterID);
            SaveSystem.DeleteMon(currentMonsterData.monsterID);
            ServiceLocator.Get<MonsterCatalogueListUI>()?.RefreshCatalogue();
            SetDetails(null);


        }
        else
        {
            Debug.LogWarning($"Monster with ID {currentMonsterData.monsterID} not found in active monsters!");
        }
    }

}
