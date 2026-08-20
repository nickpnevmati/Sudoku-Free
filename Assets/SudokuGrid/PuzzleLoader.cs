using UnityEngine;

[System.Serializable]
public class PuzzleSaveData
{
    public string puzzle;
    public int elapsedSeconds;
    public string solution;
    public string[] history;
}

public class PuzzleLoader
{
    public static string savePath => System.IO.Path.Combine(Application.persistentDataPath, "savefile.json");
    public static bool hasPreviousSave => System.IO.File.Exists(savePath);

    public static (string, int, string, string[]) LoadSaved()
    {
        Debug.Log(savePath);
        if (!hasPreviousSave) throw new System.Exception("No previous save file exists");
        string json = System.IO.File.ReadAllText(savePath);
        PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);
        return (data.puzzle, data.elapsedSeconds, data.solution, data.history);
    }

    public static void SavePuzzle(string puzzle, int secondsElapsed, string solution, string[] pastStates)
    {
        // Debug.Log("PuzzleLoader - SavePuzzle");
        PuzzleSaveData data = new PuzzleSaveData { puzzle = puzzle, elapsedSeconds = secondsElapsed, solution = solution, history = pastStates };
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(savePath, json);
    }

    public static void DeleteSave()
    {
        if (!hasPreviousSave) return;
        System.IO.File.Delete(savePath);
    }
}
