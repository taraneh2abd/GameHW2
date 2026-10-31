using UnityEngine;

public class MovingObstacleCar : ObstacleCar
{
    [SerializeField] private float approachSpeed = 6f;

    public float ApproachSpeed
    {
        get => approachSpeed;
        set => approachSpeed = value;
    }

    protected override void Tick()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
        {
            transform.position += Vector3.forward * (approachSpeed * Time.deltaTime);
        }

        base.Tick();
    }
}
