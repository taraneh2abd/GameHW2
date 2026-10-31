using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    private const int LaneCount = 3;

    [SerializeField] private RoadManager roadManager;
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float laneChangeSpeed = 18f;

    public float currentSpeed = 12f;

    private int currentLane = 1;
    private bool subscribed;

    public int CurrentLane => currentLane;
    public float CurrentSpeed => currentSpeed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        Vector3 position = transform.position;
        position.x = GetLaneX(currentLane);
        transform.position = position;
    }

    private void OnDisable()
    {
        if (subscribed && InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveLeft -= HandleMoveLeft;
            InputManager.Instance.OnMoveRight -= HandleMoveRight;
        }

        subscribed = false;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        Vector3 position = transform.position;
        position.z += currentSpeed * Time.deltaTime;
        position.x = Mathf.MoveTowards(position.x, GetLaneX(currentLane), laneChangeSpeed * Time.deltaTime);
        transform.position = position;

        if (roadManager != null)
        {
            roadManager.AdvanceAndMaybeSpawn(transform.position.z);
        }
    }

    public float GetLaneX(int lane)
    {
        return (lane - 1) * laneWidth;
    }

    private void TrySubscribe()
    {
        if (subscribed || InputManager.Instance == null)
        {
            return;
        }

        InputManager.Instance.OnMoveLeft += HandleMoveLeft;
        InputManager.Instance.OnMoveRight += HandleMoveRight;
        subscribed = true;
    }

    private void HandleMoveLeft()
    {
        currentLane = Mathf.Max(0, currentLane - 1);
    }

    private void HandleMoveRight()
    {
        currentLane = Mathf.Min(LaneCount - 1, currentLane + 1);
    }
}
