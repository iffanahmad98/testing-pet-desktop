using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    public TooltipDataSO tooltipData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipData != null)
        {
            TooltipManager.Instance.StartHover(tooltipData.infoData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.EndHover();
    }
}
