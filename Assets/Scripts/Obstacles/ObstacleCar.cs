using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class ObstacleCar : Interactable
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public static event Action<ObstacleCar> OnPlayerHit;

    [SerializeField] private float despawnBehindDistance = 15f;

    [Header("Look")]
    [SerializeField] private CarVariant[] variants;
    [SerializeField] private Color[] palette;
    [SerializeField] private Material bodyMaterial;

    protected Transform player;

    private BoxCollider hitBox;
    private MaterialPropertyBlock paint;
    private bool cached;

    public void Bind(Transform playerTransform)
    {
        player = playerTransform;
    }

    public override void OnSpawned()
    {
        base.OnSpawned();
        Cache();

        if (variants == null || variants.Length == 0)
        {
            return;
        }

        int chosen = Random.Range(0, variants.Length);
        for (int i = 0; i < variants.Length; i++)
        {
            bool active = i == chosen;
            if (variants[i].root.activeSelf != active)
            {
                variants[i].root.SetActive(active);
            }
        }

        CarVariant variant = variants[chosen];
        if (hitBox != null)
        {
            hitBox.center = variant.colliderCenter;
            hitBox.size = variant.colliderSize;
        }

        if (palette != null && palette.Length > 0)
        {
            Color color = palette[Random.Range(0, palette.Length)];
            paint.SetColor(BaseColorId, color);
            paint.SetColor(ColorId, color);
            Renderer[] painted = variant.paintedRenderers;
            for (int i = 0; i < painted.Length; i++)
            {
                painted[i].SetPropertyBlock(paint);
            }
        }
    }

    protected override void OnInteract(PlayerCarController playerController)
    {
        OnPlayerHit?.Invoke(this);
        Finish();
    }

    protected override void Tick()
    {
        if (player != null && transform.position.z < player.position.z - despawnBehindDistance)
        {
            Finish();
        }
    }

    private void Awake()
    {
        Cache();
    }

    private void Cache()
    {
        if (cached)
        {
            return;
        }

        cached = true;
        hitBox = GetComponent<BoxCollider>();
        paint = new MaterialPropertyBlock();

        if (variants == null)
        {
            return;
        }

        List<Renderer> buffer = new List<Renderer>();
        for (int i = 0; i < variants.Length; i++)
        {
            buffer.Clear();
            foreach (Renderer candidate in variants[i].root.GetComponentsInChildren<Renderer>(true))
            {
                if (candidate.sharedMaterial == bodyMaterial)
                {
                    buffer.Add(candidate);
                }
            }

            variants[i].paintedRenderers = buffer.ToArray();
        }
    }
}
