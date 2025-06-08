using UnityEngine;
using UnityEngine.EventSystems;

public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public int poopValue = 1;

    public void OnPointerDown(PointerEventData eventData)
    {
        ServiceLocator.Get<GameManager>().poopCollected += poopValue;
        ServiceLocator.Get<GameManager>().OnPoopChanged?.Invoke(ServiceLocator.Get<GameManager>().poopCollected);
        ServiceLocator.Get<GameManager>().DespawnToPool(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Set(CursorType.Poop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Reset();
    }
}