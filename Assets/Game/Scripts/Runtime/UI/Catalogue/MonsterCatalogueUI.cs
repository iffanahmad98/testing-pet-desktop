using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterCatalogueUI : MonoBehaviour
{
    [Header("Button Components")]
    public Button StoreBtn;
    public Button TypeBtn;
    public Button RenameGameAreaBtn;
    public Button SwitchGameAreaBtn;
    public Button CloseMonsterDetailsBtn;
    public CanvasGroup MonsterDetailsCanvasGroup;
    public Button CloseMonsterListBtn;
    public CanvasGroup MonsterListCanvasGroup;
    public UISmoothFitter smoothFitter;

    [Header("Game Area Components")]
    public Button[] gameAreaButtons;
    public Button AddGameAreaBtn;
    public GameObject gameAreaButtonPrefab;
    private Button seletectedGameAreaButton;
    
    [Header("Game Area Button Highlighting")]
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = Color.gray;
    
    [Header("Object Pool")]
    private Queue<GameObject> gameAreaButtonPool = new Queue<GameObject>();
    private List<GameObject> activeGameAreaButtons = new List<GameObject>();
    private const int INITIAL_POOL_SIZE = 10;

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
            InitializeGameAreaButtonPool();
            Init();
            InitGameAreaButtons();
        }

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
        CloseMonsterDetailsBtn.onClick.RemoveAllListeners();
        CloseMonsterListBtn.onClick.RemoveAllListeners();

        // StoreBtn.onClick.AddListener(OnStoreButtonClicked);
        TypeBtn.onClick.AddListener(OnTypeButtonClicked);
        RenameGameAreaBtn.onClick.AddListener(OnRenameGameAreaButtonClicked);
        SwitchGameAreaBtn.onClick.AddListener(OnSwitchGameAreaButtonClicked);
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
            GameObject pooledButton = Instantiate(gameAreaButtonPrefab, AddGameAreaBtn.transform.parent);
            pooledButton.SetActive(false);
            gameAreaButtonPool.Enqueue(pooledButton);
        }
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
            return Instantiate(gameAreaButtonPrefab, AddGameAreaBtn.transform.parent);
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
        yield return null; // Wait for the next frame to ensure UI updates

        int gameAreaCount = SaveSystem.GetPlayerConfig().maxGameArea;

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
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"Game Area {i + 1}";

            Button button = buttonObj.GetComponent<Button>();
            gameAreaButtons[i] = button;
            activeGameAreaButtons.Add(buttonObj);

            // Set the button position in hierarchy - use specific index for vertical layout
            int targetIndex = AddGameAreaBtn.transform.GetSiblingIndex() - 1;
            buttonObj.transform.SetSiblingIndex(targetIndex);
        }

        // Force layout rebuild after all buttons are positioned
        yield return null;
        // LayoutRebuilder.ForceRebuildLayoutImmediate(AddGameAreaBtn.transform.parent.GetComponent<RectTransform>());
        SetupGameAreaButtonListeners();
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
        int newIndex = SaveSystem.GetPlayerConfig().maxGameArea + 1;

        // Get button from pool
        GameObject newButtonObj = GetPooledGameAreaButton();
        newButtonObj.SetActive(true);
        newButtonObj.name = $"MyGameAreaBtn";
        newButtonObj.transform.SetSiblingIndex(AddGameAreaBtn.transform.GetSiblingIndex());
        newButtonObj.GetComponentInChildren<TextMeshProUGUI>().text = $"Game Area {newIndex}";

        activeGameAreaButtons.Add(newButtonObj);

        SaveSystem.GetPlayerConfig().maxGameArea++;
        SaveSystem.SaveAll();

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
                // Rename the selected game area
                string newName = "New Game Area Name"; // Replace with actual input logic
                Debug.Log($"Renaming Game Area {index} to '{newName}'.");
                // Here you would typically call a method to rename the game area
                // GameAreaManager.RenameGameArea(index, newName);
                gameAreaButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = newName; // Assuming the button has a TextMeshProUGUI component
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

    private void OnSwitchGameAreaButtonClicked()
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

    private void OnGameAreaButtonClicked(int index)
    {
        // Logic for when a game area button is clicked
        Debug.Log($"selected Game Area button at index {index} clicked.");
        
        // Clear previous selection
        ClearGameAreaButtonSelection();
        
        // Set new selection
        seletectedGameAreaButton = gameAreaButtons[index];
        HighlightSelectedGameAreaButton(gameAreaButtons[index]);

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
}