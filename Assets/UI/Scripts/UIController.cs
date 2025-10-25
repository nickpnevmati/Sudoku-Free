using UnityEngine;
using deVoid.UIFramework;
using deVoid.Utils;
using System.Collections.Generic;

public class GoBackSignal : ASignal { }

public class UIController : MonoBehaviour
{
    [SerializeField] UISettings settings;

    private UIFrame uiFrame;
    private GameManager gameManager;

    private Stack<string> screenHistory = new Stack<string>();
    private string currentScreen;

    void Awake()
    {
        gameManager = GetComponent<GameManager>();

        Signals.Get<StartNewGameSignal>().AddListener(StartNewGame);
        Signals.Get<ContinueGameSignal>().AddListener(ContinueGame);
        Signals.Get<SettingsButtonClickedSignal>().AddListener(ShowSettings);
        Signals.Get<PlayButtonClickedSignal>().AddListener(ShowDifficultySelect);
        Signals.Get<GoBackSignal>().AddListener(ShowPreviousScreen);
    }

    private void Start()
    {
        uiFrame = settings.CreateUIInstance();
        uiFrame.OpenWindow(ScreenIds.mainMenu);
        currentScreen = ScreenIds.mainMenu;
    }

    private void StartNewGame(int difficulty)
    {
        gameManager.StartGame(difficulty);
        ShowScreenWithHistory(ScreenIds.gameWindow);
    }

    private void ContinueGame()
    {
        gameManager.ContinueGame();
        ShowScreenWithHistory(ScreenIds.gameWindow);
    }

    private void ShowSettings() => ShowScreenWithHistory(ScreenIds.settignsMenu);
    private void ShowDifficultySelect() => ShowScreenWithHistory(ScreenIds.difficultySelect);

    private void ShowScreenWithHistory(string screenId)
    {
        screenHistory.Push(currentScreen);
        currentScreen = screenId;
        uiFrame.OpenWindow(screenId);
    }

    private void ShowPreviousScreen()
    {
        string screenId = screenHistory.Pop();
        uiFrame.OpenWindow(screenId);
        currentScreen = screenId;
    }

    private static class ScreenIds
    {
        public const string mainMenu = "MainMenu";
        public const string difficultySelect = "DifficultySelect";
        public const string settignsMenu = "settingsMenu";
        public const string gameWindow = "WindowInGame";
    }
}
