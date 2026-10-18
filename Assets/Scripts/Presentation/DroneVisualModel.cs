using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Cosmetic replacement for the legacy block models; leaves gameplay colliders intact.</summary>
[DisallowMultipleComponent]
public sealed class DroneVisualModel : MonoBehaviour
{
    private GameObject model;
    private TrainingDrone drone;
    private Renderer[] legacyRenderers;
    private bool[] legacyVisibility;
    private Light[] legacyLights;
    private bool[] legacyLightEnabled;
    private readonly List<Transform> propellers = new List<Transform>();
    private readonly List<Material> materials = new List<Material>();
    private readonly List<Mesh> meshes = new List<Mesh>();
    private readonly List<MeshFilter> staticParts = new List<MeshFilter>();
    public bool IsBuilt => model != null;

    public void Initialize(TrainingDrone.DroneVariant variant)
    {
        if (model != null) return;
        Material template = Resources.Load<Material>("Presentation/AirframeSurface");
        if (template == null)
        {
            Debug.LogError("AirframeSurface material is missing; keeping the original drone model.", this);
            return;
        }
        drone = GetComponent<TrainingDrone>();
        legacyRenderers = GetComponentsInChildren<Renderer>(true);
        legacyVisibility = new bool[legacyRenderers.Length];
        for (int i = 0; i < legacyRenderers.Length; i++) legacyVisibility[i] = legacyRenderers[i].enabled;
        legacyLights = GetComponentsInChildren<Light>(true);
        legacyLightEnabled = new bool[legacyLights.Length];
        for (int i = 0; i < legacyLights.Length; i++) legacyLightEnabled[i] = legacyLights[i].enabled;

        model = new GameObject("AirframeVisuals");
        model.transform.SetParent(transform, false);
        model.layer = gameObject.layer;
        bool racer = variant == TrainingDrone.DroneVariant.CheapRacer;
        bool heavy = variant == TrainingDrone.DroneVariant.CheapHeavy || variant == TrainingDrone.DroneVariant.MilitaryHeavy;
        bool wing = variant == TrainingDrone.DroneVariant.MilitarySurveillance || variant == TrainingDrone.DroneVariant.MilitaryInterceptor;
        Color bodyColor = racer ? new Color(0.96f, 0.28f, 0.065f) : heavy ?
            new Color(0.38f, 0.46f, 0.49f) : new Color(0.82f, 0.86f, 0.87f);
        Material shell = MakeMaterial(template, "Shell", bodyColor, 0.12f, 0.48f);
        Material frame = MakeMaterial(template, "CarbonFrame", new Color(0.055f, 0.075f, 0.085f), 0.32f, 0.3f);
        Material metal = MakeMaterial(template, "MachinedMetal", new Color(0.42f, 0.46f, 0.5f), 0.65f, 0.65f);
        Material accent = MakeMaterial(template, "Trim", wing ? new Color(0.95f, 0.39f, 0.07f) : new Color(0.1f, 0.75f, 0.82f), 0.1f, 0.55f);
        Material lens = MakeMaterial(template, "OpticalGlass", new Color(0.025f, 0.2f, 0.29f), 0.65f, 0.95f);

        if (wing) BuildWing(shell, frame, accent, lens);
        else BuildMultirotor(heavy ? 6 : 4, racer, shell, frame, metal, accent, lens);
        CombineStaticParts();
        SetReplacementVisible(isActiveAndEnabled);
    }

    private Material MakeMaterial(Material template, string label, Color color, float metallic, float smoothness)
    {
        Material material = new Material(template) { name = "Airframe_" + label, hideFlags = HideFlags.DontSave };
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        materials.Add(material);
        return material;
    }

