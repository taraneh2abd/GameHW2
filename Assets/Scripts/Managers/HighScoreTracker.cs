using System;
using UnityEngine;

public class HighScoreTracker : MonoBehaviour
{
    private GameManager subscribedTo;

    public static event Action<int, bool> OnScoreSubmitted;

    public static int BestScore => HighScoreService.Load();

    public static int LastSubmittedScore { get; private set; }

    public static bool LastWasNewRecord { get; private set; }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (subscribedTo != null)
        {
            subscribedTo.OnStateChanged -= HandleStateChanged;
            subscribedTo = null;
        }
    }

    private void TrySubscribe()
    {
        if (subscribedTo != null || GameManager.Instance == null)
        {
            return;
        }

        subscribedTo = GameManager.Instance;
        subscribedTo.OnStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state != GameState.GameOver)
        {
            return;
        }

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        bool isRecord = HighScoreService.TrySubmit(score);
        LastSubmittedScore = score;
        LastWasNewRecord = isRecord;
        OnScoreSubmitted?.Invoke(score, isRecord);
    }
}
