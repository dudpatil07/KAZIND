using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Owns runtime lighting changes and restores the original scene settings.</summary>
public sealed class SceneLightingPresentation : IDisposable
{
    private struct LightSnapshot
    {
        public Light light;
        public Color color;
        public float intensity;
        public float strength;
        public LightShadows shadows;
    }

    private readonly LightSnapshot[] lights;
    private readonly AmbientMode ambientMode;
    private readonly Color skyColor, equatorColor, groundColor;
    private readonly float ambientIntensity;
    private readonly SphericalHarmonicsL2 ambientProbe;
    private readonly Material originalSky;
    private Material daylightSky;

    public SceneLightingPresentation()
    {
        ambientMode = RenderSettings.ambientMode;
        skyColor = RenderSettings.ambientSkyColor;
        equatorColor = RenderSettings.ambientEquatorColor;
        groundColor = RenderSettings.ambientGroundColor;
        ambientIntensity = RenderSettings.ambientIntensity;
        ambientProbe = RenderSettings.ambientProbe;
        originalSky = RenderSettings.skybox;
        if (originalSky != null)
        {
            daylightSky = new Material(originalSky) { name = "PresentationDaylightSky", hideFlags = HideFlags.DontSave };
            SetSkyColor("_SkyTint", new Color(0.48f, 0.59f, 0.7f));
            SetSkyColor("_GroundColor", new Color(0.34f, 0.36f, 0.38f));
            SetSkyFloat("_AtmosphereThickness", 1f);
            SetSkyFloat("_Exposure", 1.15f);
            SetSkyFloat("_SunSize", 0.025f);
        }
        Light[] sceneLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        lights = new LightSnapshot[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
        {
            Light light = sceneLights[i];
            lights[i] = new LightSnapshot { light = light, color = light.color, intensity = light.intensity,
                strength = light.shadowStrength, shadows = light.shadows };
        }
    }

    public void Apply(bool enhanced, Vector3 viewpoint)
    {
        Restore();
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.56f, 0.66f, 0.78f);
        RenderSettings.ambientEquatorColor = new Color(0.43f, 0.48f, 0.53f);
        RenderSettings.ambientGroundColor = new Color(0.27f, 0.26f, 0.24f);
        RenderSettings.ambientIntensity = 1f;
        // Update the ambient probe immediately: moving meshes are not baked into scene lighting.
        SphericalHarmonicsL2 fill = new SphericalHarmonicsL2();
        fill.AddAmbientLight(new Color(0.27f, 0.29f, 0.32f));
        fill.AddDirectionalLight(Vector3.up, new Color(0.56f, 0.67f, 0.82f), 0.35f);
        RenderSettings.ambientProbe = fill;
        if (daylightSky != null) RenderSettings.skybox = daylightSky;

        Light nearest = null;
        Light second = null;
        float nearestDistance = float.PositiveInfinity;
        float secondDistance = float.PositiveInfinity;
        foreach (LightSnapshot snapshot in lights)
        {
            Light light = snapshot.light;
            if (light == null || light.type != LightType.Spot || !light.isActiveAndEnabled || snapshot.shadows == LightShadows.None)
                continue;
            float distance = (light.transform.position - viewpoint).sqrMagnitude;
            if (distance < nearestDistance)
            {
                second = nearest;
                secondDistance = nearestDistance;
                nearest = light;
                nearestDistance = distance;
            }
            else if (distance < secondDistance) { second = light; secondDistance = distance; }
        }
        foreach (LightSnapshot snapshot in lights)
        {
            Light light = snapshot.light;
            if (light == null) continue;
            if (light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.95f, 0.88f);
                light.intensity = Mathf.Max(snapshot.intensity, 1.35f);
                light.shadowStrength = 0.8f;
                light.shadows = enhanced ? LightShadows.Soft : LightShadows.Hard;
            }
            else
            {
                // Two local shadow casters fit the existing atlas; other lamps still illuminate.
                light.shadows = enhanced && (light == nearest || light == second) ? LightShadows.Soft : LightShadows.None;
                if (light.name == "BuildingCornerLight")
                {
                    light.intensity = Mathf.Min(snapshot.intensity, 8f);
                    light.color = new Color(1f, 0.89f, 0.73f);
                }
            }
        }
    }

    public void Restore()
    {
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientGroundColor = groundColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.ambientProbe = ambientProbe;
        RenderSettings.skybox = originalSky;
        foreach (LightSnapshot snapshot in lights)
        {
            if (snapshot.light == null) continue;
            snapshot.light.color = snapshot.color;
            snapshot.light.intensity = snapshot.intensity;
            snapshot.light.shadowStrength = snapshot.strength;
            snapshot.light.shadows = snapshot.shadows;
        }
    }

    private void SetSkyColor(string property, Color color)
    {
        if (daylightSky.HasProperty(property)) daylightSky.SetColor(property, color);
    }

    private void SetSkyFloat(string property, float value)
    {
        if (daylightSky.HasProperty(property)) daylightSky.SetFloat(property, value);
    }

    public void Dispose()
    {
        Restore();
        if (daylightSky != null) UnityEngine.Object.Destroy(daylightSky);
        daylightSky = null;
    }
}
