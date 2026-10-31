using System;
using UnityEngine;

public abstract class ObstacleCar : Interactable
{
    public static event Action<ObstacleCar> OnPlayerHit;

    [SerializeField] private float despawnBehindDistance = 15f;

    protected Transform player;

    public void Bind(Transform playerTransform)
    {
        player = playerTransform;
    }

    protected override void OnInteract(PlayerCarController playerController)
    {
        OnPlayerHit?.Invoke(this);
        Finish();
    }

    protected override void Tick()
    {
        if (player != null && transform.position.z < player.position.z - despawnBehindDistance)
        {
            Finish();
        }
    }
}
