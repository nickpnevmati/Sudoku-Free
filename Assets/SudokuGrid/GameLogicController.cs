using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogicController : MonoBehaviour
{
    [SerializeField] NumpadController numpad;
    [SerializeField] SudokuGridController gridController;
    [SerializeField] Toggle fastModeButton;
    [SerializeField] Toggle noteToggle;
    [SerializeField] Button undoButton;

    private bool fastMode;
    private bool noteMode;

    public int? selectedNumber;
    public int? selectedCell;

    private Stack<(int, int?, List<int>)> history = new Stack<(int, int?, List<int>)>();

    private PuzzleLoader puzzleLoader;
    private string solution;

    private void Awake()
    {
        gridController.onCellClicked += HandleCellClicked;
        numpad.onButtonClicked += HandleNumpadClicked;
        fastModeButton.onValueChanged.AddListener(SetFastMode);
        noteToggle.onValueChanged.AddListener(value => noteMode = value);
        undoButton.onClick.AddListener(Undo);
    }

    private void Start()
    {
        puzzleLoader = new PuzzleLoader();
        int randomIndex = Mathf.FloorToInt(Random.Range(0, 100));
        (string puzzle, string solution) = puzzleLoader.LoadPuzzle(randomIndex);
        gridController.ConstructPuzzle(puzzle);
        this.solution = solution;
    }

    private void SetFastMode(bool value)
    {
        gridController.Deselect();

        fastMode = value;
        if (!fastMode) selectedNumber = null;
        numpad.SetModeToggle(fastMode);
    }

    private void HandleNumpadClicked(int? number)
    {
        if (number == null) return;

        if (fastMode)
        {
            selectedNumber = number;
            gridController.HighlightNumber((int) number);
            return;
        }

        if (selectedCell == null) return;

        history.Push(gridController.GetCellData((int)selectedCell));
        gridController.PaintCell((int)selectedCell, (int)number, noteMode);

        if (!IsCorrect((int)selectedCell, (int)number))
        {
            // TODO
        }
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
        gridController.HighlightNumber((int) selectedNumber);

        if (!IsCorrect(cellIndex, (int)selectedNumber))
        {
            // TODO
        }
    }

    private void Undo()
    {
        if (history.Count == 0) return;

        (int cellIndex, int? number, List<int> notes) = history.Pop();
        gridController.PaintCell(cellIndex, number, false);
        foreach (var note in notes)
            gridController.PaintCell(cellIndex, note, true);
    }

    private bool IsCorrect(int cellIndex, int number) => solution[cellIndex] == number;

    private void OnDestroy()
    {
        gridController.onCellClicked -= HandleCellClicked;
    }
}
