using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Facility", menuName = "Facility/Facility Data")]
public class FacilityDataSO : ScriptableObject
{
    [Header("Metadata")]
    public string facilityID = "facility_default";
    public string facilityName = "New Facility";
    public string description = "Description here.";
    public int price = 100;
    public Sprite thumbnail; // Used in shop card UI
    public AnimatorController animator;

    [Header("Functionality")]
    public float cooldownSeconds = 30f;
    
    // Add any other facility-specific properties here
}
