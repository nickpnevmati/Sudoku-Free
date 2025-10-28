using System.Collections;
using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindowController : AWindowController
{
    [SerializeField] Button backButton;
    [SerializeField] SliderToggle checkErrorsToggle;
    [SerializeField] SliderToggle disableQuicknoteToggle;
    [SerializeField] SliderToggle darkThemeToggle;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);

        checkErrorsToggle.SetState(SettingsManager.instance.checkErrors);
        disableQuicknoteToggle.SetState(SettingsManager.instance.disableQuicknote);
        darkThemeToggle.SetState(SettingsManager.instance.isDarkTheme);
    }

    public void ToggleCheckErrors() => SettingsManager.instance.checkErrors = !SettingsManager.instance.checkErrors;
    public void ToggleQuicknote() => SettingsManager.instance.disableQuicknote = !SettingsManager.instance.disableQuicknote;
    public void ToggleTheme() => SettingsManager.instance.ToggleTheme();

    private void OnBackButtonClicked() => Signals.Get<MainMenuSignal>().Dispatch();
}