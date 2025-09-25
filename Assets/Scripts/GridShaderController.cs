using UnityEngine;

[ExecuteAlways]
public class GridShaderController : MonoBehaviour
{
    public Material gridMaterial; // assign in inspector

    void Start()
    {
        gridMaterial.SetColor("_BaseColor", Color.white);
        gridMaterial.SetColor("_GridColor", Color.black);
        gridMaterial.SetColor("_SubGridColor", new Color(0.5f, 0.5f, 0.5f));

        gridMaterial.SetFloat("_MainLineThickness", 0.03f);
        gridMaterial.SetFloat("_SubLineThickness", 0.01f);

        gridMaterial.SetInt("_GridSize", 9);
        gridMaterial.SetInt("_SubGridSize", 3);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!gridMaterial) return;

        gridMaterial.SetColor("_BaseColor", Color.white);
        gridMaterial.SetColor("_GridColor", Color.black);
        gridMaterial.SetColor("_SubGridColor", new Color(0.5f, 0.5f, 0.5f));

        gridMaterial.SetFloat("_MainLineThickness", 0.03f);
        gridMaterial.SetFloat("_SubLineThickness", 0.01f);

        gridMaterial.SetInt("_GridSize", 9);
        gridMaterial.SetInt("_SubGridSize", 3);
    }
#endif
}
