using UnityEngine;
using UnityEngine.UI;
using deVoid.Utils;
using deVoid.UIFramework;
using System.Collections.Generic;
using System.Linq;

public class GameWindowController : AWindowController
{
    [Header("Custom Properties")]
    [SerializeField] NumpadController numpad;
    [SerializeField] SudokuGridController gridController;
    [SerializeField] Toggle fastModeToggle;
    [SerializeField] Toggle noteToggle;
    [SerializeField] Button undoButton;
    [SerializeField] Toggle quickNoteToggle;
    [SerializeField] Button exitButton;

    private bool fastMode;
    private bool noteMode;

    public int? selectedNumber;
    public int? selectedCell;

    private Stack<(int, int?, List<int>)> history = new Stack<(int, int?, List<int>)>();

    private string puzzle, solution;

    private string savePath;

    public bool hasPreviousSave => System.IO.File.Exists(savePath);

    public static GameWindowController instance;

    private PuzzleLoader puzzleLoader;

    new private void Awake()
    {
        instance = this;

        puzzleLoader = new PuzzleLoader();

        gridController.onCellClicked += HandleCellClicked;
        numpad.onButtonClicked += HandleNumpadClicked;
        fastModeToggle.onValueChanged.AddListener(SetFastMode);
        noteToggle.onValueChanged.AddListener(value => noteMode = value);
        undoButton.onClick.AddListener(Undo);
        quickNoteToggle.onValueChanged.AddListener(SetQuickNote);
        exitButton.onClick.AddListener(ExitGame);

        savePath = System.IO.Path.Combine(Application.persistentDataPath, "savefile");

        Signals.Get<StartNewGameSignal>().AddListener(StartGame);
        Signals.Get<ContinueGameSignal>().AddListener(ContinueGame);
    }

    new private void OnDestroy()
    {
        Signals.Get<ContinueGameSignal>().RemoveListener(ContinueGame);
        Signals.Get<StartNewGameSignal>().RemoveListener(StartGame);
        gridController.onCellClicked -= HandleCellClicked;
    }

    private void StartGame(int difficulty)
    {
        int randomIndex = Mathf.FloorToInt(Random.Range(0, 100));
        (puzzle, solution) = puzzleLoader.LoadPuzzle(randomIndex);

        InitializeGame();
    }

    private void ContinueGame()
    {        
        if (!hasPreviousSave) return;

        (puzzle, solution) = puzzleLoader.LoadPuzzle(System.IO.File.ReadAllText(savePath));

        InitializeGame();
    }

    private void InitializeGame()
    {
        SetQuickNote(false);
        quickNoteToggle.isOn = false;

        SetFastMode(false);
        fastModeToggle.isOn = false;

        noteMode = false;
        noteToggle.isOn = false;

        gridController.Deselect();

        gridController.ConstructPuzzle(puzzle);
    }

    private void ExitGame()
    {
        Signals.Get<ShowConfirmationPopupSignal>().Dispatch(
            new ConfirmationPopupProperties(
                title: "Exit Game",
                message: "Are you sure you want exit the game?",
                confirmButtonText: "Yes, exit",
                confirmAction: delegate
                {
                    Signals.Get<MainMenuSignal>().Dispatch();
                },
                cancelButtonText: "No, stay",
                cancelAction: delegate { }
            )
        );
    }

    private void SetFastMode(bool value)
    {
        gridController.Deselect();

        fastMode = value;
        if (!fastMode) selectedNumber = null;
        numpad.SetModeToggle(fastMode);
    }

    private void SetQuickNote(bool value)
    {
        gridController.QuickNote(value);

        // fuck this if statement in particular
        if (value)
        {
            if (selectedNumber != null)
                gridController.HighlightNumber((int)selectedNumber);

            if (selectedCell != null && gridController.GetCellData((int)selectedCell).Item2 != null)
                gridController.HighlightNumber((int)gridController.GetCellData((int)selectedCell).Item2);
        }
    }

    private void HandleNumpadClicked(int? number)
    {
        if (number == null) return;

        if (fastMode)
        {
            selectedNumber = number;
            gridController.HighlightNumber((int)number);
            return;
        }

        if (selectedCell == null) return;


        history.Push(gridController.GetCellData((int)selectedCell));
        gridController.PaintCell((int)selectedCell, (int)number, noteMode);

        System.IO.File.WriteAllText(savePath, gridController.GetGridAsString() + solution);
        CheckCorrect((int)selectedCell, (int)number);
    }

    private void HandleCellClicked(int cellIndex)
    {
        if (!fastMode)
        {
            selectedCell = cellIndex == selectedCell ? null : cellIndex;
            return;
        }

        var cellData = gridController.GetCellData(cellIndex);
        if (cellData.Item2 != null)
        {
            selectedNumber = cellData.Item2;
            numpad.SetActiveToggle((int)cellData.Item2);
            return;
        }

        history.Push(cellData);
        gridController.PaintCell(cellIndex, (int)selectedNumber, noteMode);
        gridController.HighlightNumber((int)selectedNumber);

        System.IO.File.WriteAllText(savePath, gridController.GetGridAsString() + solution);
        CheckCorrect(cellIndex, (int)selectedNumber);
    }

    private void Undo()
    {
        if (history.Count == 0) return;

        (int cellIndex, int? number, List<int> notes) = history.Pop();
        gridController.PaintCell(cellIndex, number, false);
        foreach (var note in notes) gridController.PaintCell(cellIndex, note, true);
    }

    private void CheckCorrect(int cellIndex, int number)
    {
        if (noteMode) return;
        gridController.ResetCellColor(cellIndex);
        if (solution[cellIndex].ToString() == number.ToString())
        {
            CheckFinished();
            return;
        }

        Debug.Log($"Incorrect input provided {number} -- Correct input: {solution[cellIndex]}");

        gridController.PaintCellError(cellIndex);
    }

    private void CheckFinished()
    {
        string grid = gridController.GetGridAsString();
        if (grid != solution) return;

        if (hasPreviousSave) System.IO.File.Delete(savePath);
        Signals.Get<ShowConfirmationPopupSignal>().Dispatch(GetPopupProperties());
    }

    private ConfirmationPopupProperties GetPopupProperties()
    {
        return new ConfirmationPopupProperties(
            title: "Good Job!",
            message: "Play again?",
            confirmButtonText: "Yes!",
            confirmAction: delegate { Signals.Get<StartNewGameSignal>().Dispatch(0); },
            cancelButtonText: "Nah",
            cancelAction: delegate {}
        );
    }
}
