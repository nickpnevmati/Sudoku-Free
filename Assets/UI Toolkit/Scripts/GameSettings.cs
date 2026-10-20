using UnityEngine;

/// <summary>
/// Player preferences, persisted in PlayerPrefs. Read these wherever a setting matters
/// rather than caching them, so changes on the settings screen apply immediately.
/// </summary>
public static class GameSettings
{
    public static bool DarkTheme
    {
        get => Get("DarkTheme");
        set => Set("DarkTheme", value);
    }

    public static bool CheckErrors
    {
        get => Get("CheckErrors");
        set => Set("CheckErrors", value);
    }

    public static bool DisableQuickNote
    {
        get => Get("DisableQuickNote");
        set => Set("DisableQuickNote", value);
    }

    public static bool DisableWinAnimation
    {
        get => Get("DisableWinAnimation");
        set => Set("DisableWinAnimation", value);
    }

    private static bool Get(string key) => PlayerPrefs.GetInt(key, 0) == 1;

    private static void Set(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
