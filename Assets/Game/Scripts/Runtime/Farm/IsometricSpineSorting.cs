using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class IsometricSpineSorting : MonoBehaviour
{
    // [Header("Sorting Offset")]
    // public int offset = 0;

    [Header("Center Offset (Y)")]
    public float centerOffset = 0f;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (rend != null)
        {
            int sortingOrder = -(int)((transform.position.y + centerOffset) * 100);
            rend.sortingOrder = sortingOrder;
        }
    }
}