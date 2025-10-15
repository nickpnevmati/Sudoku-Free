using System.Text;
using Unity.Android.Gradle.Manifest;
using UnityEditor.UI;
using UnityEngine;

public class PuzzleLoader
{
    byte[] puzzleData;

    const int puzzleSize = 163; // each puzzle consists of the puzzle and the solution concatenated (plus a newline), a total of 9x9x2+1=163 chars [0-9]
    int numPuzzles = 0;

    public PuzzleLoader()
    {
        var puzzlesFile = Resources.Load<TextAsset>("puzzles");
        puzzleData = puzzlesFile.bytes;
        numPuzzles = puzzleData.Length / puzzleSize;
    }

    public (string, string) LoadPuzzle(int index)
    {
        byte[] buffer = new byte[puzzleSize];
        System.Buffer.BlockCopy(puzzleData, index * puzzleSize, buffer, 0, puzzleSize);
        string puzzleString = Encoding.UTF8.GetString(buffer).TrimEnd('\0', ' ', '\n', '\r');
        string puzzle = puzzleString.Substring(0, 81);
        string solution = puzzleString.Substring(81, 81);
        return (puzzle, solution);
    }
    
}
