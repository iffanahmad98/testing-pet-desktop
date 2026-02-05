using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RequirementTipClick2D : MonoBehaviour
{
    [Header("Tooltip Data")]
    public RequirementTipDataSO requirementData;

    void Start () {
        RequirementTipManager.Instance.AddRequirementTipClick2d (this);
    }

    void OnMouseDown()
    {
        if (!enabled) return;   // guard tambahan
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
