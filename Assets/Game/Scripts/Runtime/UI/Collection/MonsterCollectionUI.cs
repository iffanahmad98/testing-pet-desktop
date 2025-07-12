using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Linq;

public class MonsterCollectionUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeButton;
    public Button catalogueButton;

    [Header("UI Components")]
    public CanvasGroup monsterCatalogueCanvasGroup;
    public CanvasGroup monsterCollectionCanvasGroup;
    public CanvasGroup monsterCollectionInfo;
    private RectTransform monsterCollectionInfoRect;
    private TextMeshProUGUI monsterCollectionInfoText;

    public List<MonsterCollection> monsterCollections = new List<MonsterCollection>();
    public List<MonsterSaveData> ownedMonsters = new List<MonsterSaveData>();
    public List<MonsterCollectionItemUI> monsterCollectionItems = new List<MonsterCollectionItemUI>();
    private void Awake()
    {
        // Start the coroutine to get PlayerConfig
        StartCoroutine(TryGetPlayerConfig());
        // Initialize listeners
        InitListeners();
        // Get the CanvasGroup component
        monsterCollectionCanvasGroup = GetComponent<CanvasGroup>();
        monsterCollectionInfoRect = monsterCollectionInfo.GetComponent<RectTransform>();
        monsterCollectionInfoText = monsterCollectionInfo.GetComponentInChildren<TextMeshProUGUI>();
    }

    private IEnumerator TryGetPlayerConfig()
    {
        // Wait until PlayerConfig is available
        yield return null;
        // Now we can safely access PlayerConfig
        ownedMonsters = SaveSystem.GetPlayerConfig().ownedMonsters;
        // Import monsters after getting the data
        if (ownedMonsters == null || ownedMonsters.Count == 0)
        {
            Debug.LogWarning("No owned monsters found in PlayerConfig.");
            yield break;
        }
        ImportOwnedMonstersToCollection();
    }

    private void ImportOwnedMonstersToCollection()
    {
        // Clear existing collection
        monsterCollections.Clear();

        // Dictionary to count monsters by ID
        Dictionary<string, int> monsterCounts = new Dictionary<string, int>();
        // Change this to track all unlocked evolution levels, not just the highest
        Dictionary<string, HashSet<int>> monsterUnlockedEvolutions = new Dictionary<string, HashSet<int>>();

        // Count each monster and track all evolution levels that have been unlocked
        foreach (var ownedMonster in ownedMonsters)
        {
            if (monsterCounts.ContainsKey(ownedMonster.monsterId))
            {
                monsterCounts[ownedMonster.monsterId]++;
            }
            else
            {
                monsterCounts[ownedMonster.monsterId] = 1;
                monsterUnlockedEvolutions[ownedMonster.monsterId] = new HashSet<int>();
            }
            
            // Add this evolution level to the unlocked set
            monsterUnlockedEvolutions[ownedMonster.monsterId].Add(ownedMonster.currentEvolutionLevel);
        }

        // Convert counted monsters to MonsterCollection
        foreach (var kvp in monsterCounts)
        {
            // Get the highest evolution level for display purposes
            int highestEvolution = monsterUnlockedEvolutions[kvp.Key].Max();
            
            MonsterCollection collection = new MonsterCollection
            {
                monsterId = kvp.Key,
                monsterName = kvp.Key,
                monsterCount = kvp.Value.ToString(),
                evolutionLevel = highestEvolution,
                // Add a new field to track all unlocked evolutions
                unlockedEvolutions = monsterUnlockedEvolutions[kvp.Key].ToList()
            };

            monsterCollections.Add(collection);
        }

        // Update UI items after importing
        UpdateMonsterCollectionItemsUI();
    }

    private void UpdateMonsterCollectionItemsUI()
    {
        // Clear the existing list
        monsterCollectionItems.Clear();
        
        // Get all MonsterCollectionItemUI components from children
        MonsterCollectionItemUI[] childItems = GetComponentsInChildren<MonsterCollectionItemUI>();
        
        // Add them to our list
        monsterCollectionItems.AddRange(childItems);
        
        // Create dictionaries for owned monster data
        Dictionary<string, List<int>> ownedMonsterEvolutions = new Dictionary<string, List<int>>();
        Dictionary<string, int> ownedMonsterCounts = new Dictionary<string, int>();
        
        // Populate dictionaries with owned monster data
        foreach (var collection in monsterCollections)
        {
            ownedMonsterEvolutions[collection.monsterId] = collection.unlockedEvolutions;
            ownedMonsterCounts[collection.monsterId] = int.Parse(collection.monsterCount);
        }
        
        // Update each MonsterCollectionItemUI (all start as locked by default)
        foreach (var itemUI in monsterCollectionItems)
        {
            string monsterId = itemUI.GetMonsterId();
            
            // Check if this monster is owned
            if (ownedMonsterEvolutions.ContainsKey(monsterId) && ownedMonsterCounts[monsterId] > 0)
            {
                // Monster is owned, unlock it and set data
                itemUI.SetUnlocked(true);
                itemUI.SetUnlockedEvolutions(ownedMonsterEvolutions[monsterId]);
                itemUI.SetMonsterCount(ownedMonsterCounts[monsterId]);
            }
            else
            {
                // Monster is not owned, keep it locked (default state)
                itemUI.SetUnlocked(false);
                itemUI.SetUnlockedEvolutions(new List<int>()); // Empty list
                itemUI.SetMonsterCount(0);
            }
        }
    }

    private void InitListeners()
    {
        // Remove existing listeners to prevent duplicates
        closeButton.onClick.RemoveAllListeners();
        catalogueButton.onClick.RemoveAllListeners();
        // Add new listeners
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        catalogueButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        monsterCollectionCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            monsterCollectionCanvasGroup.interactable = false;
            monsterCollectionCanvasGroup.blocksRaycasts = false;
        });
    }

    public void OnShowInfo(float x, int count, int evolutionLevel = 1)
    {
        monsterCollectionInfo.DOFade(1f, 0.2f).SetEase(Ease.Linear);
        monsterCollectionInfoRect.position = new Vector2(x, monsterCollectionInfoRect.position.y);
        monsterCollectionInfoText.text = count.ToString();
        monsterCollectionInfoRect.GetChild(0).transform.GetChild(evolutionLevel - 1).gameObject.SetActive(true);
    }

    internal void OnHideInfo()
    {
        monsterCollectionInfo.DOFade(0f, 0.2f).SetEase(Ease.Linear);
        monsterCollectionInfoRect.position = new Vector2(0, monsterCollectionInfoRect.position.y);
        foreach (Transform child in monsterCollectionInfoRect.GetChild(0).transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class MonsterCollection
{
    public string monsterId;
    public string monsterName;
    public string monsterCount;
    public int evolutionLevel; // Highest evolution level for display
    public List<int> unlockedEvolutions = new List<int>(); // All unlocked evolution levels
}
