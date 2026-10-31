using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip engineClip;
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip fuelClip;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float musicDuckedMultiplier = 0.3f;
    [SerializeField, Range(0f, 1f)] private float engineMinVolume = 0.25f;
    [SerializeField, Range(0f, 1f)] private float engineMaxVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float crashVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float coinVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float fuelVolume = 0.8f;
    [SerializeField] private float fadeSpeed = 2.5f;

    [Header("Engine")]
    [SerializeField] private float minSpeed = 12f;
    [SerializeField] private float maxSpeed = 24f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.6f;
    [SerializeField] private float pitchSmoothing = 4f;

    private GameManager subscribedManager;
    private PlayerCarController player;
    private GameState state = GameState.Playing;
    private float musicLevel;
    private float engineLevel;
    private float playerSearchTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        ConfigureSources();
    }

    private void OnEnable()
    {
        ObstacleCar.OnPlayerHit += HandlePlayerHit;
        Coin.OnCollected += HandleCoin;
        FuelCan.OnCollected += HandleFuel;
        TrySubscribeToGameManager();
    }

    private void Start()
    {
        TrySubscribeToGameManager();
        if (subscribedManager != null)
        {
            state = subscribedManager.CurrentState;
        }

        musicLevel = TargetMusicLevel();
        if (musicSource != null && musicClip != null)
        {
            musicSource.volume = musicLevel * masterVolume;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    private void OnDisable()
    {
        ObstacleCar.OnPlayerHit -= HandlePlayerHit;
        Coin.OnCollected -= HandleCoin;
        FuelCan.OnCollected -= HandleFuel;

        if (subscribedManager != null)
        {
            subscribedManager.OnStateChanged -= HandleStateChanged;
            subscribedManager = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (subscribedManager == null && GameManager.Instance != null)
        {
            TrySubscribeToGameManager();
            state = subscribedManager.CurrentState;
        }

        float dt = Time.unscaledDeltaTime;
        UpdateMusic(dt);
        UpdateEngine(dt);
    }

    private void UpdateMusic(float dt)
    {
        if (musicSource == null)
        {
            return;
        }

        musicLevel = Mathf.MoveTowards(musicLevel, TargetMusicLevel(), fadeSpeed * dt);
        musicSource.volume = musicLevel * masterVolume;
    }

    private void UpdateEngine(float dt)
    {
        if (engineSource == null || engineClip == null)
        {
            return;
        }

        bool active = state == GameState.Playing;
        FindPlayerIfNeeded(dt);

        float speed01 = 0f;
        if (player != null)
        {
            speed01 = Mathf.InverseLerp(minSpeed, maxSpeed, player.CurrentSpeed);
        }

        float targetLevel = active ? Mathf.Lerp(engineMinVolume, engineMaxVolume, speed01) : 0f;
        engineLevel = Mathf.MoveTowards(engineLevel, targetLevel, fadeSpeed * dt);
        engineSource.volume = engineLevel * masterVolume;

        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speed01);
        engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, 1f - Mathf.Exp(-pitchSmoothing * dt));

        if (active && !engineSource.isPlaying)
        {
            engineSource.Play();
        }
        else if (!active && engineLevel <= 0.001f && engineSource.isPlaying)
        {
            engineSource.Stop();
        }
    }

    private void FindPlayerIfNeeded(float dt)
    {
        if (player != null)
        {
            return;
        }

        playerSearchTimer -= dt;
        if (playerSearchTimer > 0f)
        {
            return;
        }

        playerSearchTimer = 0.5f;
        player = FindAnyObjectByType<PlayerCarController>();
    }

    private float TargetMusicLevel()
    {
        bool ducked = state == GameState.Paused || state == GameState.GameOver;
        return musicVolume * (ducked ? musicDuckedMultiplier : 1f);
    }

    private void ConfigureSources()
    {
        if (musicSource != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.ignoreListenerPause = true;
        }

        if (engineSource != null)
        {
            engineSource.clip = engineClip;
            engineSource.loop = true;
            engineSource.playOnAwake = false;
            engineSource.spatialBlend = 0f;
            engineSource.volume = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }
    }

    private void TrySubscribeToGameManager()
    {
        if (subscribedManager != null || GameManager.Instance == null)
        {
            return;
        }

        subscribedManager = GameManager.Instance;
        subscribedManager.OnStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(GameState newState)
    {
        state = newState;
    }

    private void HandlePlayerHit(ObstacleCar obstacle)
    {
        PlaySfx(crashClip, crashVolume);
    }

    private void HandleCoin(Coin coin)
    {
        PlaySfx(coinClip, coinVolume);
    }

    private void HandleFuel(FuelCan fuel)
    {
        PlaySfx(fuelClip, fuelVolume);
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
    }
}
