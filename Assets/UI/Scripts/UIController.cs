using UnityEngine;
using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine.UI;

public class MainMenuSignal : ASignal { }

public class UIController : MonoBehaviour
{
    [SerializeField] UISettings settings;

    [SerializeField] Image backgroundImage;

    private UIFrame uiFrame;

    void Awake()
    {
        Signals.Get<MainMenuSignal>().AddListener(ShowMainMenu);
        Signals.Get<SettingsButtonClickedSignal>().AddListener(ShowSettings);
        Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
        Signals.Get<StartNewGameSignal>().AddListener(OnStartNewGameSignal);
        Signals.Get<ContinueGameSignal>().AddListener(OnContinueGameSignal);

        Signals.Get<PlayButtonClickedSignal>().AddListener(ShowDifficultySelect);

        Signals.Get<ShowConfirmationPopupSignal>().AddListener(ShowConfirmationPopup);
    }

    void OnDestroy()
    {
        Signals.Get<MainMenuSignal>().RemoveListener(ShowMainMenu);
        Signals.Get<SettingsButtonClickedSignal>().RemoveListener(ShowSettings);
        Signals.Get<OnSettingsChangedSignal>().RemoveListener(OnSettingsChanged);
        Signals.Get<StartNewGameSignal>().RemoveListener(OnStartNewGameSignal);
        Signals.Get<ContinueGameSignal>().RemoveListener(OnContinueGameSignal);

        Signals.Get<PlayButtonClickedSignal>().RemoveListener(ShowDifficultySelect);

        Signals.Get<ShowConfirmationPopupSignal>().RemoveListener(ShowConfirmationPopup);
    }

    private void Start()
    {
        uiFrame = settings.CreateUIInstance();
        uiFrame.OpenWindow(ScreenIds.mainMenu);
    }

    private void ShowMainMenu() => uiFrame.OpenWindow(ScreenIds.mainMenu);
    private void OnStartNewGameSignal(int difficulty) => uiFrame.OpenWindow(ScreenIds.gameWindow);
    private void OnContinueGameSignal() => uiFrame.OpenWindow(ScreenIds.gameWindow);
    private void ShowSettings() => uiFrame.OpenWindow(ScreenIds.settignsMenu);
    private void ShowDifficultySelect() => uiFrame.OpenWindow(ScreenIds.difficultySelect);

    private void OnSettingsChanged()
    {
        backgroundImage.color = ColorSaver.LoadColor("background_color", Color.white);
    }

    private void ShowConfirmationPopup(ConfirmationPopupProperties props)
    {
        uiFrame.OpenWindow(ScreenIds.confirmationPrompt, props);
    }

    private static class ScreenIds
    {
        public const string mainMenu = "MainMenu";
        public const string difficultySelect = "DifficultySelect";
        public const string settignsMenu = "SettingsWindow";
        public const string gameWindow = "WindowInGame";
        public const string confirmationPrompt = "ConfirmationPrompt";
    }
}
