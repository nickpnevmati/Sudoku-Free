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

    private Cell[] gridCells;
    private HStack<string> history;

    private Task<(string, string)> generatePuzzleTask;
    private CancellationToken token;
    private string puzzle, solution;

    // Flags
    private bool m_fastMode, m_noteMode, m_QuickNote;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        m_document = GetComponent<UIDocument>();
        m_root = m_document.rootVisualElement;
        ConstructGrid();
        token = destroyCancellationToken;
    }

    private void Update()
    {
        GameLoaded();
    }

    public void RequestStartGame(bool continueFlag, int difficulty)
    {
        if (continueFlag)
            PuzzleLoader.LoadSaved();
        else
            generatePuzzleTask = Task.Run(() => PuzzleGenerator.Instance.Generate((PuzzleDifficulty)difficulty, token), token);
    }

    private void GameLoaded()
    {
        if (generatePuzzleTask == null || !generatePuzzleTask.IsCompleted) return;

        var finished = generatePuzzleTask;
        generatePuzzleTask = null;

        if (finished.IsCompletedSuccessfully) StartGame(finished.Result);
        else if (finished.IsFaulted) Debug.LogException(finished.Exception);
    }

    private void StartGame((string, string) result)
    {
        (puzzle, solution) = result;
        for (int i = 0; i < solution.Length; i++)
            gridCells[i].SetNumber(puzzle[i].ToString() == "0" ? "" : puzzle[i].ToString());
    }

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
                gridCells[index] = new Cell(cell);
                index++;
            }
        }
    }

    private void FinishedCheck() => GridToString().Equals(solution);
    private void IsPlacementCorrect(int cellIndex, string number) => solution[cellIndex].ToString().Equals(number);
    private string GridToString()
    {
        string s = "";
        foreach (var cell in gridCells) s += cell.number;
        return s;
    }
    private void GridFromString(string state)
    {
        if (state.Length != gridCells.Length)
            throw new Exception(); // TODO give this some message
        for (int i = 0; i < state.Length; i++)
        {
            gridCells[i].SetNumber(state[i].ToString());
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

    private class Cell
    {
        VisualElement root;
        Label[] notes;
        Label label;

        public Cell(VisualElement root)
        {
            this.root = root;
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
        public void SetNote(int number) => notes[number].style.display = DisplayStyle.Flex;
        public void ClearNote(int number) => notes[number].style.display = DisplayStyle.None;
        public void ToggleANote(int number)
        {
            var note = notes[number];
            note.style.display = note.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
        }
        public void ClearAll()
        {
            for (int i = 0; i < notes.Length; i++)
                ClearNote(i);
        }

        public (char, string) GetState()
        {
            char numberChar = label.text.ToCharArray()[0];
            string notes = "";
            foreach (var n in this.notes)
            {
                notes += n.text;
            }

            return (numberChar, notes);
        }

        public void SetState(string number, string notes)
        {
            label.text = number;
            for (int i = 0; i < notes.Length; i++)
            {
                if (notes[i] == '0') continue;
                ToggleANote(i);
            }
        }
    }

    private class GridState
    {
        string state = new string('0', 81);
        string[] notes = new string[81];

        public GridState(Cell[] grid)
        {
            for (int i = 0; i < 81; i++)
            {
                FromCell(grid[i], i);
            }
        }

        public void FromCell(Cell cell, int index)
        {
            (char number, string notes) = cell.GetState();
            char[] stateArray = state.ToCharArray();
            stateArray[index] = number;
            state = stateArray.ToString();
            this.notes[index] = notes;
        }
    }

    private class HStack<T>
    {
        private List<T> innerList;

        public int Count => innerList.Count;

        public HStack()
        {
            innerList = new List<T>();
        }

        public HStack(IEnumerable<T> collection)
        {
            innerList = new List<T>(collection);
        }

        public void Push(T value)
        {
            innerList.Append(value);
        }

        public T Pop()
        {
            T value = innerList.Last();
            innerList.RemoveAt(innerList.Count - 1);
            return value;
        }

        public T PopGetPrevious()
        {
            Pop();
            return innerList.Last();
        }
    }
}
