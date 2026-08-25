using UnityEngine;

public class ColorSaver
{
    public static void SaveColor(string key, Color color)
    {
        string colorString = ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString(key, colorString);
        PlayerPrefs.Save();
    }

    public static Color LoadColor(string key, Color defaultColor)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string colorString = PlayerPrefs.GetString(key);
            if (ColorUtility.TryParseHtmlString("#" + colorString, out Color color))
            {
                return color;
            }
        }
        return defaultColor;
    }
}
