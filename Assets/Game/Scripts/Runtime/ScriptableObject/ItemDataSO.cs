using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ItemData")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int price;
    public int fullness; //optional
    public string description;
    public string category; // e.g., "Food", "Medicine", etc.
}
