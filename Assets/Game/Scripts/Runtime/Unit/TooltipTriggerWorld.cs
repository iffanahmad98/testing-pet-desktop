using UnityEngine;

public class TooltipTriggerWorld : MonoBehaviour
{
    [Header("Tooltip Data")]
    public TooltipDataSO tooltipData;

    void Start () {
        TooltipManager.Instance.AddToolTipClick2d (this);
    }

    void OnMouseEnter()
    {
        if (!enabled) return;   // guard tambahan
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
