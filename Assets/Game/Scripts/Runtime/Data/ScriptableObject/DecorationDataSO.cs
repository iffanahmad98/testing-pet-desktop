using UnityEngine;

[CreateAssetMenu(fileName = "New Decoration", menuName = "Decoration/Decoration Data")]
public class DecorationDataSO : Rewardable
{
    
    public string decorationID;
    public string decorationName;
    public string description;
    public Sprite thumbnail;
    public int price;

    [Header("Buying Requirements")]
    public MonsterRequirements[] monsterRequirements;

    [Header ("Rewardable")]
    public Vector3 rewardScale;
    public override string ItemName => decorationName;
    public override Sprite RewardSprite => thumbnail;
    public override Vector3 RewardScale => rewardScale;

    #region Rewardable
        
    public override void RewardGotItem(int quantities)
    {
        Debug.Log($"You got item {decorationName} x{quantities}");
        SaveSystem.PlayerConfig.AddDecoration (decorationID,false);
        SaveSystem.SaveAll ();
    }
    #endregion
}
