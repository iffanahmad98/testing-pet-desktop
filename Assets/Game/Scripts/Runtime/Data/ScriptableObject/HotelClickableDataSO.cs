using UnityEngine;

[CreateAssetMenu(fileName = "HotelClickableDataSO", menuName = "Scriptable Objects/HotelClickableDataSO")]
public class HotelClickableDataSO : ScriptableObject {
    public string objectName = "";
    public Vector3 objectNormalScale = new Vector3 (0,0,0);
    public Vector3 objectHoverScale = new Vector3 (0,0,0);
}
