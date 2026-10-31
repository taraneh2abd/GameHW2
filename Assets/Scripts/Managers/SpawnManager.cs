using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private static readonly float[] LaneX = { -3f, 0f, 3f };

    [Header("References")]
    [SerializeField] private PlayerCarController player;

    [Header("Pools")]
    [SerializeField] private StaticObstacleCarPool staticObstaclePool;
    [SerializeField] private MovingObstacleCarPool movingObstaclePool;
    [SerializeField] private CoinPool coinPool;
    [SerializeField] private FuelCanPool fuelCanPool;

    [Header("Prefabs")]
    [SerializeField] private StaticObstacleCar staticObstaclePrefab;
    [SerializeField] private MovingObstacleCar movingObstaclePrefab;
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private FuelCan fuelCanPrefab;

    [Header("Pool sizes")]
    [SerializeField] private int staticObstaclePoolSize = 16;
    [SerializeField] private int movingObstaclePoolSize = 8;
    [SerializeField] private int coinPoolSize = 48;
    [SerializeField] private int fuelCanPoolSize = 8;

    [Header("Placement")]
    [SerializeField] private float firstSpawnDistance = 40f;
    [SerializeField] private float spawnAheadDistance = 90f;
    [SerializeField] private float rowSpacing = 14f;

    [Header("Row weights")]
    [SerializeField] private float obstacleWeight = 5f;
    [SerializeField] private float coinWeight = 3f;
    [SerializeField] private float fuelCanWeight = 1f;
    [SerializeField] private float emptyWeight = 1f;

    [Header("Obstacles")]
    [SerializeField, Range(0f, 1f)] private float movingObstacleChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float secondObstacleChance = 0.25f;
    [SerializeField] private float movingApproachSpeed = 6f;
    [SerializeField] private int maxActiveMovingObstacles = 2;

    [Header("Coins")]
    [SerializeField] private int coinsPerStream = 4;
    [SerializeField] private float coinSpacing = 2.5f;

    private readonly Dictionary<Interactable, int> laneOfActive = new Dictionary<Interactable, int>();
    private readonly int[] activeInLane = new int[3];

    private int activeMovingObstacles;
    private float nextRowZ;

    public float RowSpacing
    {
        get => rowSpacing;
        set => rowSpacing = value;
    }

    public float ObstacleWeight
    {
        get => obstacleWeight;
        set => obstacleWeight = value;
    }

    public float CoinWeight
    {
        get => coinWeight;
        set => coinWeight = value;
    }

    public float FuelCanWeight
    {
        get => fuelCanWeight;
        set => fuelCanWeight = value;
    }

    public float EmptyWeight
    {
        get => emptyWeight;
        set => emptyWeight = value;
    }

    public float MovingObstacleChance
    {
        get => movingObstacleChance;
        set => movingObstacleChance = value;
    }

    public float SecondObstacleChance
    {
        get => secondObstacleChance;
        set => secondObstacleChance = value;
    }

    public float MovingApproachSpeed
    {
        get => movingApproachSpeed;
        set => movingApproachSpeed = value;
    }

    private void Start()
    {
        if (staticObstaclePool != null && staticObstaclePrefab != null)
        {
            staticObstaclePool.Initialize(staticObstaclePrefab, staticObstaclePoolSize);
        }

        if (movingObstaclePool != null && movingObstaclePrefab != null)
        {
            movingObstaclePool.Initialize(movingObstaclePrefab, movingObstaclePoolSize);
        }

        if (coinPool != null && coinPrefab != null)
        {
            coinPool.Initialize(coinPrefab, coinPoolSize);
        }

        if (fuelCanPool != null && fuelCanPrefab != null)
        {
            fuelCanPool.Initialize(fuelCanPrefab, fuelCanPoolSize);
        }

        float playerZ = player != null ? player.transform.position.z : 0f;
        nextRowZ = playerZ + firstSpawnDistance;
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

        float limit = player.transform.position.z + spawnAheadDistance;
        while (nextRowZ < limit)
        {
            SpawnRow(nextRowZ);
            nextRowZ += GetEffectiveRowSpacing();
        }
    }

    private float GetEffectiveRowSpacing()
    {
        float streamLength = Mathf.Max(0, coinsPerStream - 1) * coinSpacing;
        return Mathf.Max(rowSpacing, streamLength + 5f);
    }

    private void SpawnRow(float z)
    {
        float total = Mathf.Max(0f, obstacleWeight) + Mathf.Max(0f, coinWeight) + Mathf.Max(0f, fuelCanWeight) + Mathf.Max(0f, emptyWeight);
        if (total <= 0f)
        {
            return;
        }

        float roll = Random.value * total;

        roll -= Mathf.Max(0f, obstacleWeight);
        if (roll < 0f)
        {
            SpawnObstacleRow(z);
            return;
        }

        roll -= Mathf.Max(0f, coinWeight);
        if (roll < 0f)
        {
            SpawnCoinStream(z);
            return;
        }

        roll -= Mathf.Max(0f, fuelCanWeight);
        if (roll < 0f)
        {
            SpawnFuelCan(z);
        }
    }

    private void SpawnObstacleRow(float z)
    {
        int firstLane = Random.Range(0, LaneX.Length);

        bool movingAllowed = movingObstaclePool != null
            && activeMovingObstacles < maxActiveMovingObstacles
            && activeInLane[firstLane] == 0
            && Random.value < movingObstacleChance;

        if (movingAllowed)
        {
            SpawnMovingObstacle(firstLane, z);
            return;
        }

        SpawnStaticObstacle(firstLane, z);

        if (Random.value < secondObstacleChance)
        {
            int secondLane = (firstLane + Random.Range(1, LaneX.Length)) % LaneX.Length;
            SpawnStaticObstacle(secondLane, z);
        }
    }

    private void SpawnCoinStream(float z)
    {
        int lane = Random.Range(0, LaneX.Length);
        for (int i = 0; i < coinsPerStream; i++)
        {
            Coin coin = coinPool.Get();
            Place(coin, coinPrefab.transform, lane, z + i * coinSpacing);
            coin.Bind(player.transform);
            Register(coin, lane);
            coin.OnSpawned();
        }
    }

    private void SpawnFuelCan(float z)
    {
        int lane = Random.Range(0, LaneX.Length);
        FuelCan can = fuelCanPool.Get();
        Place(can, fuelCanPrefab.transform, lane, z);
        can.Bind(player.transform);
        Register(can, lane);
        can.OnSpawned();
    }

    private void SpawnStaticObstacle(int lane, float z)
    {
        StaticObstacleCar car = staticObstaclePool.Get();
        Place(car, staticObstaclePrefab.transform, lane, z, true);
        car.Bind(player.transform);
        Register(car, lane);
        car.OnSpawned();
    }

    private void SpawnMovingObstacle(int lane, float z)
    {
        MovingObstacleCar car = movingObstaclePool.Get();
        car.ApproachSpeed = movingApproachSpeed;
        Place(car, movingObstaclePrefab.transform, lane, z, true);
        car.Bind(player.transform);
        Register(car, lane);
        activeMovingObstacles++;
        car.OnSpawned();
    }

    private static void Place(Component instance, Transform prefabTransform, int lane, float z, bool turnAround = false)
    {
        Quaternion rotation = turnAround
            ? prefabTransform.rotation * Quaternion.Euler(0f, 180f, 0f)
            : prefabTransform.rotation;

        instance.transform.SetPositionAndRotation(
            new Vector3(LaneX[lane], prefabTransform.position.y, z),
            rotation);
    }

    private void Register(Interactable item, int lane)
    {
        laneOfActive[item] = lane;
        activeInLane[lane]++;
        item.Finished += HandleFinished;
    }

    private void HandleFinished(Interactable item)
    {
        item.Finished -= HandleFinished;

        if (laneOfActive.TryGetValue(item, out int lane))
        {
            activeInLane[lane]--;
            laneOfActive.Remove(item);
        }

        switch (item)
        {
            case StaticObstacleCar staticCar:
                staticObstaclePool.Release(staticCar);
                break;
            case MovingObstacleCar movingCar:
                activeMovingObstacles--;
                movingObstaclePool.Release(movingCar);
                break;
            case Coin coin:
                coinPool.Release(coin);
                break;
            case FuelCan can:
                fuelCanPool.Release(can);
                break;
        }
    }
}
