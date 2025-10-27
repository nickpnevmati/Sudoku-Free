using deVoid.UIFramework;
using deVoid.Utils;
using UnityEngine;
using UnityEngine.UI;

public class StartNewGameSignal : ASignal<int> { }

public class DifficultySelectionCtrl : AWindowController
{
    [SerializeField] Button easyButton, mediumButton, hardButton, evilButton, backButton;


    new private void Awake()
    {
        easyButton.onClick.AddListener(delegate { StartGame(0); });
        mediumButton.onClick.AddListener(delegate { StartGame(1); });
        hardButton.onClick.AddListener(delegate { StartGame(2); });
        evilButton.onClick.AddListener(delegate { StartGame(3); });
        backButton.onClick.AddListener(Signals.Get<MainMenuSignal>().Dispatch);
    }

    private void StartGame(int difficulty)
    {
        Signals.Get<StartNewGameSignal>().Dispatch(difficulty);
    }
}
