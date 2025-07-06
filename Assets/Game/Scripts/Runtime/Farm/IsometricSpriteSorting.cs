using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsometricSpriteSorting : MonoBehaviour
{
    public int offset = 0;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = -(int)(transform.position.y * 100) + offset;
        }
    }
}