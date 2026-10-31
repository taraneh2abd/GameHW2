using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCarController player;
    [SerializeField] private SpawnManager spawnManager;

    [Header("Progress")]
    [SerializeField] private float rampDistance = 2500f;
    [SerializeField] private AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Speed")]
    [SerializeField] private float maxSpeed = 24f;

    [Header("Density")]
    [SerializeField] private float maxObstacleWeight = 9f;
    [SerializeField] private float minEmptyWeight = 0.25f;
    [SerializeField] private float minRowSpacing = 12.5f;
    [SerializeField, Range(0f, 1f)] private float maxMovingObstacleChance = 0.45f;
    [SerializeField, Range(0f, 1f)] private float maxSecondObstacleChance = 0.4f;
    [SerializeField] private float maxMovingApproachSpeed = 9f;

    private float startZ;
    private float startSpeed;
    private float startObstacleWeight;
    private float startEmptyWeight;
    private float startRowSpacing;
    private float startMovingChance;
    private float startSecondChance;
    private float startApproachSpeed;
    private bool captured;

    public float Progress { get; private set; }

    private void Start()
    {
        Capture();
    }

    private void Update()
    {
        if (!captured || player == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        float distance = Mathf.Max(0f, player.transform.position.z - startZ);
        float linear = rampDistance > 0f ? Mathf.Clamp01(distance / rampDistance) : 1f;
        Progress = Mathf.Clamp01(progressCurve.Evaluate(linear));

        player.currentSpeed = Mathf.Lerp(startSpeed, Mathf.Max(startSpeed, maxSpeed), Progress);

        if (spawnManager == null)
        {
            return;
        }

        spawnManager.ObstacleWeight = Mathf.Lerp(startObstacleWeight, Mathf.Max(startObstacleWeight, maxObstacleWeight), Progress);
        spawnManager.EmptyWeight = Mathf.Lerp(startEmptyWeight, Mathf.Min(startEmptyWeight, minEmptyWeight), Progress);
        spawnManager.RowSpacing = Mathf.Lerp(startRowSpacing, Mathf.Min(startRowSpacing, minRowSpacing), Progress);
        spawnManager.MovingObstacleChance = Mathf.Lerp(startMovingChance, Mathf.Max(startMovingChance, maxMovingObstacleChance), Progress);
        spawnManager.SecondObstacleChance = Mathf.Lerp(startSecondChance, Mathf.Max(startSecondChance, maxSecondObstacleChance), Progress);
        spawnManager.MovingApproachSpeed = Mathf.Lerp(startApproachSpeed, Mathf.Max(startApproachSpeed, maxMovingApproachSpeed), Progress);
    }

    private void Capture()
    {
        if (player != null)
        {
            startZ = player.transform.position.z;
            startSpeed = player.currentSpeed;
        }

        if (spawnManager != null)
        {
            startObstacleWeight = spawnManager.ObstacleWeight;
            startEmptyWeight = spawnManager.EmptyWeight;
            startRowSpacing = spawnManager.RowSpacing;
            startMovingChance = spawnManager.MovingObstacleChance;
            startSecondChance = spawnManager.SecondObstacleChance;
            startApproachSpeed = spawnManager.MovingApproachSpeed;
        }

        captured = player != null;
    }
}
