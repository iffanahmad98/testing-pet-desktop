using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RequirementTipClick2D : MonoBehaviour
{
    [Header("Tooltip Data")]
    public RequirementTipDataSO requirementData;

    void OnMouseDown()
    {
        if (requirementData != null && gameObject.activeInHierarchy)
        {
            RequirementTipManager.Instance.StartClick(requirementData.infoData);
        }
    }

    void OnMouseExit()
    {
        if (RequirementTipManager.Instance != null)
        {
            RequirementTipManager.Instance.EndHover();
        }
    }
}
