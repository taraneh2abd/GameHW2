using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private RoadSegmentPool segmentPool;
    [SerializeField] private RoadSegment segmentPrefab;
    [SerializeField] private int spareSegments = 2;
    [SerializeField] private float lookaheadDistance = 150f;
    [SerializeField] private float recycleThreshold = 20f;

    [Header("Placeholder driving (until feat/car-control wires in the real player)")]
    [SerializeField] private bool usePlaceholderDriving = true;
    [SerializeField] private float placeholderSpeed = 8f;

    private readonly List<RoadSegment> activeSegments = new List<RoadSegment>();
    private float currentDistance;
    private float nextSpawnZ;

    private void Start()
    {
        if (segmentPool == null || segmentPrefab == null)
        {
            return;
        }

        float segmentLength = segmentPrefab.GetLength();
        int needed = Mathf.CeilToInt((lookaheadDistance + recycleThreshold) / segmentLength) + 1 + spareSegments;
        if (segmentPool.TotalCount < needed)
        {
            segmentPool.Initialize(segmentPrefab, needed - segmentPool.TotalCount);
        }

        while (nextSpawnZ - currentDistance < lookaheadDistance)
        {
            SpawnNextSegment();
        }
    }

    private void Update()
    {
        // Placeholder: advances currentDistance on its own so spawning/recycling
        // can be verified before a player car exists. feat/car-control should set
        // usePlaceholderDriving to false and call AdvanceAndMaybeSpawn directly
        // with the player's actual forward distance each frame instead.
        if (usePlaceholderDriving)
        {
            currentDistance += placeholderSpeed * Time.deltaTime;
            Tick(currentDistance);
        }
    }

    public void AdvanceAndMaybeSpawn(float distance)
    {
        currentDistance = distance;
        Tick(currentDistance);
    }

    private void Tick(float distance)
    {
        if (segmentPool == null)
        {
            return;
        }

        while (activeSegments.Count > 0 && GetSegmentFrontEdge(activeSegments[0]) < distance - recycleThreshold)
        {
            RecycleRearSegment();
        }

        while (nextSpawnZ - distance < lookaheadDistance)
        {
            SpawnNextSegment();
        }
    }

    private float GetSegmentFrontEdge(RoadSegment segment)
    {
        return segment.transform.position.z + segment.GetLength() * 0.5f;
    }

    private void SpawnNextSegment()
    {
        RoadSegment segment = segmentPool.Get();
        float centerZ = nextSpawnZ + segment.GetLength() * 0.5f;
        segment.transform.position = new Vector3(0f, 0f, centerZ);
        nextSpawnZ += segment.GetLength();
        activeSegments.Add(segment);
    }

    private void RecycleRearSegment()
    {
        if (segmentPool == null || activeSegments.Count == 0)
        {
            return;
        }

        RoadSegment rear = activeSegments[0];
        activeSegments.RemoveAt(0);
        segmentPool.Release(rear);
    }
}
