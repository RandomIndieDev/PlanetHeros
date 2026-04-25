using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGrid : MonoBehaviour
{
    public int columns = 4;
    public int rows = 1;
    public Vector2 padding;  // extra offsets

    private GridLayoutGroup grid;
    private RectTransform rect;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
        UpdateGrid();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        float width = rect.rect.width;
        float height = rect.rect.height;

        // calculate cell size
        float cellWidth = (width - (grid.spacing.x * (columns - 1)) - padding.x) / columns;
        float cellHeight = (height - (grid.spacing.y * (rows - 1)) - padding.y) / rows;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }
}