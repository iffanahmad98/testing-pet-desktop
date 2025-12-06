using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupScaler : MonoBehaviour
{
    public Camera cam;

    [Header("Base Ortho")]
    public float baseOrthographicSize = 11.12f;

    [Header("Base Item Size")]
    public float baseWidth = 100f;
    public float baseHeight = 100f;

    [Header("Base Spacing")]
    public float baseSpacing = 10f;

    private HorizontalOrVerticalLayoutGroup hvGroup;
    private GridLayoutGroup gridGroup;
    private RectTransform rect;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        hvGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        gridGroup = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        float ratio = cam.orthographicSize / baseOrthographicSize;

        float scaledW = baseWidth * ratio;
        float scaledH = baseHeight * ratio;
        float scaledSpacing = baseSpacing * ratio;

        // ==== Horizontal or Vertical Layout ====
        if (hvGroup != null)
        {
            hvGroup.spacing = scaledSpacing;

            for (int i = 0; i < rect.childCount; i++)
            {
                RectTransform child = rect.GetChild(i) as RectTransform;
                if (child == null) continue;

                if (hvGroup is HorizontalLayoutGroup)
                {
                    // --- Horizontal Layout scale width ---
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaledW);
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaledH);
                }
                else
                {
                    // --- Vertical Layout scale height ---
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scaledW);
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scaledH);
                }
            }
        }

        // ==== Grid Layout ====
        if (gridGroup != null)
        {
            gridGroup.cellSize = new Vector2(scaledW, scaledH);
            gridGroup.spacing = new Vector2(scaledSpacing, scaledSpacing);
        }
    }
}
