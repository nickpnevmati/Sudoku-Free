using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsUIController : MonoBehaviour
{
    UIDocument m_document;

    VisualElement m_root;

    Button m_backButton;

    SettingToggle[] m_toggles;

    private void Awake()
    {
        m_document = GetComponent<UIDocument>();
        m_root = m_document.rootVisualElement;
        m_backButton = m_root.Q<Button>("BackButton");

        m_toggles = new[]
        {
            new SettingToggle(m_root.Q("DarkThemeToggle"),
                () => GameSettings.DarkTheme,
                value => UIMasterController.Instance.SetDarkTheme(value)),
            new SettingToggle(m_root.Q("CheckErrorsToggle"),
                () => GameSettings.CheckErrors,
                value => GameSettings.CheckErrors = value),
            new SettingToggle(m_root.Q("DisableQuickNoteToggle"),
                () => GameSettings.DisableQuickNote,
                value => GameSettings.DisableQuickNote = value),
            new SettingToggle(m_root.Q("DisableWinAnimationToggle"),
                () => GameSettings.DisableWinAnimation,
                value => GameSettings.DisableWinAnimation = value),
        };
    }

    private void OnEnable()
    {
        m_backButton.clicked += OnBackClicked;
        foreach (var toggle in m_toggles) toggle.Register();
    }

    private void OnDisable()
    {
        m_backButton.clicked -= OnBackClicked;
        foreach (var toggle in m_toggles) toggle.Unregister();
    }

    // Goes through NavigateToMenu so the menu refreshes on the way back
    private void OnBackClicked() => UIMasterController.Instance.NavigateToMenu();

    /// <summary>
    /// Binds a CustomToggle instance to a setting. The whole row is the hit area, and the
    /// "checked" class on it drives the checkbox visuals in Stylesheet.uss.
    /// </summary>
    private class SettingToggle
    {
        readonly VisualElement row;
        readonly Func<bool> get;
        readonly Action<bool> set;

        public SettingToggle(VisualElement instance, Func<bool> get, Action<bool> set)
        {
            row = instance.Q(className: "custom_toggle");
            this.get = get;
            this.set = set;
            row.EnableInClassList("checked", get());
        }

        public void Register() => row.RegisterCallback<ClickEvent>(OnClicked);
        public void Unregister() => row.UnregisterCallback<ClickEvent>(OnClicked);

        private void OnClicked(ClickEvent e)
        {
            bool value = !get();
            set(value);
            row.EnableInClassList("checked", value);
        }
    }
}
