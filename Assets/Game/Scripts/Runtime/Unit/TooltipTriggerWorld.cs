using UnityEngine;

public class TooltipTriggerWorld : MonoBehaviour
{
    [Header("Tooltip Data")]
    public TooltipDataSO tooltipData;

    void OnMouseEnter()
    {
        Debug.Log ("Mouse Trigger");
        if (tooltipData == null) return;
        if (!gameObject.activeInHierarchy) return;

        TooltipManager.Instance.StartHover(tooltipData.infoData);
    }

    void OnMouseExit()
    {
        TooltipManager.Instance.EndHover();
    }
}
