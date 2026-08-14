using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Board Theme")]
public class BoardThemeSO : ScriptableObject
{
    [Header("Grid shader")]
    public Color gridBase = new Color(0.11f, 0.13f, 0.19f);
    public Color gridHighlight = new Color(0.42f, 0.49f, 0.94f);
    public Color gridBackground = new Color(0.06f, 0.07f, 0.09f);

    [Header("Cell numbers")]
    public Color cellText = Color.white;
    public Color cellTextError = new Color(0.94f, 0.37f, 0.45f);

    [Header("Notes")]
    public Color noteText = new Color(0.57f, 0.60f, 0.71f);
    public Color noteHighlight = new Color(0.42f, 0.49f, 0.94f);
}
