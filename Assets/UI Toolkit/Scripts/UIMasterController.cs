using UnityEngine;
using UnityEngine.UIElements;

public class UIMasterController : MonoBehaviour
{
    [SerializeField] UIDocument m_menu, m_settings, m_game;

    VisualElement m_menu_root, m_settings_root, m_game_root;

    VisualElement m_current_screen;

    public static UIMasterController Instance { get; private set; }

    const string k_darkThemeClass = "theme_dark";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        m_menu_root = m_menu.rootVisualElement;
        m_settings_root = m_settings.rootVisualElement;
        m_game_root = m_game.rootVisualElement;
        m_current_screen = m_menu_root;
        m_game_root.style.display = DisplayStyle.None;
        m_settings_root.style.display = DisplayStyle.None;
    }

    // The panel isn't guaranteed to be attached during Awake
    private void Start() => ApplyTheme();

    public void SetDarkTheme(bool dark)
    {
        GameSettings.DarkTheme = dark;
        ApplyTheme();
    }

    /// <summary>
    /// All three documents share one PanelSettings, so toggling the class on the panel
    /// root re-themes every screen at once - the palette variables in Theme.uss inherit down.
    /// </summary>
    private void ApplyTheme()
    {
        m_menu_root.panel.visualTree.EnableInClassList(k_darkThemeClass, GameSettings.DarkTheme);
    }

    private void NavigateTo(VisualElement screen)
    {
        m_current_screen.style.display = DisplayStyle.None;
        screen.style.display = DisplayStyle.Flex;
        m_current_screen = screen;
    }

    public void NavigateToMenu()
    {
        m_menu.GetComponent<MenuUIController>().Refresh();
        NavigateTo(m_menu_root);
    }
    public void NavigateToSettings() => NavigateTo(m_settings_root);

    /// <summary>Back to the menu with the difficulty picker already open ("New game" after a win).</summary>
    public void NavigateToDifficultySelect()
    {
        NavigateToMenu();
        m_menu.GetComponent<MenuUIController>().ShowDifficultyModal();
    }
    public void NavigateToGame(bool continueFlag, int difficulty)
    {
        NavigateTo(m_game_root);
        GameUIController.Instance.RequestStartGame(continueFlag, difficulty);
    }
}
