using System;
using UnityEngine;

public class Firework : MonoBehaviour
{
    private enum Phase
    {
        Idle,
        Rising,
        Bursting
    }

    [SerializeField] private ParticleSystem rocket;
    [SerializeField] private ParticleSystem burst;
    [SerializeField] private float riseDuration = 1.3f;
    [SerializeField] private float hueShift = 0.12f;

    private Phase phase;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsed;
    private Action<Firework> onFinished;

    public void Launch(Vector3 start, Vector3 target, float scale, float hue, Action<Firework> finishedCallback)
    {
        onFinished = finishedCallback;
        startPosition = start;
        targetPosition = target;
        elapsed = 0f;
        phase = Phase.Rising;

        transform.localScale = Vector3.one * scale;
        transform.position = start;

        Color primary = Color.HSVToRGB(Mathf.Repeat(hue, 1f), 0.85f, 1f);
        Color secondary = Color.HSVToRGB(Mathf.Repeat(hue + hueShift, 1f), 0.75f, 1f);
        Color trail = Color.Lerp(primary, Color.white, 0.55f);

        ParticleSystem.MainModule rocketMain = rocket.main;
        rocketMain.startColor = trail;

        ParticleSystem.MainModule burstMain = burst.main;
        burstMain.startColor = new ParticleSystem.MinMaxGradient(primary, secondary);

        rocket.Clear(true);
        burst.Clear(true);
        rocket.Play(true);
    }

    private void Update()
    {
        if (phase == Phase.Rising)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / riseDuration);
            float eased = 1f - (1f - k) * (1f - k);
            transform.position = Vector3.Lerp(startPosition, targetPosition, eased);

            if (k >= 1f)
            {
                phase = Phase.Bursting;
                rocket.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                burst.Play(true);
            }
        }
        else if (phase == Phase.Bursting)
        {
            if (!rocket.IsAlive(true) && !burst.IsAlive(true))
            {
                phase = Phase.Idle;
                Action<Firework> callback = onFinished;
                onFinished = null;
                callback?.Invoke(this);
            }
        }
    }
}
