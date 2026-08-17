using System;
using ReactUnity.UGUI;
using UnityEngine;

[RequireComponent(typeof(ReactRendererUGUI))]
public class ReactBridge : MonoBehaviour
{
    [SerializeField] GameObject boardPrefab;
    
    ReactRendererUGUI react;

    /// <summary>
    /// Instance should NOT be used in Awake() as it may not be fully initialized yet.
    /// Use in Start() only!
    /// </summary>
    public static ReactBridge Instance { get; set; }

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        react = GetComponent<ReactRendererUGUI>();
        react.Globals[CommandKeys.StartGame] = (Action<int>)StartGameWithDifficulty;
        react.Globals[CommandKeys.ExitGame] = (Action)Application.Quit;
        react.Globals[CommandKeys.NavigateTo] = (Action<string>)NavigateTo;
        react.Globals["screen"] = ScreenKeys.MainMenu;

        react.Globals["boardPrefab"] = boardPrefab;

        react.Globals[FlagKeys.hasPreviousSave] = PuzzleLoader.hasPreviousSave;
    }

    public void NavigateTo(string screen) => 
        react.Globals["screen"] = screen;

    public void StartGameWithDifficulty(int difficulty)
    {
        react.Globals["screen"] = ScreenKeys.GameScreen;
        // TODO?
    }

    public void SetGlobal(string key, object value)
    {
        react.Globals[key] = value;
    }
}

public class ScreenKeys
{
    public const string MainMenu = "mainMenu";
    public const string GameMenu = "gameMenu";
    public const string GameScreen = "gameScreen";
    public const string Settings = "settings";
}

public class FlagKeys
{
    public const string continueGame = "continueGame";
    public const string hasPreviousSave = "hasPreviousSave";
}

public class SettingsKeys
{
    public const string darkTheme = "darkTheme";
    public const string checkErrors = "checkErrors";
    public const string disableQuickNote = "disableQuickNote";
}

public class CommandKeys
{
    public const string StartGame = "startGame";
    public const string ExitGame = "exitGame";
    public const string NavigateTo = "navigateTo";
    public const string ChangeSetting = "changeSetting";
}