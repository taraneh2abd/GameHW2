using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private PlayerCarController car;
    [SerializeField] private PlayerLives lives;
    [SerializeField] private PlayerFuel fuel;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Text livesText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestText;
    [SerializeField] private Text speedText;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Image fuelFill;
    [SerializeField] private Text fuelPercentText;
    [SerializeField] private float speedToKmh = 10f;
    [SerializeField] private float lowFuelThreshold = 0.25f;
    [SerializeField] private float lowFuelFlashRate = 6f;
    [SerializeField] private Color fuelFullColor = new Color(0.1f, 1f, 0.45f, 1f);
    [SerializeField] private Color fuelEmptyColor = new Color(1f, 0.1f, 0.2f, 1f);

    private GameManager subscribedManager;
    private int bestSaved;
    private int currentScore;
    private float fuelValue = 1f;

    private void OnEnable()
    {
        bestSaved = HighScoreTracker.BestScore;
        SetLives(lives != null ? lives.CurrentLives : 0);
        SetScore(ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0);
        SetFuel(fuel != null ? fuel.NormalizedFuel : 1f);

        if (lives != null)
        {
            lives.OnLivesChanged += SetLives;
        }

        if (fuel != null)
        {
            fuel.OnFuelChanged += SetFuel;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += SetScore;
        }

        HighScoreTracker.OnScoreSubmitted += HandleScoreSubmitted;
        TrySubscribeToGame();
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= SetScore;
            ScoreManager.Instance.OnScoreChanged += SetScore;
            SetScore(ScoreManager.Instance.Score);
        }

        TrySubscribeToGame();
        if (subscribedManager != null)
        {
            ApplyState(subscribedManager.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (lives != null)
        {
            lives.OnLivesChanged -= SetLives;
        }

        if (fuel != null)
        {
            fuel.OnFuelChanged -= SetFuel;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= SetScore;
        }

        HighScoreTracker.OnScoreSubmitted -= HandleScoreSubmitted;

        if (subscribedManager != null)
        {
            subscribedManager.OnStateChanged -= ApplyState;
            subscribedManager = null;
        }
    }

    private void Update()
    {
        if (car != null && speedText != null)
        {
            speedText.text = Mathf.RoundToInt(car.CurrentSpeed * speedToKmh) + " km/h";
        }

        UpdateFuelColor();
    }

    private void TrySubscribeToGame()
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
        if (hudRoot != null)
        {
            hudRoot.SetActive(state == GameState.Playing || state == GameState.Paused);
        }
    }

    private void HandleScoreSubmitted(int score, bool isNewRecord)
    {
        bestSaved = HighScoreTracker.BestScore;
        SetScore(score);
    }

    private void SetLives(int value)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + value;
        }
    }

    private void SetScore(int value)
    {
        currentScore = value;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + value;
        }

        if (bestText != null)
        {
            bestText.text = "Best: " + Mathf.Max(bestSaved, currentScore);
        }
    }

    private void SetFuel(float normalized)
    {
        fuelValue = Mathf.Clamp01(normalized);
        if (fuelSlider != null)
        {
            fuelSlider.value = fuelValue;
        }

        if (fuelPercentText != null)
        {
            fuelPercentText.text = Mathf.RoundToInt(fuelValue * 100f) + "%";
        }

        UpdateFuelColor();
    }

    private void UpdateFuelColor()
    {
        if (fuelFill == null)
        {
            return;
        }

        Color color = Color.Lerp(fuelEmptyColor, fuelFullColor, fuelValue);
        if (fuelValue < lowFuelThreshold)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * lowFuelFlashRate);
            color = Color.Lerp(fuelEmptyColor, Color.white, pulse * 0.6f);
        }

        fuelFill.color = color;
    }
}
