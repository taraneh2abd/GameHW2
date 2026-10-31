using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CarModelBuilder
{
    private const string MaterialDir = "Assets/Materials/";
    private const string StaticPrefab = "Assets/Prefabs/StaticObstacleCar.prefab";
    private const string MovingPrefab = "Assets/Prefabs/MovingObstacleCar.prefab";
    private const float GroundOffset = 0.1f;

    private static readonly Color Red = new Color(0.85f, 0.1f, 0.1f);
    private static readonly Color Blue = new Color(0.1f, 0.3f, 0.9f);
    private static readonly Color Yellow = new Color(1f, 0.8f, 0.05f);
    private static readonly Color Orange = new Color(1f, 0.45f, 0.05f);
    private static readonly Color White = new Color(0.92f, 0.92f, 0.92f);
    private static readonly Color Silver = new Color(0.65f, 0.67f, 0.7f);
    private static readonly Color Green = new Color(0.1f, 0.65f, 0.2f);
    private static readonly Color Purple = new Color(0.5f, 0.15f, 0.75f);

    private class Spec
    {
        public string name;
        public float w, l, r, clear, bodyH, cabW, cabH, cabL, cabZ, overhang;
        public bool hood, trunk, frontRake, rearRake, bed, mirrors = true;
        public float cargoW, cargoH, cargoL, cargoZ;
    }

    private class Mats
    {
        public Material body, glass, tyre, hub, head, tail;
    }

    private static readonly Spec Sedan = new Spec
    {
        name = "Sedan", w = 1.85f, l = 4.3f, r = 0.32f, clear = 0.27f, bodyH = 0.55f,
        cabW = 1.58f, cabH = 0.5f, cabL = 2.0f, cabZ = 0.3f, overhang = 0.85f,
        hood = true, trunk = true, frontRake = true, rearRake = true
    };

    private static readonly Spec Hatchback = new Spec
    {
        name = "Hatchback", w = 1.75f, l = 3.7f, r = 0.3f, clear = 0.27f, bodyH = 0.58f,
        cabW = 1.55f, cabH = 0.52f, cabL = 1.9f, cabZ = 0.35f, overhang = 0.7f,
        hood = true, frontRake = true, rearRake = true
    };

    private static readonly Spec Suv = new Spec
    {
        name = "Suv", w = 1.95f, l = 4.5f, r = 0.4f, clear = 0.4f, bodyH = 0.65f,
        cabW = 1.8f, cabH = 0.62f, cabL = 2.8f, cabZ = 0.45f, overhang = 0.9f,
        hood = true, frontRake = true
    };

    private static readonly Spec Sports = new Spec
    {
        name = "Sports", w = 1.95f, l = 4.2f, r = 0.31f, clear = 0.18f, bodyH = 0.45f,
        cabW = 1.45f, cabH = 0.38f, cabL = 1.5f, cabZ = 0.35f, overhang = 0.8f,
        hood = true, trunk = true, frontRake = true, rearRake = true
    };

    private static readonly Spec Pickup = new Spec
    {
        name = "Pickup", w = 2.0f, l = 5.0f, r = 0.38f, clear = 0.38f, bodyH = 0.6f,
        cabW = 1.85f, cabH = 0.6f, cabL = 1.5f, cabZ = -0.55f, overhang = 0.95f,
        hood = true, frontRake = true, bed = true, mirrors = false
    };

    private static readonly Spec Van = new Spec
    {
        name = "Van", w = 2.0f, l = 4.8f, r = 0.36f, clear = 0.32f, bodyH = 1.0f,
        cabW = 1.92f, cabH = 0.72f, cabL = 4.0f, cabZ = 0.35f, overhang = 0.85f,
        frontRake = true
    };

    private static readonly Spec BoxTruck = new Spec
    {
        name = "BoxTruck", w = 2.25f, l = 5.6f, r = 0.42f, clear = 0.4f, bodyH = 0.5f,
        cabW = 2.1f, cabH = 1.25f, cabL = 1.5f, cabZ = -1.9f, overhang = 0.9f,
        cargoW = 2.3f, cargoH = 1.8f, cargoL = 3.8f, cargoZ = 0.85f
    };

    [MenuItem("Tools/Build Obstacle Cars")]
    public static void Build()
    {
        Mats mats = LoadOrCreateMaterials();
        BuildPrefab(StaticPrefab, mats, new[] { Sedan, Suv, Pickup, Van, BoxTruck, Hatchback },
            new[] { Red, Blue, White, Silver, Green, Purple });
        BuildPrefab(MovingPrefab, mats, new[] { Sports, Hatchback, Sedan, Suv },
            new[] { Red, Yellow, Orange, Blue, White, Green });
        AssetDatabase.SaveAssets();
    }

    private static void BuildPrefab(string path, Mats mats, Spec[] specs, Color[] palette)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }

            MeshRenderer rootRenderer = root.GetComponent<MeshRenderer>();
            if (rootRenderer != null)
            {
                Object.DestroyImmediate(rootRenderer);
            }

            MeshFilter rootFilter = root.GetComponent<MeshFilter>();
            if (rootFilter != null)
            {
                Object.DestroyImmediate(rootFilter);
            }

            root.transform.localPosition = new Vector3(0f, GroundOffset, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            BoxCollider box = root.GetComponent<BoxCollider>();
            ObstacleCar car = root.GetComponent<ObstacleCar>();
            SerializedObject so = new SerializedObject(car);
            SerializedProperty list = so.FindProperty("variants");
            list.arraySize = specs.Length;
            for (int i = 0; i < specs.Length; i++)
            {
                GameObject variant = BuildVariant(root.transform, specs[i], mats);
                Bounds bounds = MeasureLocal(root.transform, variant);
                if (i == 0)
                {
                    box.center = bounds.center;
                    box.size = bounds.size;
                    box.isTrigger = true;
                }

                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("root").objectReferenceValue = variant;
                entry.FindPropertyRelative("colliderCenter").vector3Value = bounds.center;
                entry.FindPropertyRelative("colliderSize").vector3Value = bounds.size;
                variant.SetActive(i == 0);
            }

            SerializedProperty paletteProp = so.FindProperty("palette");
            paletteProp.arraySize = palette.Length;
            for (int i = 0; i < palette.Length; i++)
            {
                paletteProp.GetArrayElementAtIndex(i).colorValue = palette[i];
            }

            so.FindProperty("bodyMaterial").objectReferenceValue = mats.body;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Bounds MeasureLocal(Transform root, GameObject variant)
    {
        bool first = true;
        Bounds result = new Bounds();
        foreach (MeshFilter filter in variant.GetComponentsInChildren<MeshFilter>())
        {
            Matrix4x4 toRoot = root.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            Bounds mesh = filter.sharedMesh.bounds;
            for (int c = 0; c < 8; c++)
            {
                Vector3 corner = mesh.center + Vector3.Scale(mesh.extents, new Vector3(
                    (c & 1) == 0 ? -1f : 1f,
                    (c & 2) == 0 ? -1f : 1f,
                    (c & 4) == 0 ? -1f : 1f));
                Vector3 p = toRoot.MultiplyPoint3x4(corner);
                if (first)
                {
                    result = new Bounds(p, Vector3.zero);
                    first = false;
                }
                else
                {
                    result.Encapsulate(p);
                }
            }
        }

        return result;
    }

    private static GameObject BuildVariant(Transform root, Spec s, Mats m)
    {
        GameObject variant = new GameObject("Variant_" + s.name);
        variant.transform.SetParent(root, false);
        Transform v = variant.transform;

        float top = s.clear + s.bodyH;
        float cabY = top + s.cabH * 0.5f;
        float z0 = s.cabZ - s.cabL * 0.5f;
        float z1 = s.cabZ + s.cabL * 0.5f;
        float rakeF = s.frontRake ? s.cabH * 0.85f : 0f;
        float rakeR = s.rearRake ? s.cabH * 0.85f : 0f;
        float halfL = s.l * 0.5f;

        Part(v, "Body", m.body, new Vector3(0f, s.clear + s.bodyH * 0.5f, 0f), new Vector3(s.w, s.bodyH, s.l), true);
        Part(v, "Cabin", m.body, new Vector3(0f, cabY, s.cabZ), new Vector3(s.cabW, s.cabH, s.cabL), true);

        if (s.hood)
        {
            float start = -halfL + 0.04f;
            float end = z0 - rakeF + 0.02f;
            Part(v, "Hood", m.body, new Vector3(0f, top + 0.04f, (start + end) * 0.5f),
                new Vector3(s.w * 0.94f, 0.08f, end - start), false);
        }

        if (s.trunk)
        {
            float start = z1 + rakeR - 0.02f;
            float end = halfL - 0.04f;
            Part(v, "Trunk", m.body, new Vector3(0f, top + 0.04f, (start + end) * 0.5f),
                new Vector3(s.w * 0.94f, 0.08f, end - start), false);
        }

        if (s.bed)
        {
            float bedStart = z1;
            float bedEnd = halfL - 0.04f;
            float bedLen = bedEnd - bedStart;
            float wallH = 0.35f;
            for (int side = -1; side <= 1; side += 2)
            {
                Part(v, "BedWall", m.body, new Vector3(side * (s.w * 0.5f - 0.04f), top + wallH * 0.5f, bedStart + bedLen * 0.5f),
                    new Vector3(0.08f, wallH, bedLen), true);
            }

            Part(v, "Tailgate", m.body, new Vector3(0f, top + wallH * 0.5f, bedEnd - 0.04f),
                new Vector3(s.w, wallH, 0.08f), true);
        }

        if (s.cargoL > 0f)
        {
            Part(v, "Cargo", m.body, new Vector3(0f, top + s.cargoH * 0.5f, s.cargoZ),
                new Vector3(s.cargoW, s.cargoH, s.cargoL), true);
        }

        if (s.frontRake)
        {
            float a = rakeF * Mathf.Sqrt(2f);
            Part(v, "Windshield", m.glass, new Vector3(0f, top, z0), new Vector3(s.cabW * 0.92f, a, a), false, 45f);
        }
        else
        {
            Part(v, "Windshield", m.glass, new Vector3(0f, cabY, z0), new Vector3(s.cabW * 0.88f, s.cabH * 0.7f, 0.03f), false);
        }

        if (s.cargoL <= 0f)
        {
            if (s.rearRake)
            {
                float a = rakeR * Mathf.Sqrt(2f);
                Part(v, "RearWindow", m.glass, new Vector3(0f, top, z1), new Vector3(s.cabW * 0.92f, a, a), false, 45f);
            }
            else
            {
                Part(v, "RearWindow", m.glass, new Vector3(0f, cabY, z1), new Vector3(s.cabW * 0.88f, s.cabH * 0.7f, 0.03f), false);
            }
        }

        float wheelW = s.r > 0.37f ? 0.3f : 0.25f;
        float wheelX = s.w * 0.5f - wheelW * 0.5f + 0.02f;
        float axle = halfL - s.overhang;
        for (int side = -1; side <= 1; side += 2)
        {
            Part(v, "SideGlass", m.glass, new Vector3(side * s.cabW * 0.5f, cabY, s.cabZ),
                new Vector3(0.03f, s.cabH * 0.65f, s.cabL * 0.8f), false);

            for (int end = -1; end <= 1; end += 2)
            {
                Vector3 center = new Vector3(side * wheelX, s.r, end * axle);
                Cylinder(v, "Tyre", m.tyre, center, new Vector3(s.r * 2f, wheelW * 0.5f, s.r * 2f), true);
                Cylinder(v, "Hubcap", m.hub, center + new Vector3(side * wheelW * 0.5f, 0f, 0f),
                    new Vector3(s.r * 1.1f, 0.02f, s.r * 1.1f), false);
            }

            Part(v, "Headlight", m.head, new Vector3(side * (s.w * 0.5f - 0.3f), s.clear + s.bodyH * 0.65f, -halfL - 0.015f),
                new Vector3(0.34f, 0.13f, 0.04f), false);
            Part(v, "Taillight", m.tail, new Vector3(side * (s.w * 0.5f - 0.3f), s.clear + s.bodyH * 0.65f, halfL + 0.015f),
                new Vector3(0.34f, 0.13f, 0.04f), false);

            if (s.mirrors)
            {
                Part(v, "Mirror", m.tyre, new Vector3(side * (s.cabW * 0.5f + 0.08f), top + s.cabH * 0.5f, z0 + 0.2f),
                    new Vector3(0.12f, 0.1f, 0.14f), false);
            }
        }

        Part(v, "FrontBumper", m.tyre, new Vector3(0f, s.clear + 0.1f, -halfL - 0.02f), new Vector3(s.w - 0.12f, 0.2f, 0.12f), false);
        Part(v, "RearBumper", m.tyre, new Vector3(0f, s.clear + 0.1f, halfL + 0.02f), new Vector3(s.w - 0.12f, 0.2f, 0.12f), false);

        return variant;
    }

    private static GameObject Part(Transform parent, string name, Material material, Vector3 pos, Vector3 size, bool shadow, float rotX = 0f)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        return Finish(g, parent, name, material, pos, size, Quaternion.Euler(rotX, 0f, 0f), shadow);
    }

    private static GameObject Cylinder(Transform parent, string name, Material material, Vector3 pos, Vector3 size, bool shadow)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        return Finish(g, parent, name, material, pos, size, Quaternion.Euler(0f, 0f, 90f), shadow);
    }

    private static GameObject Finish(GameObject g, Transform parent, string name, Material material, Vector3 pos, Vector3 size, Quaternion rot, bool shadow)
    {
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localRotation = rot;
        g.transform.localScale = size;
        MeshRenderer r = g.GetComponent<MeshRenderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = shadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
        return g;
    }

    private static Mats LoadOrCreateMaterials()
    {
        return new Mats
        {
            body = MakeMaterial("CarBody", Color.white, 0.35f, 0.65f, Color.black),
            glass = MakeMaterial("CarGlass", new Color(0.03f, 0.04f, 0.06f), 0.6f, 0.92f, Color.black),
            tyre = MakeMaterial("CarTyre", new Color(0.04f, 0.04f, 0.045f), 0f, 0.15f, Color.black),
            hub = MakeMaterial("CarHubcap", new Color(0.62f, 0.64f, 0.68f), 0.85f, 0.6f, Color.black),
            head = MakeMaterial("CarHeadlight", new Color(1f, 0.95f, 0.8f), 0f, 0.8f, new Color(2.2f, 1.98f, 1.43f)),
            tail = MakeMaterial("CarTaillight", new Color(0.3f, 0f, 0f), 0f, 0.8f, new Color(2f, 0.1f, 0.1f))
        };
    }

    private static Material MakeMaterial(string name, Color color, float metallic, float smoothness, Color emission)
    {
        string path = MaterialDir + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null)
        {
            return m;
        }

        m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Smoothness", smoothness);
        if (emission.maxColorComponent > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        }

        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
