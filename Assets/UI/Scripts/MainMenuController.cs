using System.ComponentModel.Design;
using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;
using UnityEngine.UI;

public class ContinueGameSignal : ASignal { }
public class PlayButtonClickedSignal : ASignal { }
public class SettingsButtonClickedSignal : ASignal { }

public class MainMenuController : AWindowController
{
    [SerializeField] Button playButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;

    new private void Awake()
    {
        playButton.onClick.AddListener(PlayButtonClicked);
        continueButton.onClick.AddListener(ContinueButtonClicked);
        settingsButton.onClick.AddListener(SettingsButtonClicked);
        exitButton.onClick.AddListener(ExitButtonClicked);

        Signals.Get<MainMenuSignal>().AddListener(CheckContinue);
    }

    private void Start() {
        CheckContinue();
    }

    private void CheckContinue() => continueButton.gameObject.SetActive(GameWindowController.instance.hasPreviousSave);

    private void PlayButtonClicked() => Signals.Get<PlayButtonClickedSignal>().Dispatch();
    private void ContinueButtonClicked() => Signals.Get<ContinueGameSignal>().Dispatch();
    private void SettingsButtonClicked() => Signals.Get<SettingsButtonClickedSignal>().Dispatch();
    private void ExitButtonClicked() => Application.Quit();
}
