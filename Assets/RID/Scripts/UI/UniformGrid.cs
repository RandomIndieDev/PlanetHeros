using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class UniformGridEditorReady : MonoBehaviour
{
    public int columns = 3;
    public Vector2 margin;
    public bool matchWidth = true;

    private GridLayoutGroup grid;
    private RectTransform rect;

    private void OnEnable()
    {
        grid = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void UpdateGrid(Vector2Int size)
    {
        grid.constraintCount = size.x;
        columns = size.x;
        
        UpdateGrid();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateGrid();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
            UpdateGrid();
    }
#endif

    private void UpdateGrid()
    {
        if (grid == null || rect == null) return;

        float areaWidth = rect.rect.width - margin.x;
        float areaHeight = rect.rect.height - margin.y;

        float totalSpacingX = grid.spacing.x * (columns - 1);
        float rawCellWidth = (areaWidth - totalSpacingX) / columns;

        float finalSize = rawCellWidth;

        // Optional: fit height too if you want exact vertical match
        if (!matchWidth)
        {
            int rows = Mathf.CeilToInt(rect.childCount / (float)columns);
            float totalSpacingY = grid.spacing.y * (rows - 1);
            float rawCellHeight = (areaHeight - totalSpacingY) / rows;
            finalSize = Mathf.Min(rawCellWidth, rawCellHeight);
        }

        grid.cellSize = new Vector2(finalSize, finalSize);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(grid);
#endif
    }
}