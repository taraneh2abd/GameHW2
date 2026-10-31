using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PooledEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    private Action<PooledEffect> onFinished;

    private void Reset()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        if (particles == null)
        {
            particles = GetComponent<ParticleSystem>();
        }
    }

    public void Play(Vector3 position, Color tint, Action<PooledEffect> finishedCallback)
    {
        onFinished = finishedCallback;
        transform.position = position;

        ParticleSystem.MainModule main = particles.main;
        main.startColor = tint;

        particles.Clear(true);
        particles.Play(true);
    }

    private void OnParticleSystemStopped()
    {
        Action<PooledEffect> callback = onFinished;
        onFinished = null;
        callback?.Invoke(this);
    }
}
