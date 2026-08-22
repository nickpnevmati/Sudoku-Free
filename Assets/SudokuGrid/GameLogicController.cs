using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ReactUnity.Helpers;
using ReactUnity.UGUI;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

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

    private Callback onCellSelected,
        onGameFinished,
        onGameReady;
    private int lastCommandSeq = -1;

    // Timer stuff
    bool gameStarted = false,
        gamePaused = false;
    float timer = 0;
    int timerUpdate = 0;

    private string puzzle,
        solution;
    private HStack<string> history = new HStack<string>();

    // MultiThreading Stuff
    const long puzzle_gen_millis = 500;

    // Immutable, so it can be handed to the main thread by a single reference
    // swap - no lock, and the Interlocked publish/consume pair is the barrier
    // that makes the fields visible on the other side.
    private class GenResult
    {
        public string puzzle,
            solution;
    }

    private Thread genThread;
    private CancellationTokenSource genCts;
    private GenResult pendingResult;

    void DelayCallback(Callback c, params object[] args)
    {
        StartCoroutine(DelayCallbackCoroutine(c, args));
    }

    private IEnumerator DelayCallbackCoroutine(Callback callback, object[] args)
    {
        yield return null;
        callback?.Call(args);
    }

    private void Awake()
    {
        gridController = GetComponent<SudokuGridController>();
        gridController.onCellClicked += HandleCellClicked;
        if (SettingsManager.instance != null)
            SettingsManager.instance.Subscribe(OnSettingsChanged);
    }

    private void Start()
    {
        ReactBridge.Instance.SetGlobal(PropertyKeys.solveTime, -1);
    }

    void Update()
    {
        TryInitializeGame();
        HandleTimerUpdate();
    }

    public bool SetProperty(string propertyName, object value)
    {
        switch (propertyName)
        {
            // NOTE to future self: this initialization can bite you.
            // React loads these on-mount - before the game is initialized
            // meaning that it can fuck with game state in ways that are hard to debug
            // make sure to account for it.
            case PropertyKeys.NoteMode:
                SetNoteMode(Convert.ToBoolean(value));
                return true;
            case PropertyKeys.FastMode:
                SetFastMode(Convert.ToBoolean(value));
                return true;
            case PropertyKeys.QuickNote:
                SetQuickNote(Convert.ToBoolean(value));
                return true;
            case PropertyKeys.EraseMode:
                SetEraseMode(Convert.ToBoolean(value));
                return true;
            case PropertyKeys.Command:
                RunCommand(Convert.ToString(value));
                return true;
            case PropertyKeys.GamePaused:
                gamePaused = Convert.ToBoolean(value);
                return true;
            default:
                return false;
        }
    }

    public Action AddEventListener(string eventName, Callback callback)
    {
        // Debug.Log($"AddEventListener: {eventName} handler={callback != null}");
        switch (eventName)
        {
            case "onCellSelected":
                onCellSelected = callback;
                return () => onCellSelected = null;
            case "onGameFinished":
                onGameFinished = callback;
                return () => onGameFinished = null;
            case "onGameReady":
                onGameReady = callback;
                return () => onGameReady = null;
            default:
                return null;
        }
    }

    public void Mount(PrefabComponent cmp) { }

    public void Unmount(PrefabComponent cmp) { }

    public void RunCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd))
            return;

        var parts = cmd.Split(':');
        if (parts.Length < 2)
            return;
        if (!int.TryParse(parts[parts.Length - 1], out var seq))
            return;
        if (seq == lastCommandSeq)
            return;
        lastCommandSeq = seq;

        switch (parts[0])
        {
            case Commands.Numpad:
                HandleNumpadClicked(int.Parse(parts[1]));
                break;
            case Commands.Undo:
                Undo();
                break;
            case Commands.StartGame:
                StartGame(int.Parse(parts[1]));
                break;
            case Commands.ContinueGame:
                ContinueGame();
                break;
            case Commands.Erase:
                HandleEraseClicked();
                break;
        }
    }

    private void StartGame(int difficulty)
    {
        PuzzleDifficulty diff = (PuzzleDifficulty)
            Math.Clamp(difficulty, (int)PuzzleDifficulty.EASY, (int)PuzzleDifficulty.EVIL);

        // A Thread can only be started once, so each generation gets its own.
        if (genThread != null && genThread.IsAlive)
        {
            Debug.Log("GameLogicController: Start Game ignored - already generating");
            return;
        }

        Debug.Log($"GameLogicController: Start Game - difficulty: {diff}");

        Interlocked.Exchange(ref pendingResult, null); // drop a result nobody consumed
        genCts?.Dispose();
        genCts = new CancellationTokenSource();

        // Captured, rather than read off the fields, so the worker keeps working
        // against the token it was started with
        CancellationToken token = genCts.Token;
        genThread = new Thread(() => GenerateGameAsync(diff, token))
        {
            IsBackground = true, // never keep the process alive on its own
            Name = "PuzzleGen",
        };
        genThread.Start();
    }

    private void HandleTimerUpdate()
    {
        if (!gameStarted || gamePaused)
            return;
        timer += Time.deltaTime;
        int timerSec = (int)timer;
        if (timerSec > timerUpdate)
        {
            timerUpdate = timerSec;
            ReactBridge.Instance.SetGlobal(PropertyKeys.solveTime, timerUpdate.ToString());
            PuzzleLoader.SavePuzzle(puzzle, timerUpdate, solution, history.ToArray());
        }
    }

    private void TryInitializeGame()
    {
        // Takes the pending result and clears it in one step, so a puzzle is
        // never initialized twice.
        GenResult result = Interlocked.Exchange(ref pendingResult, null);
        if (result == null)
            return;

        (puzzle, solution) = (result.puzzle, result.solution);
        InitializeGame();
        SaveGame();
    }

    private void GenerateGameAsync(PuzzleDifficulty difficulty, CancellationToken token)
    {
        try
        {
            var watch = Stopwatch.StartNew();

            var (newPuzzle, newSolution) = PuzzleGenerator.Instance.Generate(difficulty, token);

            // Hold the loading screen for a minimum beat, before handing over -
            // publishing first would let the main thread start the game early.
            long sleep = puzzle_gen_millis - watch.ElapsedMilliseconds;
            if (sleep > 0)
                Thread.Sleep((int)sleep);

            token.ThrowIfCancellationRequested();
            Interlocked.Exchange(
                ref pendingResult,
                new GenResult { puzzle = newPuzzle, solution = newSolution }
            );
        }
        catch (OperationCanceledException)
        {
            Debug.Log("GameLogicController: puzzle generation cancelled");
        }
        catch (Exception e)
        {
            // Otherwise the thread dies silently and the game never starts
            Debug.LogError($"GameLogicController: puzzle generation failed - {e}");
        }
    }

    private void ContinueGame()
    {
        if (!PuzzleLoader.hasPreviousSave)
            return;
        Debug.Log("Continue Game");
        string[] historyArray;
        // TODO LeadSaved can throw - handle this
        (puzzle, timerUpdate, solution, historyArray) = PuzzleLoader.LoadSaved();
        timer = timerUpdate;
        history = new HStack<string>(historyArray);

        InitializeGame();

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
        gameStarted = true;
        DelayCallback(onGameReady);
    }

    private void SetNoteMode(bool value) => noteMode = value;

    private void SetFastMode(bool value)
    {
        fastMode = value;

        if (value)
        {
            bool hasSelectedNumber =
                selectedCell != null && gridController.GetNumber((int)selectedCell) != null;
            int number = hasSelectedNumber ? (int)gridController.GetNumber((int)selectedCell) : 1;
            selectedNumber = number;
            DelayCallback(onCellSelected, number);
            gridController.HighlightNumbers(number);
        }
    }

    private void SetQuickNote(bool value)
    {
        gridController.QuickNote(value);
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
            if (selectedCell == null)
                return;
            if (puzzle[(int)selectedCell] != '0')
                return;

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
        // Debug.Log(
        //     $"GameLogicController: HandleCellClicked - fastMode: {fastMode} - noteMode: {noteMode} - eraseMode: {eraseMode}"
        // );
        if (fastMode)
        {
            if (selectedNumber == null)
                return;

            int? number = gridController.GetNumber(cellIndex);

            if (number != null && !eraseMode)
            {
                gridController.HighlightNumbers((int)number);
                DelayCallback(onCellSelected, (int)number);
                selectedNumber = number;
                return;
            }
            Debug.Log(
                $"GameLogicController: HandleCellClicked - fastMode: {fastMode} - noteMode: {noteMode} - eraseMode: {eraseMode} - selectedNumber: {selectedNumber}"
            );
            if (noteMode)
            {
                gridController.ToggleNote((int)selectedNumber, cellIndex);
                gridController.HighlightNumbers((int)selectedNumber);
            }
            else if (eraseMode)
            {
                // Debug.Log(
                //     $"GameLogicController: HandleCellClicked - puzzleAtIndex {puzzle[cellIndex]}"
                // );
                if (puzzle[cellIndex] != '0')
                    return;

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

            if (selectedNumber != null)
                gridController.HighlightNumbers((int)selectedNumber);
        }
    }

    private void HandleEraseClicked()
    {
        Debug.Log(
            $"GameLogicController: HandleEraseClicked - fastMode: {fastMode} - selectedCell: {selectedCell} - puzzleAtIndex: {(selectedCell != null ? puzzle[(int)selectedCell] : "NaN")}"
        );
        if (fastMode)
        {
            eraseMode = !eraseMode;
            return;
        }

        if (selectedCell == null || puzzle[(int)selectedCell] != '0')
            return;

        gridController.SetNumber(null, (int)selectedCell);
        gridController.ClearNotes((int)selectedCell);
    }

    private void SaveGame()
    {
        history.Push(gridController.gridState);
        PuzzleLoader.SavePuzzle(puzzle, timerUpdate, solution, history.ToArray());
        // Debug.Log("Game Saved");
    }

    private void DeleteSave()
    {
        history = new HStack<string>();
        PuzzleLoader.DeleteSave();
        ReactBridge.Instance.SetGlobal(FlagKeys.hasPreviousSave, false);
    }

    private void Undo()
    {
        if (history.Count <= 1)
            return;

        string previousState = history.Pop();
        gridController.SetGridState(previousState);
        // quickNoteToggle.SetIsOnWithoutNotify(previousState.Contains('q'));
        if (selectedNumber != null)
            gridController.HighlightNumbers((int)selectedNumber);
        CorrectnessCheckAll();
    }

    private void CorrectnessCheckAll()
    {
        foreach (var (num, index) in gridController.EnumerateCells()) // Ew
        {
            if (num == null)
                continue;
            CorrectnessCheck((int)num, index);
        }
    }

    private void CorrectnessCheck(int number, int cellIndex)
    {
        string cellCorrect = solution[cellIndex].ToString();
        string actual = number.ToString();

        bool isCorrect = !checkErrors || cellCorrect.Equals(actual);

        // Debug.Log($"GameLogicController: CorrectnessCheck - cellIndex: {cellIndex} - cellCorrect: {cellCorrect} - actual: {actual} - isCorrect: {isCorrect}");

        gridController.SetError(cellIndex, !isCorrect);
    }

    private void FinishedCheck()
    {
        string grid = gridController.gridString;
        string fmtGrid = grid.Replace(" ", string.Empty);
        bool isFinished = fmtGrid.Equals(solution);
        Debug.Log($"Finished Check Returned {isFinished}");
        if (isFinished)
        {
            DelayCallback(onGameFinished);
            DeleteSave();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = 0;
        timerUpdate = 0;
        gameStarted = false;
    }

    private void OnSettingsChanged(Settings settings)
    {
        checkErrors = settings.checkErrors;
    }

    void OnDestroy()
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.Unsubscribe(OnSettingsChanged);

        // The thread is background so it can't outlive the player, but in the
        // editor it would keep digging until the next domain reload.
        genCts?.Cancel();
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
