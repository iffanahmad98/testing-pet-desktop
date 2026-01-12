using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public abstract class Rewardable : ScriptableObject
{
    public abstract string ItemId {get;}
    public abstract string ItemName {get; }
    public abstract Sprite RewardSprite { get; }
    public abstract Vector3 RewardScale {get;}
    public abstract void RewardGotItem(int quantities);

}
