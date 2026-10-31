using UnityEngine;

public class NeonUnderglow : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private PlayerCarController car;
    [SerializeField] private Light[] glowLights;
    [SerializeField] private Renderer[] glowRenderers;

    [Header("Speed Range")]
    [SerializeField] private float minSpeed = 12f;
    [SerializeField] private float maxSpeed = 24f;

    [Header("Light")]
    [SerializeField] private float minLightIntensity = 4f;
    [SerializeField] private float maxLightIntensity = 12f;
    [SerializeField] private float minLightRange = 4f;
    [SerializeField] private float maxLightRange = 7f;

    [Header("Emission (HDR)")]
    [SerializeField] private float minEmission = 1.5f;
    [SerializeField] private float maxEmission = 4f;

    [Header("Color")]
    [SerializeField] private Gradient colorBySpeed;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.25f;

    private MaterialPropertyBlock block;
    private float smoothedT;
    private float velocity;

    private void Reset()
    {
        car = GetComponentInParent<PlayerCarController>();
    }

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        if (car == null)
        {
            car = GetComponentInParent<PlayerCarController>();
        }

        if (colorBySpeed == null || colorBySpeed.colorKeys.Length == 0)
        {
            colorBySpeed = BuildDefaultGradient();
        }
    }

    private void Start()
    {
        smoothedT = GetTargetT();
        Apply(smoothedT);
    }

    private void Update()
    {
        float target = GetTargetT();
        smoothedT = Mathf.SmoothDamp(smoothedT, target, ref velocity, smoothTime);
        Apply(smoothedT);
    }

    private float GetTargetT()
    {
        if (car == null)
        {
            return 0f;
        }

        return Mathf.InverseLerp(minSpeed, maxSpeed, car.CurrentSpeed);
    }

    private void Apply(float t)
    {
        Color color = colorBySpeed.Evaluate(t);
        float lightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
        float lightRange = Mathf.Lerp(minLightRange, maxLightRange, t);
        float emission = Mathf.Lerp(minEmission, maxEmission, t);

        for (int i = 0; i < glowLights.Length; i++)
        {
            Light glowLight = glowLights[i];
            if (glowLight == null)
            {
                continue;
            }

            glowLight.color = color;
            glowLight.intensity = lightIntensity;
            glowLight.range = lightRange;
        }

        Color hdr = color * Mathf.Pow(2f, emission);
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            Renderer glowRenderer = glowRenderers[i];
            if (glowRenderer == null)
            {
                continue;
            }

            glowRenderer.GetPropertyBlock(block);
            block.SetColor(EmissionColorId, hdr);
            block.SetColor(BaseColorId, color * 0.2f);
            glowRenderer.SetPropertyBlock(block);
        }
    }

    private static Gradient BuildDefaultGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.1f, 0.6f, 1f), 0f),
                new GradientColorKey(new Color(0.1f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.2f, 0.9f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return gradient;
    }
}
