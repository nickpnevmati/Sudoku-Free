using UnityEngine;
using System.Collections.Generic;
using ReactUnity.UGUI.Behaviours;
using ReactUnity.Helpers;
using ReactUnity.UGUI;
using System;
using Unity.VisualScripting;
using System.Collections;
using ReactUnity;

[RequireComponent(typeof(SudokuGridController))]
public class GameLogicController : MonoBehaviour, IPrefabTarget
{
    private SudokuGridController gridController;

    private bool fastMode;
    private bool noteMode;
    private bool eraseMode;

    private bool checkErrors;

    public int? selectedNumber;
    public int? selectedCell;

    private Callback onCellSelected, onGameFinished;
    private int lastCommandSeq = -1;

    private string puzzle, solution;
    private HStack<string> history = new HStack<string>();

    void DelayCallback(Callback c, params object[] args)
    {
        StartCoroutine(DelayCallbackCoroutine(c, args));
    }

    private IEnumerator DelayCallbackCoroutine(Callback callback, object[] args)
    {
        yield return null;
        callback?.Call(args);
    }

    void Awake()
    {
        gridController = GetComponent<SudokuGridController>();
        gridController.onCellClicked += HandleCellClicked;
        if (SettingsManager.instance != null)
            SettingsManager.instance.Subscribe(OnSettingsChanged);
    }

    public bool SetProperty(string propertyName, object value)
    {
        switch (propertyName)
        {
            // NOTE to future self: this initialization can bite you.
            // React loads these on-mount - before the game is initialized
            // meaning that it can fuck with game state in ways that are hard to debug
            // make sure to account for it.
            case PropertyKeys.NoteMode: SetNoteMode(Convert.ToBoolean(value)); return true;
            case PropertyKeys.FastMode: SetFastMode(Convert.ToBoolean(value)); return true;
            case PropertyKeys.QuickNote: SetQuickNote(Convert.ToBoolean(value)); return true;
            case PropertyKeys.EraseMode: SetEraseMode(Convert.ToBoolean(value)); return true;
            case PropertyKeys.Command: RunCommand(Convert.ToString(value)); return true;
            default: return false;
        }
    }

    public Action AddEventListener(string eventName, Callback callback)
    {
        Debug.Log($"AddEventListener: {eventName} handler={callback != null}");
        switch (eventName)
        {
            case "onCellSelected": onCellSelected = callback; return () => onCellSelected = null;
            case "onGameFinished": onGameFinished = callback; return () => onGameFinished = null;
            default: return null;
        }
    }

    public void Mount(PrefabComponent cmp) { }
    public void Unmount(PrefabComponent cmp) { }

    public void RunCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return;

        var parts = cmd.Split(':');
        if (parts.Length < 2) return;
        if (!int.TryParse(parts[parts.Length - 1], out var seq)) return;
        if (seq == lastCommandSeq) return;
        lastCommandSeq = seq;

