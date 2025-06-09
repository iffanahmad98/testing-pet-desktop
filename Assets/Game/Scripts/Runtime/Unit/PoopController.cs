using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public PoopType poopType;
    public int poopValue;

    private void Start()
    {
        //TEMP: randomize poop type
        poopType = (PoopType)Random.Range(0, System.Enum.GetValues(typeof(PoopType)).Length);

        poopValue = (int)poopType;

        //TEMP: different poop color depending on type
        switch (poopType)
        {
            case PoopType.Normal:
                GetComponent<Image>().color = Color.black;
                break;
            case PoopType.Special:
                GetComponent<Image>().color = Color.white;
                break;
            default:
                break;
        }
        

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