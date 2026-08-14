using System;
using ReactUnity.UGUI;
using UnityEngine;

[RequireComponent(typeof(ReactRendererUGUI))]
public class ReactTest : MonoBehaviour
{
    [SerializeField] GameObject boardPrefab;

    ReactRendererUGUI react;

    void Awake()
    {
        react = GetComponent<ReactRendererUGUI>();
        react.Globals["startGame"] = (Action<int>)StartGameWithDifficulty;
        react.Globals["exitGame"] = (Action)Application.Quit;
        react.Globals["navigateTo"] = (Action<string>)NavigateTo;
        react.Globals["screen"] = "menu";

        react.Globals["boardPrefab"] = boardPrefab;

        react.Globals["hasPreviousSave"] = PuzzleLoader.hasPreviousSave;
    }

    public void NavigateTo(string screen) => react.Globals["screen"] = screen;

    public void StartGameWithDifficulty(int difficulty)
    {
        react.Globals["screen"] = "gameScreen";
    }
}
