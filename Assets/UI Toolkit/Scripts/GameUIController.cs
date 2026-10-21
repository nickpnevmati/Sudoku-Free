using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { private set; get; }
    private UIDocument m_document;
    private VisualElement m_root;

    // Buttons
    Button undoButton, exitButton, fastModeButton, noteModeButton, eraseButton, quickNoteButton;

    // Numpad
    Button[] numpad = new Button[9];

    // Grid
    private Cell[] gridCells;
    private HStack<string> history;

    // Timer
    private Label timer;
    private float gameTime;

    // Confirmation modal
    private VisualElement m_confirmModal, m_confirmBackdrop;
    private Label m_confirmTitle, m_confirmMessage;
    private Button m_confirmButton, m_cancelButton;
    private Action m_onConfirm;

    // Loading overlay
    private VisualElement m_loadingOverlay, m_loadingSpinner;
    private Button m_loadingCancelButton;
    private float m_spinnerAngle;

    private Task<(string, string)> generatePuzzleTask;
    private CancellationTokenSource m_generationCts;
    private string puzzle, solution;

    // Flags
    private bool m_fastMode, m_noteMode, m_eraseMode, m_solved;

    // Win animation: pending steps, so a new game or exit can stop it part-way
    private readonly List<IVisualElementScheduledItem> m_winAnimation = new List<IVisualElementScheduledItem>();
    const long k_winRippleStepMs = 70, k_winFlashMs = 350, k_winPromptDelayMs = 400;

    // Undo depth. Each entry is 810 chars and the whole history is rewritten on every
    // move, so this keeps the save file under ~160 KB however long the game runs
    const int k_maxHistory = 200;
    private int? m_selectedCell, m_selectedNumber;

    private void ConstructGrid()
    {
        VisualElement grid_root = m_root.Q<VisualElement>("GridInner");
        int index = 0;
        gridCells = new Cell[81];
        foreach (var row in grid_root.Children())
        {
            if (!row.name.Equals("GridRow")) continue;
            foreach (var cell in row.Children())
            {
                if (!cell.name.Equals("GridCell")) continue;
                int cellIndex = index;
                cell.RegisterCallback<PointerDownEvent>(delegate { OnCellClicked(cellIndex); });
                gridCells[index] = new Cell(cell);
                index++;
            }
        }
    }

    private void ConstructNumpad()
    {
        var numpadRoot = m_root.Q<VisualElement>("Numpad");
        for (int i = 0; i < 9; i++)
        {
            numpad[i] = numpadRoot.Q<Button>((i + 1).ToString());
            int number = i + 1;
            numpad[i].clicked += delegate { OnNumpadClicked(number); };
        }
    }

    private void RegisterButtons()
    {
        undoButton = m_root.Q<Button>("UndoButton");
        exitButton = m_root.Q<Button>("ExitButton");
        fastModeButton = m_root.Q<Button>("FastModeButton");
        quickNoteButton = m_root.Q<Button>("QuickNoteButton");
        eraseButton = m_root.Q<Button>("EraseButton");
        noteModeButton = m_root.Q<Button>("NoteModeButton");

        undoButton.clicked += Undo;
        exitButton.clicked += ExitRequested;
        fastModeButton.clicked += OnFastModeToggled;
        quickNoteButton.clicked += QuickNote;
        eraseButton.clicked += OnEraseModeClicked;
        noteModeButton.clicked += OnNoteModeToggled;
    }

    private void ConstructConfirmModal()
    {
        m_confirmModal = m_root.Q<VisualElement>("ConfirmModal");
        m_confirmBackdrop = m_root.Q<VisualElement>("ConfirmBackdrop");
        m_confirmTitle = m_root.Q<Label>("ConfirmTitle");
        m_confirmMessage = m_root.Q<Label>("ConfirmMessage");
        m_confirmButton = m_root.Q<Button>("ConfirmButton");
        m_cancelButton = m_root.Q<Button>("CancelButton");

        m_confirmButton.clicked += OnConfirmClicked;
        m_cancelButton.clicked += HideConfirmation;
        // Clicks on the panel bubble up here too, so only dismiss when the backdrop itself was hit
        m_confirmBackdrop.RegisterCallback<ClickEvent>(e => { if (e.target == m_confirmBackdrop) HideConfirmation(); });

        HideConfirmation();
    }

    private void ConstructLoadingOverlay()
    {
        m_loadingOverlay = m_root.Q<VisualElement>("LoadingOverlay");
        m_loadingSpinner = m_root.Q<VisualElement>("LoadingSpinner");
        m_loadingCancelButton = m_root.Q<Button>("LoadingCancelButton");

        m_loadingCancelButton.clicked += ExitToMenu;

        HideLoading();
    }

    // ------------------ Unity Functions -----------------

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        m_document = GetComponent<UIDocument>();
        m_root = m_document.rootVisualElement;
        ConstructGrid();
        ConstructNumpad();
        RegisterButtons();
        ConstructConfirmModal();
        ConstructLoadingOverlay();
        timer = m_root.Q<Label>("GameTimer");
    }

    private void Update()
    {
        GameLoaded();

        if (generatePuzzleTask != null)
        {
            m_spinnerAngle = (m_spinnerAngle + 360f * Time.deltaTime) % 360f;
            m_loadingSpinner.style.rotate = new Rotate(Angle.Degrees(m_spinnerAngle));
            return;
        }

        // Update Game Timer
        if (IsGameInProgress) gameTime += Time.deltaTime;
        timer.text = FormatTime(gameTime);
    }

    /// <summary>
    /// True only while the player can actually play: a puzzle is loaded and unsolved, the
    /// game screen is showing (UIMasterController hides it on the menu / settings) and no
    /// prompt (exit, win) is open. Generation is already handled by Update's early return.
    /// </summary>
    private bool IsGameInProgress =>
        puzzle != null
        && !m_solved
        && m_root.style.display != DisplayStyle.None
        && m_confirmModal.style.display != DisplayStyle.Flex;

    // ------------------- Setup Function ----------------------

    public void RequestStartGame(bool continueFlag, int difficulty)
    {
        if (continueFlag)
        {
            (puzzle, solution, history, gameTime) = PuzzleLoader.LoadSaved();
            StartGame((puzzle, solution));
        }
        else
        {
            CancelGeneration();
            // Linked to destroyCancellationToken so teardown still stops the generator
            m_generationCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            var token = m_generationCts.Token;
            generatePuzzleTask = Task.Run(() => PuzzleGenerator.Instance.Generate((PuzzleDifficulty)difficulty, token), token);
            history = new HStack<string>();
            ShowLoading();
        }
    }

    private void GameLoaded()
    {
        if (generatePuzzleTask == null || !generatePuzzleTask.IsCompleted) return;

        var finished = generatePuzzleTask;
        generatePuzzleTask = null;
        DisposeGenerationCts();
        HideLoading();

        if (finished.IsCompletedSuccessfully)
        {
            StartGame(finished.Result);
            gameTime = 0f;
        }
        else if (finished.IsFaulted)
        {
            Debug.LogException(finished.Exception);
            UIMasterController.Instance.NavigateToMenu();
        }
    }

    /// <summary>
    /// Stop a puzzle that's still generating. Dropping the task as well means GameLoaded
    /// never sees it, so a result that lands after cancelling can't start a game behind the menu.
    /// </summary>
    private void CancelGeneration()
    {
        m_generationCts?.Cancel();
        DisposeGenerationCts();
        generatePuzzleTask = null;
        HideLoading();
    }

    private void DisposeGenerationCts()
    {
        m_generationCts?.Dispose();
        m_generationCts = null;
    }

    private void StartGame((string, string) result)
    {
        (puzzle, solution) = result;
        m_selectedCell = null;
        m_selectedNumber = null;

        m_fastMode = false;
        m_noteMode = false;
        m_eraseMode = false;
        m_solved = false;
        StopWinAnimation();
        quickNoteButton.style.display = GameSettings.DisableQuickNote ? DisplayStyle.None : DisplayStyle.Flex;

        // Continue: pick up where the history left off. New game (or a save with
        // no history): start from the givens and save that, so it replaces any
        // previous save straight away and gives Undo a floor to stop at
        if (history.Count > 0)
        {
            GridFromString(history.Last);
        }
        else
        {
            GridFromString(puzzle);
            SaveGame();
        }
        Refresh();
    }


    // ----------------- Event Handlers ---------------------

    private void OnCellClicked(int index)
    {
        if (m_solved) return;

        if (m_fastMode)
        {
            if (!gridCells[index].number.Equals(""))
            {
                if (m_eraseMode)
                {
                    EraseCell(index);
                    SaveGame();
                }
                else
                {
                    m_selectedNumber = int.Parse(gridCells[index].number);
                }
                Refresh();
                return;
            }

            if (m_selectedNumber == null) return;

            int selectedNumber = (int)m_selectedNumber;
            if (m_noteMode) ToggleNote(index, selectedNumber);
            else if (m_eraseMode) EraseCell(index);
            else SetNumber(index, selectedNumber);

            Refresh();
            SaveGame();
        }
        else
        {
            m_selectedCell = index;
            string selected = gridCells[index].number;
            m_selectedNumber = selected.Equals("") ? null : int.Parse(selected);
            Refresh();
        }
    }

    private void OnNumpadClicked(int number)
    {
        if (m_solved) return;

        if (m_fastMode)
        {
            m_selectedNumber = number;
            Refresh();
        }
        else
        {
            if (m_selectedCell == null) return;
            int selectedCell = (int)m_selectedCell;

            if (m_noteMode) ToggleNote(selectedCell, number);
            else if (m_eraseMode) EraseCell(selectedCell);
            else SetNumber(selectedCell, number);

            Refresh();
            SaveGame();
        }
    }

    private void OnFastModeToggled()
    {
        m_fastMode = !m_fastMode;

        if (!m_fastMode)
            m_selectedNumber = null;
        else
        {
            m_selectedNumber = m_selectedCell == null || gridCells[(int)m_selectedCell].number.Equals("") ? 1 : int.Parse(gridCells[(int)m_selectedCell].number);
            m_selectedCell = null;
        }
        Refresh();
    }

    private void OnEraseModeClicked()
    {
        if (m_fastMode)
        {
            m_noteMode = false;
            m_eraseMode = !m_eraseMode;
        }
        else
        {
            if (m_selectedCell == null) return;
            int selected = (int)m_selectedCell;
            EraseCell(selected);
            SaveGame();
        }
        Refresh();
    }

    private void OnNoteModeToggled()
    {
        m_eraseMode = false;
        m_noteMode = !m_noteMode;
        Refresh();
    }

    private void QuickNote()
    {
        if (m_solved) return;

        for (int i = 0; i < gridCells.Length; i++)
        {
            if (!gridCells[i].number.Equals("")) continue;

            var (col, row) = ColumnsRows(i);
            var seen = new HashSet<int>();
            foreach (int idx in col.Concat(row).Concat(Box(i)))
            {
                if (gridCells[idx].number.Equals("")) continue;

                seen.Add(int.Parse(gridCells[idx].number));
            }

            for (int num = 1; num <= 9; num++)
            {
                if (seen.Contains(num)) continue;
                gridCells[i].SetNote(num);
            }
        }
        SaveGame();
    }

    private void Undo()
    {
        if (m_solved || history.Count <= 1) return;
        string last = history.PopGetPrevious();
        GridFromString(last);
        Refresh();
        PuzzleLoader.SavePuzzle(puzzle, solution, history, gameTime);
    }

    private void ExitRequested()
    {
        ShowConfirmation(
            title: "Exit Game",
            message: "Are you sure you want to exit the game?",
            confirmText: "Yes, exit",
            cancelText: "No, stay",
            onConfirm: ExitToMenu);
    }

    private void ExitToMenu()
    {
        CancelGeneration();
        StopWinAnimation();
        SaveGame();
        UIMasterController.Instance.NavigateToMenu();
    }

    private void OnPuzzleSolved(int lastCell)
    {
        m_solved = true;
        m_selectedCell = null;
        m_selectedNumber = null;

        if (GameSettings.DisableWinAnimation)
            ShowWinPrompt();
        else
            PlayWinAnimation(lastCell);
    }

    private void ShowWinPrompt()
    {
        ShowConfirmation(
            title: "Puzzle Solved!",
            message: $"Your time: {FormatTime(gameTime)}",
            confirmText: "New game",
            cancelText: "Close",
            onConfirm: NewGameRequested);
    }

    private void NewGameRequested()
    {
        StopWinAnimation();
        UIMasterController.Instance.NavigateToDifficultySelect();
    }

    private void OnConfirmClicked()
    {
        // Hide first, so onConfirm is free to open another prompt
        var onConfirm = m_onConfirm;
        HideConfirmation();
        onConfirm?.Invoke();
    }

    // ------------------- Visuals ----------------

    /// <summary>
    /// Rebuild every highlight from the current state. It's only class toggles over
    /// 81 cells and a few buttons, so call this after any change instead of tracking
    /// which highlight needs updating.
    /// </summary>
    private void Refresh()
    {
        HighlightCells();
        HighlightNumpad();
        HighlightModeButtons();
    }

    private void HighlightCells()
    {
        ClearHighlights();
        if (m_selectedCell != null)
        {
            int selected = (int)m_selectedCell;
            gridCells[selected].SetHighlight("cell_selected");
            (int[], int[]) colrows = ColumnsRows(selected);
            foreach (int col in colrows.Item1)
                gridCells[col].SetHighlight("cell_related");
            foreach (int row in colrows.Item2)
                gridCells[row].SetHighlight("cell_related");

            foreach (var idx in Box(selected))
                gridCells[idx].SetHighlight("cell_related");
        }


        if (m_selectedNumber != null)
        {
            for (int i = 0; i < gridCells.Length; i++)
            {
                if (gridCells[i].number != m_selectedNumber.ToString()) continue;

                gridCells[i].SetHighlight("cell_same_number");
            }
        }

        if (GameSettings.CheckErrors && solution != null)
        {
            for (int i = 0; i < gridCells.Length; i++)
            {
                string number = gridCells[i].number;
                if (number.Equals("") || IsPlacementCorrect(i, number)) continue;

                gridCells[i].SetHighlight("cell_error");
            }
        }
    }

    private void ClearHighlights()
    {
        foreach (var cell in gridCells) cell.ClearHighlight();
    }

    private void ShowConfirmation(string title, string message, string confirmText, string cancelText, Action onConfirm)
    {
        m_confirmTitle.text = title;
        m_confirmMessage.text = message;
        m_confirmButton.text = confirmText;
        m_cancelButton.text = cancelText;
        m_onConfirm = onConfirm;
        m_confirmModal.style.display = DisplayStyle.Flex;
    }

    private void HideConfirmation()
    {
        m_onConfirm = null;
        m_confirmModal.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Flash every cell in rings spreading out from the last number placed, then show
    /// the win prompt. Driven by the UI scheduler; StopWinAnimation cancels what's pending.
    /// </summary>
    private void PlayWinAnimation(int origin)
    {
        StopWinAnimation();
        int originRow = origin / 9, originCol = origin % 9;
        long lastFlashEnd = 0;

        for (int i = 0; i < gridCells.Length; i++)
        {
            var cell = gridCells[i];
            cell.SetWinTransition(true);

            // Chebyshev distance, so each wave is a square ring around the origin
            int ring = Math.Max(Math.Abs(i / 9 - originRow), Math.Abs(i % 9 - originCol));
            long start = ring * k_winRippleStepMs;
            lastFlashEnd = Math.Max(lastFlashEnd, start + k_winFlashMs);

            m_winAnimation.Add(m_root.schedule.Execute(() => cell.SetWinFlash(true)).StartingIn(start));
            m_winAnimation.Add(m_root.schedule.Execute(() => cell.SetWinFlash(false)).StartingIn(start + k_winFlashMs));
        }

        m_winAnimation.Add(m_root.schedule.Execute(ShowWinPrompt).StartingIn(lastFlashEnd + k_winPromptDelayMs));
    }

    private void StopWinAnimation()
    {
        foreach (var item in m_winAnimation) item.Pause();
        m_winAnimation.Clear();
        foreach (var cell in gridCells)
        {
            cell.SetWinFlash(false);
            cell.SetWinTransition(false);
        }
    }

    // The overlay is opaque and covers the whole screen, so it also blocks input
    // to the previous game's grid while the new puzzle generates
    private void ShowLoading() => m_loadingOverlay.style.display = DisplayStyle.Flex;
    private void HideLoading() => m_loadingOverlay.style.display = DisplayStyle.None;

    private void HighlightNumpad()
    {
        for (int i = 0; i < numpad.Length; i++)
            numpad[i].EnableInClassList("numpad_button_active", m_selectedNumber != null && m_selectedNumber == i + 1);
    }

    private void HighlightModeButtons()
    {
        fastModeButton.EnableInClassList("game_button_active", m_fastMode);
        noteModeButton.EnableInClassList("game_button_active", m_noteMode);
        eraseButton.EnableInClassList("game_button_active", m_eraseMode && m_fastMode);
    }


    // ------------------- Utils ------------------------

    private string FormatTime(float time)
    {
        int seconds_total = (int)time;
        int hours = seconds_total / 3600;
        int minutes = seconds_total / 60 % 60;
        int seconds = seconds_total % 60;

        string h = hours > 0 ? $"{hours.ToString("D2")}:" : "";
        string m = minutes > 0 || hours > 0 ? $"{minutes.ToString("D2")}:" : "";
        return $"{h}{m}{seconds.ToString("D2")}";
    }

    // TODO history on the following 3 methods
    private void SetNumber(int cell, int number)
    {
        if (!gridCells[cell].number.Equals("")) return;
        gridCells[cell].ClearAll();
        gridCells[cell].SetNumber(number.ToString());

        var colrow = ColumnsRows(cell);
        var box = Box(cell);
        foreach (var idx in colrow.Item1.Concat(colrow.Item2).Concat(box))
            gridCells[idx].ClearNote(number);

        if (!gridCells[cell].number.Equals(""))
            m_selectedNumber = int.Parse(gridCells[cell].number);

        Refresh();

        // Wrong numbers are marked in HighlightCells, so the marking also follows Undo,
        // Continue and the Check Errors setting being toggled
        if (FinishedCheck()) OnPuzzleSolved(cell);
    }

    private void ToggleNote(int cell, int number)
    {
        if (!gridCells[cell].number.Equals("")) return;
        gridCells[cell].ToggleNote(number);
    }

    private void EraseCell(int cell)
    {
        if (puzzle[cell] != '0') return;
        gridCells[cell].ClearAll();
    }

    private bool FinishedCheck() => GridToString().Equals(solution);
    private bool IsPlacementCorrect(int cellIndex, string number) => solution[cellIndex].ToString().Equals(number);

    private void SaveGame()
    {
        // Moves that changed nothing (e.g. a number on a filled cell) shouldn't add an
        // Undo step. Compare the full state - numbers and notes - like history stores it
        string state = GridStateToString();
        if (history.Count > 0 && history.Last.Equals(state))
            return;

        history.Push(state);
        // Past the cap the oldest state goes, so Undo's floor moves up with it
        history.TrimOldest(k_maxHistory);
        // A solved grid has nothing left to continue
        if (FinishedCheck()) PuzzleLoader.DeleteSave();
        else PuzzleLoader.SavePuzzle(puzzle, solution, history, gameTime);
    }

    /// <summary>
    /// Numbers and notes, for history and saves: 81 digits ('0' for blank),
    /// then 9 '0'/'1' note flags per cell.
    /// </summary>
    private string GridStateToString()
    {
        var sb = new System.Text.StringBuilder(GridToString(), 81 * 10);
        foreach (var cell in gridCells) sb.Append(cell.NotesToString());
        return sb.ToString();
    }

    private string GridToString()
    {
        string s = "";
        foreach (var cell in gridCells) s += cell.number.Equals("") ? "0" : cell.number;
        return s;
    }

    /// <summary>
    /// Accepts numbers only (81 chars, e.g. a puzzle string) or the full
    /// numbers + notes format from GridStateToString (810 chars).
    /// </summary>
    private void GridFromString(string state)
    {
        int cells = gridCells.Length;
        bool hasNotes = state.Length == cells * 10;
        if (state.Length != cells && !hasNotes)
            throw new Exception($"Grid state must be {cells} or {cells * 10} chars, got {state.Length}");

        for (int i = 0; i < cells; i++)
        {
            gridCells[i].SetNumber(state[i] == '0' ? "" : state[i].ToString());
            if (hasNotes) gridCells[i].NotesFromString(state.Substring(cells + i * 9, 9));
            else gridCells[i].ClearNotes();
        }
    }

    private (int[], int[]) ColumnsRows(int cellIndex)
    {
        int[] rows = new int[9];
        int[] cols = new int[9];
        int row = cellIndex / 9;
        int col = cellIndex % 9;
        for (int i = 0; i < 9; i++)
        {
            cols[i] = row * 9 + i;
            rows[i] = i * 9 + col;
        }
        return (cols, rows);
    }

    private int[] Box(int cellIndex)
    {
        int[] box = new int[9];
        int rowStart = cellIndex / 9 - cellIndex / 9 % 3;
        int colStart = cellIndex % 9 - cellIndex % 9 % 3;
        for (int i = 0; i < 9; i++)
            box[i] = (rowStart + i / 3) * 9 + colStart + i % 3;
        return box;
    }


    private class Cell
    {
        VisualElement root, background;
        Label[] notes;
        Label label;

        public Cell(VisualElement root)
        {
            this.root = root;
            background = root.Q<VisualElement>(className: "grid_cell");
            label = root.Q<Label>("CellButton");
            label.text = "";
            notes = new Label[9];
            var notesContainer = root.Q<VisualElement>("NotesContainer");
            int index = 0;
            foreach (var row in notesContainer.Children())
            {
                foreach (var note in row.Children())
                {
                    notes[index] = (Label)note;
                    note.style.display = DisplayStyle.None;
                    index++;
                }
            }
        }

        public string number => label.text;
        public void SetNumber(string number) => label.text = number;
        public void SetNote(int number) => notes[number - 1].style.display = DisplayStyle.Flex;
        public void ClearNote(int number) => notes[number - 1].style.display = DisplayStyle.None;
        public void ToggleNote(int number)
        {
            var note = notes[number - 1];
            note.style.display = note.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
        }
        public void ClearNotes()
        {
            for (int i = 1; i <= notes.Length; i++)
                ClearNote(i);
        }
        public void ClearAll()
        {
            ClearNotes();
            label.text = "";
        }

        // Colours live in GridStyles.uss / Theme.uss so they follow the theme
        public void SetHighlight(string highlightClass) => background.AddToClassList(highlightClass);

        public void ClearHighlight()
        {
            background.RemoveFromClassList("cell_related");
            background.RemoveFromClassList("cell_same_number");
            background.RemoveFromClassList("cell_selected");
            background.RemoveFromClassList("cell_error");
        }

        // Win animation. The transition class is kept separate so ordinary highlights
        // don't fade - only cells taking part in the animation get the transition
        public void SetWinTransition(bool on) => background.EnableInClassList("cell_win_anim", on);
        public void SetWinFlash(bool on) => background.EnableInClassList("cell_win", on);

        public bool HasNote(int number) => notes[number - 1].style.display == DisplayStyle.Flex;

        public string NotesToString()
        {
            var chars = new char[notes.Length];
            for (int i = 0; i < notes.Length; i++)
                chars[i] = HasNote(i + 1) ? '1' : '0';
            return new string(chars);
        }

        public void NotesFromString(string flags)
        {
            for (int i = 0; i < notes.Length; i++)
            {
                if (flags[i] == '1') SetNote(i + 1);
                else ClearNote(i + 1);
            }
        }
    }

    private class HStack<T>
    {
        private List<T> innerList;

        public int Count => innerList.Count;
        public T Last => innerList.Last();

        public HStack()
        {
            innerList = new List<T>();
        }

        public HStack(IEnumerable<T> collection)
        {
            innerList = new List<T>(collection);
        }

        public void Push(T value) => innerList.Add(value);

        public void TrimOldest(int maxCount)
        {
            if (innerList.Count > maxCount)
                innerList.RemoveRange(0, innerList.Count - maxCount);
        }

        public T Pop()
        {
            T value = innerList.LastOrDefault();
            if (innerList.Count > 0)
                innerList.RemoveAt(innerList.Count - 1);
            return value;
        }

        public T PopGetPrevious()
        {
            Pop();
            return innerList.LastOrDefault();
        }

        public static implicit operator HStack<T>(T[] array)
        {
            HStack<T> stack = new HStack<T>(array);
            return stack;
        }

        public static implicit operator T[](HStack<T> stack)
        {
            return stack.innerList.ToArray();
        }
    }
}
