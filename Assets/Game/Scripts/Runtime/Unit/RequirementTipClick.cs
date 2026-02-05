using UnityEngine;
using UnityEngine.EventSystems;

public class RequirementTipClick : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    public RequirementTipDataSO dataSO;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dataSO != null)
        {
            RequirementTipManager.Instance.StartClick(dataSO.infoData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
}
