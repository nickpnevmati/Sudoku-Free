using UnityEngine;
using UnityEngine.UIElements;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { private set; get; }
    private UIDocument m_document;
    private VisualElement m_root;

    public bool continueFlag;
    private Cell[] gridCells;

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
        ContructGrid();
    }

    private void ContructGrid()
    {
        VisualElement grid_root = m_root.Q<VisualElement>("GridInner");
        int index = 0;
        gridCells = new Cell[81];
        foreach (var row in grid_root.Children())
        {
            if (!row.name.Equals("GridRow")) continue;
            foreach (var cell in row.Children())
            {
                if (!cell.name.Equals("TemplateContainer")) continue;
                gridCells[index] = new Cell(cell);
                index++;
            }
        }
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
    }
}
