using System.Text;
using UnityEngine;

[System.Serializable]
public class PuzzleSaveData
{
    public string puzzle;
    public string solution;
    public string[] history;
}

public class PuzzleLoader
{
    const int puzzleStringSize = 163;

    public static string savePath => System.IO.Path.Combine(Application.persistentDataPath, "savefile.json");
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

    public static (string, string, string[]) LoadSaved()
    {
        if (!hasPreviousSave) throw new System.Exception("No previous save file exists");
        string json = System.IO.File.ReadAllText(savePath);
        PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);
        return (data.puzzle, data.solution, data.history);
    }

    public static void SavePuzzle(string puzzle, string solution, string[] pastStates)
    {
        PuzzleSaveData data = new PuzzleSaveData { puzzle = puzzle, solution = solution, history = pastStates };
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(savePath, json);
    }

    public static void DeleteSave()
    {
        if (!hasPreviousSave) return;
        System.IO.File.Delete(savePath);
    }
}
