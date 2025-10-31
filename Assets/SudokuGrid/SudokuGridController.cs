using TMPro;
using System;
using UnityEngine;
using System.Linq;
using deVoid.Utils;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(SudokuGridShaderController))]
public class SudokuGridController : MonoBehaviour
{
    private SudokuGridShaderController shaderController;

    private Cell[] cells;

    private Action<int> _onCellClicked;
    public Action<int> onCellClicked;

    public string gridString { get => GridToString(false); }
    public string gridState { get => GridToString(true); }

    void Awake()
    {
        shaderController = GetComponent<SudokuGridShaderController>();
        cells = new Cell[81];
        foreach (Transform cell in transform)
        {
            int cellNumber = int.Parse(cell.name.Split('.')[1]) - 1;
            cells[cellNumber] = new Cell(cell, this);
        }

        _onCellClicked += OnPreCellClicked;
        _onCellClicked += onCellClicked.Invoke;

        Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
    }

    public void ConstructGrid(string puzzle)
    {
        for (int i = 0; i < puzzle.Length; i++)
        {
            cells[i].number = puzzle[i] != '0' ? int.Parse(puzzle[i].ToString()) : null;
        }
    }

    public void QuickNote(bool set)
    {
        if (!set)
        {
            foreach (var cell in cells) cell.ClearNotes();
            return;
        }

        for (int idx = 0; idx < cells.Length; idx++)
        {
            if (cells[idx].number != null) continue;

            for (int num = 1; num <= 9; num++)
            {
                bool found = false;

                foreach (int rowcol in CellRowCol(idx))
                {
                    if (cells[rowcol].number == num)
                    {
                        found = true;
                        break;
                    }
                }

                foreach (int groupCellIndex in CellGroupIndices(idx))
                {
                    if (cells[groupCellIndex].number == num)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) cells[idx].AddNote(num);
            }
        }
    }

    public void SetNumber(int? number, int index)
    {
        cells[index].number = number;
        if (number == null) return;

        cells[index].ClearNotes();
        foreach (int idx in CellRCG(index))
            cells[idx].RemoveNote((int) number);
    }
    public int? GetNumber(int index) => cells[index].number;
    public void SetError(int index, bool error) => cells[index].SetError(error);
    public void SetNote(int number, int index) => cells[index].AddNote(number);
    public void EraseNote(int number, int index) => cells[index].RemoveNote(number);
    public void ToggleNote(int number, int index) => cells[index].ToggleNote(number);
    public void ClearNotes(int index) => cells[index].ClearNotes();
    public void ClearAllNotes() => Array.ForEach(cells, cell => cell.ClearNotes());

