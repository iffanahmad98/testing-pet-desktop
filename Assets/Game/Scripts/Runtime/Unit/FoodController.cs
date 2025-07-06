using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IConsumable
{
    private ItemDataSO itemData;
    [SerializeField] private Image foodImage;
    [SerializeField] private bool isRotten;

    public MonsterController claimedBy;
    public float nutritionValue;

    private RectTransform rectTransform;
    private Vector2 dragOffset;
    public bool IsBeingDragged { get; private set; }
    public event System.Action OnPlaced;
    [SerializeField] private float dropSpeed = 10f;
    [SerializeField] private float overlapDepth = 10f; // How deep into the ground it sinks visually


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(ItemDataSO data, RectTransform groundRect = null)
    {
        itemData = data;
        nutritionValue = data.nutritionValue;
        isRotten = false;
        UpdateFoodImage();

        StartCoroutine(DropUntilOnGround(groundRect));
    }

    public void PlaceFood() // Call this when valid placement is confirmed
    {
        OnPlaced?.Invoke();
    }

    public void Consume(MonsterController monster = null)
    {
        // monster.Feed(nutritionValue); // Example method on MonsterController
        ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
    }

    public ItemDataSO GetItemData() => itemData;


    public void UpdateFoodImage()
    {
        if (isRotten && itemData.itemImgs.Length > 1)
            foodImage.sprite = itemData.itemImgs[1];
        else
            foodImage.sprite = itemData.itemImgs[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsBeingDragged = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out dragOffset);
        dragOffset = rectTransform.anchoredPosition - dragOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsBeingDragged) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos))
        {
            rectTransform.anchoredPosition = localPos + dragOffset;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsBeingDragged = false;
        if (!ServiceLocator.Get<MonsterManager>().IsPositionInGameArea(rectTransform.anchoredPosition))
        {
            ServiceLocator.Get<MonsterManager>().DespawnToPool(gameObject);
        }
    }

    public bool TryClaim(MonsterController monster)
    {
        if (claimedBy == null)
        {
            claimedBy = monster;
            return true;
        }
        return claimedBy == monster;
    }

    public void ReleaseClaim(MonsterController monster)
    {
        if (claimedBy == monster)
        {
            claimedBy = null;
        }
        else if (claimedBy != null)
        {
            Debug.LogWarning($"[FoodController] {monster.name} tried to release but {claimedBy.name} owns it");
        }
    }

    public void ForceReleaseClaim()
    {
        claimedBy = null;
    }

    public bool IsClaimedBy(MonsterController monster) => claimedBy == monster;
    public bool IsClaimed => claimedBy != null;
    private IEnumerator DropUntilOnGround(RectTransform groundRect = null)
    {
        yield return null; // Ensure UI is initialized

        while (true)
        {
            rectTransform.anchoredPosition += Vector2.down * dropSpeed * Time.deltaTime;

            if (IsOverlappingGroundWithOffset(rectTransform, groundRect, overlapDepth))
            {
                break;
            }

            yield return null;
        }
    }
    private bool IsOverlappingGroundWithOffset(RectTransform food, RectTransform ground, float depthOffset)
    {
        Rect foodRect = GetWorldRect(food);
        Rect groundRectWorld = GetWorldRect(ground);

        // Lower the foodRect down by 'depthOffset' pixels to simulate visual overlap
        foodRect.position = new Vector2(foodRect.position.x, foodRect.position.y - depthOffset);

        return foodRect.Overlaps(groundRectWorld);
    }


    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 size = corners[2] - corners[0];
        return new Rect(corners[0], size);
    }

}
