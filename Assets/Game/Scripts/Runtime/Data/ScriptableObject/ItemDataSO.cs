using UnityEngine;
using Spine.Unity;

[CreateAssetMenu(menuName = "Item/Item Data")]
public class ItemDataSO : Rewardable
{
    [Header("Basic Info")]
    public string itemID; // Unique identifier for the item
    public string itemName;
    [TextArea]
    public string description;
    public ItemType category; // Use enum ItemType for categorization (e.g., Food, Medicine)
    public float unlockRequirement; // If used as a requirement for unlocking or crafting

    [Header("Buy Requirements")]
    public MonsterRequirements[] monsterRequirements;

    [Header("Stats")]
    public int price;
    public float nutritionValue; // If used as food & medicine

    [Header("Visuals")]
    public Sprite[] itemImgs; // [0] base, [1+] rotten forms
    public SkeletonDataAsset skeletonDataAsset;

    [Header ("Rewardable")]
    public Vector3 rewardScale;
    public override string ItemId => itemID;
    public override string ItemName => itemName;
    public override Sprite RewardSprite => itemImgs[0];
    public override Vector3 RewardScale => rewardScale;
    #region Rewardable
        
    public override void RewardGotItem(int quantities)
    {
        Debug.Log($"You got item {itemName} x{quantities}");
        SaveSystem.PlayerConfig.AddItem (itemID,category,quantities);
        SaveSystem.SaveAll ();
    }
    #endregion
}
