using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

#if UNITY_2023_1_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

public static class UpgradeTrainingGroundVisuals
{
    const string ROOT = "Assets/GeneratedTrainingGround";

    [MenuItem("26247/UPGRADE TERRAIN VISUALS")]
    public static void Upgrade()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode first.");
            return;
        }

        GameObject trainingGround =
            GameObject.Find("TrainingGround");

        if (trainingGround == null)
        {
            Debug.LogError(
                "TrainingGround not found. Run BUILD TRAINING GROUND first."
            );
            return;
        }

        Debug.Log("========== UPGRADING TRAINING GROUND VISUALS ==========");

        CreateBetterMaterials();
        UpgradeExistingRenderers(trainingGround);
        AddGlowLights(trainingGround);
        SetupPostProcessing();
        ImproveLighting();

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("========== VISUAL UPGRADE COMPLETE ==========");
    }

    // ============================================================
    // MATERIALS
    // ============================================================

    static void CreateBetterMaterials()
    {
        CreateMaterial(
            "PremiumGrass",
            new Color(0.16f, 0.25f, 0.10f),
            0.0f,
            0.32f,
            10f,
            0.08f
        );

        CreateMaterial(
            "PremiumDirt",
            new Color(0.22f, 0.13f, 0.065f),
            0.0f,
            0.18f,
            9f,
            0.05f
        );

        CreateMaterial(
            "PremiumAsphalt",
            new Color(0.035f, 0.042f, 0.045f),
            0.12f,
            0.42f,
            14f,
            0.18f
        );

        CreateMaterial(
            "PremiumConcrete",
            new Color(0.46f, 0.47f, 0.45f),
            0.0f,
            0.38f,
            8f,
            0.12f
        );

        CreateMaterial(
            "PremiumConcreteDark",
            new Color(0.18f, 0.19f, 0.18f),
            0.05f,
            0.34f,
            8f,
            0.10f
        );

        CreateMaterial(
            "PremiumMilitaryMetal",
            new Color(0.16f, 0.21f, 0.15f),
            0.72f,
            0.63f,
            7f,
            0.14f
        );

        CreateMaterial(
            "PremiumDarkMetal",
            new Color(0.035f, 0.045f, 0.04f),
            0.82f,
            0.60f,
            6f,
            0.10f
        );

        CreateEmissiveMaterial(
            "TrainingGlow",
            new Color(0.05f, 0.45f, 0.8f),
            4.0f
        );

        CreateEmissiveMaterial(
            "WarningGlow",
            new Color(0.85f, 0.24f, 0.03f),
            3.0f
        );

        CreateEmissiveMaterial(
            "LandingPadGlow",
            new Color(0.05f, 0.8f, 0.55f),
            3.5f
        );
    }

    static Material CreateMaterial(
        string name,
        Color color,
        float metallic,
        float smoothness,
        float tiling,
        float normalStrength
    )
    {
        string matPath =
            ROOT + "/Materials/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(matPath);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (mat == null)
        {
            mat = new Material(shader);

            mat.name = name;

            AssetDatabase.CreateAsset(
                mat,
                matPath
            );
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor(
                "_BaseColor",
                color
            );

        if (mat.HasProperty("_Color"))
            mat.SetColor(
                "_Color",
                color
            );

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat(
                "_Metallic",
                metallic
            );

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat(
                "_Smoothness",
                smoothness
            );

        Texture2D albedo =
            CreateSurfaceTexture(
                name + "_Albedo",
                color,
                tiling
            );

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture(
                "_BaseMap",
                albedo
            );
        }
        else if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture(
                "_MainTex",
                albedo
            );
        }

        Texture2D normal =
            CreateNormalTexture(
                name + "_Normal",
                tiling
            );

        if (mat.HasProperty("_BumpMap"))
        {
            mat.SetTexture(
                "_BumpMap",
                normal
            );

            mat.EnableKeyword("_NORMALMAP");

            if (mat.HasProperty("_BumpScale"))
                mat.SetFloat(
                    "_BumpScale",
                    normalStrength
                );
        }

        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 0f);

        EditorUtility.SetDirty(mat);

        return mat;
    }

    static Material CreateEmissiveMaterial(
        string name,
        Color glowColor,
        float intensity
    )
    {
        string path =
            ROOT + "/Materials/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(
                path
            );

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
            shader = Shader.Find("Standard");

        if (mat == null)
        {
            mat = new Material(shader);

            mat.name = name;

            AssetDatabase.CreateAsset(
                mat,
                path
            );
        }

        Color emission =
            glowColor * intensity;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor(
                "_BaseColor",
                glowColor
            );

        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor(
                "_EmissionColor",
                emission
            );

        mat.EnableKeyword("_EMISSION");

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat(
                "_Smoothness",
                0.8f
            );

        EditorUtility.SetDirty(mat);

        return mat;
    }

    // ============================================================
    // PROCEDURAL SURFACE TEXTURE
    // ============================================================

    static Texture2D CreateSurfaceTexture(
        string name,
        Color baseColor,
        float tiling
    )
    {
        string path =
            ROOT + "/Textures/" + name + ".asset";

        Texture2D existing =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                path
            );

        if (existing != null)
            return existing;

        const int size = 256;

        Texture2D tex =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true
            );

        tex.wrapMode =
            TextureWrapMode.Repeat;

        tex.filterMode =
            FilterMode.Trilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u =
                    (float)x / size;

                float v =
                    (float)y / size;

                float large =
                    Mathf.PerlinNoise(
                        u * 3.5f,
                        v * 3.5f
                    );

                float medium =
                    Mathf.PerlinNoise(
                        u * 12f + 10f,
                        v * 12f + 20f
                    );

                float tiny =
                    Mathf.PerlinNoise(
                        u * 35f + 50f,
                        v * 35f + 70f
                    );

                float noise =
                    large * 0.52f +
                    medium * 0.30f +
                    tiny * 0.18f;

                float brightness =
                    Mathf.Lerp(
                        0.72f,
                        1.18f,
                        noise
                    );

                Color c =
                    baseColor * brightness;

                // subtle variation
                c.r *= 0.96f + tiny * 0.08f;
                c.g *= 0.96f + large * 0.10f;
                c.b *= 0.96f + medium * 0.08f;

                c.a = 1f;

                tex.SetPixel(
                    x,
                    y,
                    c
                );
            }
        }

        tex.Apply(
            true,
            false
        );

        AssetDatabase.CreateAsset(
            tex,
            path
        );

        return tex;
    }

    static Texture2D CreateNormalTexture(
        string name,
        float tiling
    )
    {
        string path =
            ROOT + "/Textures/" + name + ".asset";

        Texture2D existing =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                path
            );

        if (existing != null)
            return existing;

        const int size = 256;

        Texture2D tex =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true
            );

        tex.wrapMode =
            TextureWrapMode.Repeat;

        tex.filterMode =
            FilterMode.Trilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u =
                    (float)x / size;

                float v =
                    (float)y / size;

                float h =
                    Mathf.PerlinNoise(
                        u * 18f,
                        v * 18f
                    );

                float dx =
                    Mathf.PerlinNoise(
                        (u + 0.004f) * 18f,
                        v * 18f
                    ) - h;

                float dy =
                    Mathf.PerlinNoise(
                        u * 18f,
                        (v + 0.004f) * 18f
                    ) - h;

                Vector3 normal =
                    new Vector3(
                        -dx * 8f,
                        -dy * 8f,
                        1f
                    ).normalized;

                Color c =
                    new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f,
                        1f
                    );

                tex.SetPixel(
                    x,
                    y,
                    c
                );
            }
        }

        tex.Apply(
            true,
            false
        );

        AssetDatabase.CreateAsset(
            tex,
            path
        );

        return tex;
    }

    // ============================================================
    // APPLY MATERIALS TO EXISTING MAP
    // ============================================================

    static void UpgradeExistingRenderers(
        GameObject root
    )
    {
        Material grass =
            LoadMaterial("PremiumGrass");

        Material dirt =
            LoadMaterial("PremiumDirt");

        Material asphalt =
            LoadMaterial("PremiumAsphalt");

        Material concrete =
            LoadMaterial("PremiumConcrete");

        Material concreteDark =
            LoadMaterial("PremiumConcreteDark");

        Material metal =
            LoadMaterial("PremiumMilitaryMetal");

        Material darkMetal =
            LoadMaterial("PremiumDarkMetal");

        Transform ground =
            root.transform.Find(
                "Environment/Ground"
            );

        Transform roads =
            root.transform.Find(
                "Environment/Roads"
            );

        Transform walls =
            root.transform.Find(
                "Environment/Walls"
            );

        Transform buildings =
            root.transform.Find(
                "Environment/Buildings"
            );

        Transform containers =
            root.transform.Find(
                "Environment/Containers"
            );

        Transform barriers =
            root.transform.Find(
                "Environment/Barriers"
            );

        Transform fence =
            root.transform.Find(
                "Environment/Fence"
            );

        Transform tower =
            root.transform.Find(
                "Environment/WatchTower"
            );

        if (ground != null)
        {
            foreach (
                Renderer r in
                ground.GetComponentsInChildren<Renderer>(
                    true
                )
            )
            {
                r.sharedMaterial =
                    r.name.Contains("Dirt")
                    ? dirt
                    : grass;
            }
        }

        ApplyToChildren(
            roads,
            asphalt
        );

        ApplyToChildren(
            walls,
            concrete
        );

        ApplyToChildren(
            barriers,
            concrete
        );

        ApplyToChildren(
            buildings,
            concrete
        );

        ApplyToChildren(
            containers,
            metal
        );

        ApplyToChildren(
            fence,
            darkMetal
        );

        ApplyToChildren(
            tower,
            metal
        );

        // Keep windows looking polished
        if (buildings != null)
        {
            foreach (
                Renderer r in
                buildings.GetComponentsInChildren<Renderer>(
                    true
                )
            )
            {
                if (
                    r.name.Contains("Window")
                )
                {
                    Material glass =
                        LoadMaterial(
                            "PremiumDarkMetal"
                        );

                    r.sharedMaterial =
                        glass;
                }
            }
        }

        // Dark roof details
        if (buildings != null)
        {
            foreach (
                Renderer r in
                buildings.GetComponentsInChildren<Renderer>(
                    true
                )
            )
            {
                if (
                    r.name.Contains("Roof") ||
                    r.name.Contains("Equipment")
                )
                {
                    r.sharedMaterial =
                        concreteDark;
                }
            }
        }
    }

    static void ApplyToChildren(
        Transform parent,
        Material material
    )
    {
        if (parent == null)
            return;

        Renderer[] renderers =
            parent.GetComponentsInChildren<Renderer>(
                true
            );

        foreach (Renderer r in renderers)
        {
            r.sharedMaterial =
                material;
        }
    }

    static Material LoadMaterial(
        string name
    )
    {
        return AssetDatabase.LoadAssetAtPath<Material>(
            ROOT + "/Materials/" + name + ".mat"
        );
    }

    // ============================================================
    // GLOW LIGHTS
    // ============================================================

    static void AddGlowLights(
        GameObject root
    )
    {
        GameObject existing =
            root.transform.Find(
                "GlowLighting"
            )?.gameObject;

        if (existing != null)
            Object.DestroyImmediate(existing);

        GameObject glowRoot =
            new GameObject(
                "GlowLighting"
            );

        glowRoot.transform.SetParent(
            root.transform
        );

        Material blue =
            LoadMaterial(
                "TrainingGlow"
            );

        Material orange =
            LoadMaterial(
                "WarningGlow"
            );

        Material green =
            LoadMaterial(
                "LandingPadGlow"
            );

        // --------------------------------------------------------
        // Road lights
        // --------------------------------------------------------

        for (int i = 0; i < 12; i++)
        {
            float x =
                -88f + i * 16f;

            CreateGlowPoint(
                "RoadLight",
                new Vector3(
                    x,
                    0.65f,
                    5.8f
                ),
                blue,
                glowRoot.transform
            );

            CreateGlowPoint(
                "RoadLight",
                new Vector3(
                    x,
                    0.65f,
                    -9.8f
                ),
                blue,
                glowRoot.transform
            );
        }

        // --------------------------------------------------------
        // Drone training pad lighting
        // --------------------------------------------------------

        Vector3[] padLights =
        {
            new Vector3(9f, 0.6f, 47f),
            new Vector3(31f, 0.6f, 47f),
            new Vector3(9f, 0.6f, 69f),
            new Vector3(31f, 0.6f, 69f)
        };

        foreach (Vector3 p in padLights)
        {
            CreateGlowPoint(
                "DronePadLight",
                p,
                green,
                glowRoot.transform
            );
        }

        // --------------------------------------------------------
        // Warning lights near range
        // --------------------------------------------------------

        Vector3[] warningLights =
        {
            new Vector3(-18f, 1.2f, 70f),
            new Vector3(-8f, 1.2f, 70f),
            new Vector3(2f, 1.2f, 70f)
        };

        foreach (Vector3 p in warningLights)
        {
            CreateGlowPoint(
                "WarningLight",
                p,
                orange,
                glowRoot.transform
            );
        }
    }

    static void CreateGlowPoint(
        string name,
        Vector3 position,
        Material material,
        Transform parent
    )
    {
        GameObject cube =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        cube.name =
            name;

        cube.transform.position =
            position;

        cube.transform.localScale =
            new Vector3(
                0.28f,
                0.12f,
                0.28f
            );

        cube.GetComponent<Renderer>()
            .sharedMaterial =
            material;

        cube.transform.SetParent(
            parent
        );

        Light point =
            cube.AddComponent<Light>();

        point.type =
            LightType.Point;

        point.range =
            7f;

        point.intensity =
            2.2f;

        point.shadows =
            LightShadows.None;

        if (
            material.name.Contains(
                "Warning"
            )
        )
        {
            point.color =
                new Color(
                    1f,
                    0.25f,
                    0.05f
                );
        }
        else if (
            material.name.Contains(
                "Landing"
            )
        )
        {
            point.color =
                new Color(
                    0.05f,
                    1f,
                    0.6f
                );
        }
        else
        {
            point.color =
                new Color(
                    0.1f,
                    0.55f,
                    1f
                );
        }
    }

    // ============================================================
    // POST PROCESSING / BLOOM
    // ============================================================

    static void SetupPostProcessing()
    {
#if UNITY_2023_1_OR_NEWER

        GameObject existing =
            GameObject.Find(
                "TrainingGround_PostProcessing"
            );

        if (existing != null)
            Object.DestroyImmediate(
                existing
            );

        GameObject volumeObject =
            new GameObject(
                "TrainingGround_PostProcessing"
            );

        Volume volume =
            volumeObject.AddComponent<Volume>();

        volume.isGlobal =
            true;

        volume.priority =
            100f;

        VolumeProfile profile =
            ScriptableObject.CreateInstance<
                VolumeProfile
            >();

        // Bloom
        Bloom bloom =
            profile.Add<Bloom>(
                true
            );

        bloom.active =
            true;

        bloom.intensity.Override(
            1.25f
        );

        bloom.threshold.Override(
            0.7f
        );

        bloom.scatter.Override(
            0.72f
        );

        // Exposure / contrast
        ColorAdjustments color =
            profile.Add<ColorAdjustments>(
                true
            );

        color.active =
            true;

        color.postExposure.Override(
            0.18f
        );

        color.contrast.Override(
            8f
        );

        color.saturation.Override(
            5f
        );

        // Tonemapping
        Tonemapping tone =
            profile.Add<Tonemapping>(
                true
            );

        tone.active =
            true;

        tone.mode.Override(
            TonemappingMode.ACES
        );

        volume.profile =
            profile;

        volumeObject.transform.position =
            Vector3.zero;

#endif
    }

    // ============================================================
    // LIGHTING
    // ============================================================

    static void ImproveLighting()
    {
        Light sun =
            RenderSettings.sun;

        if (sun != null)
        {
            sun.intensity =
                1.45f;

            sun.shadowStrength =
                0.78f;

            sun.shadowBias =
                0.035f;

            sun.shadowNormalBias =
                0.3f;
        }

        RenderSettings.ambientIntensity =
            0.85f;

        RenderSettings.fog =
            true;

        RenderSettings.fogDensity =
            0.0020f;

        RenderSettings.fogColor =
            new Color(
                0.46f,
                0.50f,
                0.48f
            );
    }
}
