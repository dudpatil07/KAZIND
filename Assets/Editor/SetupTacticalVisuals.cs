using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

#if UNITY_2023_1_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

public static class SetupTacticalVisuals
{
    [MenuItem("26247/TACTICAL VISUAL STYLE")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode first.");
            return;
        }

        Debug.Log("=== APPLYING TACTICAL VISUAL STYLE ===");

        RemoveOldVisualVolume();
        RemoveOldGlowObjects();

        SetupWorldLighting();
        SetupColorGrading();
        SetupBuildingCornerLights();
        DarkenMaterials();

        AssetDatabase.SaveAssets();

        Debug.Log("=== TACTICAL VISUAL STYLE COMPLETE ===");
    }

    // ============================================================
    // REMOVE PREVIOUS BLOOM / POST PROCESS
    // ============================================================

    static void RemoveOldVisualVolume()
    {
        GameObject old =
            GameObject.Find("TrainingGround_PostProcessing");

        if (old != null)
            Object.DestroyImmediate(old);

        // Also remove any old volume created by this script
        GameObject tacticalOld =
            GameObject.Find("TacticalVisualVolume");

        if (tacticalOld != null)
            Object.DestroyImmediate(tacticalOld);
    }

    // ============================================================
    // REMOVE GLOW OBJECTS FROM PREVIOUS VERSION
    // ============================================================

    static void RemoveOldGlowObjects()
    {
        GameObject root =
            GameObject.Find("TrainingGround");

        if (root == null)
            return;

        Transform glow =
            root.transform.Find("GlowLighting");

        if (glow != null)
            Object.DestroyImmediate(
                glow.gameObject
            );
    }

    // ============================================================
    // WORLD LIGHTING
    // ============================================================

    static void SetupWorldLighting()
    {
        Light[] lights =
            Object.FindObjectsByType<Light>(
                FindObjectsSortMode.None
            );

        Light sun = null;

        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                sun = l;
                break;
            }
        }

        if (sun == null)
        {
            GameObject sunObj =
                new GameObject("Sun");

            sun =
                sunObj.AddComponent<Light>();

            sun.type =
                LightType.Directional;

            RenderSettings.sun =
                sun;
        }

        // More dramatic FPS-style sun
        sun.transform.rotation =
            Quaternion.Euler(
                47f,
                -32f,
                0f
            );

        sun.intensity =
            0.92f;

        sun.color =
            new Color(
                0.90f,
                0.95f,
                0.90f
            );

        sun.shadows =
            LightShadows.Soft;

        sun.shadowStrength =
            0.90f;

        sun.shadowBias =
            0.035f;

        sun.shadowNormalBias =
            0.25f;

        // --------------------------------------------------------
        // AMBIENT LIGHT
        // --------------------------------------------------------

        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Trilight;

        // Sky illumination
        RenderSettings.ambientSkyColor =
            new Color(
                0.20f,
                0.25f,
                0.22f
            );

        // Mid-level bounce
        RenderSettings.ambientEquatorColor =
            new Color(
                0.25f,
                0.30f,
                0.25f
            );

        // Ground bounce
        RenderSettings.ambientGroundColor =
            new Color(
                0.09f,
                0.11f,
                0.085f
            );

        RenderSettings.ambientIntensity =
            0.62f;

        // --------------------------------------------------------
        // FOG
        // --------------------------------------------------------

        RenderSettings.fog =
            true;

        RenderSettings.fogMode =
            FogMode.ExponentialSquared;

        RenderSettings.fogDensity =
            0.0028f;

        RenderSettings.fogColor =
            new Color(
                0.20f,
                0.25f,
                0.21f
            );

        // --------------------------------------------------------
        // SKY
        // --------------------------------------------------------

        Shader skyShader =
            Shader.Find(
                "Skybox/Procedural"
            );

        if (skyShader != null)
        {
            Material sky =
                new Material(
                    skyShader
                );

            sky.name =
                "TacticalSky";

            if (sky.HasProperty(
                "_SkyTint"))
            {
                sky.SetColor(
                    "_SkyTint",
                    new Color(
                        0.22f,
                        0.28f,
                        0.24f
                    )
                );
            }

            if (sky.HasProperty(
                "_GroundColor"))
            {
                sky.SetColor(
                    "_GroundColor",
                    new Color(
                        0.10f,
                        0.12f,
                        0.10f
                    )
                );
            }

            if (sky.HasProperty(
                "_AtmosphereThickness"))
            {
                sky.SetFloat(
                    "_AtmosphereThickness",
                    1.35f
                );
            }

            RenderSettings.skybox =
                sky;
        }
    }

    // ============================================================
    // COLOR GRADING
    // ============================================================

    static void SetupColorGrading()
    {
#if UNITY_2023_1_OR_NEWER

        GameObject volumeObject =
            new GameObject(
                "TacticalVisualVolume"
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

        // --------------------------------------------------------
        // COLOR
        // --------------------------------------------------------

        ColorAdjustments color =
            profile.Add<ColorAdjustments>(
                true
            );

        color.active =
            true;

        // Slightly darker image
        color.postExposure.Override(
            -0.18f
        );

        // Stronger FPS-style contrast
        color.contrast.Override(
            14f
        );

        // Slightly muted colors
        color.saturation.Override(
            -8f
        );

        // Olive/green tactical tint
        color.colorFilter.Override(
            new Color(
                0.84f,
                1.00f,
                0.87f,
                1f
            )
        );

        // --------------------------------------------------------
        // TONEMAPPING
        // --------------------------------------------------------

        Tonemapping tone =
            profile.Add<Tonemapping>(
                true
            );

        tone.active =
            true;

        tone.mode.Override(
            TonemappingMode.ACES
        );

        // --------------------------------------------------------
        // BLOOM = OFF
        // --------------------------------------------------------

        Bloom bloom =
            profile.Add<Bloom>(
                true
            );

        bloom.active =
            false;

        // --------------------------------------------------------
        // VIGNETTE
        // --------------------------------------------------------

        Vignette vignette =
            profile.Add<Vignette>(
                true
            );

        vignette.active =
            true;

        vignette.intensity.Override(
            0.16f
        );

        vignette.smoothness.Override(
            0.65f
        );

        volume.profile =
            profile;

#endif
    }

    // ============================================================
    // BUILDING CORNER LIGHTS
    // ============================================================

    static void SetupBuildingCornerLights()
    {
        GameObject old =
            GameObject.Find(
                "BuildingCornerLighting"
            );

        if (old != null)
            Object.DestroyImmediate(
                old
            );

        GameObject root =
            new GameObject(
                "BuildingCornerLighting"
            );

        // --------------------------------------------------------
        // Command Building
        // --------------------------------------------------------

        AddCornerLight(
            root.transform,
            new Vector3(
                51f,
                5.5f,
                40f
            )
        );

        AddCornerLight(
            root.transform,
            new Vector3(
                85f,
                5.5f,
                40f
            )
        );

        AddCornerLight(
            root.transform,
            new Vector3(
                51f,
                5.5f,
                68f
            )
        );

        AddCornerLight(
            root.transform,
            new Vector3(
                85f,
                5.5f,
                68f
            )
        );

        // --------------------------------------------------------
        // Operations Building
        // --------------------------------------------------------

        AddCornerLight(
            root.transform,
            new Vector3(
                -86f,
                5f,
                -60f
            )
        );

        AddCornerLight(
            root.transform,
            new Vector3(
                -54f,
                5f,
                -60f
            )
        );

        // --------------------------------------------------------
        // Training Building
        // --------------------------------------------------------

        AddCornerLight(
            root.transform,
            new Vector3(
                48f,
                4.5f,
                -5f
            )
        );

        AddCornerLight(
            root.transform,
            new Vector3(
                76f,
                4.5f,
                -5f
            )
        );
    }

    static void AddCornerLight(
        Transform parent,
        Vector3 position
    )
    {
        GameObject obj =
            new GameObject(
                "BuildingCornerLight"
            );

        obj.transform.position =
            position;

        obj.transform.SetParent(
            parent
        );

        Light light =
            obj.AddComponent<Light>();

        light.type =
            LightType.Spot;

        // Real illumination, not emissive glow
        light.intensity =
            350f;

        light.range =
            14f;

        light.spotAngle =
            95f;

        light.innerSpotAngle =
            55f;

        light.color =
            new Color(
                0.80f,
                0.90f,
                0.78f
            );

        light.shadows =
            LightShadows.Soft;

        light.shadowStrength =
            0.55f;

        light.shadowBias =
            0.04f;

        // Point downward and toward the map
        light.transform.rotation =
            Quaternion.Euler(
                45f,
                0f,
                0f
            );
    }

    // ============================================================
    // DARKER MATERIAL PALETTE
    // ============================================================

    static void DarkenMaterials()
    {
        GameObject root =
            GameObject.Find(
                "TrainingGround"
            );

        if (root == null)
            return;

        Renderer[] renderers =
            root.GetComponentsInChildren<Renderer>(
                true
            );

        foreach (Renderer r in renderers)
        {
            Material[] materials =
                r.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material mat =
                    materials[i];

                if (mat == null)
                    continue;

                string n =
                    mat.name.ToLower();

                if (n.Contains("grass"))
                {
                    SetMaterialColor(
                        mat,
                        new Color(
                            0.13f,
                            0.21f,
                            0.09f
                        )
                    );

                    SetSmoothness(
                        mat,
                        0.28f
                    );
                }
                else if (n.Contains("dirt"))
                {
                    SetMaterialColor(
                        mat,
                        new Color(
                            0.18f,
                            0.11f,
                            0.055f
                        )
                    );
                }
                else if (n.Contains("asphalt"))
                {
                    SetMaterialColor(
                        mat,
                        new Color(
                            0.035f,
                            0.040f,
                            0.038f
                        )
                    );

                    SetSmoothness(
                        mat,
                        0.30f
                    );
                }
                else if (n.Contains("concrete"))
                {
                    SetMaterialColor(
                        mat,
                        new Color(
                            0.34f,
                            0.35f,
                            0.32f
                        )
                    );
                }
                else if (n.Contains("metal"))
                {
                    SetMaterialColor(
                        mat,
                        new Color(
                            0.12f,
                            0.16f,
                            0.115f
                        )
                    );

                    SetSmoothness(
                        mat,
                        0.45f
                    );
                }
            }
        }
    }

    static void SetMaterialColor(
        Material mat,
        Color color
    )
    {
        if (mat.HasProperty(
            "_BaseColor"))
        {
            mat.SetColor(
                "_BaseColor",
                color
            );
        }

        if (mat.HasProperty(
            "_Color"))
        {
            mat.SetColor(
                "_Color",
                color
            );
        }

        EditorUtility.SetDirty(
            mat
        );
    }

    static void SetSmoothness(
        Material mat,
        float value
    )
    {
        if (mat.HasProperty(
            "_Smoothness"))
        {
            mat.SetFloat(
                "_Smoothness",
                value
            );

            EditorUtility.SetDirty(
                mat
            );
        }
    }
}
