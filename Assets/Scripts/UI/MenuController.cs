using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Text mainMenuBestText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverBestText;
    [SerializeField] private Text newRecordText;
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    private GameManager subscribedManager;
    private bool gameOverRefreshPending;

    private void OnEnable()
    {
        Bind(startButton, OnStartClicked);
        Bind(quitButton, OnQuitClicked);
        Bind(resumeButton, OnResumeClicked);
        Bind(pauseRestartButton, OnRestartClicked);
        Bind(pauseMainMenuButton, OnMainMenuClicked);
        Bind(gameOverRestartButton, OnRestartClicked);
        Bind(gameOverMainMenuButton, OnMainMenuClicked);
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        if (subscribedManager != null)
        {
            ApplyState(subscribedManager.CurrentState);
        }
    }

    private void OnDisable()
    {
        Unbind(startButton, OnStartClicked);
        Unbind(quitButton, OnQuitClicked);
        Unbind(resumeButton, OnResumeClicked);
        Unbind(pauseRestartButton, OnRestartClicked);
        Unbind(pauseMainMenuButton, OnMainMenuClicked);
        Unbind(gameOverRestartButton, OnRestartClicked);
        Unbind(gameOverMainMenuButton, OnMainMenuClicked);

        if (subscribedManager != null)
        {
            subscribedManager.OnStateChanged -= ApplyState;
            subscribedManager = null;
        }
    }

    private void LateUpdate()
    {
        if (!gameOverRefreshPending)
        {
            return;
        }

        gameOverRefreshPending = false;
        RefreshGameOver();
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    private void TrySubscribe()
    {
        if (subscribedManager != null || GameManager.Instance == null)
        {
            return;
        }

        subscribedManager = GameManager.Instance;
        subscribedManager.OnStateChanged += ApplyState;
    }

    private void ApplyState(GameState state)
    {
        bool inMenu = state == GameState.Menu;
        bool paused = state == GameState.Paused;
        bool gameOver = state == GameState.GameOver;

        if (inMenu && mainMenuBestText != null)
        {
            mainMenuBestText.text = "Best Score: " + HighScoreService.Load();
        }

        SetActive(mainMenuPanel, inMenu);
        SetActive(pauseMenuPanel, paused);
        SetActive(gameOverPanel, gameOver);

        if (gameOver)
        {
            RefreshGameOver();
            gameOverRefreshPending = true;
        }
        else
        {
            gameOverRefreshPending = false;
        }

        if (inMenu)
        {
            Select(startButton);
        }
        else if (paused)
        {
            Select(resumeButton);
        }
        else if (gameOver)
        {
            Select(gameOverRestartButton);
        }
    }

    private void RefreshGameOver()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        int best = HighScoreService.Load();
        bool isRecord = HighScoreTracker.LastSubmittedScore == score && HighScoreTracker.LastWasNewRecord;

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + score;
        }

        if (gameOverBestText != null)
        {
            gameOverBestText.text = "Best: " + Mathf.Max(best, score);
        }

        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(isRecord);
        }
    }

    private static void Select(Button button)
    {
        if (button != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
