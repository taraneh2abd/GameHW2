using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action OnMoveLeft;
    public event Action OnMoveRight;
    public event Action OnPause;

    private GameControls controls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        controls = new GameControls();

        controls.Gameplay.MoveLeft.performed += ctx => OnMoveLeft?.Invoke();
        controls.Gameplay.MoveRight.performed += ctx => OnMoveRight?.Invoke();
        controls.Gameplay.Pause.performed += ctx => OnPause?.Invoke();
    }

    private void OnEnable()
    {
        controls?.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls?.Gameplay.Disable();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }
}
