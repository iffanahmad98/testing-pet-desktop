using UnityEngine;

[CreateAssetMenu(fileName = "FacilityData", menuName = "Facility/FacilityData")]
public class FacilityDataSO : ScriptableObject
{
    [Header("Facility Info")]
    public string facilityID;
    public string facilityName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Usage Settings")]
    public int price;                         // Player must pay to use
    public float cooldownSeconds;             // Time before it can be used again
}
