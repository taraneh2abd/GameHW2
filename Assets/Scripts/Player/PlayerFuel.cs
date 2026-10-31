using System;
using UnityEngine;

public class PlayerFuel : MonoBehaviour
{
    [SerializeField] private float drainPerSecond = 4f;
    [SerializeField] private float refillPerCan = 35f;

    public float MaxFuel = 100f;
    public float CurrentFuel;

    public float NormalizedFuel => MaxFuel > 0f ? Mathf.Clamp01(CurrentFuel / MaxFuel) : 0f;

    public event Action<float> OnFuelChanged;

    private void Awake()
    {
        CurrentFuel = MaxFuel;
    }

    private void Start()
    {
        OnFuelChanged?.Invoke(NormalizedFuel);
    }

    private void OnEnable()
    {
        FuelCan.OnCollected += HandleFuelCollected;
    }

    private void OnDisable()
    {
        FuelCan.OnCollected -= HandleFuelCollected;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (CurrentFuel <= 0f)
        {
            return;
        }

        CurrentFuel = Mathf.Max(0f, CurrentFuel - drainPerSecond * Time.deltaTime);
        OnFuelChanged?.Invoke(NormalizedFuel);

        if (CurrentFuel <= 0f && GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
    }

    public void ResetFuel()
    {
        CurrentFuel = MaxFuel;
        OnFuelChanged?.Invoke(NormalizedFuel);
    }

    private void HandleFuelCollected(FuelCan can)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        CurrentFuel = Mathf.Min(MaxFuel, CurrentFuel + refillPerCan);
        OnFuelChanged?.Invoke(NormalizedFuel);
    }
}
