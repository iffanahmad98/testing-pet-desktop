using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemDatabaseSO itemDatabase;
    public ItemDatabaseSO ItemDatabase => itemDatabase;

    [Header("Dropdown Panel (Quick View)")]
    [SerializeField] private GameObject quickViewGameObject;
    [SerializeField] private Transform dropdownContentParent;
    [SerializeField] private RectTransform dropdownContentRect;
    [SerializeField] private Button dropdownLeftScrollButton;
    [SerializeField] private Button dropdownRightScrollButton;

    [Header("Horizontal Bar")]
    [SerializeField] private GameObject horizontalBarGameObject;
    [SerializeField] private Transform horizontalContentParent;
    [SerializeField] private RectTransform horizontalContentRect;
    [SerializeField] private Button horizontalLeftScrollButton;
    [SerializeField] private Button horizontalRightScrollButton;
    [SerializeField] private Button horizontalDownScrollButton;

    [Header("Full Inventory Panel (Vertical Scroll)")]
    [SerializeField] private GameObject verticalContentGameObject;
    [SerializeField] private Transform verticalContentParent;
    [SerializeField] private RectTransform verticalContentRect;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button closeButton;

    [Header("General Settings")]
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private int slotsPerRow = 6;
    [SerializeField] private float slotHeight = 110f;
    [SerializeField] private float rowSpacing = 10f;

    // Object Pool for ItemSlotUI
    private Queue<ItemSlotUI> slotPool = new Queue<ItemSlotUI>();
    private List<ItemSlotUI> activeSlots = new List<ItemSlotUI>();
    private int initialPoolSize = 20;

    private bool isDeleteMode = false;
    public bool IsInDeleteMode => isDeleteMode;

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeSlotPool();
    }

    private void InitializeSlotPool()
    {
        // Create initial pool of slot objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            var slot = Instantiate(slotPrefab);
            slot.gameObject.SetActive(false);
            slotPool.Enqueue(slot);
        }
    }

    private ItemSlotUI GetSlotFromPool()
    {
        if (slotPool.Count > 0)
        {
            var slot = slotPool.Dequeue();
            slot.gameObject.SetActive(true);
            return slot;
        }
        else
        {
            // Create new slot if pool is empty
            return Instantiate(slotPrefab);
        }
    }

    private void ReturnSlotToPool(ItemSlotUI slot)
    {
        if (slot != null)
        {
            slot.gameObject.SetActive(false);
            slot.transform.SetParent(null);
            slotPool.Enqueue(slot);
        }
    }

    private void ReturnAllSlotsToPool()
    {
        foreach (var slot in activeSlots)
        {
            ReturnSlotToPool(slot);
        }
        activeSlots.Clear();
    }

    private void Start()
    {
        SetupNavigationButtons();
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        storeButton.onClick.AddListener(OnStoreButtonClicked);
    }

    private void OnEnable()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => ServiceLocator.Get<UIManager>().FadePanel(verticalContentGameObject, verticalContentGameObject.GetComponent<CanvasGroup>(), false));
        StartPopulateAllInventories();
    }

    private void OnDisable()
    {
        quickViewGameObject.SetActive(true);
        horizontalBarGameObject.SetActive(false);
        ReturnAllSlotsToPool();
    }

    private void SetupNavigationButtons()
    {
        dropdownLeftScrollButton.onClick.AddListener(() =>
        {
            quickViewGameObject.SetActive(false);
            horizontalBarGameObject.SetActive(true);
        });

        dropdownRightScrollButton.onClick.AddListener(() =>
            ServiceLocator.Get<UIManager>().FadePanel(this.gameObject, this.GetComponent<CanvasGroup>(), false));

        horizontalLeftScrollButton.onClick.AddListener(() =>
        {
            quickViewGameObject.SetActive(true);
            horizontalBarGameObject.SetActive(false);
        });

        horizontalDownScrollButton.onClick.AddListener(() =>
        {
            horizontalBarGameObject.SetActive(false);
            ServiceLocator.Get<UIManager>().FadePanel(verticalContentGameObject, verticalContentGameObject.GetComponent<CanvasGroup>(), true);
        });

        horizontalRightScrollButton.onClick.AddListener(() =>
            ServiceLocator.Get<UIManager>().FadePanel(this.gameObject, this.GetComponent<CanvasGroup>(), false));
    }

    public void StartPopulateAllInventories()
    {
        StartCoroutine(PopulateAllInventoriesCoroutine());
    }

    private IEnumerator PopulateAllInventoriesCoroutine()
    {
        yield return new WaitForEndOfFrame(); // Ensure UI is ready
        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;

        if (ownedItems == null)
        {
            Debug.LogError("❌ ownedItems is null.");
            yield break;
        }

        if (ownedItems.Count == 0)
            Debug.Log("ℹ️ No items in inventory.");

        var sortedItems = SortItemsByCategory(ownedItems);

        yield return PopulateInventory(dropdownContentParent, dropdownContentRect, sortedItems, 6, 2); // Quick View: Max 6 items, 2 rows
        yield return PopulateInventory(horizontalContentParent, horizontalContentRect, sortedItems, 10, 1); // Horizontal: Max 10 items
        yield return PopulateInventory(verticalContentParent, verticalContentRect, sortedItems, ownedItems.Count, Mathf.CeilToInt((float)sortedItems.Count / slotsPerRow)); // Full: All items
    }

    private List<OwnedItemData> SortItemsByCategory(List<OwnedItemData> items)
    {
        var ordered = new List<OwnedItemData>();
        ItemType[] order = { ItemType.Food, ItemType.Medicine, ItemType.Poop };

        foreach (var type in order)
        {
            foreach (var item in items)
            {
                var data = itemDatabase.GetItem(item.itemID);
                if (data != null && data.category == type)
                    ordered.Add(item);
            }
        }
        return ordered;
    }

    private IEnumerator PopulateInventory(Transform parent, RectTransform rect, List<OwnedItemData> allItems, int maxSlots, int maxRows)
    {
        // Return slots from this parent to pool
        ReturnSlotsFromParent(parent);
        yield return null;

        var displayItems = allItems.GetRange(0, Mathf.Min(maxSlots, allItems.Count));

        if (rect != null && parent == verticalContentParent)
        {
            float height = maxRows * (slotHeight + rowSpacing);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        foreach (var item in displayItems)
        {
            var itemData = itemDatabase.GetItem(item.itemID);
            if (itemData == null) continue;

            var slot = GetSlotFromPool();
            slot.transform.SetParent(parent, false);
            slot.Initialize(itemData, item.type, item.amount);
            activeSlots.Add(slot);
            yield return null;
        }
    }

    private void ReturnSlotsFromParent(Transform parent)
    {
        var slotsToReturn = new List<ItemSlotUI>();
        
        foreach (var slot in activeSlots)
        {
            if (slot.transform.parent == parent)
            {
                slotsToReturn.Add(slot);
            }
        }

        foreach (var slot in slotsToReturn)
        {
            activeSlots.Remove(slot);
            ReturnSlotToPool(slot);
        }
    }

    // === Delete Mode Logic ===

    private void OnDeleteButtonClicked()
    {
        if (isDeleteMode)
            ExitDeleteMode();
        else
            EnterDeleteMode();
    }

    private void EnterDeleteMode()
    {
        isDeleteMode = true;
        Debug.Log("🗑 Delete Mode Activated. Tap an item to remove it.");
        // Optional: Highlight slots
    }

    private void ExitDeleteMode()
    {
        isDeleteMode = false;
        Debug.Log("❌ Delete Mode Canceled.");
        // Optional: Unhighlight slots
    }

    public void ConfirmDeleteItem(ItemSlotUI slot)
    {
        if (!isDeleteMode || slot == null) return;

        SaveSystem.PlayerConfig.ownedItems.RemoveAll(x => x.itemID == slot.ItemDataSO.itemID);
        SaveSystem.SaveAll();

        Debug.Log($"✅ Deleted item: {slot.ItemDataSO.itemName}");
        ExitDeleteMode();
        StartPopulateAllInventories();
    }

    private void OnStoreButtonClicked()
    {
        ServiceLocator.Get<UIManager>().FadePanel(verticalContentGameObject, verticalContentGameObject.GetComponent<CanvasGroup>(), false);
        ServiceLocator.Get<UIManager>().FadePanel(ServiceLocator.Get<UIManager>().ShopPanel, ServiceLocator.Get<UIManager>().ShopCanvasGroup, true);
    }

    private void OnDestroy()
    {
        // Clean up pool when object is destroyed
        while (slotPool.Count > 0)
        {
            var slot = slotPool.Dequeue();
            if (slot != null)
                Destroy(slot.gameObject);
        }

        foreach (var slot in activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
    }
}