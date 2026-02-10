using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragOutOfScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Assign dari Inspector")]
    [SerializeField] private Canvas rootCanvas;       // Canvas paling atas
    [SerializeField] private RectTransform dragLayer; // DragLayer di bawah Canvas
    [SerializeField] private ScrollRect sourceScroll; // Scroll View asal item

    [SerializeField] private MonsterCatalogueItemUI monsterItemUI; // Butuh ini buat tau monsterID
    [SerializeField] private MonsterCatalogueUI mCatalogueUI; // Butuh ini supaya bisa dapat tombol game area

    private RectTransform rt;
    private CanvasGroup cg;

    private Transform originalParent;
    private int originalSiblingIndex;

    private GameObject placeholder;     // supaya layout list tidak �bolong/geser liar�
    private Vector2 pointerOffset;      // biar item tidak loncat ke center pointer

    private PlayerConfig playerConfig;
    private Vector3 originalScale;
    private int lastHoveredGameAreaIndex = -1;

    void Awake()
    {
        rt = (RectTransform)transform;

        cg = GetComponent<CanvasGroup>();
        playerConfig = SaveSystem.PlayerConfig;

        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
        if (!dragLayer) dragLayer = rootCanvas.transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalScale = transform.localScale;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        lastHoveredGameAreaIndex = -1;

        // Placeholder untuk menjaga posisi di LayoutGroup (Content biasanya pakai Vertical/Grid Layout)
        placeholder = new GameObject($"{name}_placeholder", typeof(RectTransform), typeof(LayoutElement));
        var phRt = (RectTransform)placeholder.transform;
        phRt.SetParent(originalParent, false);
        phRt.SetSiblingIndex(originalSiblingIndex);

        var phLE = placeholder.GetComponent<LayoutElement>();
        var myLE = GetComponent<LayoutElement>();
        if (myLE != null)
        {
            phLE.preferredWidth = myLE.preferredWidth;
            phLE.preferredHeight = myLE.preferredHeight;
        }
        else
        {
            phLE.preferredWidth = rt.rect.width;
            phLE.preferredHeight = rt.rect.height;
        }

        // Naikkan item ke DragLayer (di bawah Canvas) agar bebas dari Mask ScrollView
        transform.SetParent(dragLayer, true); // true = pertahankan world position
        transform.SetAsLastSibling();

        // Hitung offset pointer supaya tidak loncat
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer, eventData.position, eventData.pressEventCamera, out var localPointerPos);
        pointerOffset = rt.anchoredPosition - localPointerPos;

        // penting: supaya drop target bisa kena raycast (kalau nanti pakai IDropHandler)
        cg.blocksRaycasts = false;

        // opsional: matikan scroll saat drag item
        if (sourceScroll) sourceScroll.enabled = false;

        // Drag layer harus ada di depan catalog panel
        dragLayer.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer, eventData.position, eventData.pressEventCamera, out var localPointerPos);

        rt.anchoredPosition = localPointerPos + pointerOffset;

        UpdateHoveredGameAreaSelection(eventData);

        // Cek apakah sedang hover di atas tempat yg bisa drop
        var viewport = sourceScroll && sourceScroll.viewport
            ? sourceScroll.viewport
            : (sourceScroll ? (RectTransform)sourceScroll.transform : null);

        bool isOverViewport = viewport &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                viewport, eventData.position, eventData.pressEventCamera);

        // Beri efek scale
        if (isOverViewport )
        {
            transform.localScale = originalScale * 1.3f;
            ApplySpecialEffectToGameAreaButton();
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    private void ApplySpecialEffectToGameAreaButton()
    {
        float scaleMultiplier = 1.1f;
        float tweenTime = 0.15f;
        int destinationAreaIndex = mCatalogueUI != null ? mCatalogueUI.GetSelectedGameAreaIndex() : -1;
        if (destinationAreaIndex < 0)
        {
            destinationAreaIndex = SaveSystem.LoadActiveGameAreaIndex();
        }

        if (mCatalogueUI == null || mCatalogueUI.gameAreaButtons == null ||
            destinationAreaIndex < 0 || destinationAreaIndex >= mCatalogueUI.gameAreaButtons.Length ||
            mCatalogueUI.gameAreaButtons[destinationAreaIndex] == null)
        {
            return;
        }

        var buttonTransform = mCatalogueUI.gameAreaButtons[destinationAreaIndex].transform;
        Vector3 baseScale = buttonTransform.localScale;

        if (DOTween.IsTweening(buttonTransform)) return;

        buttonTransform.DOScale(baseScale * scaleMultiplier, tweenTime)
            .SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    private void UpdateHoveredGameAreaSelection(PointerEventData eventData)
    {
        if (mCatalogueUI == null || mCatalogueUI.gameAreaButtons == null)
        {
            return;
        }

        int hoveredIndex = -1;
        for (int i = 0; i < mCatalogueUI.gameAreaButtons.Length; i++)
        {
            Button button = mCatalogueUI.gameAreaButtons[i];
            if (button == null || !button.gameObject.activeInHierarchy)
            {
                continue;
            }

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect == null)
            {
                continue;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(
                buttonRect, eventData.position, eventData.pressEventCamera))
            {
                hoveredIndex = i;
                break;
            }
        }

        if (hoveredIndex == -1)
        {
            lastHoveredGameAreaIndex = -1;
            return;
        }

        if (hoveredIndex == lastHoveredGameAreaIndex)
        {
            return;
        }

        lastHoveredGameAreaIndex = hoveredIndex;
        mCatalogueUI.SetSelectedGameAreaButton(hoveredIndex, false);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (sourceScroll) sourceScroll.enabled = true;
        cg.blocksRaycasts = true;
        transform.localScale = originalScale;

        // === Deteksi �keluar dari wilayah Scroll View� ===
        // Umumnya yang dimaksud adalah keluar dari VIEWPORT (area yang kelihatan)
        var viewport = sourceScroll && sourceScroll.viewport
            ? sourceScroll.viewport
            : (sourceScroll ? (RectTransform)sourceScroll.transform : null);

        bool releasedOutsideViewport = viewport &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                viewport, eventData.position, eventData.pressEventCamera);

        if (!releasedOutsideViewport)
        {
            // Drop tidak valid (masih di dalam scroll area) -> balikin ke list
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        }
        else
        {
            // Drop valid (di luar viewport scroll)

            Debug.Log($"Monster ID of this UI: {monsterItemUI.GetCatalogueMonsterData().monsterID}," +
                $" dropped to area {SaveSystem.LoadActiveGameAreaIndex()} " +
                $"from area {monsterItemUI.GetCatalogueMonsterData().gameAreaId}");

            // Pindahkan monster ke area yg sedang aktif
            playerConfig.MoveMonsterToGameArea(monsterItemUI.GetCatalogueMonsterData().monsterID, 
                monsterItemUI.GetCatalogueMonsterData().gameAreaId, SaveSystem.LoadActiveGameAreaIndex());

            // refresh game area supaya monster yg tampil bener
            MonsterManager.instance.RefreshGameArea();
            SaveSystem.SaveAll();

            // kembalikan yg di-drag ke posisi awal, jangan destroy
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

            // refresh catalog
            ServiceLocator.Get<MonsterCatalogueListUI>()?.RefreshCatalogue();
        }

        if (placeholder) Destroy(placeholder);
    }
}