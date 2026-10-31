using System;

public class FuelCan : Collectible
{
    public static event Action<FuelCan> OnCollected;

    protected override void OnInteract(PlayerCarController playerController)
    {
        OnCollected?.Invoke(this);
        Finish();
    }
}
