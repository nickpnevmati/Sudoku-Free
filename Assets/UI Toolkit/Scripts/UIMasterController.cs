using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMasterController : MonoBehaviour
{
    [SerializeField] UIDocument m_menu, m_settings, m_game;

    VisualElement m_menu_root, m_settings_root, m_game_root;

    VisualElement m_current_screen;

    public static UIMasterController Instance { get; private set; }

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

    private void NavigateTo(VisualElement screen)
    {
        m_current_screen.style.display = DisplayStyle.None;
        screen.style.display = DisplayStyle.Flex;
        m_current_screen = screen;
    }

    public void NavigateToMenu() => NavigateTo(m_menu_root);
    public void NavigateToSettings() => NavigateTo(m_settings_root);
    public void NavigateToGame(bool continueFlag, int difficulty)
    {
        NavigateTo(m_game_root);
        GameUIController.Instance.RequestStartGame(continueFlag, difficulty);
    }
}