    private void BuildMultirotor(int count, bool racer, Material shell, Material frame, Material metal, Material accent, Material lens)
    {
        Part("LowerChassis", PrimitiveType.Sphere, new Vector3(0, -0.05f, 0), new Vector3(0.72f, 0.28f, 1.05f), frame);
        Part("SculptedShell", PrimitiveType.Sphere, new Vector3(0, 0.07f, 0), new Vector3(0.68f, 0.3f, racer ? 0.85f : 1.0f), shell);
        Part("Battery", PrimitiveType.Cube, new Vector3(0, 0.2f, -0.12f), new Vector3(0.32f, 0.09f, 0.46f), frame);
        Part("BatteryStrap", PrimitiveType.Cube, new Vector3(0, 0.252f, -0.12f), new Vector3(0.34f, 0.015f, 0.06f), accent);
        for (int i = 0; i < count; i++)
        {
            float angle = (count == 4 ? 45f + i * 90f : 30f + i * 60f) * Mathf.Deg2Rad;
            Vector3 end = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (count == 4 ? 1.05f : 1.12f);
            Part("Arm", PrimitiveType.Cube, end * 0.52f, new Vector3(0.12f, 0.09f, end.magnitude), frame,
                Quaternion.LookRotation(end));
            Part("ArmInset", PrimitiveType.Cube, end * 0.72f + Vector3.up * 0.055f,
                new Vector3(0.07f, 0.025f, 0.24f), accent, Quaternion.LookRotation(end));
            Part("Motor", PrimitiveType.Cylinder, end, new Vector3(0.2f, 0.09f, 0.2f), metal);
            Part("MotorCap", PrimitiveType.Cylinder, end + Vector3.up * 0.09f, new Vector3(0.14f, 0.025f, 0.14f), frame);
            GameObject pivot = new GameObject("VisualPropeller");
            pivot.transform.SetParent(model.transform, false);
            pivot.transform.localPosition = end + Vector3.up * 0.13f;
            propellers.Add(pivot.transform);
            for (int blade = 0; blade < 2; blade++)
            {
                GameObject mesh = Part("Blade", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.85f, 0.018f, 0.065f), frame,
                    Quaternion.Euler(0, blade * 90, 0), false);
                mesh.transform.SetParent(pivot.transform, false);
                mesh.transform.localPosition = Vector3.zero;
            }
        }
        for (int side = -1; side <= 1; side += 2)
        {
            Part("LandingStrut", PrimitiveType.Cube, new Vector3(side * 0.32f, -0.32f, 0),
                new Vector3(0.055f, 0.48f, 0.06f), metal, Quaternion.Euler(0, 0, side * 15));
            Part("LandingSkid", PrimitiveType.Capsule, new Vector3(side * 0.38f, -0.55f, 0),
                new Vector3(0.055f, 0.4f, 0.055f), frame, Quaternion.Euler(90, 0, 0));
            for (int vent = 0; vent < 4; vent++)
                Part("Vent", PrimitiveType.Cube, new Vector3(side * 0.31f, 0.09f, -0.18f + vent * 0.1f),
                    new Vector3(0.035f, 0.045f, 0.05f), frame);
        }
        Part("Gimbal", PrimitiveType.Sphere, new Vector3(0, -0.27f, 0.32f), new Vector3(0.28f, 0.25f, 0.24f), metal);
        Part("LensHousing", PrimitiveType.Cylinder, new Vector3(0, -0.27f, 0.47f), new Vector3(0.19f, 0.08f, 0.19f), frame, Quaternion.Euler(90, 0, 0));
        Part("Lens", PrimitiveType.Sphere, new Vector3(0, -0.27f, 0.55f), new Vector3(0.145f, 0.145f, 0.045f), lens);
    }

    private void BuildWing(Material shell, Material frame, Material accent, Material lens)
    {
        Part("Fuselage", PrimitiveType.Capsule, Vector3.zero, new Vector3(0.32f, 0.72f, 0.32f), shell, Quaternion.Euler(90, 0, 0));
        Part("Canopy", PrimitiveType.Sphere, new Vector3(0, 0.14f, 0.32f), new Vector3(0.26f, 0.18f, 0.53f), lens);
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject wing = new GameObject("TaperedWing");
            wing.layer = gameObject.layer;
            wing.transform.SetParent(model.transform, false);
            Mesh mesh = WingMesh(side);
            meshes.Add(mesh);
            wing.AddComponent<MeshFilter>().sharedMesh = mesh;
            wing.AddComponent<MeshRenderer>().sharedMaterial = shell;
            staticParts.Add(wing.GetComponent<MeshFilter>());
            Part("WingTipTrim", PrimitiveType.Cube, new Vector3(side * 1.34f, 0.015f, -0.14f), new Vector3(0.16f, 0.045f, 0.22f), accent);
            Part("Tailplane", PrimitiveType.Cube, new Vector3(side * 0.25f, 0.1f, -0.69f), new Vector3(0.58f, 0.035f, 0.23f), frame,
                Quaternion.Euler(0, side * 12, side * 12));
        }
        Part("TailFin", PrimitiveType.Cube, new Vector3(0, 0.26f, -0.66f), new Vector3(0.035f, 0.36f, 0.24f), accent, Quaternion.Euler(-20, 0, 0));
        Part("BellyPanel", PrimitiveType.Sphere, new Vector3(0, -0.11f, 0), new Vector3(0.27f, 0.16f, 0.7f), frame);
    }

    private static Mesh WingMesh(int side)
    {
        Vector3[] outline = { new Vector3(0.1f, 0, 0.38f), new Vector3(1.42f, 0, -0.03f),
            new Vector3(1.42f, 0, -0.25f), new Vector3(0.1f, 0, -0.32f) };
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        // Separate vertices per face retain crisp airfoil edges after normal calculation.
        int[,] faces = { {0,1,2,3}, {7,6,5,4}, {4,5,1,0}, {5,6,2,1}, {6,7,3,2}, {7,4,0,3} };
        for (int face = 0; face < 6; face++)
        {
            int start = vertices.Count;
            for (int corner = 0; corner < 4; corner++)
            {
                int index = faces[face, corner];
                Vector3 vertex = outline[index % 4];
                vertex.x *= side;
                vertex.y = index < 4 ? 0.025f : -0.025f;
                vertices.Add(vertex);
            }
            int[] order = side > 0 ? new[] { 0, 1, 2, 0, 2, 3 } : new[] { 0, 2, 1, 0, 3, 2 };
            foreach (int corner in order) triangles.Add(start + corner);
        }
        Mesh mesh = new Mesh { name = "TaperedAirframeWing" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private GameObject Part(string label, PrimitiveType type, Vector3 position, Vector3 scale, Material material,
        Quaternion? rotation = null, bool combine = true)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = label;
        part.layer = gameObject.layer;
        part.transform.SetParent(model.transform, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.transform.localRotation = rotation ?? Quaternion.identity;
        Collider collider = part.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(collider);
        part.GetComponent<Renderer>().sharedMaterial = material;
        if (combine) staticParts.Add(part.GetComponent<MeshFilter>());
        return part;
    }

    private void CombineStaticParts()
    {
        foreach (Material material in materials)
        {
            var instances = new List<CombineInstance>();
            foreach (MeshFilter part in staticParts)
                if (part.GetComponent<Renderer>().sharedMaterial == material)
                    instances.Add(new CombineInstance { mesh = part.sharedMesh,
                        transform = model.transform.worldToLocalMatrix * part.transform.localToWorldMatrix });
            if (instances.Count == 0) continue;
            Mesh mesh = new Mesh { name = material.name + "_Mesh", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(instances.ToArray(), true, true);
            meshes.Add(mesh);
            GameObject group = new GameObject(material.name);
            group.layer = gameObject.layer;
            group.transform.SetParent(model.transform, false);
            group.AddComponent<MeshFilter>().sharedMesh = mesh;
            group.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
        foreach (MeshFilter part in staticParts)
        {
            part.gameObject.SetActive(false);
            Destroy(part.gameObject);
        }
        staticParts.Clear();
    }

    private void Update()
    {
        if (drone == null || drone.IsDowned) return;
        for (int i = 0; i < propellers.Count; i++)
            propellers[i].Rotate(Vector3.up, (i % 2 == 0 ? 1 : -1) * 1500f * Time.deltaTime, Space.Self);
    }

    private void SetReplacementVisible(bool visible)
    {
        if (model == null) return;
        model.SetActive(visible);
        for (int i = 0; i < legacyRenderers.Length; i++)
            if (legacyRenderers[i] != null) legacyRenderers[i].enabled = !visible && legacyVisibility[i];
        for (int i = 0; i < legacyLights.Length; i++)
            if (legacyLights[i] != null) legacyLights[i].enabled = !visible && legacyLightEnabled[i];
    }

    private void OnEnable() => SetReplacementVisible(true);
    private void OnDisable() => SetReplacementVisible(false);
    private void OnDestroy()
    {
        foreach (Material material in materials) if (material != null) Destroy(material);
        foreach (Mesh mesh in meshes) if (mesh != null) Destroy(mesh);
        if (model != null) Destroy(model);
    }
}
