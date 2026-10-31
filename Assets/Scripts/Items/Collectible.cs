using UnityEngine;

public abstract class Collectible : Interactable
{
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float despawnBehindDistance = 15f;

    protected Transform player;

    public void Bind(Transform playerTransform)
    {
        player = playerTransform;
    }

    protected override void Tick()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        if (player != null && transform.position.z < player.position.z - despawnBehindDistance)
        {
            Finish();
        }
    }
}
