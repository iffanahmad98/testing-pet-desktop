using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class IsometricSpineSorting : MonoBehaviour
{
    public int offset = 0;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (rend != null)
        {
            rend.sortingOrder = -(int)(transform.position.y * 100) + offset;
        }
    }
}