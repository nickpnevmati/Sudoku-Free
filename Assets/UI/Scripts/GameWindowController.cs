using UnityEngine;
using UnityEngine.UI;
using deVoid.Utils;
using deVoid.UIFramework;
using System.Collections.Generic;

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

    private bool checkErrors;

    public int? selectedNumber;
    public int? selectedCell;

    private string puzzle, solution;
    private HStack<string> history = new HStack<string>();

    public static GameWindowController instance;

    new private void Awake()
    {
        instance = this;

        numpad.onButtonClicked += HandleNumpadClicked;
        gridController.onCellClicked += HandleCellClicked;

        undoButton.onClick.AddListener(Undo);
        exitButton.onClick.AddListener(ExitGamePrompt);
        noteToggle.onValueChanged.AddListener(SetNoteMode);
        fastModeToggle.onValueChanged.AddListener(SetFastMode);
        quickNoteToggle.onValueChanged.AddListener(SetQuickNote);

        Signals.Get<StartNewGameSignal>().AddListener(StartGame);
        Signals.Get<ContinueGameSignal>().AddListener(ContinueGame);
        Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
    }

    private void StartGame(int difficulty)
    {
        int randomIndex = Mathf.FloorToInt(Random.Range(0, 100));
        (puzzle, solution) = PuzzleLoader.LoadPuzzle(randomIndex);
        InitializeGame();
        history.Push(gridController.gridState);
    }

    private void ContinueGame()
    {
        if (!PuzzleLoader.hasPreviousSave) return;
        string[] historyArray;
        (puzzle, solution, historyArray) = PuzzleLoader.LoadSaved();
        history = new HStack<string>(historyArray);

        InitializeGame();

        if (historyArray.Length > 0)
            quickNoteToggle.SetIsOnWithoutNotify(history.Last.Contains('q'));
        gridController.SetGridState(history.Last);
        
        CorrectnessCheckAll();
    }

    private void InitializeGame()
    {
        quickNoteToggle.SetIsOnWithoutNotify(false);
        gridController.QuickNote(false);

        SetFastMode(false);
        fastModeToggle.isOn = false;

        noteMode = false;
        noteToggle.isOn = false;

        gridController.ConstructGrid(puzzle);
    }

    private void ExitGamePrompt()
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

    private void SetNoteMode(bool value) => noteMode = value;

    private void SetFastMode(bool value)
    {
        fastMode = value;
        numpad.SetToggleMode(value);

        if (value)
        {
            int number = selectedCell != null && gridController.GetNumber((int)selectedCell) != null ? (int)gridController.GetNumber((int)selectedCell) : 1;
            numpad.SetActiveToggle(number);
            gridController.HighlightNumbers(number);
        }
    }

    private void SetQuickNote(bool value)
    {
        gridController.QuickNote(value);
        history.Push(gridController.gridState + "q");
        PuzzleLoader.SavePuzzle(puzzle, solution, history.ToArray());
    }

    private void HandleNumpadClicked(int number)
    {
        if (fastMode)
        {
            selectedNumber = number;
            gridController.HighlightNumbers((int)selectedNumber);
        }
        else
        {
            if (selectedCell == null) return;
            if (puzzle[(int)selectedCell] != '0') return;

            if (noteMode)
            {
                gridController.ToggleNote(number, (int)selectedCell);
            }
            else
            {
                gridController.SetNumber(number, (int)selectedCell);
                gridController.SelectCell((int)selectedCell);
            }

            SaveGame();
            CorrectnessCheck(number, (int)selectedCell);
            FinishedCheck();
        }
    }

    private void HandleCellClicked(int cellIndex)
    {
        if (fastMode)
        {
            if (selectedNumber == null) return;

            int? number = gridController.GetNumber(cellIndex);
            if (number != null)
            {
                gridController.HighlightNumbers((int)number);
                numpad.SetActiveToggle((int)number);
                selectedNumber = number;
                return;
            }

            if (noteMode)
            {
                gridController.SetNote((int)selectedNumber, cellIndex);
                gridController.HighlightNumbers((int)selectedNumber);
            }
            else
            {
                gridController.SetNumber((int)selectedNumber, cellIndex);
                gridController.SelectCell(cellIndex);
            }

            SaveGame();
            CorrectnessCheck((int)selectedNumber, cellIndex);
            FinishedCheck();
        }
        else
        {
            gridController.SelectCell(cellIndex);
            selectedCell = cellIndex;
            selectedNumber = gridController.GetNumber(cellIndex);

            if (selectedNumber != null) gridController.HighlightNumbers((int)selectedNumber);
        }
    }

    private void SaveGame()
    {
        history.Push(gridController.gridState);
        PuzzleLoader.SavePuzzle(puzzle, solution, history.ToArray());
    }

    private void Undo()
    {
        if (history.Count <= 1) return;

        string previousState = history.Pop();
        gridController.SetGridState(previousState);
        quickNoteToggle.SetIsOnWithoutNotify(previousState.Contains('q'));
        CorrectnessCheckAll();
    }

    private void CorrectnessCheckAll()
    {
        foreach (var (num, index) in gridController.EnumerateCells()) // Ew
        {
            if (num == null) continue;
            CorrectnessCheck((int)num, index);
        }
    }

    private void CorrectnessCheck(int number, int cellIndex)
    {
        bool isCorrect = !checkErrors || solution[cellIndex].ToString() == number.ToString();
        gridController.SetError(cellIndex, !isCorrect);
    }

    private void FinishedCheck()
    {
        if (gridController.gridString != solution) return;

        Signals.Get<ShowConfirmationPopupSignal>().Dispatch(
            new ConfirmationPopupProperties(
                title: "Good Job!",
                message: "Play again?",
                confirmButtonText: "Yes!",
                confirmAction: delegate { Signals.Get<StartNewGameSignal>().Dispatch(0); },
                cancelButtonText: "No, Exit",
                cancelAction: delegate { Signals.Get<MainMenuSignal>().Dispatch(); }
            )
        );
    }

    private void OnSettingsChanged(Settings settings)
    {
        checkErrors = settings.checkErrors;
    }

    private class HStack<T>
    {
        /// <summary>
        /// Like a regular stack, only pop() discards the head element, and returns the new head 
        /// Yes, it's stupid, no I don't care ¯\_(ツ)_/¯
        /// </summary>

        private Stack<T> stack;

        public int Count => stack.Count;
        public T Last => stack.ToArray()[0];

        public HStack() => stack = new Stack<T>();
        public HStack(IEnumerable<T> collection) => stack = new Stack<T>(collection);

        public void Push(T element) => stack.Push(element);

        public T Pop()
        {
            stack.Pop();
            T value = stack.Pop();
            stack.Push(value);
            return value;
        }

        public T[] ToArray() {
            var arr = stack.ToArray();
            System.Array.Reverse(arr);
            return arr;
        }
    }
}