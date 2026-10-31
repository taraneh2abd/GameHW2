using System;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SparkBurstPool sparkPool;
    [SerializeField] private PickupBurstPool pickupPool;

    [Header("Collision Sparks")]
    [SerializeField] private Vector3 sparkOffset = new Vector3(0f, 0.7f, 1.6f);
    [SerializeField] private Color sparkColor = new Color(1f, 0.75f, 0.3f, 1f);

    [Header("Pickup Burst")]
    [SerializeField] private Vector3 pickupOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Color coinColor = new Color(1f, 0.82f, 0.2f, 1f);
    [SerializeField] private Color fuelColor = new Color(0.25f, 1f, 0.45f, 1f);

    private Action<PooledEffect> releaseSpark;
    private Action<PooledEffect> releasePickup;

    private void Awake()
    {
        if (sparkPool != null)
        {
            releaseSpark = sparkPool.Release;
        }

        if (pickupPool != null)
        {
            releasePickup = pickupPool.Release;
        }
    }

    private void OnEnable()
    {
        ObstacleCar.OnPlayerHit += HandlePlayerHit;
        Coin.OnCollected += HandleCoinCollected;
        FuelCan.OnCollected += HandleFuelCollected;
    }

    private void OnDisable()
    {
        ObstacleCar.OnPlayerHit -= HandlePlayerHit;
        Coin.OnCollected -= HandleCoinCollected;
        FuelCan.OnCollected -= HandleFuelCollected;
    }

    private void HandlePlayerHit(ObstacleCar obstacle)
    {
        if (sparkPool == null || player == null)
        {
            return;
        }

        PooledEffect effect = sparkPool.Get();
        effect.Play(player.position + sparkOffset, sparkColor, releaseSpark);
    }

    private void HandleCoinCollected(Coin coin)
    {
        SpawnPickup(coin.transform.position, coinColor);
    }

    private void HandleFuelCollected(FuelCan fuelCan)
    {
        SpawnPickup(fuelCan.transform.position, fuelColor);
    }

    private void SpawnPickup(Vector3 position, Color tint)
    {
        if (pickupPool == null)
        {
            return;
        }

        PooledEffect effect = pickupPool.Get();
        effect.Play(position + pickupOffset, tint, releasePickup);
    }
}
