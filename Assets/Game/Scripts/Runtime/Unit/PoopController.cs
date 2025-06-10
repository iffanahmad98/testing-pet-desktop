using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public PoopType poopType;
    public int poopValue;
    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(PoopType type)
    {
        poopType = type;
        poopValue = (int)poopType;

        // Set animator trigger based on poop type
        if (type == PoopType.Normal)
        {
            GetComponent<Image>().color = Color.black;
            // animator.SetTrigger("Normal");
        }
        else if (type == PoopType.Special)
        {
            GetComponent<Image>().color = Color.white;
            // animator.SetTrigger("Special");
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        ServiceLocator.Get<GameManager>().OnPoopChanged?.Invoke(ServiceLocator.Get<GameManager>().poopCollected += poopValue);
        SaveSystem.SavePoop(ServiceLocator.Get<GameManager>().poopCollected);
        SaveSystem.Flush();
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