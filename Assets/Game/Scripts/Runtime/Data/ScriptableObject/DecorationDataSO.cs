using UnityEngine;

[CreateAssetMenu(fileName = "New Decoration", menuName = "Decoration/Decoration Data")]
public class DecorationDataSO : ScriptableObject
{
    public string decorationID;
    public string decorationName;
    public string description;
    public Sprite thumbnail;
    public int price;
}
