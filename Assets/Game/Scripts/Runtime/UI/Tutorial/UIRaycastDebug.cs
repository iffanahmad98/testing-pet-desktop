using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastDebug : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[UIRaycastDebug] Click on {gameObject.name} at {eventData.position}");
    }
}
