using UnityEngine;
using UnityEngine.EventSystems;

public class RequirementTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    public TooltipDataSO tooltipData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipData != null && this.gameObject.activeInHierarchy)
        {
            RequirementTipManager.Instance.StartHover(tooltipData.infoData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
}
