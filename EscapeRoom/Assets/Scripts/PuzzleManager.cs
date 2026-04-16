using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [System.Serializable]
    public class PuzzleEntry
    {
        public string id;
        [TextArea(1, 3)] public string description;
        [TextArea(1, 2)] public string hint;
        public bool isSolved;
        public UnityEvent onSolved;
    }

    [Header("Puzzles")]
    [SerializeField] private List<PuzzleEntry> puzzles = new List<PuzzleEntry>();

    [Header("Events")]
    [SerializeField] private UnityEvent onAllPuzzlesSolved;

    private Dictionary<string, PuzzleEntry> puzzleLookup = new Dictionary<string, PuzzleEntry>();
    private int solvedCount;

    public int TotalPuzzles => puzzles.Count;
    public int SolvedPuzzles => solvedCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    void BuildLookup()
    {
        puzzleLookup.Clear();
        solvedCount = 0;

        foreach (PuzzleEntry puzzle in puzzles)
        {
            if (!string.IsNullOrEmpty(puzzle.id) && !puzzleLookup.ContainsKey(puzzle.id))
            {
                puzzleLookup.Add(puzzle.id, puzzle);
            }

            if (puzzle.isSolved)
            {
                solvedCount++;
            }
        }
    }

    public void OnPuzzleSolved(string puzzleID)
    {
        if (!puzzleLookup.TryGetValue(puzzleID, out PuzzleEntry puzzle))
        {
            Debug.LogWarning("Puzzle ID not found: " + puzzleID);
            return;
        }

        if (puzzle.isSolved)
        {
            return;
        }

        puzzle.isSolved = true;
        solvedCount++;

        Debug.Log("Solved puzzle: " + puzzleID);

        puzzle.onSolved?.Invoke();
        GameManager.Instance?.OnPuzzleProgress(solvedCount, puzzles.Count);

        if (solvedCount >= puzzles.Count)
        {
            AllPuzzlesSolved();
        }
    }

    public bool IsSolved(string puzzleID)
    {
        if (puzzleLookup.TryGetValue(puzzleID, out PuzzleEntry puzzle))
        {
            return puzzle.isSolved;
        }

        return false;
    }

    public PuzzleEntry GetCurrentPuzzle()
    {
        foreach (PuzzleEntry puzzle in puzzles)
        {
            if (!puzzle.isSolved)
            {
                return puzzle;
            }
        }

        return null;
    }

    public string GetCurrentHint()
    {
        PuzzleEntry currentPuzzle = GetCurrentPuzzle();

        if (currentPuzzle == null || string.IsNullOrEmpty(currentPuzzle.hint))
        {
            return "Find the exit";
        }

        return currentPuzzle.hint;
    }

    void AllPuzzlesSolved()
    {
        Debug.Log("All puzzles solved");

        onAllPuzzlesSolved?.Invoke();
        GameManager.Instance?.OnPlayerEscaped();
    }

    public void ResetAllPuzzles()
    {
        solvedCount = 0;

        foreach (PuzzleEntry puzzle in puzzles)
        {
            puzzle.isSolved = false;
        }

        BuildLookup();
    }

    public void LoadPuzzles(List<(string id, string description, string hint)> puzzleData)
    {
        puzzles.Clear();

        foreach (var data in puzzleData)
        {
            PuzzleEntry newPuzzle = new PuzzleEntry();
            newPuzzle.id = data.id;
            newPuzzle.description = data.description;
            newPuzzle.hint = data.hint;
            newPuzzle.isSolved = false;
            newPuzzle.onSolved = new UnityEvent();

            puzzles.Add(newPuzzle);
        }

        BuildLookup();
    }
}