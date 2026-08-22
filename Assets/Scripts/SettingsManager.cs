using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private BoardThemeSO darkBoardTheme;
    [SerializeField] private BoardThemeSO lightBoardTheme;

    public static SettingsManager instance;
    private Action<Settings> onSettingsChanged;

    private Settings settings = new Settings();

    private BoardThemeSO ActiveBoardTheme => settings.darkTheme ? darkBoardTheme : lightBoardTheme;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        ReactBridge.Instance.SetGlobal(
            Commands.ChangeSetting,
            (Action<string, object>)ChangeSetting
        );
        ReadSettings();
    }

    private void ChangeSetting(string key, object value)
    {
        switch (key)
        {
            case SettingsKeys.darkTheme:
                settings.darkTheme = Convert.ToBoolean(value);
                break;
            case SettingsKeys.checkErrors:
                settings.checkErrors = Convert.ToBoolean(value);
                break;
            case SettingsKeys.disableQuickNote:
                settings.disableQuickNote = Convert.ToBoolean(value);
                break;
            case SettingsKeys.hideEvilWarning:
                settings.hideEvilWarning = Convert.ToBoolean(value);
                break;
            default:
                return;
        }

        settings.boardTheme = ActiveBoardTheme;

        PlayerPrefs.SetString(key, value.ToString());
        PlayerPrefs.Save();
        onSettingsChanged?.Invoke(settings);
        ReactBridge.Instance.SetGlobal(key, value);
    }

    private void ReadSettings()
    {
        bool darkTheme = bool.Parse(PrefOrDefault(SettingsKeys.darkTheme, "false"));
        bool checkErrors = bool.Parse(PrefOrDefault(SettingsKeys.checkErrors, "false"));
        bool disableQuickNote = bool.Parse(PrefOrDefault(SettingsKeys.disableQuickNote, "false"));
        bool hideEvilWarning = bool.Parse(PrefOrDefault(SettingsKeys.hideEvilWarning, "false"));

        settings = new Settings
        {
            darkTheme = darkTheme,
            checkErrors = checkErrors,
            disableQuickNote = disableQuickNote,
            hideEvilWarning = hideEvilWarning,
            boardTheme = darkTheme ? darkBoardTheme : lightBoardTheme,
        };

        ReactBridge.Instance.SetGlobal(SettingsKeys.darkTheme, settings.darkTheme);
        ReactBridge.Instance.SetGlobal(SettingsKeys.checkErrors, settings.checkErrors);
        ReactBridge.Instance.SetGlobal(SettingsKeys.disableQuickNote, settings.disableQuickNote);
        ReactBridge.Instance.SetGlobal(SettingsKeys.hideEvilWarning, settings.hideEvilWarning);
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
    public bool hideEvilWarning;

    /// <summary>Colours for the board itself. Set by SettingsManager from darkTheme.</summary>
    public BoardThemeSO boardTheme;
}