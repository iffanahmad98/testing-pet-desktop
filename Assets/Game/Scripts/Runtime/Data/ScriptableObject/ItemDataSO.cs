using UnityEngine;

[CreateAssetMenu(menuName = "Item/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID; // Unique identifier for the item
    public string itemName;
    public Sprite[] itemImgs; // [0] base, [1+] rotten forms
    [TextArea]
    public string description;
    public ItemType category; // Use enum ItemType for categorization (e.g., Food, Medicine)
    public float unlockRequirement; // If used as a requirement for unlocking or crafting

    [Header("Stats")]
    public int price;
    public float nutritionValue; // If used as food & medicine
}
