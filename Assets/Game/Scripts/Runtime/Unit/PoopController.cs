using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ITargetable
{
    public PoopType poopType;
    public int poopValue;
    private string poopId;

    private Animator animator;
    private RectTransform rectTransform;

    public bool IsTargetable => gameObject.activeInHierarchy;
    public Vector2 Position => rectTransform.anchoredPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(PoopType type)
    {
        poopType = type;
        poopValue = (int)poopType;

        // Set animator trigger based on poop type
        if (type == PoopType.Normal)
        {
            animator.SetTrigger("Normal");
            poopId = "poop_ori";

        }
        else if (type == PoopType.Sparkle)
        {
            animator.SetTrigger("Special");
            poopId = "poop_rare";
        }
    }

    public void OnCollected()
    {
        // Notify the MonsterManager about the poop collection
        ServiceLocator.Get<MonsterManager>().OnPoopChanged?.Invoke(ServiceLocator.Get<MonsterManager>().poopCollected += poopValue);

        // Save the updated poop count
        SaveSystem.SavePoop(ServiceLocator.Get<MonsterManager>().poopCollected);
        SaveSystem.UpdateItemData(poopId, ItemType.Poop, 1);
        SaveSystem.Flush();

        // Despawn this poop object
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCollected();
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