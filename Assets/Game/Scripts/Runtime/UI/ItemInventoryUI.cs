using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

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

    // Add these new variables for drag & drop
    private Dictionary<Transform, List<OwnedItemData>> parentToItemsMap = new Dictionary<Transform, List<OwnedItemData>>();
    private bool isReordering = false;

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
        ItemSlotUI slot;
        if (slotPool.Count > 0)
        {
            slot = slotPool.Dequeue();
            slot.gameObject.SetActive(true);
        }
        else
        {
            // Create new slot if pool is empty
            slot = Instantiate(slotPrefab);
        }

        // Don't reset here - it's wasteful since Initialize will set everything
        return slot;
    }

    private void ReturnSlotToPool(ItemSlotUI slot)
    {
        if (slot != null)
        {
            // Remove from active slots list safely
            if (activeSlots.Contains(slot))
                activeSlots.Remove(slot);

            // ✅ Kill any running tweens BEFORE resetting
            slot.transform.DOKill();
            if (slot.GetComponent<CanvasGroup>() != null)
                slot.GetComponent<CanvasGroup>().DOKill();

            slot.ResetSlot();
            slot.gameObject.SetActive(false);
            slot.transform.SetParent(null);

            // Reset transform
            slot.transform.localScale = Vector3.one;
            slot.transform.rotation = Quaternion.identity;
            slot.transform.position = Vector3.zero;

            slotPool.Enqueue(slot);
        }
    }

    private void ReturnAllSlotsToPool()
    {
        // ✅ Create a copy of the list to avoid modification during iteration
        var slotsToReturn = new List<ItemSlotUI>(activeSlots);

        // Kill all tweens before returning to pool
        foreach (var slot in slotsToReturn)
        {
            if (slot != null)
            {
                slot.transform.DOKill();
                if (slot.GetComponent<CanvasGroup>() != null)
                    slot.GetComponent<CanvasGroup>().DOKill();
            }
        }

        // Now safely return all slots to pool
        foreach (var slot in slotsToReturn)
        {
            ReturnSlotToPool(slot);
        }

        // Clear the active slots list
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
        // ✅ Kill all active tweens when inventory is disabled
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                slot.transform.DOKill();
                if (slot.GetComponent<CanvasGroup>() != null)
                    slot.GetComponent<CanvasGroup>().DOKill();
            }
        }

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

        // Quick View: 3 Food, 2 Medicine, 1 Poop
        yield return PopulateInventoryByType(dropdownContentParent, dropdownContentRect, sortedItems,
            foodMax: 3, medicineMax: 2, poopMax: 1, rows: 2);

        // Horizontal Bar: 7 Food, 2 Medicine, 1 Poop
        yield return PopulateInventoryByType(horizontalContentParent, horizontalContentRect, sortedItems,
            foodMax: 7, medicineMax: 2, poopMax: 1, rows: 1);

        // Full Inventory: show all
        yield return PopulateInventory(verticalContentParent, verticalContentRect, sortedItems,
            sortedItems.Count, Mathf.CeilToInt((float)sortedItems.Count / slotsPerRow));

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
        // Store the items for this parent
        var displayItems = allItems.GetRange(0, Mathf.Min(maxSlots, allItems.Count));
        parentToItemsMap[parent] = displayItems;

        // Return slots from this parent to pool
        ReturnSlotsFromParent(parent);
        yield return null;

        if (rect != null && parent == verticalContentParent)
        {
            float height = maxRows * (slotHeight + rowSpacing);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        // Add smooth population with slight delay for better UX
        for (int i = 0; i < displayItems.Count; i++)
        {
            var item = displayItems[i];
            var itemData = itemDatabase.GetItem(item.itemID);
            if (itemData == null) continue;

            var slot = GetSlotFromPool();
            slot.transform.SetParent(parent, false);
            slot.Initialize(itemData, item.type, item.amount);
            activeSlots.Add(slot);

            // Small delay for smooth population
            if (i % 5 == 0) // Every 5 items
                yield return new WaitForSeconds(0.01f);
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
    private IEnumerator PopulateInventoryByType(Transform parent, RectTransform rect, List<OwnedItemData> allItems,
    int foodMax, int medicineMax, int poopMax, int rows)
    {
        // Clear UI
        foreach (Transform child in parent)
            Destroy(child.gameObject);
        yield return null;

        var foodItems = new List<OwnedItemData>();
        var medicineItems = new List<OwnedItemData>();
        var poopItems = new List<OwnedItemData>();

        foreach (var item in allItems)
        {
            var data = itemDatabase.GetItem(item.itemID);
            if (data == null) continue;

            switch (data.category)
            {
                case ItemType.Food: if (foodItems.Count < foodMax) foodItems.Add(item); break;
                case ItemType.Medicine: if (medicineItems.Count < medicineMax) medicineItems.Add(item); break;
                case ItemType.Poop: if (poopItems.Count < poopMax) poopItems.Add(item); break;
            }
        }

        var displayItems = new List<OwnedItemData>();
        displayItems.AddRange(foodItems);
        displayItems.AddRange(medicineItems);
        displayItems.AddRange(poopItems);

        if (rect != null && parent == verticalContentParent)
        {
            float height = rows * (slotHeight + rowSpacing);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        foreach (var item in displayItems)
        {
            var itemData = itemDatabase.GetItem(item.itemID);
            if (itemData == null) continue;

            var slot = Instantiate(slotPrefab, parent);
            slot.Initialize(itemData, item.type, item.amount);
            yield return null;
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

        // Visual feedback for delete mode
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                var bg = slot.GetComponent<Image>();
                if (bg != null)
                    bg.color = new Color(1f, 0.5f, 0.5f, 0.3f); // Red tint
            }
        }
    }

    private void ExitDeleteMode()
    {
        isDeleteMode = false;
        Debug.Log("❌ Delete Mode Canceled.");

        // Reset visual feedback
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                var bg = slot.GetComponent<Image>();
                if (bg != null)
                    bg.color = Color.clear;
            }
        }
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

    public void MoveItemBack(ItemSlotUI draggedSlot, ItemSlotUI targetSlot)
    {
        if (isReordering) return;

        StartCoroutine(MoveItemBackCoroutine(draggedSlot, targetSlot));
    }

    private IEnumerator MoveItemBackCoroutine(ItemSlotUI draggedSlot, ItemSlotUI targetSlot)
    {
        isReordering = true;

        // Find the items in the saved data
        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;
        if (ownedItems == null)
        {
            isReordering = false;
            yield break;
        }

        var draggedItem = ownedItems.FirstOrDefault(x => x.itemID == draggedSlot.ItemDataSO.itemID);
        var targetItem = ownedItems.FirstOrDefault(x => x.itemID == targetSlot.ItemDataSO.itemID);

        if (draggedItem == null || targetItem == null)
        {
            isReordering = false;
            yield break;
        }

        // Get indices
        int draggedIndex = ownedItems.IndexOf(draggedItem);
        int targetIndex = ownedItems.IndexOf(targetItem);

        if (draggedIndex == -1 || targetIndex == -1)
        {
            isReordering = false;
            yield break;
        }

        // Move item back by 1 position (insert before target)
        ownedItems.RemoveAt(draggedIndex);

        // Adjust target index if dragged item was before target
        if (draggedIndex < targetIndex)
            targetIndex--;

        // Insert dragged item before target (moving it back 1 position)
        ownedItems.Insert(targetIndex, draggedItem);

        // Save the changes
        SaveSystem.SaveAll();

        // Visual feedback BEFORE returning to pool
        ShowMoveBackFeedback(draggedSlot, targetSlot);

        // Wait for animation to complete
        yield return new WaitForSeconds(0.3f);

        // ✅ Kill any remaining tweens before returning to pool
        if (draggedSlot != null)
        {
            draggedSlot.transform.DOKill();
            if (draggedSlot.GetComponent<CanvasGroup>() != null)
                draggedSlot.GetComponent<CanvasGroup>().DOKill();
        }
        if (targetSlot != null)
        {
            targetSlot.transform.DOKill();
            if (targetSlot.GetComponent<CanvasGroup>() != null)
                targetSlot.GetComponent<CanvasGroup>().DOKill();
        }

        // Return both slots to pool after animation
        ReturnSlotToPool(draggedSlot);
        ReturnSlotToPool(targetSlot);

        // Wait a frame before repopulating
        yield return new WaitForEndOfFrame();

        // Repopulate all inventories with new order
        StartPopulateAllInventories();

        isReordering = false;
    }

    private void ShowMoveBackFeedback(ItemSlotUI draggedSlot, ItemSlotUI targetSlot)
    {
        // Play move animation for dragged item
        if (draggedSlot != null)
            draggedSlot.PlayMoveBackAnimation();

        // Play subtle highlight for target slot
        if (targetSlot != null)
            targetSlot.PlayTargetHighlightAnimation();
    }

    private void OnDestroy()
    {
        // ✅ Kill all tweens before destroying
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                slot.transform.DOKill();
                if (slot.GetComponent<CanvasGroup>() != null)
                    slot.GetComponent<CanvasGroup>().DOKill();
                Destroy(slot.gameObject);
            }
        }

        // Clean up pool when object is destroyed
        while (slotPool.Count > 0)
        {
            var slot = slotPool.Dequeue();
            if (slot != null)
            {
                slot.transform.DOKill();
                if (slot.GetComponent<CanvasGroup>() != null)
                    slot.GetComponent<CanvasGroup>().DOKill();
                Destroy(slot.gameObject);
            }
        }
    }

    // Add method to check if reordering is in progress
    public bool IsReordering => isReordering;

    // Add this method to handle dragged items properly
    public void HandleDraggedSlot(ItemSlotUI slot)
    {
        if (slot != null)
        {
            // Reset the slot's state
            slot.ResetSlot();

            // Remove from active slots if it exists
            if (activeSlots.Contains(slot))
                activeSlots.Remove(slot);

            // Return to pool
            ReturnSlotToPool(slot);
        }
    }

    public void HandleItemDepletion(ItemSlotUI slot)
    {
        if (slot != null)
        {
            // Remove from active slots
            if (activeSlots.Contains(slot))
                activeSlots.Remove(slot);

            // Return to pool instead of destroying
            ReturnSlotToPool(slot);

            // Refresh inventories
            StartPopulateAllInventories();
        }
    }
}