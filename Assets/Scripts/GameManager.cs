using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public string previousSave { get; private set; }

    private string puzzle, solution;

    private PuzzleLoader puzzleLoader;

    private string savePath;

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        savePath = System.IO.Path.Combine(Application.persistentDataPath, "save.txt");
    }

    public bool HasPreviousSave()
    {
        return System.IO.File.Exists(savePath);
    }

    public void StartGame(int difficulty)
    {
        puzzleLoader = new PuzzleLoader();
        int randomIndex = Mathf.FloorToInt(Random.Range(0, 100));
        (puzzle, solution) = puzzleLoader.LoadPuzzle(randomIndex);
    }

    public void ContinueGame()
    {
        if (!HasPreviousSave())
        {
            throw new System.Exception("There's no savefile to load");
        }

        puzzleLoader = new PuzzleLoader();
        (puzzle, solution) = puzzleLoader.LoadPuzzle(System.IO.File.ReadAllText(savePath));
    }

    public (string, string) GetPuzzle() => (puzzle, solution);
}
