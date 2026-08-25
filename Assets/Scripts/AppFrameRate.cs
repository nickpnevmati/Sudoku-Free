using UnityEngine;

/// <summary>
/// Unity leaves <see cref="Application.targetFrameRate"/> at the mobile default of 30 on Android,
/// which puts a 33ms floor under every visual response even when there is no work to do. This
/// phone's panel does 60/90/120, so 30 is pure latency for no benefit. 60 is the sane middle:
/// it halves input-to-pixel latency without the battery cost of chasing 120 for a Sudoku board.
/// </summary>
public static class AppFrameRate
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        Application.targetFrameRate = 60;
    }
}
