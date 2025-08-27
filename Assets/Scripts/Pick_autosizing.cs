using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoCellSize : MonoBehaviour
{
    public int columns = 4;
    public int rows = 5;
    public Vector2 spacing = new Vector2(5, 5);

    void Start() => UpdateCellSize();
    void OnRectTransformDimensionsChange() => UpdateCellSize();

    void UpdateCellSize()
    {
        var grid = GetComponent<GridLayoutGroup>();
        var rt = GetComponent<RectTransform>();

        float cellWidth = (rt.rect.width - (spacing.x * (columns - 1))) / columns;
        float cellHeight = (rt.rect.height - (spacing.y * (rows - 1))) / rows;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.spacing = spacing;
    }
}