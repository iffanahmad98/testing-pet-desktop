using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private ItemDataSO itemData;
    public ItemDataSO ItemDataSO => itemData;
    private int itemAmount;
    public int ItemAmount => itemAmount;
    private ItemInventoryUI inventoryUI;

    // Drag & Drop variables
    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;
    private Canvas dragCanvas;
    private RectTransform dragCanvasRectTransform;
    private int originalSiblingIndex;

    // Visual feedback with DOTween
    private Vector3 originalScale = Vector3.one;
    private Vector3 hoverScale = Vector3.one * 1.1f;
    private Vector3 dragScale = Vector3.one * 1.2f;
    private Tween currentTween;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Store original scale
        originalScale = transform.localScale;

        // Find the main canvas for dragging
        dragCanvas = MainCanvas.Canvas;
        if (dragCanvas != null)
            dragCanvasRectTransform = dragCanvas.GetComponent<RectTransform>();
    }

    public void Initialize(ItemDataSO data, ItemType type, int amount)
    {
       // Debug.Log ("Initiliza");
        itemData = data;
        itemAmount = amount;

        iconImage.sprite = itemData.itemImgs[0];
        iconImage.enabled = true;
        amountText.text = $"{amount} pcs";
        inventoryUI = ServiceLocator.Get<ItemInventoryUI>();

        // Reset visual state
        ResetScale();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return; // Don't handle clicks during drag

        if (itemAmount <= 0) return;

        // Check for Delete Mode first
        if (inventoryUI != null && inventoryUI.IsInDeleteMode)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                inventoryUI.AddToPendingDelete(this);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventoryUI.RemoveFromPendingDelete(this);
            }
            return;
        }


        // Right-click cancels placement
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            return;
        }

        // Click feedback
        PlayClickAnimation();

        // Normal placement behavior
        var placementManager = ServiceLocator.Get<PlacementManager>();
        GameObject prefabToPlace = placementManager.GetPrefabForItemType(itemData.category);
        RectTransform gameArea = ServiceLocator.Get<MonsterManager>().gameAreaRT;

        if (prefabToPlace == null)
        {
            Debug.LogWarning($"No prefab assigned for category {itemData.category}");
            return;
        }

        inventoryUI.HideInventory();

        placementManager.StartPlacement(
            prefabToPlace,
            gameArea,
            OnConfirmPlacement,
            OnCancelPlacement,
            allowMultiple: itemData.category == ItemType.Food,
            isMedicine: itemData.category == ItemType.Medicine,
            previewSprite: itemData.itemImgs[0]
        );
    }

    // Drag & Drop Implementation
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryUI.IsInDeleteMode) return;

        // ✅ Check if this slot is in the vertical content parent - if not, don't allow dragging
        if (transform.parent != inventoryUI.VerticalContentParent)
        {
            return;
        }

        isDragging = true;
        originalPosition = transform.position;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Move to drag canvas for proper rendering
        if (dragCanvas != null)
            transform.SetParent(dragCanvas.transform, true);

        // Visual feedback with DOTween
        PlayDragStartAnimation();

        // Disable graphic raycaster temporarily
        if (graphicRaycaster != null)
            graphicRaycaster.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (dragCanvasRectTransform != null)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvasRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition))
            {
                transform.position = dragCanvasRectTransform.TransformPoint(localPointerPosition);
            }
        }

        // Visual feedback: highlight potential drop targets
        var potentialTarget = GetDropTarget(eventData);
        if (potentialTarget != null && potentialTarget != this)
        {
            // ✅ Only highlight if the target is also in vertical content
            if (potentialTarget.transform.parent == inventoryUI.VerticalContentParent)
            {
                // Give subtle visual feedback that this is a valid drop target
                if (potentialTarget.currentTween == null || !potentialTarget.currentTween.IsActive())
                {
                    potentialTarget.PlayTargetHighlightAnimation();
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        // Reset visual state
        PlayDragEndAnimation();

        // Re-enable graphic raycaster
        if (graphicRaycaster != null)
            graphicRaycaster.enabled = true;

        // Check if dropped on a valid slot
        var dropTarget = GetDropTarget(eventData);
        if (dropTarget != null && dropTarget != this)
        {
            // ✅ Only allow drop if target is in vertical content
            if (dropTarget.transform.parent == inventoryUI.VerticalContentParent)
            {
                inventoryUI.MoveItemBack(this, dropTarget);
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }
        else
        {
            // Return to original position if no valid drop target
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
            transform.position = originalPosition;
        }
    }

    private ItemSlotUI GetDropTarget(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<ItemSlotUI>();
            if (slot != null && slot != this && !slot.isDragging)
            {
                // ✅ Only return slots that are in vertical content
                if (slot.transform.parent == inventoryUI.VerticalContentParent)
                {
                    return slot;
                }
            }
        }
        return null;
    }

    // Visual feedback for hover with DOTween
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && !inventoryUI.IsInDeleteMode)
        {
            PlayHoverEnterAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging && !inventoryUI.IsInDeleteMode)
        {
            PlayHoverExitAnimation();
        }
    }

    public void UpdateAmountText(int markedForDelete = 0)
    {
        if (markedForDelete >= 0)
        {
            // Show deletion count in delete mode
            amountText.text = $"{markedForDelete} pcs";
            amountText.color = Color.red; // Red color to indicate deletion
        }
        else
        {
            // Show normal item amount
            amountText.text = $"{itemAmount} pcs";
            amountText.color = Color.black; // Normal color
        }
    }

    // DOTween Animation Methods
    private void PlayHoverEnterAnimation()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(hoverScale, 0.2f)
            .SetEase(Ease.OutBack);
    }

    private void PlayHoverExitAnimation()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale, 0.15f)
            .SetEase(Ease.InBack);
    }

    private void PlayDragStartAnimation()
    {
        KillCurrentTween();
        // Scale up and fade slightly
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(dragScale, 0.15f).SetEase(Ease.OutBack));
        sequence.Join(canvasGroup.DOFade(0.8f, 0.15f));
        sequence.OnComplete(() => canvasGroup.blocksRaycasts = false);

        currentTween = sequence;
    }

    private void PlayDragEndAnimation()
    {
        KillCurrentTween();
        // Scale back to normal and restore alpha
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBounce));
        sequence.Join(canvasGroup.DOFade(1f, 0.2f));
        sequence.OnComplete(() => canvasGroup.blocksRaycasts = true);

        currentTween = sequence;
    }

    private void PlayClickAnimation()
    {
        KillCurrentTween();
        // Quick punch scale effect
        currentTween = transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 10, 1f);
    }

    public void PlaySwapAnimation()
    {
        // Keep this for backward compatibility or other uses
        PlayMoveBackAnimation();
    }

    public void PlayMoveBackAnimation()
    {
        KillCurrentTween();
        // Move back animation - slide effect with scale
        var sequence = DOTween.Sequence();

        // Quick slide to the left and scale up
        sequence.Append(transform.DOLocalMoveX(transform.localPosition.x - 20f, 0.1f).SetEase(Ease.OutQuad));
        sequence.Join(transform.DOScale(originalScale * 1.2f, 0.1f).SetEase(Ease.OutQuad));

        // Slide back to original position and scale down
        sequence.Append(transform.DOLocalMoveX(transform.localPosition.x, 0.15f).SetEase(Ease.OutBack));
        sequence.Join(transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack));

        currentTween = sequence;
    }

    public void PlayTargetHighlightAnimation()
    {
        KillCurrentTween();
        // Subtle highlight animation for target slot
        var sequence = DOTween.Sequence();

        // Quick pulse effect
        sequence.Append(transform.DOScale(originalScale * 1.05f, 0.1f).SetEase(Ease.OutQuad));
        sequence.Append(transform.DOScale(originalScale, 0.1f).SetEase(Ease.InQuad));

        currentTween = sequence;
    }

    private void ResetScale()
    {
        KillCurrentTween();
        transform.localScale = originalScale;
    }

    private void KillCurrentTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            currentTween = null;
        }

        // ✅ Also kill any tweens on this transform specifically
        transform.DOKill();
        canvasGroup.DOKill();
    }

    private void OnConfirmPlacement(Vector2 position)
    {
        if (itemAmount <= 0)
        {
            Debug.LogWarning("Tried to place item with 0 amount");
            return;
        }

        ServiceLocator.Get<MonsterManager>().SpawnItem(itemData, position);

        UpdateValueInventory(itemData);
    }

    private void UpdateValueInventory(ItemDataSO itemData)
    {
        var inventories = inventoryUI.ActiveSlots;

        for (int i = inventories.Count - 1; i >= 0; i--)
        {
            var slot = inventories[i];

            if (slot.itemData == itemData)
            {
                slot.itemAmount--;
                slot.amountText.text = $"{itemAmount} pcs";
                
                if (slot.itemAmount <= 0)
                {
                    ServiceLocator.Get<PlacementManager>().CancelPlacement();
                    inventoryUI.HandleItemDepletion(slot);
                    //inventoryUI.StartPopulateAllInventories();
                }
            }
        }

        if (itemAmount <= 0)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
           // inventoryUI.StartPopulateAllInventories();
        }
        inventoryUI.StartPopulateAllInventories();
        Debug.Log ("Destroy 0.1 x");
        SaveSystem.UpdateItemData(itemData.itemID, itemData.category, -1);
    }

    private void OnCancelPlacement()
    {
        inventoryUI.ShowInventory();
    }

    public void ResetSlot()
    {
        // ✅ Kill any active tweens FIRST before resetting
        KillCurrentTween();
        amountText.text = "";

        // Reset visual state
        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Reset drag state
        isDragging = false;

        // Reset data
        itemData = null;
        itemAmount = 0;
    }

    private void OnDisable()
    {
        // ✅ Kill tweens when object is disabled
        KillCurrentTween();

        // Reset visual state immediately
        transform.localScale = originalScale;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        isDragging = false;
    }

    private void OnDestroy()
    {
        KillCurrentTween();
    }
}
