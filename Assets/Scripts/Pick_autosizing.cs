using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoCellSize : MonoBehaviour
{
    public Vector2 spacing = new Vector2(200, 100);

    private GridLayoutGroup grid;
    private RectTransform rt;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        UpdateCellSize();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }

    private void UpdateCellSize()
    {
        var race = GameManager.I?.GetSelectedRace();
        if (race == null) return;

        //int totalButtons = 20; // or dynamically count children if needed
        int columns = 2;
        int rows = race.type == RaceType.Feature ? 10 : 5;

        float cellWidth = (rt.rect.width - (spacing.x * (columns - 1))) / columns;
        float cellHeight = (rt.rect.height - (spacing.y * (rows - 1))) / rows;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }
}