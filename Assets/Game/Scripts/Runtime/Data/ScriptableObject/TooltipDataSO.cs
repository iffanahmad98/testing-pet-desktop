using UnityEngine;

[CreateAssetMenu(fileName = "New Tooltip Data", menuName = "Tooltip/Tooltip Data")]
public class TooltipDataSO : ScriptableObject
{
    [Header("Hover Information")]
    [TextArea]
    public string infoData;
}
