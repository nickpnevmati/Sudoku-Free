using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SudokuGridCustomizer : MonoBehaviour
{
    [SerializeField] Material gridMaterial;
    [SerializeField] private float[] vector2Data = null;

    private const int ARRAY_SIZE = 81; // This MUST match the shader

    private int highlighedCell = 0;

    void Start()
    {
        vector2Data = new float[ARRAY_SIZE];
        UpdateShaderArray();
    }

    public void HighlightCell(int index)
    {
        vector2Data[index] = 1;
        UpdateShaderArray();
    }

    public void ClearHighlighting()
    {
        vector2Data = new float[ARRAY_SIZE];
        UpdateShaderArray();
    }

    private void UpdateShaderArray()
    {
        if (gridMaterial == null) return;

        for (int i = 0; i < ARRAY_SIZE; i++)
        {
            gridMaterial.SetFloatArray("_HighlightedCells", vector2Data);
        }
    }
}
