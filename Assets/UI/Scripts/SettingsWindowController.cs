using System;
using deVoid.UIFramework;
using deVoid.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OnSettingsChangedSignal : ASignal { }

public class SettingsWindowController : AWindowController
{
    [SerializeField] Button backButton;
    [SerializeField] SliderToggle checkErrorsToggle;
    [SerializeField] SliderToggle disableQuicknoteToggle;
    [SerializeField] SliderToggle darkThemeToggle;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);

        checkErrorsToggle.SetState(PlayerPrefBool.GetBool(SettingsKeys.checkErrors, true));
        disableQuicknoteToggle.SetState(PlayerPrefBool.GetBool(SettingsKeys.disableQuicknote, false));
        darkThemeToggle.SetState(PlayerPrefBool.GetBool(SettingsKeys.darkTheme, true));
    }

    public void OnCheckErrorsChanged(bool value)
    {
        PlayerPrefBool.SetBool(SettingsKeys.checkErrors, value);
        TriggerSettingsChanged();
    }

    public void SetDarkTheme(bool value)
    {
        PlayerPrefs.SetString(SettingsKeys.darkTheme, value ? "true" : "false");
    }

    private void OnBackButtonClicked() => Signals.Get<MainMenuSignal>().Dispatch();
    private void TriggerSettingsChanged() => Signals.Get<OnSettingsChangedSignal>().Dispatch();

    private class PlayerPrefBool {
        public static bool GetBool(string key, bool defaultValue)
        {
            string value = PlayerPrefs.GetString(key, defaultValue.ToString());
            return bool.Parse(value);
        }

        public static void SetBool(string key, bool value)
        {
            string stringValue = value.ToString();
            PlayerPrefs.SetString(key, stringValue);
        }
    }
}

public class SettingsKeys
{
    public const string checkErrors = "check_errors";
    public const string disableQuicknote = "disable_quicknote";
    public const string backgroundColor = "background_color";
    public const string darkTheme = "dark_theme";
}