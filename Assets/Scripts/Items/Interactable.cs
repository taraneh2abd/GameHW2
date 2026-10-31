using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    private bool finished;

    public event Action<Interactable> Finished;

    public bool IsFinished => finished;

    public virtual void OnSpawned()
    {
        finished = false;
    }

    protected abstract void OnInteract(PlayerCarController player);

    protected virtual void Tick()
    {
    }

    protected void Finish()
    {
        if (finished)
        {
            return;
        }

        finished = true;
        Finished?.Invoke(this);
    }

    private void Update()
    {
        if (finished)
        {
            return;
        }

        Tick();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished)
        {
            return;
        }

        PlayerCarController player = other.GetComponentInParent<PlayerCarController>();
        if (player == null)
        {
            return;
        }

        OnInteract(player);
    }
}
