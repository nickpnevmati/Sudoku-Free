using System;
using System.Text;
using Unity.Android.Gradle.Manifest;
using UnityEditor.UI;
using UnityEngine;

public class PuzzleLoader
{
    byte[] puzzleData;

    const int puzzleStringSize = 163; // each puzzle consists of the puzzle and the solution concatenated (plus a newline), a total of 9x9x2+1=163 chars [0-9]
    int numPuzzles = 0;

    public PuzzleLoader()
    {
        var puzzlesFile = Resources.Load<TextAsset>("puzzles");
        puzzleData = puzzlesFile.bytes;
        numPuzzles = puzzleData.Length / puzzleStringSize;
    }

    public (string, string) LoadPuzzle(int index)
    {
        byte[] buffer = new byte[puzzleStringSize];
        System.Buffer.BlockCopy(puzzleData, index * puzzleStringSize, buffer, 0, puzzleStringSize);
        string puzzleString = Encoding.UTF8.GetString(buffer).TrimEnd('\0', ' ', '\n', '\r');
        string puzzle = puzzleString.Substring(0, 81);
        string solution = puzzleString.Substring(81, 81);
        return (puzzle, solution);
    }

    public (string, string) LoadPuzzle(string puzzleString)
    {
        char[] puzzle = new char[81];
        char[] solution = new char[81];

        puzzleString.CopyTo(0, puzzle, 0, 81);
        puzzleString.CopyTo(81, solution, 0, 81);

        return (new string(puzzle), new string(solution));
    }
    
}
