using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MagicalGarden.Inventory;
public class PoopController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ITargetable
{
    public PoopType poopType;
    public int poopValue;
    public Vector2 cursorOffset;
    public Image[] images;
    private string poopId;

    private Animator animator;
    private RectTransform rectTransform;

    public bool IsTargetable => gameObject.activeInHierarchy && ReservedBy == null;
    public Vector2 Position => rectTransform.anchoredPosition;

    // NPC reservation system to prevent multiple NPCs targeting the same poop
    public MonsterController ReservedBy { get; private set; }

    [Header("Database")]
    public ItemData[] poopItemDatas;
    ItemData poopItemData;

    private void Awake()
    {
        animator = transform.GetChild(1).GetComponent<Animator>();
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
            poopItemData = poopItemDatas[0];
        }
        else if (type == PoopType.Sparkle)
        {
            animator.SetTrigger("Special");
            poopId = "poop_rare";
            poopItemData = poopItemDatas[1];
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
        // (Not Used)
        // ServiceLocator.Get<MonsterManager>().OnPoopChanged?.Invoke(ServiceLocator.Get<MonsterManager>().poopCollected += poopValue);

        // Save the updated poop count
        /* (Not Used)
        SaveSystem.SavePoop(ServiceLocator.Get<MonsterManager>().poopCollected);
        SaveSystem.UpdateItemData(poopId, ItemType.Poop, 1);
        SaveSystem.Flush();
        */
        Debug.Log($"PoopController: OnCollected called for '{name}' (type={poopType})");
        SaveSystem.PlayerConfig.AddItemFarm(poopItemData.itemId, 1);
        SaveSystem.SaveAll();

        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null)
        {
            Debug.LogWarning("PoopController: MonsterManager not found when trying to raise OnPoopCleaned");
        }
        else
        {
            Debug.Log("PoopController: Invoking MonsterManager.OnPoopCleaned for tutorial");
            monsterManager.OnPoopCleaned?.Invoke(this);
        }
        // Fadeout
        for (int i = 0; i < images.Length; i++)
        {
            StartCoroutine(FadeOut(images[i], 0f, 0.5f));
        }

        // Change cursor texture
        ServiceLocator.Get<CursorManager>().Set(CursorType.Default, Vector2.zero);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCollected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Set(CursorType.Poop, cursorOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ServiceLocator.Get<CursorManager>().Set(CursorType.Default, Vector2.zero);
    }

    IEnumerator FadeOut(Image image, float targetAlpha, float duration)
    {
        Color c = image.color;
        float startAlpha = c.a;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // use Time.deltaTime if you want it affected by timescale
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            image.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        image.color = c;

        // Despawn this poop object
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }
}