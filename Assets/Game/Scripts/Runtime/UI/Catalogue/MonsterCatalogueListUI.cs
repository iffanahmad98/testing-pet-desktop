using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class MonsterCatalogueListUI : MonoBehaviour
{
    public ScrollRect scrollRect;
    public GameObject itemPrefab;
    
    [SerializeField] private int initialPoolSize = 50;
    
    private MonsterManager monsterManager;
    private Queue<GameObject> itemPool = new Queue<GameObject>();
    private List<GameObject> activeItems = new List<GameObject>();
    private MonsterCatalogueItemUI currentSelectedItem;

    private void Start()
    {
        monsterManager = ServiceLocator.Get<MonsterManager>();
        InitializeItemPool();
        PopulateCatalogue();
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

    private void PopulateCatalogue()
    {
        // Return any existing items to the pool first
        ReturnAllItemsToPool();
        
        // Check if monsterManager is initialized
        if (monsterManager == null)
        {
            monsterManager = ServiceLocator.Get<MonsterManager>();
            
            // If still null, log error and return
            if (monsterManager == null)
            {
                Debug.LogError("MonsterManager is null in MonsterCatalogueListUI.PopulateCatalogue()");
                return;
            }
        }
        
        int itemIndex = 0;
        int totalSlots = 20; // Total number of slots to display
        
        // 1. First populate with active monsters
        if (monsterManager.activeMonsters != null)
        {
            foreach (var monster in monsterManager.activeMonsters)
            {
                // Skip null monsters
                if (monster == null) continue;
                
                GameObject itemGO = GetPooledItem();
                if (itemGO == null) continue;
                
                itemGO.SetActive(true);
                
                var itemUI = itemGO.GetComponent<MonsterCatalogueItemUI>();
                
                if (itemUI != null)
                {
                    itemUI.SetupItem(monster, MonsterCatalogueItemType.Monster);
                }
                
                itemIndex++;
            }
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
        PopulateCatalogue();
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

    private void OnEnable()
    {
        // Make sure monsterManager is set
        if (monsterManager == null)
        {
            monsterManager = ServiceLocator.Get<MonsterManager>();
        }

        // Refresh catalogue when enabled
        RefreshCatalogue();
    }

    private void OnDestroy()
    {
        // Clear references
        itemPool.Clear();
        activeItems.Clear();
    }
}
