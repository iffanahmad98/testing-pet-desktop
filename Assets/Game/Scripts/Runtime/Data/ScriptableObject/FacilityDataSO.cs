using UnityEngine;

[CreateAssetMenu(fileName = "New Facility", menuName = "Facility/Facility Data")]
public class FacilityDataSO : ScriptableObject
{
    [Header("Metadata")]
    public string facilityID = "facility_default";
    public string facilityName = "New Facility";
    [TextArea]
    public string description = "Description here.";
    public int price = 100;
    public Sprite thumbnail; // Used in shop card UI
    public RuntimeAnimatorController animatorController;

    [Header("Buying Requirements")]
    public MonsterRequirements[] monsterRequirements;
    public RequirementTipDataSO requirementTipDataSO;

    [Header("Functionality")]
    public float cooldownSeconds = 30f;
    public bool isFreeToggleFacility = false; // If true, this facility is free and only toggles on/off (no cooldown, no purchase)

    // Add any other facility-specific properties here
}
