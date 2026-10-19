using UnityEngine;
using UnityEngine.UIElements;

public class MenuUIController : MonoBehaviour
{
    UIDocument m_document;

    VisualElement m_root;

    Button m_newGameButton, m_settingsButton, m_continueButton, m_exitButton;

    // Difficulty modal
    VisualElement m_difficultyModal, m_difficultyBackdrop;
    Button[] m_difficultyButtons;
    System.Action[] m_difficultyCallbacks;

    private void Awake()
    {
        m_document = GetComponent<UIDocument>();
        m_root = m_document.rootVisualElement;
        m_newGameButton = m_root.Q<Button>("PlayButton");
        m_settingsButton = m_root.Q<Button>("SettingsButton");
        m_continueButton = m_root.Q<Button>("ContinueButton");
        m_exitButton = m_root.Q<Button>("ExitButton");

        m_difficultyModal = m_root.Q<VisualElement>("DifficultyModal");
        m_difficultyBackdrop = m_root.Q<VisualElement>("DifficultyBackdrop");

        (string name, PuzzleDifficulty difficulty)[] options =
        {
            ("EasyButton", PuzzleDifficulty.EASY),
            ("MediumButton", PuzzleDifficulty.MEDIUM),
            ("HardButton", PuzzleDifficulty.HARD),
            ("EvilButton", PuzzleDifficulty.EVIL),
        };
        m_difficultyButtons = new Button[options.Length];
        m_difficultyCallbacks = new System.Action[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            int difficulty = (int)options[i].difficulty;
            m_difficultyButtons[i] = m_root.Q<Button>(options[i].name);
            m_difficultyCallbacks[i] = () => OnDifficultySelected(difficulty);
        }

        HideDifficultyModal();
    }

    private void Start() => Refresh();

    /// <summary>
    /// Sync the menu with state that can change while it's hidden. Screens switch
    /// via style.display rather than enabling GameObjects, so OnEnable won't fire on
    /// return - UIMasterController calls this whenever it navigates back here.
    /// </summary>
    public void Refresh()
    {
        m_continueButton.style.display = PuzzleLoader.hasPreviousSave ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnEnable()
    {
        m_newGameButton.clicked += OnNewGameClicked;
        m_continueButton.clicked += OnContinueClicked;
        m_settingsButton.clicked += OnSettingsClicked;
        m_exitButton.clicked += OnExitClicked;

        m_difficultyBackdrop.RegisterCallback<ClickEvent>(OnBackdropClicked);
        for (int i = 0; i < m_difficultyButtons.Length; i++)
            m_difficultyButtons[i].clicked += m_difficultyCallbacks[i];
    }

    private void OnDisable()
    {
        m_newGameButton.clicked -= OnNewGameClicked;
        m_continueButton.clicked -= OnContinueClicked;
        m_settingsButton.clicked -= OnSettingsClicked;
        m_exitButton.clicked -= OnExitClicked;

        m_difficultyBackdrop.UnregisterCallback<ClickEvent>(OnBackdropClicked);
        for (int i = 0; i < m_difficultyButtons.Length; i++)
            m_difficultyButtons[i].clicked -= m_difficultyCallbacks[i];
    }

    private void OnNewGameClicked() => ShowDifficultyModal();
    private void OnContinueClicked()
    {
        UIMasterController.Instance.NavigateToGame(true, 0);
    }
    private void OnSettingsClicked() => UIMasterController.Instance.NavigateToSettings();
    private void OnExitClicked() => Application.Quit();

    // Clicks on the panel and its buttons bubble up here too, so only dismiss
    // when the backdrop itself was hit
    private void OnBackdropClicked(ClickEvent e)
    {
        if (e.target == m_difficultyBackdrop) HideDifficultyModal();
    }

    private void OnDifficultySelected(int difficulty)
    {
        HideDifficultyModal();
        UIMasterController.Instance.NavigateToGame(false, difficulty);
    }

    private void ShowDifficultyModal() => m_difficultyModal.style.display = DisplayStyle.Flex;
    private void HideDifficultyModal() => m_difficultyModal.style.display = DisplayStyle.None;
}
