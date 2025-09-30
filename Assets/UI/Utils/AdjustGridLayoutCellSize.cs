using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
public class AdjustGridLayoutCellSize : MonoBehaviour
{
    public enum Axis { X, Y };
    public enum RatioMode { Free, Fixed };
  
    [SerializeField] Axis expand;
    [SerializeField] RatioMode ratioMode;
    [SerializeField] float cellRatio = 1;
    [SerializeField] Vector2 relativeSpacing = Vector2.zero;
    [SerializeField] RectOffset relativePadding;

    new RectTransform transform;
    GridLayoutGroup grid;

    void Awake()
    {
        transform = (RectTransform)base.transform;
        grid = GetComponent<GridLayoutGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCellSize();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateCellSize();
    }

#if UNITY_EDITOR
    [ExecuteAlways]
    void Update()
    {
        UpdateCellSize();
    }
#endif

    void OnValidate()
    {
        transform = (RectTransform)base.transform;
        grid = GetComponent<GridLayoutGroup>();
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
#if UNITY_EDITOR
        if (grid == null || transform == null)
        {
            grid = GetComponent<GridLayoutGroup>();
            transform = (RectTransform)base.transform;
            if (grid == null) return;
        }
#endif

        var count = grid.constraintCount;
        if (expand == Axis.X)
        {
            float spacing = (count - 1) * grid.spacing.x;
            float contentSize = transform.rect.width - grid.padding.left - grid.padding.right - spacing;
            float sizePerCell = contentSize / count;
            grid.cellSize = new Vector2(sizePerCell, ratioMode == RatioMode.Free ? grid.cellSize.y : sizePerCell * cellRatio);

        }
        else //if (expand == Axis.Y)
        {
            float spacing = (count - 1) * grid.spacing.y;
            float contentSize = transform.rect.height - grid.padding.top - grid.padding.bottom - spacing;
            float sizePerCell = contentSize / count;
            grid.cellSize = new Vector2(ratioMode == RatioMode.Free ? grid.cellSize.x : sizePerCell * cellRatio, sizePerCell);
        }

        Vector2 cellSpacing = grid.cellSize * relativeSpacing;
        grid.spacing = cellSpacing;
    }
}