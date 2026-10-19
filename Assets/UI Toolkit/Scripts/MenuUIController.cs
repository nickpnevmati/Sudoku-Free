using UnityEngine;
using UnityEngine.UIElements;

// TODO conditionally disable TODO button

public class MenuUIController : MonoBehaviour
{
    UIDocument m_document;

    VisualElement m_root;

    VisualElement m_newGameButton, m_settingsButton, m_continueButton, m_exitButton;

    private void Awake()
    {
        m_document = GetComponent<UIDocument>();
        m_root = m_document.rootVisualElement;
        m_newGameButton = m_root.Q<Button>("PlayButton");
        m_settingsButton = m_root.Q<Button>("SettingsButton");
        m_continueButton = m_root.Q<Button>("ContinueButton");
        m_exitButton = m_root.Q<Button>("ExitButton");
    }

    private void OnEnable()
    {
        m_newGameButton.RegisterCallback<MouseUpEvent>(OnNewGameClicked);
        m_continueButton.RegisterCallback<MouseUpEvent>(OnContinueClicked);
        m_settingsButton.RegisterCallback<MouseUpEvent>(OnSettingsClicked);
        m_exitButton.RegisterCallback<MouseUpEvent>(OnExitClicked);
    }

    private void OnDisable()
    {
        m_newGameButton.UnregisterCallback<MouseUpEvent>(OnNewGameClicked);
        m_continueButton.UnregisterCallback<MouseUpEvent>(OnContinueClicked);
        m_settingsButton.UnregisterCallback<MouseUpEvent>(OnSettingsClicked);
        m_exitButton.UnregisterCallback<MouseUpEvent>(OnExitClicked);
    }

    private void OnNewGameClicked(MouseUpEvent e)
    {
        // TODO open difficulty select modal
        UIMasterController.Instance.NavigateToGame(false, 0);
    }
    private void OnContinueClicked(MouseUpEvent e)
    {
        UIMasterController.Instance.NavigateToGame(true, 0);
    }
    private void OnSettingsClicked(MouseUpEvent e) => UIMasterController.Instance.NavigateToSettings();
    private void OnExitClicked(MouseUpEvent e) => Application.Quit();
}