    public void SelectCell(int index)
    {
        shaderController.ClearHighlighting();
        int? number = cells[index].number;
        if (number == null)
        {
            shaderController.HighlightCell(index);
        }
        else
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].number != number) continue;

                shaderController.HighlightCell(i);
            }
        }
    }

    public void HighlightNumbers(int number)
    {
        shaderController.ClearHighlighting();

        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            if (cell.number == number) shaderController.HighlightCell(i);
            cell.ClearNoteHighlights();
            cell.HighlightNote(number);
        }
    }

    public void SetGridState(string gridState)
    {
        ClearAllNotes();
        int i = 0;
        bool notes = false;
        foreach (char c in gridState)
        {
            switch (c)
            {
                case ' ':
                    notes = false;
                    i++;
                    break;
                case '0':
                    notes = true;
                    cells[i].number = null;
                    if (!notes && gridState[i + 1] != ' ')
                        throw new Exception("Grid State Encoding Error");
                    break;
                case 'q':
                    break;
                default:
                    int charVal = (int)char.GetNumericValue(c);
                    if (notes)
                        cells[i].AddNote(charVal);
                    else
                        cells[i].number = charVal;
                    break;
            }
        }
    }

    private string GridToString(bool includeNotes)
    {
        string gridString = "";

        foreach (var cell in cells)
        {
            var (number, notes) = cell.GetCellData();
            if (number != null)
            {
                gridString += cell.number.ToString();
            }
            else
            {
                gridString += "0";
                if (includeNotes)
                    foreach (var note in notes) gridString += note;
            }
            gridString += " ";
        }

        return gridString.Trim();
    }

    public IEnumerable<(int?, int)> EnumerateCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            yield return (cells[i].number, i);
        }
    }

    private void OnPreCellClicked(int value)
    {
        shaderController.ClearHighlighting();
    }

    private void OnSettingsChanged(Settings settings)
    {
        shaderController.primaryColor = settings.theme.primary;
        shaderController.secondaryColor = settings.theme.secondary;
        shaderController.backgroundColor = settings.theme.border;
    }

    private IEnumerable<int> CellRCG(int index)
    {
        foreach (int idx in CellRowCol(index)) yield return idx;
        foreach (int idx in CellGroupIndices(index)) yield return idx;
    }

    private IEnumerable<int> CellRowCol(int index)
    {
        int row = index / 9;
        int col = index % 9;

        for (int i = 0; i < 9; i++)
        {
            yield return row * 9 + i;
            yield return i * 9 + col;
        }
    }

    private IEnumerable<int> CellGroupIndices(int index) => CellGroupIndices(index / 9, index % 9);

    private IEnumerable<int> CellGroupIndices(int row, int col)
    {
        int groupRow = row - row % 3;
        int groupCol = col - col % 3;

        for (int r = groupRow; r < groupRow + 3; r++)
        {
            for (int c = groupCol; c < groupCol + 3; c++)
            {
                yield return r * 9 + c;
            }
        }
    }

    struct Cell
    {
        public int? number { set => SetCellNumber(value); get => GetCellNumber(); }

        private TMP_Text mainText;
        Color mainTextColor;
        Color errorColor;
        private Note[] notes;

        private Button button;

        public Cell(Transform cell, SudokuGridController controller)
        {
            mainText = cell.GetComponent<TMP_Text>();
            mainText.text = "";

            mainTextColor = Color.white;
            errorColor = Color.red;

            notes = new Note[9];
            foreach (Transform child in cell)
            {
                if (!child.name.Contains("Note")) continue;

                int noteIndex = int.Parse(child.name.Split(".")[1]) - 1;
                notes[noteIndex] = new Note(child);
            }

            button = cell.GetComponentInChildren<Button>();
            int cellIndex = int.Parse(cell.name.Split('.')[1]) - 1;
            button.onClick.AddListener(delegate { controller._onCellClicked.Invoke(cellIndex); });

            Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
        }

        public void SetError(bool error) => mainText.color = error ? errorColor : mainTextColor;
        public void AddNote(int number) => notes[number - 1].Set();
        public void RemoveNote(int number) => notes[number - 1].Unset();
        public void ToggleNote(int number) => notes[number - 1].Toggle();
        public void ClearNotes() => Array.ForEach(notes, note => note.Unset());
        public void HighlightNote(int number) => notes[number - 1].Highlight();
        public void RemoveNoteHighlight(int number) => notes[number - 1].ClearHighlight();
        public void ClearNoteHighlights() => Array.ForEach(notes, note => note.ClearHighlight());

        public (int?, int[]) GetCellData() => (number, notes
            .Select((note, i) => note.enabled ? i + 1 : (int?)null)
            .Where(idx => idx.HasValue)
            .Select(idx => idx.Value)
            .ToArray()
        );

        private int? GetCellNumber() => mainText.text == "" ? null : int.Parse(mainText.text);
        private void SetCellNumber(int? value) => mainText.text = value == null ? "" : value.ToString();

        private void OnSettingsChanged(Settings settings)
        {
            mainTextColor = settings.theme.textPrimary;
            errorColor = settings.theme.error;
        }

        struct Note
        {
            public bool enabled { get => gameObject.activeSelf; }

            GameObject gameObject;
            TMP_Text textObject;
            Image highlight;

            public Note(Transform transform)
            {
                gameObject = transform.gameObject;
                textObject = transform.GetComponentInChildren<TMP_Text>();
                textObject.text = transform.name.Split(".")[1];
                highlight = transform.GetComponentInChildren<Image>();

                Signals.Get<OnSettingsChangedSignal>().AddListener(OnSettingsChanged);
            }

            public void Set() => SetEnabled(true);
            public void Unset() => SetEnabled(false);
            public void Toggle() => SetEnabled(!gameObject.activeSelf);
            public void Highlight() => highlight.enabled = true;
            public void ClearHighlight() => highlight.enabled = false;
            public void ToggleHighlight() => highlight.enabled = !highlight.enabled;

            private void SetEnabled(bool value)
            {
                gameObject.SetActive(value);
                if (!value) highlight.enabled = value;
            }

            private void OnSettingsChanged(Settings settings)
            {
                textObject.color = settings.theme.textSecondary;
                highlight.color = settings.theme.secondary;
            }
        }
    }
}
