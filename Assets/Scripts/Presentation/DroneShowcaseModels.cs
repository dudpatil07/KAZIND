using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Decorative models only; no colliders, AI, payloads or gameplay scripts.</summary>
public static class DroneShowcaseModels
{
    public static GameObject Build(int index, Transform parent, Material shell, Material dark, Material accent)
    {
        GameObject root = new GameObject("IllustrativeAirframe");
        root.transform.SetParent(parent, false);
        if (index == 1)
        {
            Part(root.transform, "Fuselage", PrimitiveType.Capsule, Vector3.zero,
                new Vector3(0.38f, 1.25f, 0.38f), new Vector3(90, 0, 0), shell);
            Part(root.transform, "MainWing", PrimitiveType.Cube, new Vector3(0, 0, 0.1f),
                new Vector3(3.7f, 0.07f, 0.65f), Vector3.zero, shell);
            Part(root.transform, "Tailplane", PrimitiveType.Cube, new Vector3(0, 0.08f, -1),
                new Vector3(1.25f, 0.05f, 0.32f), Vector3.zero, dark);
            Part(root.transform, "Fin", PrimitiveType.Cube, new Vector3(0, 0.26f, -1),
                new Vector3(0.06f, 0.48f, 0.35f), Vector3.zero, shell);
            Part(root.transform, "Canopy", PrimitiveType.Sphere, new Vector3(0, 0.17f, 0.5f),
                new Vector3(0.3f, 0.14f, 0.6f), Vector3.zero, dark);
        }
        else if (index == 2)
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject member = new GameObject("FormationMember");
                member.transform.SetParent(root.transform, false);
                member.transform.localPosition = new Vector3((i - 2) * 0.85f, 0, -Mathf.Abs(i - 2) * 0.65f);
                member.transform.localScale = Vector3.one * 0.38f;
                Quad(member.transform, shell, dark, accent);
            }
        }
        else Quad(root.transform, shell, dark, accent);
        return root;
    }

    private static void Quad(Transform parent, Material shell, Material dark, Material accent)
    {
        Part(parent, "Shell", PrimitiveType.Capsule, Vector3.zero, new Vector3(0.6f, 0.38f, 0.8f), new Vector3(90, 0, 0), shell);
        Part(parent, "CameraHousing", PrimitiveType.Sphere, new Vector3(0, -0.23f, 0.35f), Vector3.one * 0.22f, Vector3.zero, dark);
        Part(parent, "Lens", PrimitiveType.Sphere, new Vector3(0, -0.23f, 0.45f), new Vector3(0.12f, 0.12f, 0.06f), Vector3.zero, accent);
        for (int x = -1; x <= 1; x += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            Vector3 end = new Vector3(x * 0.7f, 0, z * 0.65f);
            Part(parent, "Arm", PrimitiveType.Cube, end * 0.5f,
                new Vector3(0.12f, 0.1f, end.magnitude), new Vector3(0, Mathf.Atan2(end.x, end.z) * Mathf.Rad2Deg, 0), dark);
            Part(parent, "Motor", PrimitiveType.Cylinder, end, new Vector3(0.19f, 0.1f, 0.19f), Vector3.zero, shell);
            Part(parent, "Propeller", PrimitiveType.Cube, end + Vector3.up * 0.12f,
                new Vector3(0.85f, 0.018f, 0.075f), new Vector3(0, x * z * 30, 0), dark);
            Part(parent, "LandingLeg", PrimitiveType.Cube, end * 0.65f + Vector3.down * 0.22f,
                new Vector3(0.055f, 0.4f, 0.055f), new Vector3(0, 0, x * 12), dark);
            Part(parent, "Marker", PrimitiveType.Sphere, end + Vector3.forward * z * 0.1f,
                Vector3.one * 0.08f, Vector3.zero, accent);
        }
    }

    private static void Part(Transform parent, string name, PrimitiveType type, Vector3 position,
        Vector3 scale, Vector3 angles, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.layer = 31;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.transform.localRotation = Quaternion.Euler(angles);
        Collider collider = part.GetComponent<Collider>();
        collider.enabled = false;
        Object.Destroy(collider);
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }
}
