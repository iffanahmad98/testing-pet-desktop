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
            animator.SetTrigger("Normal");
        }
        else if (type == PoopType.Sparkle)
        {
            animator.SetTrigger("Special");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ServiceLocator.Get<MonsterManager>().OnPoopChanged?.Invoke(ServiceLocator.Get<MonsterManager>().poopCollected += poopValue);
        SaveSystem.SavePoop(ServiceLocator.Get<MonsterManager>().poopCollected);
        SaveSystem.Flush();
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
        ServiceLocator.Get<CursorManager>().Set(CursorType.Default);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Set(CursorType.Poop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Set(CursorType.Default);
    }
}