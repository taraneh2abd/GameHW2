using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private PlayerCarController player;
    [SerializeField] private float pointsPerMeter = 1f;
    [SerializeField] private int pointsPerCoin = 10;

    private float startZ;
    private float lastZ;
    private float distanceScore;
    private int coinScore;

    public int Score { get; private set; }

    public float DistanceTraveled { get; private set; }

    public float CurrentSpeed => player != null ? player.currentSpeed : 0f;

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        Coin.OnCollected += HandleCoinCollected;
    }

    private void Start()
    {
        CaptureStart();
        OnScoreChanged?.Invoke(Score);
    }

    private void OnDisable()
    {
        Coin.OnCollected -= HandleCoinCollected;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        float z = player.transform.position.z;
        float delta = z - lastZ;
        lastZ = z;

        if (delta <= 0f)
        {
            return;
        }

        DistanceTraveled = z - startZ;
        distanceScore += delta * pointsPerMeter;
        Refresh();
    }

    public void ResetScore()
    {
        distanceScore = 0f;
        coinScore = 0;
        DistanceTraveled = 0f;
        CaptureStart();
        Refresh();
    }

    private void CaptureStart()
    {
        startZ = player != null ? player.transform.position.z : 0f;
        lastZ = startZ;
    }

    private void HandleCoinCollected(Coin coin)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        coinScore += pointsPerCoin;
        Refresh();
    }

    private void Refresh()
    {
        int total = Mathf.FloorToInt(distanceScore) + coinScore;
        if (total == Score)
        {
            return;
        }

        Score = total;
        OnScoreChanged?.Invoke(Score);
    }
}
