using System;

public class Coin : Collectible
{
    public static event Action<Coin> OnCollected;

    protected override void OnInteract(PlayerCarController playerController)
    {
        OnCollected?.Invoke(this);
        Finish();
    }
}