        switch (parts[0])
        {
            case Commands.Numpad: HandleNumpadClicked(int.Parse(parts[1])); break;
            case Commands.Undo: Undo(); break;
            case Commands.StartGame: StartGame(int.Parse(parts[1])); break;
            case Commands.ContinueGame: ContinueGame(); break;
            case Commands.Erase: HandleEraseClicked(); break;
        }
    }

    private void StartGame(int difficulty)
    {
        Debug.Log($"GameLogicController: Start Game - difficulty: {difficulty}");
        int randomIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, 100));
        (puzzle, solution) = PuzzleLoader.LoadPuzzle(randomIndex);
        InitializeGame();
        history.Push(gridController.gridState);
    }

    private void ContinueGame()
    {
        if (!PuzzleLoader.hasPreviousSave) return;
        Debug.Log("Continue Game");
        string[] historyArray;
        (puzzle, solution, historyArray) = PuzzleLoader.LoadSaved();
        history = new HStack<string>(historyArray);

        InitializeGame();

        // if (historyArray.Length > 0) // TODO
        //     quickNoteToggle.SetIsOnWithoutNotify(history.Last.Contains('q'));
        gridController.SetGridState(history.Last);

        CorrectnessCheckAll();
        FinishedCheck();
    }

    private void InitializeGame()
    {
        gridController.QuickNote(false);
        SetFastMode(false);
        noteMode = false;
        gridController.ConstructGrid(puzzle);
    }

    private void SetNoteMode(bool value) => noteMode = value;

    private void SetFastMode(bool value)
    {
        fastMode = value;

        if (value)
        {
            bool hasSelectedNumber = selectedCell != null && gridController.GetNumber((int)selectedCell) != null;
            int number = hasSelectedNumber ? (int)gridController.GetNumber((int)selectedCell) : 1;
            selectedNumber = number;
            DelayCallback(onCellSelected, number);
            gridController.HighlightNumbers(number);
        }
    }

    private void SetQuickNote(bool value)
    {
        gridController.QuickNote(value);
        if (history.Count == 0) return;
        history.Push(gridController.gridState + "q");
        PuzzleLoader.SavePuzzle(puzzle, solution, history.ToArray());
    }

    private void SetEraseMode(bool value) => eraseMode = value;

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
        Debug.Log($"GameLogicController: HandleCellClicked - fastMode: {fastMode} - noteMode: {noteMode} - eraseMode: {eraseMode}");
        if (fastMode)
        {
            if (selectedNumber == null) return;

            int? number = gridController.GetNumber(cellIndex);

            if (number != null && !eraseMode)
            {
                gridController.HighlightNumbers((int)number);
                DelayCallback(onCellSelected, (int)number);
                selectedNumber = number;
                return;
            }

            if (noteMode)
            {
                gridController.SetNote((int)selectedNumber, cellIndex);
                gridController.HighlightNumbers((int)selectedNumber);
            }
            else if (eraseMode)
            {
                Debug.Log($"GameLogicController: HandleCellClicked - puzzleAtIndex {puzzle[cellIndex]}");
                if (puzzle[cellIndex] != '0') return;

                gridController.SetNumber(null, cellIndex);
                gridController.ClearNotes(cellIndex);
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

    private void HandleEraseClicked()
    {
        Debug.Log($"GameLogicController: HandleEraseClicked - fastMode: {fastMode} - selectedCell: {selectedCell} - puzzleAtIndex: {(selectedCell != null ? puzzle[(int)selectedCell] : "NaN")}");
        if (fastMode)
        {
            eraseMode = !eraseMode;
            return;
        }

        if (selectedCell == null || puzzle[(int)selectedCell] != '0') return;

        gridController.SetNumber(null, (int)selectedCell);
        gridController.ClearNotes((int)selectedCell);
    }

    private void SaveGame()
    {
        history.Push(gridController.gridState);
        PuzzleLoader.SavePuzzle(puzzle, solution, history.ToArray());
        Debug.Log("Game Saved");
    }

    private void DeleteSave()
    {
        history = new HStack<string>();
        PuzzleLoader.DeleteSave();
    }

    private void Undo()
    {
        if (history.Count <= 1) return;

        string previousState = history.Pop();
        gridController.SetGridState(previousState);
        // quickNoteToggle.SetIsOnWithoutNotify(previousState.Contains('q'));
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
        string grid = gridController.gridString;
        string fmtGrid = grid.Replace(" ", string.Empty);
        bool isFinished = fmtGrid.Equals(solution);
        Debug.Log("Finished Check Returned " + isFinished.ToSafeString());
        if (isFinished)
        {
            DelayCallback(onGameFinished);
            DeleteSave();
        }
    }

    private void OnSettingsChanged(Settings settings)
    {
        checkErrors = settings.checkErrors;
    }

    void OnDestroy()
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.Unsubscribe(OnSettingsChanged);
    }

    private class HStack<T>
    {
        // <summary>
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

        public T[] ToArray()
        {
            var arr = stack.ToArray();
            System.Array.Reverse(arr);
            return arr;
        }
    }
}