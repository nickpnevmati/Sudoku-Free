using System.Text;
using UnityEngine;

[System.Serializable]
public class PuzzleSaveData
{
    public string puzzle;
    public string solution;
    public string[] history;
    public float time;
}

public class PuzzleLoader
{
    public static string savePath => System.IO.Path.Combine(Application.persistentDataPath, "savefile.json");
    public static bool hasPreviousSave => System.IO.File.Exists(savePath);

    public static (string, string, string[], float) LoadSaved()
    {
        if (!hasPreviousSave) throw new System.Exception("No previous save file exists");
        string json = System.IO.File.ReadAllText(savePath);
        PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);
        if (data == null || data.puzzle?.Length != 81 || data.solution?.Length != 81 || data.history == null)
            throw new System.Exception("Save file is incomplete or corrupt");
        return (data.puzzle, data.solution, data.history, data.time);
    }

    public static void SavePuzzle(string puzzle, string solution, string[] pastStates, float time)
    {
        PuzzleSaveData data = new PuzzleSaveData { puzzle = puzzle, solution = solution, history = pastStates, time = time };
        string json = JsonUtility.ToJson(data);

        // Write to a temp file and swap it in, so a kill mid-write can't leave a truncated save
        string tempPath = savePath + ".tmp";
        System.IO.File.WriteAllText(tempPath, json);
        if (hasPreviousSave) System.IO.File.Replace(tempPath, savePath, null);
        else System.IO.File.Move(tempPath, savePath);
    }

    public static void DeleteSave()
    {
        if (!hasPreviousSave) return;
        System.IO.File.Delete(savePath);
    }
}
