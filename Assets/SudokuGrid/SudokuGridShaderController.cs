using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SudokuGridShaderController : MonoBehaviour
{
    [SerializeField] Material gridMaterial;

    float[] highlighedCells = null;

    private const int ARRAY_SIZE = 81; // This MUST match the shader

    void Start()
    {
        highlighedCells = new float[ARRAY_SIZE];
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

    private void UpdateShaderArray()
    {
        if (gridMaterial == null) return;

        for (int i = 0; i < ARRAY_SIZE; i++)
        {
            gridMaterial.SetFloatArray("_HighlightedCells", highlighedCells);
        }
    }

    private void OnDestroy()
    {
        ClearHighlighting();
    }
}
