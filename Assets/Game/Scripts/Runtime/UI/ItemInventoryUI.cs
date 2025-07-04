using System.Collections;
using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent; // The layout parent (e.g. Horizontal Layout Group)
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private RectTransform contentRectTransform; // RectTransform of contentParent
    public ItemDatabaseSO ItemDatabase => itemDatabase;

    [Header("Config")]
    [SerializeField] private int defaultSlotCount = 6;
    [SerializeField] private float slotWidth = 110f;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnEnable()
    {
        StartPopulateInventory();
    }

    public void StartPopulateInventory()
    {
        StartCoroutine(PopulateInventoryCoroutine());
    }

    private IEnumerator PopulateInventoryCoroutine()
    {

        // Clear previous
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        yield return null; // Wait a frame for UI clearing

        var ownedItems = SaveSystem.PlayerConfig?.ownedItems;

        if (ownedItems == null)
        {
            Debug.LogError("ownedItems is null. Aborting inventory population.");
            yield break;
        }

        if (ownedItems.Count == 0)
        {
            Debug.LogWarning("No items found in inventory, adding starter items.");
        }

        // Set content width dynamically
        int extraItems = Mathf.Max(0, ownedItems.Count - defaultSlotCount);
        float baseWidth = defaultSlotCount * slotWidth;
        float extraWidth = extraItems * slotWidth;

        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseWidth + extraWidth);

        yield return null; // Wait a frame after resizing

        // Populate slots
        foreach (var entry in ownedItems)
        {
            ItemDataSO itemData = itemDatabase.GetItem(entry.itemID);
            Debug.Log($"Adding item: {itemData?.itemName} (ID: {entry.itemID}, Amount: {entry.amount})");

            if (itemData != null)
            {
                var slot = Instantiate(slotPrefab, contentParent);
                slot.Initialize(itemData, entry.type, entry.amount);
            }

            // Optional: yield to avoid UI stutter if many items
            yield return null;
        }
    }

}
