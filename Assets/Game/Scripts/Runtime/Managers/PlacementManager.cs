using UnityEngine;
using UnityEngine.UI;

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

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlacementManager>();
    }

    /// <summary>
    /// Starts item placement mode.
    /// </summary>
    /// <param name="prefab">Prefab to place</param>
    /// <param name="area">Game area (RectTransform)</param>
    /// <param name="onPlace">Callback when placement is confirmed</param>
    /// <param name="onCancel">Callback when placement is canceled</param>
    /// <param name="allowMultiple">If true, placement mode stays active after placing</param>
    public void StartPlacement(GameObject prefab, RectTransform area, System.Action<Vector2> onPlace, System.Action onCancel = null, bool allowMultiple = false, Sprite previewSprite = null)
    {
        CancelPlacement(); // Clear previous

        gameArea = area;
        currentPreview = Instantiate(prefab, area);
        previewRect = currentPreview.GetComponent<RectTransform>();

        this.onConfirmPlace = onPlace;
        this.onCancel = onCancel;
        this.allowMultiplePlacement = allowMultiple;

        // 👇 Set preview sprite if provided
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

        bool isValid = IsInsideGameArea(pos);
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
                // else: do nothing, just let it keep following mouse
            }
            else
            {
                ServiceLocator.Get<UIManager>()?.ShowMessage("Can't place outside the game area!", 1f);
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

    private void UpdatePreviewVisual(bool isValid)
    {
        if (currentPreview.TryGetComponent<Image>(out var img))
        {
            img.color = isValid ? validColor : invalidColor;
        }
    }

    public bool IsPlacing => currentPreview != null;
}
