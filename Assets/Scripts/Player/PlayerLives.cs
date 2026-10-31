using System;
using UnityEngine;

public class PlayerLives : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Color hitColor = new Color(1f, 0.1f, 0.1f, 1f);
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private float flashDuration = 0.35f;
    [SerializeField] private float shakeDuration = 0.35f;
    [SerializeField] private float shakeMagnitude = 0.35f;

    public int MaxLives = 3;
    public int CurrentLives;

    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private float invulnerableUntil;
    private float flashUntil;
    private bool tinted;

    public event Action<int> OnLivesChanged;

    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private void Awake()
    {
        CurrentLives = MaxLives;
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(CurrentLives);
    }

    private void OnEnable()
    {
        ObstacleCar.OnPlayerHit += HandlePlayerHit;
    }

    private void OnDisable()
    {
        ObstacleCar.OnPlayerHit -= HandlePlayerHit;
    }

    private void Update()
    {
        if (tinted && Time.time >= flashUntil)
        {
            ClearTint();
        }
    }

    public void ResetLives()
    {
        CurrentLives = MaxLives;
        invulnerableUntil = 0f;
        ClearTint();
        OnLivesChanged?.Invoke(CurrentLives);
    }

    private void HandlePlayerHit(ObstacleCar obstacle)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (IsInvulnerable || CurrentLives <= 0)
        {
            return;
        }

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        invulnerableUntil = Time.time + invulnerabilityDuration;
        OnLivesChanged?.Invoke(CurrentLives);

        ApplyTint(hitColor);
        flashUntil = Time.time + flashDuration;

        if (cameraFollow != null)
        {
            cameraFollow.Shake(shakeDuration, shakeMagnitude);
        }

        if (CurrentLives <= 0 && GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
    }

    private void ApplyTint(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            r.SetPropertyBlock(block);
        }

        tinted = true;
    }

    private void ClearTint()
    {
        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].SetPropertyBlock(null);
        }

        tinted = false;
    }
}
