using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Theme")]
public class ThemeSO : ScriptableObject
{
    public Color primary;
    public Color secondary;
    public Color background;
    public Color surface;

    public Color textPrimary;
    public Color textSecondary;

    public Color border;
    public Color success;
    public Color error;
}
