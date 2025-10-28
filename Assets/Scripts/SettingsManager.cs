using UnityEngine;
using deVoid.Utils;
using UnityEngine.UI;

public class OnSettingsChangedSignal : ASignal<Settings> { }

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private ThemeSO darkTheme, lightTheme;
    [SerializeField] private Image backgroundImage;

    public static SettingsManager instance;

    public ThemeSO theme
    {
        set
        {
            settings.theme = value;
            TriggerUpdate();
        }
        get { return settings.theme; }
    }

    public bool isDarkTheme { get { return theme == darkTheme; } }

    public bool checkErrors
    {
        set
        {
            settings.checkErrors = value;
            TriggerUpdate();
        }
        get { return settings.checkErrors; }
    }

    public bool disableQuicknote
    {
        set
        {
            settings.disableQuicknote = value;
            TriggerUpdate();
        }
        get { return settings.disableQuicknote; }
    }

    private Settings settings;

    private void Awake()
    {
        instance = this;
        settings = new Settings();
        settings.checkErrors = PlayerPrefBool.GetBool(SettingsKeys.checkErrors, true);
        settings.disableQuicknote = PlayerPrefBool.GetBool(SettingsKeys.disableQuicknote, false);
        settings.theme = PlayerPrefBool.GetBool(SettingsKeys.darkTheme, true) ? darkTheme : lightTheme;

        Signals.Get<OnSettingsChangedSignal>().AddListener(SetBackground);
    }

    private void OnDestroy()
    {
        Signals.Get<OnSettingsChangedSignal>().RemoveListener(SetBackground);
    }

    private void Start()
    {
        TriggerUpdate();
    }

    private void TriggerUpdate() => Signals.Get<OnSettingsChangedSignal>().Dispatch(settings);
    private void SetBackground(Settings settings) => backgroundImage.color = settings.theme.background;

    public void ToggleTheme() => theme = theme == darkTheme ? lightTheme : darkTheme;

    private class PlayerPrefBool
    {
        public static bool GetBool(string key, bool defaultValue)
        {
            int value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0);
            return value != 0;
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}

public class Settings
{
    public ThemeSO theme;
    public bool checkErrors, disableQuicknote;
}

public class SettingsKeys
{
    public const string checkErrors = "check_errors";
    public const string disableQuicknote = "disable_quicknote";
    public const string backgroundColor = "background_color";
    public const string darkTheme = "dark_theme";
}