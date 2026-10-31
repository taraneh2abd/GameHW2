using System;
using UnityEngine;

[Serializable]
public class CarVariant
{
    public GameObject root;
    public Vector3 colliderCenter;
    public Vector3 colliderSize;

    [NonSerialized] public Renderer[] paintedRenderers;
}
