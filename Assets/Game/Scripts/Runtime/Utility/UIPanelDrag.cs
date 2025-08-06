using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [Header("Drag Settings")]
    [SerializeField] private bool constrainToScreen = true;
    [SerializeField] private bool snapToGrid = false;
    [SerializeField] private float gridSize = 50f;
    [SerializeField] private RectTransform dragTarget;
    [SerializeField] private bool bringToFrontOnDrag = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private float dragOpacity = 0.8f;
    [SerializeField] private bool showDragFeedback = true;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Vector2 pointerOffset;
    private float originalOpacity;
    private int originalSiblingIndex;
    
    private bool isDragging = false;
    
    private void Awake()
    {
        rectTransform = dragTarget != null ? dragTarget : GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = rectTransform.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && showDragFeedback)
        {
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Store original position and state
        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
        isDragging = false;
        
        // Calculate offset from pointer to panel center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition
        );
        pointerOffset = rectTransform.anchoredPosition - localPointerPosition;
        
        // Visual feedback
        if (showDragFeedback && canvasGroup != null)
        {
            originalOpacity = canvasGroup.alpha;
            canvasGroup.alpha = dragOpacity;
        }
        
        // Bring to front
        if (bringToFrontOnDrag)
        {
            rectTransform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || canvas == null) return;
        
        isDragging = true;

        Vector2 localPointerPosition;
        
        // Convert screen point to local point in parent canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition))
        {
            Vector2 newPosition = localPointerPosition + pointerOffset;
            
            // Constrain to screen bounds if enabled
            if (constrainToScreen)
            {
                newPosition = ConstrainToParent(newPosition);
            }
            
            rectTransform.anchoredPosition = newPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RestoreVisualFeedback();
        
        // Snap to grid if enabled
        if (snapToGrid)
        {
            SnapToGrid();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Handle case where user clicks but doesn't drag
        if (!isDragging)
        {
            RestoreVisualFeedback();
        }
    }

    private void RestoreVisualFeedback()
    {
        // Restore visual feedback
        if (showDragFeedback && canvasGroup != null)
        {
            canvasGroup.alpha = originalOpacity;
        }
    }

    private Vector2 ConstrainToParent(Vector2 position)
    {
        if (rectTransform?.parent == null) return position;

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return position;

        Vector2 parentSize = parentRect.rect.size;
        Vector2 panelSize = rectTransform.rect.size;
        
        // Calculate bounds based on parent size
        float minX = -(parentSize.x - panelSize.x) * 0.5f;
        float maxX = (parentSize.x - panelSize.x) * 0.5f;
        float minY = -(parentSize.y - panelSize.y) * 0.5f;
        float maxY = (parentSize.y - panelSize.y) * 0.5f;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private void SnapToGrid()
    {
        if (rectTransform == null) return;

        Vector2 position = rectTransform.anchoredPosition;
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        
        // Apply constraints after snapping
        if (constrainToScreen)
        {
            position = ConstrainToParent(position);
        }
        
        rectTransform.anchoredPosition = position;
    }

    // Public methods for external control
    public void ResetToOriginalPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
            if (bringToFrontOnDrag)
            {
                rectTransform.SetSiblingIndex(originalSiblingIndex);
            }
        }
    }

    public void SetDragTarget(RectTransform target)
    {
        dragTarget = target;
        rectTransform = target;
        canvasGroup = rectTransform.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && showDragFeedback)
        {
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
        }
    }
}
