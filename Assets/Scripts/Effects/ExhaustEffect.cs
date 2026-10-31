using UnityEngine;

public class ExhaustEffect : MonoBehaviour
{
    [SerializeField] private PlayerCarController car;
    [SerializeField] private ParticleSystem exhaust;
    [SerializeField] private float minSpeed = 12f;
    [SerializeField] private float maxSpeed = 24f;
    [SerializeField] private float minRate = 18f;
    [SerializeField] private float maxRate = 45f;

    private void Reset()
    {
        car = GetComponentInParent<PlayerCarController>();
        exhaust = GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        if (car == null)
        {
            car = GetComponentInParent<PlayerCarController>();
        }

        if (exhaust == null)
        {
            exhaust = GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (exhaust == null)
        {
            return;
        }

        bool playing = GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing;

        if (!playing)
        {
            if (exhaust.isEmitting)
            {
                exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            return;
        }

        float t = car != null ? Mathf.InverseLerp(minSpeed, maxSpeed, car.CurrentSpeed) : 0f;
        ParticleSystem.EmissionModule emission = exhaust.emission;
        emission.rateOverTime = Mathf.Lerp(minRate, maxRate, t);

        if (!exhaust.isEmitting)
        {
            exhaust.Play(true);
        }
    }
}
