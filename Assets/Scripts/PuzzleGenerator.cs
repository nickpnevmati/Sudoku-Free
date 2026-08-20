using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ListOfLists = System.Collections.Generic.List<System.Collections.Generic.List<int>>;

public enum PuzzleDifficulty
{
    EASY,
    MEDIUM,
    HARD,
    EVIL
}

public class PuzzleGenerator
{
    private static PuzzleGenerator instance;

    // Guesses separating hard from evil. Counts come in twos: the search always
    // branches on a two-candidate cell and tries both, so this is 2 branch points.
    const int EVIL_GUESSES = 4;

    private PuzzleSolver solver;
    private Random rand;

    public static PuzzleGenerator Instance
    {
        get
        {
            if (instance == null)
                instance = new PuzzleGenerator();
            return instance;
        }
    }

    private PuzzleGenerator()
    {
        solver = PuzzleSolver.Instance;
        rand = new Random();
    }

    /// <summary>Reseed the RNG for reproducible runs.</summary>
    public void Seed(int seed) => rand = new Random(seed);

    /// <summary>
    /// Dig random solutions until one yields a puzzle in the requested band.
    /// Returns (puzzle, solution) as 81-char strings, '0' for a blank.
    /// The loop is unbounded, so a caller running this off the main thread should
    /// pass a token it can cancel on teardown.
    /// </summary>
    public (string, string) Generate(PuzzleDifficulty difficulty, CancellationToken token = default)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            int[] solution = RandomSolution();
            var found = Dig(solution, token);
            if (found.TryGetValue(difficulty, out int[] puzzle))
                return (GridToString(puzzle), GridToString(solution));
        }
    }

    /// <summary>
    /// One dig pass, keeping every band it passes through. Cheaper than calling
    /// Generate once per band when you want a full set.
    /// </summary>
    public Dictionary<PuzzleDifficulty, (string, string)> GenerateBatch()
    {
        int[] solution = RandomSolution();
        string solutionStr = GridToString(solution);
        return Dig(solution).ToDictionary(
            kv => kv.Key,
            kv => (GridToString(kv.Value), solutionStr));
    }

    private static bool IsPlacementValid(int[] grid, int index, int value)
    {
        int row = index / 9;
        int col = index % 9;

        for (int i = 0; i < 9; i++)
            if (grid[i * 9 + col] == value || grid[row * 9 + i] == value)
                return false;

        int rowStart = row - row % 3;
        int colStart = col - col % 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (grid[(rowStart + i) * 9 + (colStart + j)] == value)
                    return false;

        return grid[index] == 0;
    }

    /// <summary>Fill an empty grid by randomised backtracking.</summary>
    private int[] RandomSolution()
    {
        int[] grid = new int[81];
        Fill(grid, 0);
        return grid;
    }

    private bool Fill(int[] grid, int pos)
    {
        if (pos == 81)
            return true;

        int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(values);

        foreach (int val in values)
        {
            if (!IsPlacementValid(grid, pos, val))
                continue;
            grid[pos] = val;
            if (Fill(grid, pos + 1))
                return true;
            grid[pos] = 0;
        }
        return false;
    }

    private void Shuffle(int[] items)
    {
        for (int i = items.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    /// <summary>Difficulty band, from the hardest technique the grid actually demands.</summary>
    private PuzzleDifficulty Classify(int[] grid, int guesses)
    {
        if (solver.NakedSingleSolvable(grid))
            return PuzzleDifficulty.EASY;
        if (guesses == 0)
            return PuzzleDifficulty.MEDIUM; // hidden singles were needed, but never a guess
        if (guesses <= EVIL_GUESSES)
            return PuzzleDifficulty.HARD;
        return PuzzleDifficulty.EVIL;
    }

    /// <summary>
    /// Remove clues at random for as long as the grid stays uniquely solvable.
    /// A dig walks the same grid from easy towards evil, so one solution can seed
    /// a puzzle in several bands. Returns {band: puzzle}, holding the last grid
    /// seen in each band - the one dug furthest, so an easy puzzle is as sparse as
    /// it can get while still yielding to naked singles, rather than a full grid
    /// with a couple of blanks.
    /// </summary>
    private Dictionary<PuzzleDifficulty, int[]> Dig(int[] solution, CancellationToken token = default)
    {
        int[] grid = (int[])solution.Clone();
        var found = new Dictionary<PuzzleDifficulty, int[]>();

        int[] order = Enumerable.Range(0, 81).ToArray();
        Shuffle(order);

        foreach (int idx in order)
        {
            token.ThrowIfCancellationRequested();
            int removed = grid[idx];
            grid[idx] = 0;

            var (solutions, guesses) = solver.Solve(grid);
            if (solutions == null || solutions.Count != 1)
            {
                grid[idx] = removed; // that clue was load-bearing, put it back
                continue;
            }

            found[Classify(grid, guesses)] = (int[])grid.Clone();
        }
        return found;
    }

    private static string GridToString(int[] grid)
    {
        var sb = new StringBuilder(81);
        foreach (int v in grid)
            sb.Append((char)('0' + v));
        return sb.ToString();
    }
}

public class PuzzleSolver
{
    private static PuzzleSolver instance;

    private int[][] prebuiltNeighbors;
    private int[][] prebuiltUnits;

    public static PuzzleSolver Instance
    {
        get
        {
            if (instance == null)
                instance = new PuzzleSolver();
            return instance;
        }
    }

    private PuzzleSolver()
    {
        prebuiltNeighbors = BuildNeighbors();
        prebuiltUnits = BuildUnits();
    }

    /// <summary>
    /// Return (solutions, guesses); at most 2 solutions, enough to test uniqueness.
    /// guesses is the number of values tried at branch points: 0 means the grid
    /// fell out of propagation alone, with no search at all.
    /// </summary>
    public (List<ListOfLists>, int) Solve(int[] grid)
    {
        var neighbors = prebuiltNeighbors;
        var domain = InitialDomain(grid);
        if (!Propagate(domain, neighbors))
            return (null, 0);
        var solutions = new List<ListOfLists>();
        int guesses = Backtrack(domain, neighbors, solutions, 0);
        return (solutions, guesses);
    }

    /// <summary>
    /// True if plain arc consistency fills the grid on its own.
    /// No hidden singles and no search, so this is the weakest technique tier.
    /// </summary>
    public bool NakedSingleSolvable(int[] grid)
    {
        var domain = InitialDomain(grid);
        if (!AC3(domain, prebuiltNeighbors))
            return false;
        return domain.All(d => d.Count == 1);
    }

    /// <summary>Flatten a solved domain back into an 81-cell grid.</summary>
    public static int[] GridFromDomain(ListOfLists domain) =>
        domain.Select(d => d[0]).ToArray();

    private static ListOfLists InitialDomain(int[] grid) =>
        Enumerable.Range(0, 81)
            .Select(i => grid[i] != 0
                ? new List<int> { grid[i] }
                : Enumerable.Range(1, 9).ToList())
            .ToList();

    private int Backtrack(ListOfLists domain, int[][] neighbors, List<ListOfLists> solutions, int guesses)
    {
        var maybeMinVar = SelectMinRemainingVar(domain);
        if (maybeMinVar == null)
        {
            solutions.Add(domain);
            return guesses;
        }

        int minVar = maybeMinVar.Value;

        foreach (var val in domain[minVar].ToArray())
        {
            var newDomain = domain.Select(d => new List<int>(d)).ToList();
            newDomain[minVar] = new List<int> { val };
            var initialArcs = neighbors[minVar].Select(z => (z, minVar)).ToList();

            guesses++; // count the attempt, whether or not it survives
            if (Propagate(newDomain, neighbors, initialArcs))
            {
                guesses = Backtrack(newDomain, neighbors, solutions, guesses);

                if (solutions.Count > 1)
                    break;
            }
        }
        return guesses;
    }

    private int? SelectMinRemainingVar(ListOfLists domain)
    {
        int best = -1;
        int bestCount = int.MaxValue;

        for (int i = 0; i < 81; i++)
        {
            int count = domain[i].Count;
            if (count > 1 && count < bestCount)
            {
                best = i;
                bestCount = count;
            }
        }

        return best < 0 ? (int?)null : best;
    }

    /// <summary>Run ac3 and hidden singles alternately until neither changes anything.</summary>
    private bool Propagate(ListOfLists domain, int[][] neighbors, List<(int, int)> worklist = null)
    {
        if (!AC3(domain, neighbors, worklist))
            return false;

        while (true)
        {
            var assigned = HiddenSingles(domain);
            if (assigned == null)
                return false;
            if (assigned.Count == 0)
                return true;

            var arcs = (
                from i in assigned
                from z in neighbors[i]
                select (z, i)
            ).ToList();
            if (!AC3(domain, neighbors, arcs))
                return false;
        }
    }

    private bool ArcReduce(int x, int y, ListOfLists domain)
    {
        // y only constrains x once it is down to a single value
        if (domain[y].Count != 1)
            return false;

        return domain[x].Remove(domain[y][0]);
    }

    /// <summary>
    /// Assign each value that fits only one cell of a unit.
    /// Returns the list of cells assigned, or null if some unit has a value
    /// with nowhere left to go.
    /// </summary>
    private List<int> HiddenSingles(ListOfLists domain)
    {
        var assigned = new List<int>();
        foreach (int[] unit in prebuiltUnits)
        {
            for (int val = 1; val < 10; val++)
            {
                int spot = -1;
                int spotCount = 0;
                foreach (int i in unit)
                {
                    if (!domain[i].Contains(val))
                        continue;
                    spot = i;
                    if (++spotCount > 1)
                        break;
                }

                if (spotCount == 0)
                    return null;
                if (spotCount == 1 && domain[spot].Count > 1)
                {
                    domain[spot] = new List<int> { val };
                    assigned.Add(spot);
                }
            }
        }
        return assigned;
    }

    private bool AC3(ListOfLists domain, int[][] neighbors, List<(int, int)> worklist = null)
    {
        var stack = new Stack<(int, int)>();

        if (worklist != null && worklist.Count > 0)
        {
            foreach (var arc in worklist)
                stack.Push(arc);
        }
        else
        {
            for (int x = 0; x < 81; x++)
                foreach (int y in neighbors[x])
                    stack.Push((x, y));
        }

        while (stack.Count > 0)
        {
            (int x, int y) = stack.Pop();

            if (ArcReduce(x, y, domain))
            {
                if (domain[x].Count == 0)
                    return false;

                foreach (int z in neighbors[x])
                    if (z != y)
                        stack.Push((z, x));
            }
        }

        return true;
    }

    private int[][] BuildUnits()
    {
        int[][] units = new int[27][];
        int writeIndex = 0;

        for (int row = 0; row < 9; row++)
        {
            units[writeIndex] = new int[9];
            for (int col = 0; col < 9; col++)
                units[writeIndex][col] = row * 9 + col;
            writeIndex++;
        }

        for (int col = 0; col < 9; col++)
        {
            units[writeIndex] = new int[9];
            for (int row = 0; row < 9; row++)
                units[writeIndex][row] = row * 9 + col;
            writeIndex++;
        }

        for (int startRow = 0; startRow < 9; startRow += 3)
        {
            for (int startCol = 0; startCol < 9; startCol += 3)
            {
                units[writeIndex] = new int[9];
                int idxInternal = 0;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        units[writeIndex][idxInternal++] = (startRow + i) * 9 + startCol + j;
                writeIndex++;
            }
        }
        return units;
    }

    /// <summary>The 20 cells sharing a row, column or box with index.</summary>
    private int[] CellNeighbors(int index)
    {
        var neighbors = new List<int>(20);

        int row = index / 9;
        int col = index % 9;

        for (int i = 0; i < 9; i++)
        {
            int rowCell = row * 9 + i;
            if (rowCell != index) neighbors.Add(rowCell);

            int colCell = i * 9 + col;
            if (colCell != index) neighbors.Add(colCell);
        }

        int startCol = col - col % 3;
        int startRow = row - row % 3;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int r = startRow + i;
                int c = startCol + j;
                int idx = r * 9 + c;
                if (idx == index) continue;
                if (r == row || c == col) continue; // already added above
                neighbors.Add(idx);
            }
        }

        return neighbors.ToArray();
    }

    private int[][] BuildNeighbors()
    {
        int[][] neighbors = new int[81][];
        for (int i = 0; i < 81; i++)
            neighbors[i] = CellNeighbors(i);

        return neighbors;
    }
}
