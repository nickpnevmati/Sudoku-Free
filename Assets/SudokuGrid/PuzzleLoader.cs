using System.Text;
using UnityEngine;

public class PuzzleLoader
{
    // each puzzle consists of the puzzle and the solution concatenated (plus a newline), a total of 9x9x2+1=163 chars [0-9]
    const int puzzleStringSize = 163;

    public static string savePath => System.IO.Path.Combine(Application.persistentDataPath, "savefile");
    public static bool hasPreviousSave => System.IO.File.Exists(savePath);

    public static (string, string) LoadPuzzle(int index)
    {
        var puzzlesFile = Resources.Load<TextAsset>("puzzles");
        byte[] buffer = new byte[puzzleStringSize];
        System.Buffer.BlockCopy(puzzlesFile.bytes, index * puzzleStringSize, buffer, 0, puzzleStringSize);
        string puzzleString = Encoding.UTF8.GetString(buffer).TrimEnd('\0', ' ', '\n', '\r');
        string puzzle = puzzleString.Substring(0, 81);
        string solution = puzzleString.Substring(81, 81);
        return (puzzle, solution);
    }

    public static (string, string) LoadSaved()
    {
        if (!hasPreviousSave) throw new System.Exception("No previous save file exists");
        string puzzleString = System.IO.File.ReadAllText(savePath);

        char[] puzzle = new char[81];
        char[] solution = new char[81];

        puzzleString.CopyTo(0, puzzle, 0, 81);
        puzzleString.CopyTo(81, solution, 0, 81);

        return (new string(puzzle), new string(solution));
    }

    public static void SavePuzzle(string puzzle, string solution) => System.IO.File.WriteAllText(savePath, puzzle + solution);
    public static void DeleteSave()
    {
        if (!hasPreviousSave) return;
        System.IO.File.Delete(savePath);
    }
}
