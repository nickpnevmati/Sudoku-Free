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
        react.Globals[Commands.ExitGame] = (Action)Application.Quit;
        react.Globals[Commands.NavigateTo] = (Action<string>)NavigateTo;
        react.Globals[PropertyKeys.screen] = ScreenKeys.MainMenu;

        react.Globals[PropertyKeys.boardPrefab] = boardPrefab;

        react.Globals[FlagKeys.hasPreviousSave] = PuzzleLoader.hasPreviousSave;
    }

    public void NavigateTo(string screen) => 
        react.Globals[PropertyKeys.screen] = screen;

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

public class Commands
{
    public const string StartGame = "start_game";
    public const string ContinueGame = "continue_game";
    public const string ExitGame = "exitGame";
    public const string NavigateTo = "navigateTo";
    public const string ChangeSetting = "changeSetting";

    public const string Numpad = "numpad";
    public const string Undo = "undo";
    public const string Erase = "erase";
}

public class PropertyKeys
{
    public const string screen = "screen";
    public const string boardPrefab = "boardPrefab";

    // GameLogicController
    public const string NoteMode = "noteMode";
    public const string FastMode = "fastMode";
    public const string QuickNote = "quickNote";
    public const string EraseMode = "eraseMode";
    public const string Command = "command";
}