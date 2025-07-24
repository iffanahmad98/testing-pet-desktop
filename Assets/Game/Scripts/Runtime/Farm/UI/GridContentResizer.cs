using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridContentResizer : MonoBehaviour
{
    public RectTransform content;       // Assign: Content dari ScrollView
    public int columnCount = 3;         // Kolom tetap 3

    [SerializeField] private GridLayoutGroup grid;

    public void Refresh(int itemCount)
    {
        int rowCount = Mathf.CeilToInt((float)itemCount / columnCount);
        float cellHeight = grid.cellSize.y;
        float spacingY = grid.spacing.y;
        float paddingTop = grid.padding.top;
        float paddingBottom = grid.padding.bottom;

        float totalHeight = (rowCount * cellHeight) + ((rowCount - 1) * spacingY) + paddingTop + paddingBottom;

        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
    }
}