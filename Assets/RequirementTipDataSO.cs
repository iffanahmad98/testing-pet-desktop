using UnityEngine;

public enum RequirementTipType {
    Manual,
    Eligible
}
[CreateAssetMenu(fileName = "New Requirement Data", menuName = "Tooltip/Requirement Data")]
public class RequirementTipDataSO : ScriptableObject
{
    public RequirementTipType requirementTipType;
    [Header("Hover Information")]
    [TextArea]
    public string infoData;

    public string GetInfoData () {
        return infoData;
    }
}
