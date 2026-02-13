using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterCatalogueUI : MonoBehaviour
{
    [Header("UI Components")]
    public Button StoreBtn;
    public Button TypeBtn;
    public Button RenameGameAreaBtn;
    public Button SwitchGameAreaBtn;
    public Button DeleteGameAreaBtn;
    public Button CloseMonsterDetailsBtn;
    public CanvasGroup MonsterDetailsCanvasGroup;
    public Button CloseMonsterListBtn;
    public CanvasGroup MonsterListCanvasGroup;
    public Button monsterCollectionBtn;
    public CanvasGroup monsterCollectionCanvasGroup;
    public UISmoothFitter smoothFitter;
    public ScrollRect scrollRect;


    [Header("Game Area Components")]
    public Button[] gameAreaButtons;
    public Button AddGameAreaBtn;
    public GameObject gameAreaButtonPrefab;
    public Button seletectedGameAreaButton;
    public int selectedGameAreaIndex = -1;
    private int maxAreaVisible = 5;
    private float gameAreaHeight = 100;
    public RectTransform scrollViewContent;

    [Header("Game Area Button Highlighting")]
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = Color.gray;

    [Header("Object Pool")]
    private Queue<GameObject> gameAreaButtonPool = new Queue<GameObject>();
    private List<GameObject> activeGameAreaButtons = new List<GameObject>();
    private const int INITIAL_POOL_SIZE = 10;

    // Global player config variable
    private PlayerConfig playerConfig;
    private bool isPlayerConfigLoaded = false;

    private BiomeManager biomeManager;

    private void Start()
    {
        biomeManager = ServiceLocator.Get<BiomeManager>();
    }

    private void Awake()
    {
        if (StoreBtn == null || TypeBtn == null || RenameGameAreaBtn == null ||
            SwitchGameAreaBtn == null || CloseMonsterDetailsBtn == null ||
            MonsterDetailsCanvasGroup == null || CloseMonsterListBtn == null ||
            MonsterListCanvasGroup == null || smoothFitter == null)
        {
            Debug.LogError("One or more UI components are not assigned in the MonsterCatalogueUI.");
        }
        else
        {
            StartCoroutine(InitializeAsync());
        }

        ServiceLocator.Register(this);
    }

    private IEnumerator InitializeAsync()
    {
        // Load player config asynchronously first
        yield return StartCoroutine(LoadPlayerConfigAsync());

        // Initialize components after player config is loaded
        InitializeGameAreaButtonPool();
        Init();
        InitGameAreaButtons();
    }

    private IEnumerator LoadPlayerConfigAsync()
    {
        Debug.Log("Starting to load player config asynchronously...");

        // Yield for a frame to allow UI updates
        yield return null;

        // Simulate async loading time if needed
        yield return new WaitForSeconds(0.1f);

        // Load the player config and store it globally
        playerConfig = SaveSystem.GetPlayerConfig();

        if (playerConfig == null)
        {
            Debug.LogError("Failed to load player config!");
            isPlayerConfigLoaded = false;
            yield break;
        }

        isPlayerConfigLoaded = true;
        Debug.Log("Player config loaded successfully!");
    }

    public void Init()
    {
        smoothFitter = GetComponent<UISmoothFitter>();
        SetupListener();
    }

    private void SetupListener()
    {
        // Initialize button listeners
        StoreBtn.onClick.RemoveAllListeners();
        TypeBtn.onClick.RemoveAllListeners();
        RenameGameAreaBtn.onClick.RemoveAllListeners();
        SwitchGameAreaBtn.onClick.RemoveAllListeners();
        DeleteGameAreaBtn.onClick.RemoveAllListeners();
        CloseMonsterDetailsBtn.onClick.RemoveAllListeners();
        CloseMonsterListBtn.onClick.RemoveAllListeners();
        monsterCollectionBtn.onClick.RemoveAllListeners();

        StoreBtn.onClick.AddListener(OnStoreButtonClicked);
        TypeBtn.onClick.AddListener(OnTypeButtonClicked);
        RenameGameAreaBtn.onClick.AddListener(OnRenameGameAreaButtonClicked);
        SwitchGameAreaBtn.onClick.AddListener(OnSwitchGameAreaButtonClicked);
        DeleteGameAreaBtn.onClick.AddListener(OnDeleteGameAreaButtonClicked);
        CloseMonsterDetailsBtn.onClick.AddListener(() =>
        {
            MonsterDetailsCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                MonsterDetailsCanvasGroup.interactable = false;
                MonsterDetailsCanvasGroup.blocksRaycasts = false;
                smoothFitter.Kick();
            });
        });
        CloseMonsterListBtn.onClick.AddListener(() =>
        {
            MonsterListCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                MonsterListCanvasGroup.interactable = false;
                MonsterListCanvasGroup.blocksRaycasts = false;
                smoothFitter.Kick();
            });
        });
        monsterCollectionBtn.onClick.AddListener(() =>
        {
            monsterCollectionCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                monsterCollectionCanvasGroup.interactable = true;
                monsterCollectionCanvasGroup.blocksRaycasts = true;
                monsterCollectionCanvasGroup.transform.SetAsLastSibling();
            });
        });
    }

    private void OnStoreButtonClicked()
    {
        Debug.Log("Store button clicked.");
        var ui = ServiceLocator.Get<UIManager>();
        ui.FadePanel(ui.panels.ShopPanel, ui.panels.ShopCanvasGroup, true);
    }

    private void InitializeGameAreaButtonPool()
    {
        if (gameAreaButtonPrefab == null)
        {
            Debug.LogError("Game Area button prefab is not assigned.");
            return;
        }

        // Create initial pool of buttons
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            GameObject pooledButton = Instantiate(gameAreaButtonPrefab, scrollViewContent);
            pooledButton.SetActive(false);
            gameAreaButtonPool.Enqueue(pooledButton);
        }
        UpdateGameAreaUI();
    }

    private void UpdateGameAreaUI()
    {
        int count = gameAreaButtons.Length;

        scrollRect.enabled = count > maxAreaVisible;

        int visibleCount = Mathf.Min(count, maxAreaVisible);
        float height = visibleCount * gameAreaHeight;
        // Posisi tombol +
        RectTransform addButtonRect = AddGameAreaBtn.gameObject.GetComponent<RectTransform>();
        Vector2 pos = addButtonRect.anchoredPosition;
        pos.y = -(height - 30 * (visibleCount - 1));
        if (count == 1) pos.y = -(height);
        addButtonRect.anchoredPosition = pos;


    }

    private GameObject GetPooledGameAreaButton()
    {
        if (gameAreaButtonPool.Count > 0)
        {
            return gameAreaButtonPool.Dequeue();
        }
        else
        {

            // Pool is empty, create a new button
            return Instantiate(gameAreaButtonPrefab, scrollViewContent);

        }
    }

    private void ReturnButtonToPool(GameObject button)
    {
        button.SetActive(false);
        button.GetComponent<Button>().onClick.RemoveAllListeners();
        gameAreaButtonPool.Enqueue(button);
    }

    private void InitGameAreaButtons()
    {
        ClearActiveGameAreaButtons();
        StartCoroutine(PopulateGameAreaButtons());
    }

    private void ClearActiveGameAreaButtons()
    {
        // Return all active buttons to pool
        foreach (GameObject button in activeGameAreaButtons)
        {
            ReturnButtonToPool(button);
        }
        activeGameAreaButtons.Clear();

        // Clear the gameAreaButtons array
        if (gameAreaButtons != null)
        {
            for (int i = 0; i < gameAreaButtons.Length; i++)
            {
                gameAreaButtons[i] = null;
            }
        }
    }

    private IEnumerator PopulateGameAreaButtons()
    {
        // Wait until player config is loaded
        while (!isPlayerConfigLoaded)
        {
            yield return null;
        }

        yield return null; // Wait for the next frame to ensure UI updates

        int gameAreaCount = playerConfig.maxGameArea;

        // Resize gameAreaButtons array if needed
        if (gameAreaButtons == null || gameAreaButtons.Length < gameAreaCount)
        {
            gameAreaButtons = new Button[gameAreaCount];
        }

        for (int i = 0; i < gameAreaCount; i++)
        {
            // Get button from pool
            GameObject buttonObj = GetPooledGameAreaButton();
            buttonObj.SetActive(true);
            buttonObj.name = $"GameAreaButton_{i + 1}";

            // Get the saved name or use default
            string gameAreaName = GetSavedGameAreaName(i);
            if (string.IsNullOrEmpty(gameAreaName))
            {
                gameAreaName = $"Game Area {i + 1}";
            }

            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = gameAreaName;

            // Ensure the input field is hidden initially
            TMP_InputField inputField = buttonObj.GetComponentInChildren<TMP_InputField>(true);
            if (inputField != null)
            {
                inputField.gameObject.SetActive(false);
            }

            Button button = buttonObj.GetComponent<Button>();
            gameAreaButtons[i] = button;
            activeGameAreaButtons.Add(buttonObj);

            // Set the button position in hierarchy - use specific index for vertical layout
            int targetIndex = AddGameAreaBtn.transform.GetSiblingIndex() - 1;
            buttonObj.transform.SetSiblingIndex(targetIndex);
        }


        // Force layout rebuild after all buttons are positioned
        yield return null;
        SetupGameAreaButtonListeners();
        UpdateGameAreaUI(); // Update UI setelah buttons berhasil di-populate
    }

    private string GetSavedGameAreaName(int index)
    {
        if (!isPlayerConfigLoaded || playerConfig?.gameAreas == null || index >= playerConfig.gameAreas.Count)
        {
            return null;
        }
        return playerConfig.gameAreas[index].name;
    }

    private void SetupGameAreaButtonListeners()
    {
        if (AddGameAreaBtn == null)
        {
            Debug.LogError("Add Game Area button is not assigned.");
            return;
        }

        AddGameAreaBtn.onClick.RemoveAllListeners();
        AddGameAreaBtn.onClick.AddListener(() => AddGameArea());

        if (gameAreaButtons == null || gameAreaButtons.Length == 0)
        {
            Debug.LogError("Game Area buttons are not assigned or empty.");
            return;
        }

        for (int i = 0; i < gameAreaButtons.Length; i++)
        {
            if (gameAreaButtons[i] != null)
            {
                int index = i; // Capture the current index
                gameAreaButtons[i].onClick.RemoveAllListeners();
                gameAreaButtons[i].onClick.AddListener(() => OnGameAreaButtonClicked(index));
            }
        }
    }

    private void AddGameArea()
    {
        if (!isPlayerConfigLoaded || playerConfig == null)
        {
            Debug.LogError("Player config is not loaded!");
            return;
        }

        int newIndex = playerConfig.maxGameArea + 1;

        if (newIndex > 5)
        {
            Debug.Log("Game area is more than 5");
            TooltipManager.Instance.StartHoverForDuration("You already have maximum number of game areas.", 4.0f);
            return;
        }

        // Get button from pool
        GameObject newButtonObj = GetPooledGameAreaButton();
        newButtonObj.SetActive(true);
        newButtonObj.name = $"MyGameAreaBtn";
        newButtonObj.transform.SetSiblingIndex(AddGameAreaBtn.transform.GetSiblingIndex());
        newButtonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"Game Area {newIndex}";

        activeGameAreaButtons.Add(newButtonObj);

        playerConfig.maxGameArea++;
        playerConfig.gameAreas.Add(new GameAreaData
        {
            name = $"Game Area {newIndex}",
            index = newIndex - 1 // Index is zero-based
        });

        SaveSystem.SaveAll();
        UpdateGameAreaUI();
        // Refresh all buttons to update the array and listeners
        InitGameAreaButtons();

    }

    private void OnTypeButtonClicked()
    {
        // Logic for Type button click
        Debug.Log("Type button clicked.");
    }

    private void OnRenameGameAreaButtonClicked()
    {
        // Logic for Rename Game Area button click
        Debug.Log("Rename Game Area button clicked.");
        if (seletectedGameAreaButton != null)
        {
            int index = System.Array.IndexOf(gameAreaButtons, seletectedGameAreaButton);
            if (index >= 0 && index < gameAreaButtons.Length)
            {
                StartRenameMode(seletectedGameAreaButton, index);
            }
            else
            {
                Debug.LogWarning("Selected game area button index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("No game area button is selected.");
        }
    }

    private void StartRenameMode(Button gameAreaButton, int index)
    {
        // Find the TMP InputField as a child of the button
        TMP_InputField inputField = gameAreaButton.GetComponentInChildren<TMP_InputField>(true);
        TextMeshProUGUI buttonText = gameAreaButton.GetComponentInChildren<TextMeshProUGUI>();

        if (inputField == null)
        {
            Debug.LogError("TMP_InputField not found as child of game area button.");
            return;
        }

        if (buttonText == null)
        {
            Debug.LogError("TextMeshProUGUI not found as child of game area button.");
            return;
        }

        // Hide the button text and show the input field
        buttonText.gameObject.SetActive(false);
        inputField.gameObject.SetActive(true);

        // Set the current text as the input field value
        inputField.text = buttonText.text;

        // Focus the input field and select all text
        inputField.Select();
        inputField.ActivateInputField();

        // Remove any existing listeners to prevent duplicates
        inputField.onEndEdit.RemoveAllListeners();

        // Add listener for when editing is finished
        inputField.onEndEdit.AddListener((newName) => OnRenameComplete(gameAreaButton, index, newName, inputField, buttonText));

        // Add listener for when input field loses focus
        inputField.onDeselect.AddListener((value) => OnRenameComplete(gameAreaButton, index, value, inputField, buttonText));
    }

    private void OnRenameComplete(Button gameAreaButton, int index, string newName, TMP_InputField inputField, TextMeshProUGUI buttonText)
    {
        // Validate the new name
        if (string.IsNullOrWhiteSpace(newName))
        {
            newName = $"Game Area {index + 1}"; // Default name if empty
        }

        // Trim whitespace and limit length if needed
        newName = newName.Trim();
        if (newName.Length > 10) // Adjust max length as needed
        {
            newName = newName.Substring(0, 10);
        }

        // Update the button text
        buttonText.text = newName;

        // Hide input field and show button text
        inputField.gameObject.SetActive(false);
        buttonText.gameObject.SetActive(true);

        // Save the new name to your save system
        SaveGameAreaName(index, newName);

        // Remove listeners to prevent memory leaks
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onDeselect.RemoveAllListeners();

        Debug.Log($"Game Area {index + 1} renamed to: {newName}");
    }

    private void SaveGameAreaName(int index, string name)
    {
        if (!isPlayerConfigLoaded || playerConfig?.gameAreas == null || index >= playerConfig.gameAreas.Count)
        {
            Debug.LogError($"Cannot save game area name - invalid index {index} or player config not loaded");
            return;
        }

        playerConfig.gameAreas[index].name = name;
        SaveSystem.SaveAll();
        Debug.Log($"Saving Game Area {index + 1} name: {name}");
    }

    public void OnSwitchGameAreaButtonClicked()
    {
        // Logic for Switch Game Area button click
        Debug.Log("Switch Game Area button clicked.");
        if (seletectedGameAreaButton != null)
        {
            int index = System.Array.IndexOf(gameAreaButtons, seletectedGameAreaButton);
            if (index >= 0 && index < gameAreaButtons.Length)
            {
                // Switch to the selected game area
                Debug.Log($"Switching to Game Area {index}.");
                // Here you would typically call a method to switch the game area
                // GameAreaManager.SetActiveGameArea(index);
                ServiceLocator.Get<MonsterManager>().SwitchToGameArea(index);


                // reinitilize pumpkin
                ServiceLocator.Get<FacilityManager>().InitializePumpkinFacilityState();

                // change decoration active/inactive
                foreach (OwnedDecorationData ownedDecorationData in playerConfig.ownedDecorations)
                {
                    Debug.Log($"Checking {ownedDecorationData.decorationID} at area {playerConfig.lastGameAreaIndex}");
                    if (ownedDecorationData.isActive)
                    {
                        Debug.Log($"Apply decoration {ownedDecorationData.decorationID}");
                        ServiceLocator.Get<DecorationManager>()?.ApplyDecorationByID(ownedDecorationData.decorationID);
                        DecorationShopManager.instance.SetLastLoadTreeDecoration1(ownedDecorationData.decorationID);
                    }
                    else
                    {
                        Debug.Log($"Remove decoration {ownedDecorationData.decorationID}");
                        ServiceLocator.Get<DecorationManager>()?.RemoveActiveDecoration(ownedDecorationData.decorationID);
                    }

                    DecorationUIFixHandler.SetDecorationStats(ownedDecorationData.decorationID);
                }
            }
            else
            {
                Debug.LogWarning("Selected game area button index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("No game area button is selected.");
        }
    }

    private void OnDeleteGameAreaButtonClicked()
    {
        Debug.Log("Delete Game Area button clicked.");

        if (seletectedGameAreaButton != null)
        {
            int index = System.Array.IndexOf(gameAreaButtons, seletectedGameAreaButton);
            if (index >= 0 && index < gameAreaButtons.Length)
            {
                DeleteGameArea(index);
            }
            else
            {
                Debug.LogWarning("Selected game area button index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("No game area button is selected.");
        }
    }

    private void DeleteGameArea(int index)
    {
        if (!isPlayerConfigLoaded || playerConfig == null)
        {
            Debug.LogError("Player config is not loaded!");
            return;
        }

        // Prevent deleting if only one area remains
        if (playerConfig.maxGameArea <= 1)
        {
            Debug.LogWarning("Cannot delete the last game area! At least one area must exist.");
            return;
        }

        // Validate index
        if (index < 0 || index >= playerConfig.gameAreas.Count)
        {
            Debug.LogError($"Invalid game area index: {index}");
            return;
        }

        Debug.Log($"Deleting Game Area at index {index}: {playerConfig.gameAreas[index].name}");

        // Remove from player config
        playerConfig.gameAreas.RemoveAt(index);
        playerConfig.maxGameArea--;

        // Update indices for remaining areas
        for (int i = index; i < playerConfig.gameAreas.Count; i++)
        {
            playerConfig.gameAreas[i].index = i;
        }

        // Save changes
        SaveSystem.SaveAll();

        // Clear selection
        seletectedGameAreaButton = null;

        // Refresh UI
        InitGameAreaButtons();

        Debug.Log($"Game Area deleted successfully. Remaining areas: {playerConfig.maxGameArea}");
    }

    private void OnGameAreaButtonClicked(int index)
    {
        // Logic for when a game area button is clicked
        Debug.Log($"selected Game Area button at index {index} clicked.");
        SetSelectedGameAreaButton(index, true);
    }

    public void SetSelectedGameAreaButton(int index, bool notifyList)
    {
        if (gameAreaButtons == null || index < 0 || index >= gameAreaButtons.Length)
        {
            return;
        }

        Button targetButton = gameAreaButtons[index];
        if (targetButton == null)
        {
            return;
        }

        if (seletectedGameAreaButton == targetButton && selectedGameAreaIndex == index)
        {
            return;
        }

        // Clear previous selection
        ClearGameAreaButtonSelection();

        // Set new selection
        seletectedGameAreaButton = targetButton;
        selectedGameAreaIndex = index;
        HighlightSelectedGameAreaButton(targetButton);

        if (!notifyList)
        {
            return;
        }

        MonsterCatalogueListUI catalogueListUI = GetComponentInChildren<MonsterCatalogueListUI>();
        if (catalogueListUI != null)
        {
            catalogueListUI.OnGameAreaButtonClicked(index);
        }
        else
        {
            Debug.LogError("MonsterCatalogueListUI component not found in children.");
        }
    }

    public int GetSelectedGameAreaIndex()
    {
        return selectedGameAreaIndex;
    }

    private void ClearGameAreaButtonSelection()
    {
        // Reset all buttons to normal color
        foreach (var button in gameAreaButtons)
        {
            if (button != null)
            {
                SetButtonColor(button, normalButtonColor);
            }
        }
        seletectedGameAreaButton = null;
        selectedGameAreaIndex = -1;
    }

    private void HighlightSelectedGameAreaButton(Button button)
    {
        if (button != null)
        {
            SetButtonColor(button, selectedButtonColor);

            // Optional: Add a subtle scale animation
            button.transform.DOScale(1.05f, 0.1f).SetEase(Ease.OutQuad)
                .OnComplete(() => button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
        }
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }



}

[System.Serializable]
public class CatalogueMonsterData
{
    public string monsterID;
    public MonsterDataSO monsterData;
    public int evolutionLevel;
    public float currentHunger;
    public float currentHappiness;
    public float currentHealth;
    public int gameAreaId;
    public bool isNPC;

    // For save data
    public CatalogueMonsterData(MonsterSaveData saveData, MonsterDataSO data)
    {
        monsterID = saveData.instanceId;
        monsterData = data;
        evolutionLevel = saveData.currentEvolutionLevel;
        currentHunger = saveData.currentHunger;
        currentHappiness = saveData.currentHappiness;
        currentHealth = saveData.currentHealth;
        gameAreaId = saveData.gameAreaId;
        isNPC = false;
    }

    // For active monsters
    public CatalogueMonsterData(MonsterController controller)
    {
        monsterID = controller.monsterID;
        monsterData = controller.MonsterData;
        evolutionLevel = controller.evolutionLevel;
        currentHunger = controller.StatsHandler.CurrentHunger;
        currentHappiness = controller.StatsHandler.CurrentHappiness;
        currentHealth = controller.StatsHandler.CurrentHP;
        gameAreaId = ServiceLocator.Get<MonsterManager>().currentGameAreaIndex;
        isNPC = controller.isNPC;
    }

    // Utility methods
    public Sprite GetMonsterIcon(MonsterIconType iconType)
    {
        return monsterData.GetEvolutionIcon(evolutionLevel, iconType);
    }

    public int GetSellPrice()
    {
        return monsterData.GetSellPrice(evolutionLevel);
    }

    public float GetGoldCoinDropRate()
    {
        return monsterData.GetGoldCoinDropRate(evolutionLevel);
    }

    public string GetEvolutionStageName()
    {
        return monsterData.GetEvolutionStageName(evolutionLevel);
    }

    public MonsterType GetMonsterType()
    {
        return monsterData.monType;
    }
}