using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    private GameObject currentPreview;
    private RectTransform gameArea;
    private RectTransform previewRect;

    private System.Action<Vector2> onConfirmPlace;
    private System.Action onCancel;

    [Header("Visual Feedback")]
    [SerializeField] private Color validColor = Color.white;
    [SerializeField] private Color invalidColor = Color.red;

    private bool allowMultiplePlacement = false;
    private bool isPlacingMedicine = false;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlacementManager>();
    }

    public void StartPlacement(
        GameObject prefab,
        RectTransform area,
        System.Action<Vector2> onPlace,
        System.Action onCancel = null,
        bool allowMultiple = false,
        Sprite previewSprite = null,
        bool isMedicine = false)
    {
        CancelPlacement(); // Clear previous

        gameArea = area;
        currentPreview = Instantiate(prefab, area);
        previewRect = currentPreview.GetComponent<RectTransform>();

        this.onConfirmPlace = onPlace;
        this.onCancel = onCancel;
        this.allowMultiplePlacement = allowMultiple;
        this.isPlacingMedicine = isMedicine;

        // Set preview sprite if provided
        if (previewSprite != null && currentPreview.TryGetComponent<Image>(out var image))
        {
            image.sprite = previewSprite;
        }
    }

    private void Update()
    {
        if (currentPreview == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gameArea, Input.mousePosition, null, out var pos);
        previewRect.anchoredPosition = pos;

        bool isValid = isPlacingMedicine ? IsOverSickMonster() : IsInsideGameArea(pos);
        UpdatePreviewVisual(isValid);

        if (Input.GetMouseButtonDown(0))
        {
            if (isValid)
            {
                onConfirmPlace?.Invoke(pos);

                if (!allowMultiplePlacement)
                {
                    CancelPlacement();
                }
            }
            else
            {
                string msg = isPlacingMedicine
                    ? "Only usable on sick monsters!"
                    : "Can't place outside the game area!";
                ServiceLocator.Get<UIManager>()?.ShowMessage(msg, 1f);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    public void CancelPlacement()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
        onCancel?.Invoke();
    }

    private bool IsInsideGameArea(Vector2 localPos)
    {
        if (gameArea == null) return false;
        Rect rect = gameArea.rect;
        return rect.Contains(localPos);
    }

    private bool IsOverSickMonster()
    {
        return TryGetMonsterUnderCursor() != null;
    }

    private void UpdatePreviewVisual(bool isValid)
    {
        if (currentPreview.TryGetComponent<Image>(out var img))
        {
            img.color = isValid ? validColor : invalidColor;
        }
    }
    public MonsterController TryGetMonsterUnderCursor()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        foreach (var result in raycastResults)
        {
            MonsterController monster = result.gameObject.GetComponentInParent<MonsterController>();
            if (monster != null)
            {
                return monster;
            }
        }

        return null;
    }
}
