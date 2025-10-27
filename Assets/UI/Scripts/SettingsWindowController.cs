using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindowController : AWindowController
{
    [SerializeField] Button backButton;

    new private void Awake()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked() => Signals.Get<MainMenuSignal>().Dispatch();
}
