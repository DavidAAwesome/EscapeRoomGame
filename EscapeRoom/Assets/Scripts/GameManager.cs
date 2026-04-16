using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum State { MainMenu, Playing, Caught, Escaped }
    public State CurrentState { get; private set; } = State.MainMenu;

    public bool IsPlaying => CurrentState == State.Playing;

    public int RunNumber { get; private set; } = 0;
    public float DifficultyScale => 1f + (RunNumber * 0.25f);

    [Header("UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject caughtPanel;
    [SerializeField] private GameObject escapedPanel;

    [SerializeField] private TextMeshProUGUI puzzleProgressText;
    [SerializeField] private TextMeshProUGUI currentObjectiveText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI runNumberText;

    [SerializeField] private TextMeshProUGUI caughtRunText;
    [SerializeField] private TextMeshProUGUI escapedRunText;
    [SerializeField] private TextMeshProUGUI escapedDifficultyText;

    [Header("Transition")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicMenu;
    [SerializeField] private AudioClip musicGame;
    [SerializeField] private AudioClip musicCaught;
    [SerializeField] private AudioClip musicEscaped;

    private Coroutine hintRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetState(State.Playing);
    }

    void SetState(State newState)
    {
        CurrentState = newState;

        mainMenuPanel?.SetActive(false);
        hudPanel?.SetActive(false);
        caughtPanel?.SetActive(false);
        escapedPanel?.SetActive(false);

        switch (newState)
        {
            case State.MainMenu:
                mainMenuPanel?.SetActive(true);
                PlayMusic(musicMenu);
                UnlockCursor();
                break;

            case State.Playing:
                hudPanel?.SetActive(true);
                PlayMusic(musicGame);
                LockCursor();
                UpdateHUD();
                break;

            case State.Caught:
                Debug.Log("Caught!");
                caughtPanel?.SetActive(true);
                PlayMusic(musicCaught);
                UnlockCursor();

                if (caughtRunText)
                    caughtRunText.text = $"Run #{RunNumber} — You were caught";

                break;

            case State.Escaped:
                escapedPanel?.SetActive(true);
                PlayMusic(musicEscaped);
                UnlockCursor();

                if (escapedRunText)
                    escapedRunText.text = $"Run #{RunNumber} Complete";

                if (escapedDifficultyText)
                    escapedDifficultyText.text =
                        $"Next run: x{1f + ((RunNumber + 1) * 0.25f):F2}";

                break;
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        foreach (var listener in FindObjectsOfType<MonoBehaviour>(true))
        {
            listener.SendMessage("GameStarting", SendMessageOptions.DontRequireReceiver);
        }
        
        yield return Fade(0f, 1f);

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ResetPlayer();
        }

        foreach (var enemy in FindObjectsOfType<GrannyAI>())
        {
            enemy.ApplyDifficultyMultiplier(DifficultyScale);
            enemy.ResetAI();
        }

        PuzzleManager.Instance?.ResetAllPuzzles();

        SetState(State.Playing);

        yield return Fade(1f, 0f);
    }

    public void RetryRun()
    {
        StartGame();
    }

    public void StartNextRun()
    {
        RunNumber++;
        StartGame();
    }

    public void GoToMainMenu()
    {
        StartCoroutine(MenuRoutine());
    }

    IEnumerator MenuRoutine()
    {
        yield return Fade(0f, 1f);
        SetState(State.MainMenu);
        yield return Fade(1f, 0f);
    }

    public void OnPlayerCaught()
    {
        if (!IsPlaying) return;

        var player = FindObjectOfType<PlayerController>();
        player?.FreezePlayer();

        StartCoroutine(DelayedState(State.Caught, 1.2f));
    }

    public void OnPlayerEscaped()
    {
        if (!IsPlaying) return;

        StartCoroutine(DelayedState(State.Escaped, 0.5f));
    }

    public void OnPuzzleProgress(int solved, int total)
    {
        UpdateHUD();
        ShowHint(PuzzleManager.Instance?.GetCurrentHint());
    }

    public void OnItemPickedUp(string itemID)
    {
        ShowHint("Picked up: " + itemID);
    }

    void UpdateHUD()
    {
        if (PuzzleManager.Instance == null) return;

        int solved = PuzzleManager.Instance.SolvedPuzzles;
        int total = PuzzleManager.Instance.TotalPuzzles;

        if (puzzleProgressText)
            puzzleProgressText.text = $"Objectives: {solved}/{total}";

        if (currentObjectiveText)
        {
            var puzzle = PuzzleManager.Instance.GetCurrentPuzzle();
            currentObjectiveText.text = puzzle != null ? puzzle.description : "Find the exit";
        }

        if (runNumberText)
            runNumberText.text = $"Run #{RunNumber}";
    }

    void ShowHint(string text, float duration = 3f)
    {
        if (hintText == null || string.IsNullOrEmpty(text)) return;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        hintRoutine = StartCoroutine(HintRoutine(text, duration));
    }

    IEnumerator HintRoutine(string text, float duration)
    {
        hintText.gameObject.SetActive(true);
        hintText.text = text;

        yield return new WaitForSeconds(duration);

        hintText.gameObject.SetActive(false);
    }

    IEnumerator DelayedState(State state, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(state);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;

        float t = 0f;
        fadePanel.gameObject.SetActive(true);

        while (t < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = to;

        if (to <= 0f)
            fadePanel.gameObject.SetActive(false);
    }

    void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}