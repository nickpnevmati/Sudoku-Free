using UnityEngine;
using UnityEngine.UI;

public class SudokuGridShaderController : MonoBehaviour
{
    /// <summary>
    /// Template asset wired up in the prefab. Never written to: React tears the board down and
    /// rebuilds it on every screen switch, so writing to the shared asset would leak highlight
    /// and theme state between instances - and in the editor it would persist to disk.
    /// </summary>
    [SerializeField] Material gridMaterial;

    /// <summary>Runtime copy of <see cref="gridMaterial"/>, owned by this component.</summary>
    private Material materialInstance;

    float[] highlighedCells = null;

    private const int ARRAY_SIZE = 81; // This MUST match the shader

    public Color primaryColor { set => SetColor("_BaseColor", value); }
    public Color secondaryColor { set => SetColor("_HighlightColor", value); }
    public Color backgroundColor { set => SetColor("_BackgroundColor", value); }

    // Awake, not Start: SudokuGridController pushes the board theme through this component from
    // its own Start, and Start order between two components is undefined. Every Awake runs first.
    void Awake()
    {
        highlighedCells = new float[ARRAY_SIZE];

        if (gridMaterial == null)
        {
            Debug.LogError($"{nameof(SudokuGridShaderController)}: no grid material assigned", this);
            return;
        }

        materialInstance = new Material(gridMaterial);

        var graphic = GetComponent<Graphic>();
        if (graphic != null) graphic.material = materialInstance;
        else Debug.LogWarning($"{nameof(SudokuGridShaderController)}: no Graphic to render the material instance", this);

        UpdateShaderArray();
    }

    public void HighlightCell(int index)
    {
        highlighedCells[index] = 1;
        UpdateShaderArray();
    }

    public void ClearHighlighting()
    {
        highlighedCells = new float[ARRAY_SIZE];
        UpdateShaderArray();
    }

    private void SetColor(string property, Color value)
    {
        if (materialInstance == null) return;
        materialInstance.SetColor(property, value);
    }

    private void UpdateShaderArray()
    {
        if (materialInstance == null) return;
        materialInstance.SetFloatArray("_HighlightedCells", highlighedCells);
    }

    private void OnDestroy()
    {
        // No ClearHighlighting() here - the instance dies with this component, and clearing it
        // was only ever needed because the state used to live on the shared asset.
        if (materialInstance != null) Destroy(materialInstance);
    }
}
