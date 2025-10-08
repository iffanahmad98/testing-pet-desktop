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

    public bool IsTargetable => gameObject.activeInHierarchy && ReservedBy == null;
    public Vector2 Position => rectTransform.anchoredPosition;

    // NPC reservation system to prevent multiple NPCs targeting the same poop
    public MonsterController ReservedBy { get; private set; }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rectTransform = GetComponentInChildren<RectTransform>();
    }

    public void Initialize(PoopType type)
    {
        poopType = type;
        poopValue = (int)poopType;
        ReservedBy = null; // Reset reservation

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

    /// <summary>
    /// Reserve this poop for a specific NPC
    /// </summary>
    public void Reserve(MonsterController npc)
    {
        ReservedBy = npc;
    }

    /// <summary>
    /// Release the reservation (e.g., if NPC changes target)
    /// </summary>
    public void ReleaseReservation()
    {
        ReservedBy = null;
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