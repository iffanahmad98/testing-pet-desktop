using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private Button cartButton;

    [Header("Horizontal Bar")]
    [SerializeField] private GameObject horizontalBarGameObject;
    [SerializeField] private Transform horizontalContentParent;
    [SerializeField] private RectTransform horizontalContentRect;
    [SerializeField] private Button horizontalLeftScrollButton;
    [SerializeField] private Button horizontalRightScrollButton;
    [SerializeField] private Button horizontalDownScrollButton;
    [Header("Shop Under Inventory")]
    [SerializeField] private Transform shopInventoryContentParent;
    [SerializeField] private RectTransform shopInventoryContentRect;

    [Header("Full Inventory Panel (Vertical Scroll)")]
    [SerializeField] private GameObject verticalContentGameObject;
    [SerializeField] private Transform verticalContentParent;
    public Transform VerticalContentParent => verticalContentParent;
    [SerializeField] private RectTransform verticalContentRect;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button closeButton;

    [Header("General Settings")]
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private int slotsPerRow = 6;
    [SerializeField] private float slotHeight = 110f;
    [SerializeField] private float rowSpacing = 10f;
    [Header("Delete Confirmation")]
    [SerializeField] private GameObject deleteConfirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationMessageText;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;


    // Object Pool for ItemSlotUI
    public Queue<ItemSlotUI> slotPool = new Queue<ItemSlotUI>();
    public List<ItemSlotUI> activeSlots = new List<ItemSlotUI>();
    public List<ItemSlotUI> fullActiveSlots = new();
    private int initialPoolSize = 50;

    private bool isDeleteMode = false;
    public bool IsInDeleteMode => isDeleteMode;
    private Dictionary<ItemSlotUI, int> pendingDeleteMap = new();


    // Add these new variables for drag & drop
    private Dictionary<Transform, List<OwnedItemData>> parentToItemsMap = new Dictionary<Transform, List<OwnedItemData>>();
    private bool isReordering = false;
    bool refreshWhenOpen = false;
    public List<ItemSlotUI> ActiveSlots => activeSlots;

    private void Awake()
    {
        InitializeSlotPool();
        ServiceLocator.Register(this);
    }

    private void InitializeSlotPool()
    {
        // Create initial pool of slot objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            var slot = Instantiate(slotPrefab, verticalContentParent);
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
        confirmDeleteButton.onClick.AddListener(HandleConfirmedDelete);
        cancelDeleteButton.onClick.AddListener(() =>
        {
            SetCanvasGroupVisibility(deleteConfirmationPanel, false);
            ExitDeleteMode();
        });

    }

    bool oncePopulate = false;
    private void OnEnable()
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            HideInventory();
            ResetInventoryGroupvisibility();
            ExitDeleteMode();
        });

        if (!oncePopulate)
        {
            oncePopulate = true;
            StartPopulateAllInventories();
        }

        if (refreshWhenOpen)
        {
            refreshWhenOpen = false;
            StartPopulateAllInventories();
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance.isQuitting) { return; }
        if (!this.gameObject.activeInHierarchy) { return; }
        /*
       foreach (var slot in activeSlots) {
        Debug.Log ("Destroy Active Slots");
            Destroy (slot.gameObject);
        }
        */
        foreach (ItemSlotUI itemSlot in activeSlots)
        {
            ReturnSlotToPool(itemSlot);
            //  Debug.Log ("Destroy 0.8x");
        }

        activeSlots.Clear();

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
        //    horizontalBarGameObject.SetActive(false); (Non Used ini bikin bug)
        SetCanvasGroupVisibility(horizontalBarGameObject, false);
        SetCanvasGroupVisibility(cartButton.gameObject, false);
        HideInventory();
        ResetInventoryGroupvisibility();

        //  ReturnAllSlotsToPool();
    }
    private void SetCanvasGroupVisibility(GameObject target, bool show)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null) group = target.AddComponent<CanvasGroup>();

        group.alpha = show ? 1f : 0f;
        group.interactable = show;
        group.blocksRaycasts = show;
    }


    private void SetupNavigationButtons()
    {
        dropdownLeftScrollButton.onClick.AddListener(() =>
        {
            SetCanvasGroupVisibility(quickViewGameObject, false);
            SetCanvasGroupVisibility(horizontalBarGameObject, true);
        });

        dropdownRightScrollButton.onClick.AddListener(() =>
        {
            HideInventory();
            ResetInventoryGroupvisibility();
        });


        horizontalLeftScrollButton.onClick.AddListener(() =>
        {
            SetCanvasGroupVisibility(quickViewGameObject, true);
            SetCanvasGroupVisibility(horizontalBarGameObject, false);
        });

        horizontalDownScrollButton.onClick.AddListener(() =>
        {
            SetCanvasGroupVisibility(horizontalBarGameObject, false);
            SetCanvasGroupVisibility(cartButton.gameObject, false);
            ServiceLocator.Get<UIManager>().FadePanel(verticalContentGameObject, verticalContentGameObject.GetComponent<CanvasGroup>(), true);
        });

        horizontalRightScrollButton.onClick.AddListener(() =>
        {
            HideInventory();
            ResetInventoryGroupvisibility();
        });

        cartButton.onClick.AddListener(() =>
        {
            HideInventory();
            ResetInventoryGroupvisibility();
        });
    }

    public void StartPopulateAllInventories()
    {
        StartCoroutine(PopulateAllInventoriesCoroutine());
        //  Debug.Log ("Destroy Slot 0.3x ");
        StartCoroutine(PopulateShopInventoryCoroutine());
    }

    private IEnumerator PopulateAllInventoriesCoroutine()
    {


        //  Debug.Log ("Start Populate");
        // ✅ Kill all active tweens when inventory is disabled


        yield return new WaitForEndOfFrame(); // Ensure UI is ready
        ClearAllUnusedDatas();
        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;

        if (ownedItems == null)
        {
            Debug.LogError("❌ ownedItems is null.");
            yield break;
        }

        if (ownedItems.Count == 0)
        {
            Debug.LogWarning("ℹ️ No items in inventory.");
            yield break;
        }

        var sortedItems = SortItemsByCategory(ownedItems);

        // Quick View: 3 Food, 2 Medicine, 1 Poop
        yield return PopulateInventoryByType(dropdownContentParent, dropdownContentRect, sortedItems,
            foodMax: 3, medicineMax: 2, poopMax: 1, rows: 2);

        // Horizontal Bar: 7 Food, 2 Medicine, 1 Poop
        yield return PopulateInventoryByType(horizontalContentParent, horizontalContentRect, sortedItems,
            foodMax: 9, medicineMax: 2, poopMax: 1, rows: 1);

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

        foreach (ItemSlotUI itemSlot in fullActiveSlots)
        {
            // Destroy (itemSlot.gameObject);
            ReturnSlotToPool(itemSlot);
            //  Debug.Log ("Destroy Slot");
        }

        fullActiveSlots.Clear();

        // Add smooth population with slight delay for better UX
        for (int i = 0; i < displayItems.Count; i++)
        {
            var item = displayItems[i];
            var itemData = itemDatabase.GetItem(item.itemID);
            if (itemData == null) continue;

            var slot = GetSlotFromPool();
            slot.transform.SetParent(parent, false);
            slot.Initialize(itemData, item.type, item.amount);
            fullActiveSlots.Add(slot);
            // Small delay for smooth population
            if (i % 5 == 0) // Every 5 items
                yield return new WaitForSeconds(0.01f);
        }

    }
    public void StartPopulateShopInventory()
    {
        StartCoroutine(PopulateShopInventoryCoroutine());
    }

    private IEnumerator PopulateInventoryByType(Transform parent, RectTransform rect, List<OwnedItemData> allItems,
    int foodMax, int medicineMax, int poopMax, int rows)
    {
        // Clear UI using pooling system instead of destroying
        ReturnSlotsFromParent(parent);
        yield return null;

        var foodItems = new List<OwnedItemData>();
        var medicineItems = new List<OwnedItemData>();
        var poopItems = new List<OwnedItemData>();

        foreach (var item in allItems)
        {
            var data = itemDatabase.GetItem(item.itemID);
            if (data == null) continue;
            if (item.amount == 0) continue;

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

        // Store the items for this parent
        parentToItemsMap[parent] = displayItems;

        if (rect != null && parent == verticalContentParent)
        {
            float height = rows * (slotHeight + rowSpacing);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        // Use pooling system to create slots
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

    private IEnumerator PopulateShopInventoryCoroutine()
    {
        // Debug.Log ("Destroy Slot 0.6x ");
        yield return new WaitForEndOfFrame();

        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;

        if (ownedItems == null || ownedItems.Count == 0)
        {
            Debug.LogWarning("ℹ️ No items to show in shop inventory.");
            yield break;
        }

        // Filter only Food and Medicine
        var displayItems = new List<OwnedItemData>();

        foreach (var item in ownedItems)
        {
            var data = itemDatabase.GetItem(item.itemID);
            if (data == null) continue;

            if (data.category == ItemType.Food || data.category == ItemType.Medicine)
            {
                displayItems.Add(item);
            }
        }

        // Clear existing UI using pooling system
        ReturnSlotsFromParent(shopInventoryContentParent);
        yield return null;

        // Store the items for this parent
        parentToItemsMap[shopInventoryContentParent] = displayItems;

        // NOTE: We don't set vertical height here — assume horizontal layout is managed via prefab size + LayoutGroup

        for (int i = 0; i < displayItems.Count; i++)
        {
            var item = displayItems[i];
            var itemData = itemDatabase.GetItem(item.itemID);
            if (itemData == null) continue;
            if (item.amount == 0) continue;

            var slot = GetSlotFromPool();
            slot.transform.SetParent(shopInventoryContentParent, false);
            slot.Initialize(itemData, item.type, item.amount);
            Debug.Log($"Populating shop inventory with item: {itemData.itemName} (ID: {item.itemID}) - Amount: {item.amount}");
            activeSlots.Add(slot);

            // Small delay for smooth population
            if (i % 5 == 0) // Every 5 items
                yield return new WaitForSeconds(0.01f);
        }
        // Debug.Log ("Destroy Slot 1.0x ");

    }

    public void HideInventory()
    {
        ServiceLocator.Get<UIManager>().FadePanel(gameObject, GetComponent<CanvasGroup>(), false, 0.3f, 1.08f, 0.15f, true);
    }

    public void ShowInventory()
    {
        ServiceLocator.Get<UIManager>().FadePanel(gameObject, GetComponent<CanvasGroup>(), true, 0.3f, 1.08f, 0.15f, true);
    }

    public void ResetInventoryGroupvisibility()
    {
        SetCanvasGroupVisibility(quickViewGameObject, true);
        SetCanvasGroupVisibility(horizontalBarGameObject, false);
        SetCanvasGroupVisibility(cartButton.gameObject, true);
        SetCanvasGroupVisibility(verticalContentGameObject, false);
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
        if (isDeleteMode && pendingDeleteMap.Count > 0)
        {
            ShowDeleteConfirmationPanel();
        }
        else if (!isDeleteMode)
        {
            EnterDeleteMode();
        }
        else
        {
            ExitDeleteMode();
        }
    }

    private void ShowDeleteConfirmationPanel()
    {
        string message = "Delete the following item(s)?\n";
        confirmationMessageText.text = message;
        SetCanvasGroupVisibility(deleteConfirmationPanel, true);
    }

    private void EnterDeleteMode()
    {
        isDeleteMode = true;

        // Visual feedback for delete mode
        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                var bg = slot.GetComponent<Image>();
                if (bg != null)
                    bg.color = new Color(1f, 0.5f, 0.5f, 0.3f); // Red tint

                // ✅ Initialize amount text to show 0 items marked for deletion
                slot.UpdateAmountText(0);
            }
        }
    }
    public void AddToPendingDelete(ItemSlotUI slot)
    {
        if (!pendingDeleteMap.ContainsKey(slot))
            pendingDeleteMap[slot] = 0;

        if (pendingDeleteMap[slot] < slot.ItemAmount) // Add 1
        {
            pendingDeleteMap[slot]++;
            slot.UpdateAmountText(pendingDeleteMap[slot]); // 🔄 update UI
        }
    }

    public void RemoveFromPendingDelete(ItemSlotUI slot)
    {
        if (!pendingDeleteMap.ContainsKey(slot)) return;

        pendingDeleteMap[slot]--;
        if (pendingDeleteMap[slot] <= 0)
            pendingDeleteMap.Remove(slot);
        slot.UpdateAmountText(pendingDeleteMap.GetValueOrDefault(slot, 0)); // 🔄 update UI
    }

    public void ExitDeleteMode()
    {
        isDeleteMode = false;
        pendingDeleteMap.Clear();

        foreach (var slot in activeSlots)
        {
            if (slot != null)
            {
                slot.UpdateAmountText(-1);
                var bg = slot.GetComponent<Image>();
                if (bg != null)
                    bg.color = Color.white;
            }
        }
    }

    private void HandleConfirmedDelete()
    {
        foreach (var kvp in pendingDeleteMap)
        {
            var slot = kvp.Key;
            int deleteQty = kvp.Value;
            var item = SaveSystem.PlayerConfig.ownedItems.FirstOrDefault(x => x.itemID == slot.ItemDataSO.itemID);

            if (item != null)
            {
                item.amount -= deleteQty;
                if (item.amount <= 0)
                    SaveSystem.PlayerConfig.ownedItems.Remove(item);
            }
        }

        SaveSystem.SaveAll();

        pendingDeleteMap.Clear();
        SetCanvasGroupVisibility(deleteConfirmationPanel, false);
        ExitDeleteMode();
        StartPopulateAllInventories();
    }

    private void OnStoreButtonClicked()
    {
        SidebarManager sidebarManager = ServiceLocator.Get<SidebarManager>();
        sidebarManager.ShowPanel(sidebarManager.sidebarLinks[2]);
        ServiceLocator.Get<UIManager>().FadePanel(ServiceLocator.Get<UIManager>().panels.ShopPanel, ServiceLocator.Get<UIManager>().panels.ShopCanvasGroup, true);
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

    public void HandleItemDepletion(ItemSlotUI slot)
    {
        if (slot != null)
        {
            // Remove from active slots
            if (activeSlots.Contains(slot))
            {
                activeSlots.Remove(slot);
                Debug.Log("Is Getting Remove!");
            }
            else
            {
                Debug.Log($"Doesn't have slot: this is slot from inventory {slot.gameObject.name}");
            }

            // Return to pool instead of destroying
            ReturnSlotToPool(slot);

            // Refresh inventories
            //StartPopulateAllInventories();
        }
        else
        {
            Debug.Log("Slot is Null");
        }
    }

    #region ItemSlotUI
    public void RefreshInventoryMaximizeSlot(string id, int amount, ItemSlotUI itemSlotUI)
    { // ItemSlotUI
        ItemSlotUI slot = fullActiveSlots.Find(slot => slot.ItemDataSO.itemID == id);
        slot.UpdateValueInventoryOnly(slot.ItemDataSO, amount, itemSlotUI);
    }
    #endregion
    #region ServiceLocator
    public void StartPopulateAllInventoriesWhenOpen()
    { // InventoryUISendToPlains.cs
        refreshWhenOpen = true;
    }
    #endregion
    #region UnusedData
    void ClearAllUnusedDatas()
    {
        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;
        for (int i = ownedItems.Count - 1; i >= 0; i--)
        {
            if (ownedItems[i].itemID == "poop_ori" || ownedItems[i].itemID == "poop_rare")
            {
                SaveSystem.PlayerConfig.ClearItem(
                    ownedItems[i].itemID
                );
            }
        }
        SaveSystem.SaveAll();
    }
    #endregion
}