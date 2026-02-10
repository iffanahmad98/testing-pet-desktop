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
        // Debug.LogError ("Mouse Trigger");
        if (!enabled) return;   // guard tambahan
        
        if (tooltipData == null) return;
        if (!gameObject.activeInHierarchy) return;

        TooltipManager.Instance.StartHover(tooltipData.infoData);
    }

    void OnMouseExit()
    {
        TooltipManager.Instance.EndHover();
    }

    void OnDestroy (){
        TooltipManager.Instance.RemoveToolTipClick2d (this);
    }
}
