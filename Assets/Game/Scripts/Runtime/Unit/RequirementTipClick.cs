using UnityEngine;
using UnityEngine.EventSystems;

public class RequirementTipClick : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    public TooltipDataSO tooltipData;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tooltipData != null && gameObject.activeInHierarchy)
        {
            RequirementTipManager.Instance.StartClick(tooltipData.infoData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
}
