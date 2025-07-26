using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainInventoryUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button storeBtn;
    public Button deleteBtn;
    
    [Header("Inventory Setup")]
    public ScrollRect scrollRect;
    public GameObject itemPrefab; // This should be ItemSlotUI prefab
    public ItemDatabaseSO itemDatabase;
    
    [Header("Pooling Config")]
    [SerializeField] private int poolSize = 20;
    
    private Queue<ItemSlotUI> itemSlotPool = new Queue<ItemSlotUI>();
    private List<ItemSlotUI> activeSlots = new List<ItemSlotUI>();
    private Transform contentParent;

    private void Awake()
    {
        // Get references
        contentParent = scrollRect.content;
        
        // Initialize pool and listeners
        InitializePool();
        InitListeners();
    }

    private void OnEnable()
    {
        StartPopulateInventory();
    }

    private void InitializePool()
    {
        // Create pool of ItemSlotUI objects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject slotObj = Instantiate(itemPrefab, contentParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            
            if (slot == null)
            {
                Debug.LogError("itemPrefab must have ItemSlotUI component!");
                return;
            }
            
            slotObj.SetActive(false);
            itemSlotPool.Enqueue(slot);
        }
    }

    private void InitListeners()
    {
        storeBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.RemoveAllListeners();

        storeBtn?.onClick.AddListener(OnStoreButtonClicked);
        deleteBtn?.onClick.AddListener(OnDeleteButtonClicked);
    }

    public void StartPopulateInventory()
    {
        StartCoroutine(PopulateInventoryCoroutine());
    }

    private IEnumerator PopulateInventoryCoroutine()
    {
        // Return all active slots to pool
        ReturnAllSlotsToPool();

        yield return null; // Wait a frame for UI clearing

        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;

        if (ownedItems == null)
        {
            Debug.LogError("ownedItems is null. Aborting inventory population.");
            yield break;
        }

        if (ownedItems.Count == 0)
        {
            Debug.LogWarning("No items found in inventory.");
            yield break;
        }

        // Populate slots from pool
        foreach (var entry in ownedItems)
        {
            ItemDataSO itemData = itemDatabase.GetItem(entry.itemID);
            
            if (itemData != null)
            {
                ItemSlotUI slot = GetSlotFromPool();
                if (slot != null)
                {
                    slot.gameObject.SetActive(true);
                    slot.Initialize(itemData, entry.type, entry.amount);
                    activeSlots.Add(slot);
                }
                else
                {
                    Debug.LogWarning("Pool exhausted! Consider increasing pool size.");
                    break;
                }
            }

            // Optional: yield to avoid UI stutter if many items
            yield return null;
        }
    }

    private ItemSlotUI GetSlotFromPool()
    {
        if (itemSlotPool.Count > 0)
        {
            return itemSlotPool.Dequeue();
        }
        
        // If pool is empty, create new slot (emergency expansion)
        GameObject slotObj = Instantiate(itemPrefab, contentParent);
        ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
        
        if (slot == null)
        {
            Debug.LogError("itemPrefab must have ItemSlotUI component!");
            Destroy(slotObj);
            return null;
        }
        
        return slot;
    }

    private void ReturnSlotToPool(ItemSlotUI slot)
    {
        if (slot != null)
        {
            slot.gameObject.SetActive(false);
            slot.transform.SetAsLastSibling(); // Move to end for organization
            itemSlotPool.Enqueue(slot);
            activeSlots.Remove(slot);
        }
    }

    private void ReturnAllSlotsToPool()
    {
        // Return all active slots to pool
        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            ReturnSlotToPool(activeSlots[i]);
        }
        activeSlots.Clear();
    }

    // Public method to refresh inventory (call this when items change)
    public void RefreshInventory()
    {
        StartPopulateInventory();
    }

    // Method to handle item removal from inventory
    public void OnItemRemoved(ItemSlotUI slot)
    {
        ReturnSlotToPool(slot);
    }

    private void OnStoreButtonClicked()
    {
        // Handle store button click
        Debug.Log("Store button clicked");
        ServiceLocator.Get<UIManager>().FadePanel(ServiceLocator.Get<UIManager>().ShopPanel, ServiceLocator.Get<UIManager>().ShopCanvasGroup, true);
    }

    private void OnDeleteButtonClicked()
    {
        // Handle delete button click
        Debug.Log("Delete button clicked");
    }
}
