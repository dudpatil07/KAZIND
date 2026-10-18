using UnityEngine;

[DisallowMultipleComponent]
public sealed class ExponentialSquaredFog : MonoBehaviour
{
    [Header("Fog appearance")]
    public Color fogColor = new Color(0.18f, 0.19f, 0.20f, 1f);
    [Min(0f)] public float density = 0.04f;
    public bool fogEnabled = true;

    private bool previousFogEnabled;
    private FogMode previousFogMode;
    private Color previousFogColor;
    private float previousFogDensity;
    private bool settingsCaptured;

    private void Awake()
    {
        CaptureCurrentSettings();
        ApplyFog(fogEnabled);
    }

    /// <summary>Enables or disables the sensor-degradation fog at runtime.</summary>
    public void SetFogEnabled(bool enabled)
    {
        fogEnabled = enabled;
        ApplyFog(enabled);
    }

    /// <summary>Toggles the sensor-degradation fog at runtime.</summary>
    public void ToggleFog()
    {
        SetFogEnabled(!fogEnabled);
    }

    private void ApplyFog(bool enabled)
    {
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = Mathf.Max(0f, density);
        RenderSettings.fog = enabled;
    }

    private void CaptureCurrentSettings()
    {
        previousFogEnabled = RenderSettings.fog;
        previousFogMode = RenderSettings.fogMode;
        previousFogColor = RenderSettings.fogColor;
        previousFogDensity = RenderSettings.fogDensity;
        settingsCaptured = true;
    }

    private void OnDestroy()
    {
        if (!settingsCaptured)
            return;

        RenderSettings.fog = previousFogEnabled;
        RenderSettings.fogMode = previousFogMode;
        RenderSettings.fogColor = previousFogColor;
        RenderSettings.fogDensity = previousFogDensity;
    }
}
