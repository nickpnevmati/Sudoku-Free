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

    int?[] sampleSudoku = new int?[] { // Temp
        9, 5, null, 1, null, null, 8, 4, null,
        4, 2, 3, 6, null, null, 9, null, null,
        null, null, null, 9, null, null, null, 2, null,
        null, 9, null, 3, null, null, 4, null, null,
        6, null, null, 8, null, 5, 3, null, 9,
        null, 8, 1, null, 4, 9, null, null, null, 
        5, null, 9, 4, null, 6, null, null, null, 
        null, null, 8, null, 2, 3, null, null, 4,
        null, null, null, null, null, null, 1, 3, null
    };

    int[] solution = new int[81]; // temp

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
        gridController.ConstructPuzzle(sampleSudoku);
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
