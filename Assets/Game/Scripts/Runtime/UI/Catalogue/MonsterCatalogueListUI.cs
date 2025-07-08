using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Collections;

public class MonsterCatalogueListUI : MonoBehaviour
{
    public ScrollRect scrollRect;
    public GameObject itemPrefab;

    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int currentViewingGameArea = 0; // Default to first game area

    private MonsterManager monsterManager;
    private Queue<GameObject> itemPool = new Queue<GameObject>();
    private List<GameObject> activeItems = new List<GameObject>();
    private MonsterCatalogueItemUI currentSelectedItem;

    private void Start()
    {
        monsterManager = ServiceLocator.Get<MonsterManager>();
        InitializeItemPool();
        RefreshCatalogue();
    }

    private void InitializeItemPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePoolItem();
        }
    }

    private void CreatePoolItem()
    {
        GameObject item = Instantiate(itemPrefab, scrollRect.content);
        item.SetActive(false);
        itemPool.Enqueue(item);
    }

    private GameObject GetPooledItem()
    {
        if (itemPool.Count == 0)
        {
            CreatePoolItem();
        }

        GameObject item = itemPool.Dequeue();
        activeItems.Add(item);
        return item;
    }

    private void ReturnAllItemsToPool()
    {
        foreach (var item in activeItems)
        {
            item.SetActive(false);
            itemPool.Enqueue(item);
        }

        activeItems.Clear();
    }

    private IEnumerator PopulateCatalogue()
    {
        // Return any existing items to the pool first
        ReturnAllItemsToPool();
        
        yield return null; // Wait for the next frame to ensure UI is ready
        
        // Check if monsterManager is initialized
        if (monsterManager == null)
        {
            monsterManager = ServiceLocator.Get<MonsterManager>();

            // If still null, log error and return
            if (monsterManager == null)
            {
                Debug.LogError("MonsterManager is null in MonsterCatalogueListUI.PopulateCatalogue()");
                yield break;
            }
        }
        
        int itemIndex = 0;
        int totalSlots = 20; // Total number of slots to display
        
        // 1. Get monster data for the specific game area
        List<CatalogueMonsterData> areaMonsters = GetMonstersForGameArea(currentViewingGameArea);
        
        // Populate with monsters from the selected area
        foreach (var monsterData in areaMonsters)
        {
            // Skip null monsters
            if (monsterData == null) continue;
            
            GameObject itemGO = GetPooledItem();
            if (itemGO == null) continue;
            
            itemGO.SetActive(true);
            
            var itemUI = itemGO.GetComponent<MonsterCatalogueItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetupItem(monsterData, MonsterCatalogueItemType.Monster);
            }
            
            itemIndex++;
        }
        
        // 2. Add unlocked (empty) slots up to maxMonsterSlots
        int emptySlots = Mathf.Max(0, monsterManager.maxMonstersSlots - itemIndex);
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject itemGO = GetPooledItem();
            itemGO.SetActive(true);

            var itemUI = itemGO.GetComponent<MonsterCatalogueItemUI>();

            if (itemUI != null)
            {
                itemUI.SetupItem(null, MonsterCatalogueItemType.Unlocked);
            }

            itemIndex++;
        }

        // 3. Add the "add monster" button right after maxMonsterSlots
        if (itemIndex < totalSlots)
        {
            GameObject itemGO = GetPooledItem();
            itemGO.SetActive(true);

            var itemUI = itemGO.GetComponent<MonsterCatalogueItemUI>();

            if (itemUI != null)
            {
                itemUI.SetupItem(null, MonsterCatalogueItemType.Add);
            }

            itemIndex++;
        }

        // 4. Fill remaining slots with locked slots
        while (itemIndex < totalSlots)
        {
            GameObject itemGO = GetPooledItem();
            itemGO.SetActive(true);

            var itemUI = itemGO.GetComponent<MonsterCatalogueItemUI>();

            if (itemUI != null)
            {
                itemUI.SetupItem(null, MonsterCatalogueItemType.Locked);
            }

            itemIndex++;
        }
    }

    // Public method to refresh the catalogue if needed
    public void RefreshCatalogue()
    {
        StartCoroutine(PopulateCatalogue());
    }

    public void SelectItem(MonsterCatalogueItemUI newSelectedItem)
    {
        // Deselect previous item if exists
        if (currentSelectedItem != null && currentSelectedItem != newSelectedItem)
        {
            currentSelectedItem.SetSelected(false);
        }

        // Update current selection
        currentSelectedItem = newSelectedItem;
    }

    // Add this method to your MonsterCatalogueListUI class
    public void SetNextLockedItemToAdd()
    {
        // Find the first locked item
        MonsterCatalogueItemUI nextLockedItem = null;

        // Assuming you have a list of catalogue items
        foreach (var item in activeItems)
        {
            // Check if this is a locked item
            if (item.GetComponent<MonsterCatalogueItemUI>().GetItemType() == MonsterCatalogueItemType.Locked)
            {
                nextLockedItem = item.GetComponent<MonsterCatalogueItemUI>();
                break;
            }
        }

        // If we found a locked item, convert it to "Add"
        if (nextLockedItem != null)
        {
            nextLockedItem.SetItemType(MonsterCatalogueItemType.Add);
        }

        // Optionally refresh the UI
        RefreshCatalogue();
    }

    public void OnGameAreaButtonClicked(int index)
    {
        Debug.Log($"Game Area Button {index} clicked");

        // Update the current viewing area
        currentViewingGameArea = index;

        // Refresh the catalogue to show monsters from the selected area
        RefreshCatalogue();
    }

    // Update this helper method to return CatalogueMonsterData instead of MonsterController
    private List<CatalogueMonsterData> GetMonstersForGameArea(int gameAreaIndex)
    {
        List<CatalogueMonsterData> areaMonsters = new List<CatalogueMonsterData>();
        
        // If viewing the current active area, show active monsters
        if (gameAreaIndex == monsterManager.currentGameAreaIndex)
        {
            foreach (var controller in monsterManager.activeMonsters)
            {
                if (controller != null)
                {
                    areaMonsters.Add(new CatalogueMonsterData(controller));
                }
            }
        }
        else
        {
            // Load saved monsters for the specified area
            var savedMonsterIDs = SaveSystem.LoadMonIDs();
            
            foreach (var id in savedMonsterIDs)
            {
                if (SaveSystem.LoadMon(id, out MonsterSaveData saveData))
                {
                    // Only include monsters from the specified game area
                    if (saveData.gameAreaId == gameAreaIndex)
                    {
                        var (monsterData, evolutionLevel) = GetMonsterDataAndLevelFromID(id);
                        if (monsterData != null)
                        {
                            areaMonsters.Add(new CatalogueMonsterData(saveData, monsterData));
                        }
                    }
                }
            }
        }
        
        return areaMonsters;
    }

    // Helper method to parse monster ID (copied from MonsterManager)
    private (MonsterDataSO, int) GetMonsterDataAndLevelFromID(string monsterID)
    {
        var parts = monsterID.Split('_');
        if (parts.Length >= 2)
        {
            string monsterTypeId = parts[0];
            string levelPart = parts[1];
            int evolutionLevel = 0;

            if (levelPart.StartsWith("Stage"))
            {
                if (!int.TryParse(levelPart.Substring(5), out evolutionLevel))
                {
                    evolutionLevel = 0;
                }
            }

            foreach (var data in monsterManager.monsterDatabase.monsters)
            {
                if (data.id == monsterTypeId)
                {
                    return (data, evolutionLevel);
                }
            }
        }

        return (null, 0);
    }
}


