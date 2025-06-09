using UnityEngine;
using UnityEngine.EventSystems;

public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public PoopType poopType;
    public int poopValue;

    private void Start()
    {
        poopValue = (int)poopType;
    }

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