using System;
using UnityEngine;

public class FireworkLauncher : MonoBehaviour
{
    [SerializeField] private FireworkPool pool;
    [SerializeField] private Transform player;
    [SerializeField] private Transform viewCamera;

    [Header("Timing")]
    [SerializeField] private float minInterval = 1.5f;
    [SerializeField] private float maxInterval = 4f;

    [Header("Placement")]
    [SerializeField] private float minForwardDistance = 110f;
    [SerializeField] private float maxForwardDistance = 190f;
    [SerializeField] private float maxDistance = 450f;
    [SerializeField] private float minAltitude = 35f;
    [SerializeField] private float maxAltitude = 70f;
    [SerializeField] private float lateralSpread = 0.4f;
    [SerializeField] private float maxElevationDegrees = 8f;
    [SerializeField] private float launchAltitude = 6f;
    [SerializeField] private float referenceDistance = 100f;

    private Action<Firework> release;
    private float timer;

    private void Awake()
    {
        if (pool != null)
        {
            release = pool.Release;
        }

        ScheduleNext();
    }

    private void Update()
    {
        if (pool == null || player == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }

        LaunchOne();
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        timer = UnityEngine.Random.Range(minInterval, maxInterval);
    }

    private void LaunchOne()
    {
        float altitude = UnityEngine.Random.Range(minAltitude, maxAltitude);
        float cameraHeight = viewCamera != null ? viewCamera.position.y : player.position.y + 5f;
        float minDistanceForView = (altitude - cameraHeight) / Mathf.Tan(maxElevationDegrees * Mathf.Deg2Rad);
        float distance = Mathf.Min(maxDistance, Mathf.Max(UnityEngine.Random.Range(minForwardDistance, maxForwardDistance), minDistanceForView));

        float x = player.position.x + UnityEngine.Random.Range(-1f, 1f) * lateralSpread * distance;
        float z = player.position.z + distance;

        Vector3 start = new Vector3(x, launchAltitude, z);
        Vector3 target = new Vector3(x, altitude, z);

        Firework firework = pool.Get();
        firework.Launch(start, target, distance / referenceDistance, UnityEngine.Random.value, release);
    }
}
