using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState initialState = GameState.Playing;

    private bool subscribed;

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = initialState;
        Time.timeScale = initialState == GameState.Playing ? 1f : 0f;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        OnStateChanged?.Invoke(CurrentState);
    }

    private void OnDisable()
    {
        if (subscribed && InputManager.Instance != null)
        {
            InputManager.Instance.OnPause -= HandlePause;
        }

        subscribed = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Menu)
        {
            return;
        }

        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
        {
            return;
        }

        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void EndGame()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        if (CurrentState == GameState.Menu)
        {
            return;
        }

        Time.timeScale = 0f;
        SetState(GameState.Menu);
    }

    private void SetState(GameState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);
    }

    private void TrySubscribe()
    {
        if (subscribed || InputManager.Instance == null)
        {
            return;
        }

        InputManager.Instance.OnPause += HandlePause;
        subscribed = true;
    }

    private void HandlePause()
    {
        if (CurrentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }
}
