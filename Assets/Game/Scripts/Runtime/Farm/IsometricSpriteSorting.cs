using UnityEngine;

[ExecuteAlways] // agar OnDrawGizmos & OnValidate juga jalan di edit mode
public class IsometricSorting : MonoBehaviour
{
    [Header("Aktifkan jenis renderer")]
    public bool useSpriteRenderer = true;
    public bool useMeshRenderer = false;

    [Header("Center Offset (Y)")]
    public float centerOffset = 0f;

    [Header("Optimisasi")]
    public bool isStaticSorting = false;

    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        if (useSpriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (useMeshRenderer)
            meshRenderer = GetComponent<MeshRenderer>();

        if (isStaticSorting)
        {
            UpdateSortingOrder();
            enabled = false; // matikan script setelah sorting dilakukan sekali
        }
    }

    void LateUpdate()
    {
        if (!isStaticSorting)
            UpdateSortingOrder();
    }

    void UpdateSortingOrder()
    {
        int sortingOrder = -(int)((transform.position.y + centerOffset) * 100);

        if (useSpriteRenderer && spriteRenderer != null)
            spriteRenderer.sortingOrder = sortingOrder;

        if (useMeshRenderer && meshRenderer != null)
            meshRenderer.sortingOrder = sortingOrder;
    }

    // =============================
    // Tambahan untuk Editor & Debug
    // =============================

    [ContextMenu("Print Center Offset")]
    void PrintCenterOffset()
    {
        Debug.Log($"[IsometricSorting] GameObject: {gameObject.name}, Center Offset: {centerOffset}", this);
    }

    void OnValidate()
    {
        Debug.Log($"[IsometricSorting] Center Offset Changed: {centerOffset} pada {gameObject.name}", this);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 basePos = transform.position;
        Vector3 offsetPos = basePos + new Vector3(0f, centerOffset, 0f);

        Gizmos.DrawLine(basePos, offsetPos);
        Gizmos.DrawSphere(offsetPos, 0.02f);
    }
}