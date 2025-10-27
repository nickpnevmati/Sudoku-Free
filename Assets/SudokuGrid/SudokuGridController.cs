using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(SudokuGridShaderController))]
public class SudokuGridController : MonoBehaviour
{
    private SudokuGridShaderController shaderController;

    private Cell[] cells;
    private int? selectedCell;

    private Action<int> _onCellClicked;
    public Action<int> onCellClicked;


    void Awake()
    {
        shaderController = GetComponent<SudokuGridShaderController>();
        cells = new Cell[81];
        foreach (Transform cell in transform)
        {
            int cellNumber = int.Parse(cell.name.Split('.')[1]) - 1;
            cells[cellNumber] = new Cell(cell, this);
        }

        _onCellClicked += HandleCellClicked;
        _onCellClicked += onCellClicked.Invoke;
    }

    public void ConstructPuzzle(string puzzle)
    {
        for (int i = 0; i < puzzle.Length; i++)
        {
            int? number = puzzle[i] != '0' ? int.Parse(puzzle[i].ToString()) : null;
            cells[i].number = number;
        }
    }

    public void PaintCell(int cellIndex, int? number, bool note)
    {
        if (note)
        {
            if (number == null)
                throw new ArgumentNullException(nameof(number), "Number cannot be null when adding a note.");

            if (cells[cellIndex].number != null) return;

            cells[cellIndex].ToggleNote((int)number);
            ReselectCell();
            return;
        }

        cells[cellIndex].number = number;
        cells[cellIndex].ClearNotes();
        ReselectCell();

        if (number != null)
        {
            ClearGroupNotes(cellIndex, (int)number);
            ClearNoteHighlights();
        }
    }

    public (int, int?, List<int>) GetCellData(int cellIndex) => (cellIndex, cells[cellIndex].number, cells[cellIndex].GetNotes());

    public void PaintCellError(int cellIndex) => cells[cellIndex].SetMainTextColor(Color.red);
    public void ResetCellColor(int cellIndex) => cells[cellIndex].SetMainTextColor(Color.white);

    public void Deselect()
    {
        selectedCell = null;
        shaderController.ClearHighlighting();
        ClearNoteHighlights();
    }

    public void QuickNote(bool set)
    {
        if (!set)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                ClearNoteHighlights();
                cells[i].ClearNotes();
            }
            return;
        }

        for (int idx = 0; idx < cells.Length; idx++)
        {
            if (cells[idx].number != null) continue;

            for (int num = 1; num <= 9; num++)
            {
                int col = idx % 9;
                int row = idx / 9;

                bool found = false;

                for (int k = 0; k < 9; k++)
                {
                    if (cells[col + k * 9].number == num || cells[row * 9 + k].number == num)
                    {
                        found = true;
                        break;
                    }
                }

                int groupCol = col - col % 3;
                int groupRow = row - row % 3;
                for (int k = 0; k < 3; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        if (cells[(groupRow + k) * 9 + groupCol + l].number == num)
                        {
                            found = true;
                            break;
                        }
                    }
                }


                if (!found) cells[idx].AddNote(num);
            }
        }
    }

    private void HandleCellClicked(int index)
    {
        shaderController.ClearHighlighting();
        ClearNoteHighlights();

        if (selectedCell == index)
        {
            selectedCell = null;
            return;
        }
        selectedCell = index;

        shaderController.HighlightCell(index);

        if (cells[index].number == null) return;

        HighlightNotes(cells[index]);

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].number == cells[index].number)
                shaderController.HighlightCell(i);
        }
    }

    private void ReselectCell()
    {
        if (selectedCell == null) return;
        int cell = (int)selectedCell;
        selectedCell = null;
        HandleCellClicked(cell);
    }

    private void ClearGroupNotes(int cellIndex, int number)
    {
        int row = cellIndex / 9;
        int col = cellIndex % 9;

        for (int i = 0; i < 9; i++)
        {
            cells[row * 9 + i].RemoveNote(number);
            cells[i * 9 + col].RemoveNote(number);
        }

        int groupRow = row - row % 3;
        int groupCol = col - col % 3;

        for (int i = groupRow; i < groupRow + 3; i++)
        {
            for (int j = groupCol; j < groupCol + 3; j++)
            {
                cells[i * 9 + j].RemoveNote(number);
            }
        }
    }

    public void HighlightNumber(int number)
    {
        shaderController.ClearHighlighting();
        ClearNoteHighlights();

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].HighlightNote(number);
            if (cells[i].number == number) shaderController.HighlightCell(i);
        }
    }

    private void HighlightNotes(Cell highlightedCell)
    {
        foreach (var cell in cells)
        {
            cell.HighlightNote((int)highlightedCell.number);
        }
    }

    private void ClearNoteHighlights()
    {
        foreach (var cell in cells)
        {
            cell.RemoveNoteHighlights();
        }
    }

    public string GetGridAsString()
    {
        string gridString = "";
        foreach (var cell in cells)
            gridString += (cell.number == null ? 0 : (int)cell.number).ToString();
        return gridString;
    }

    struct Cell
    {
        public int? number
        {
            set
            {
                mainText.text = value == null ? "" : value.ToString();
                if (mainText.text == "") SetMainTextColor(Color.white);
            }
            get { return mainText.text == "" ? null : int.Parse(mainText.text); }
        }

        private TMP_Text mainText;
        private TMP_Text[] notes;
        private Image[] noteHighlights;

        private Button button;

        public Cell(Transform cell, SudokuGridController controller)
        {
            mainText = cell.GetComponent<TMP_Text>();
            mainText.text = "";

            notes = new TMP_Text[9];
            noteHighlights = new Image[9];
            foreach (Transform child in cell)
            {
                if (!child.name.Contains("Note")) continue;

                TMP_Text note_text = child.GetComponentInChildren<TMP_Text>();
                if (note_text == null) continue;


                int index = int.Parse(child.name.Split('.')[1]) - 1;
                notes[index] = note_text;
                notes[index].enabled = false;
                notes[index].text = (index + 1).ToString();

                noteHighlights[index] = child.GetComponentInChildren<Image>();
            }

            button = cell.GetComponentInChildren<Button>();
            int cellIndex = int.Parse(cell.name.Split('.')[1]) - 1;
            button.onClick.AddListener(delegate { controller._onCellClicked.Invoke(cellIndex); });
        }

        public void AddNote(int number) => notes[number - 1].enabled = true;
        public void RemoveNote(int number) => notes[number - 1].enabled = false;
        public void ToggleNote(int number) => notes[number - 1].enabled = !notes[number - 1].enabled;

        public void ClearNotes()
        {
            foreach (var note in notes)
            {
                note.enabled = false;
            }
        }

        public List<int> GetNotes()
        {
            List<int> activeNotes = new List<int>();
            for (int i = 0; i < notes.Length; i++)
            {
                if (notes[i].enabled) activeNotes.Add(i + 1);
            }
            return activeNotes;
        }

        public void HighlightNote(int number)
        {
            if (!notes[number - 1].enabled) return;
            noteHighlights[number - 1].enabled = true;
        }

        public void RemoveNoteHighlights()
        {
            foreach (var note in noteHighlights) note.enabled = false;
        }

        public void SetNoteColor(Color c)
        {
            foreach (var image in noteHighlights)
            {
                image.color = c;
            }
        }

        public void SetMainTextColor(Color c) => mainText.color = c;
    }
}
