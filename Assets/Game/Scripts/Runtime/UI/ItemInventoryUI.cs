using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent; // The layout parent (e.g. Horizontal Layout Group)
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private RectTransform contentRectTransform; // RectTransform of contentParent

    [Header("Config")]
    [SerializeField] private int defaultSlotCount = 6;
    [SerializeField] private float slotWidth = 110f;


    private void OnEnable()
    {
        PopulateInventory();
    }


    public void PopulateInventory()
    {
        // Clear previous
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        AddStarterItems();
        var ownedItems = SaveSystem.PlayerConfig.ownedItems;

        // Set content width dynamically
        int extraItems = Mathf.Max(0, ownedItems.Count - defaultSlotCount);
        float baseWidth = defaultSlotCount * slotWidth;
        float extraWidth = extraItems * slotWidth;

        contentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseWidth + extraWidth);

        // Populate slots
        foreach (var entry in ownedItems)
        {
            ItemDataSO itemData = itemDatabase.GetItem(entry.itemID);
            Debug.Log($"Adding item: {itemData?.itemName} (ID: {entry.itemID}, Amount: {entry.amount})");
            if (itemData != null)
            {
                var slot = Instantiate(slotPrefab, contentParent);
                slot.Initialize(itemData, entry.amount);

            }
        }
    }
    void AddStarterItems()
    {
        var playerConfig = SaveSystem.PlayerConfig;

        playerConfig.AddItem("IF01", 3);
        playerConfig.AddItem("IM01", 2);

        SaveSystem.SaveAll(); // Save changes
    }

}
