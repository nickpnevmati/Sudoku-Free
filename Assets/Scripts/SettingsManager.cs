using System;
using UnityEngine;
using ReactUnity.UGUI;

[RequireComponent(typeof(ReactRendererUGUI))]
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private BoardThemeSO darkBoardTheme;
    [SerializeField] private BoardThemeSO lightBoardTheme;

    public static SettingsManager instance;
    private Action<Settings> onSettingsChanged;

    private ReactRendererUGUI react;

    private Settings settings;

    private BoardThemeSO ActiveBoardTheme => settings.darkTheme ? darkBoardTheme : lightBoardTheme;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        react = GetComponent<ReactRendererUGUI>();

        react.Globals["changeSetting"] = (Action<string, object>)ChangeSetting;
        ReadSettings();
    }

    private void ChangeSetting(string key, object value)
    {
        switch (key)
        {
            case "darkTheme":
                settings.darkTheme = Convert.ToBoolean(value);
                break;
            case "checkErrors":
                settings.checkErrors = Convert.ToBoolean(value);
                break;
            case "disableQuickNote":
                settings.disableQuickNote = Convert.ToBoolean(value);
                break;
            default:
                return;
        }

        settings.boardTheme = ActiveBoardTheme;

        PlayerPrefs.SetString(key, value.ToString());
        PlayerPrefs.Save();
        onSettingsChanged?.Invoke(settings);
        react.Globals[key] = value;
    }

    private void ReadSettings()
    {
        bool darkTheme = bool.Parse(PrefOrDefault("darkTheme", "false"));
        bool checkErrors = bool.Parse(PrefOrDefault("checkErrors", "false"));
        bool disableQuickNote = bool.Parse(PrefOrDefault("disableQuickNote", "false"));
        
        settings = new Settings
        {
            darkTheme = darkTheme,
            checkErrors = checkErrors,
            disableQuickNote = disableQuickNote,
        };
        settings.boardTheme = ActiveBoardTheme;

        react.Globals["darkTheme"] = settings.darkTheme;
        react.Globals["checkErrors"] = settings.checkErrors;
        react.Globals["disableQuickNote"] = settings.disableQuickNote;
    }

    /// <summary>
    /// Subscribers are invoked immediately with the current settings. The board prefab is
    /// instantiated by React long after Awake, so without this replay it would never see a
    /// theme until the user happened to toggle one.
    /// </summary>
    public void Subscribe(Action<Settings> callback)
    {
        onSettingsChanged += callback;
        callback?.Invoke(settings);
    }

    public void Unsubscribe(Action<Settings> callback) => onSettingsChanged -= callback;

    void OnDestroy()
    {
        onSettingsChanged = null;
        if (instance == this) instance = null;
    }

    private string PrefOrDefault(string key, string defaultValue)
    {
        string value = PlayerPrefs.GetString(key);
        if (value.Length == 0) return defaultValue;
        return value;
    }
}

public class Settings
{
    public bool darkTheme;
    public bool checkErrors;
    public bool disableQuickNote;

    /// <summary>Colours for the board itself. Set by SettingsManager from darkTheme.</summary>
    public BoardThemeSO boardTheme;
}