using UnityEngine;
using deVoid.UIFramework;
using deVoid.Utils;
using System.Collections.Generic;

public class MainMenuSignal : ASignal { }

public class UIController : MonoBehaviour
{
    [SerializeField] UISettings settings;

    private UIFrame uiFrame;

    private Stack<string> screenHistory = new Stack<string>();
    private string currentScreen;

    void Awake()
    {
        Signals.Get<MainMenuSignal>().AddListener(ShowMainMenu);
        Signals.Get<SettingsButtonClickedSignal>().AddListener(ShowSettings);
        Signals.Get<StartNewGameSignal>().AddListener(StartNewGame);
        Signals.Get<ContinueGameSignal>().AddListener(ContinueGame);

        Signals.Get<PlayButtonClickedSignal>().AddListener(ShowDifficultySelect);

        Signals.Get<ShowConfirmationPopupSignal>().AddListener(ShowConfirmationPopup);
    }

    void OnDestroy()
    {
        Signals.Get<MainMenuSignal>().RemoveListener(ShowMainMenu);
        Signals.Get<SettingsButtonClickedSignal>().RemoveListener(ShowSettings);
        Signals.Get<StartNewGameSignal>().RemoveListener(StartNewGame);
        Signals.Get<ContinueGameSignal>().RemoveListener(ContinueGame);

        Signals.Get<PlayButtonClickedSignal>().RemoveListener(ShowDifficultySelect);

        Signals.Get<ShowConfirmationPopupSignal>().RemoveListener(ShowConfirmationPopup);
    }

    private void Start()
    {
        uiFrame = settings.CreateUIInstance();
        uiFrame.OpenWindow(ScreenIds.mainMenu);
        currentScreen = ScreenIds.mainMenu;
    }

    private void ShowMainMenu() => uiFrame.OpenWindow(ScreenIds.mainMenu);

    private void StartNewGame(int difficulty)
    {
        uiFrame.OpenWindow(ScreenIds.gameWindow);
    }

    private void ContinueGame() => uiFrame.OpenWindow(ScreenIds.gameWindow);
    private void ShowSettings() => uiFrame.OpenWindow(ScreenIds.settignsMenu);
    private void ShowDifficultySelect() => uiFrame.OpenWindow(ScreenIds.difficultySelect);

    private void ShowConfirmationPopup(ConfirmationPopupProperties props)
    {
        uiFrame.OpenWindow(ScreenIds.confirmationPrompt, props);
    }

    private static class ScreenIds
    {
        public const string mainMenu = "MainMenu";
        public const string difficultySelect = "DifficultySelect";
        public const string settignsMenu = "settingsMenu";
        public const string gameWindow = "WindowInGame";
        public const string confirmationPrompt = "ConfirmationPrompt";
    }
}
