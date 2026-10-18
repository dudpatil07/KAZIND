using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WeatherManager : MonoBehaviour
{
    public enum WeatherType
    {
        Clear,
        Fog,
        HeavyRain
    }

    [Header("Transition")]
    [Min(0f)] public float transitionDuration = 5f;
    public WeatherType startingWeather = WeatherType.Clear;

    [Header("Fog")]
    public Color fogColor = new Color(0.18f, 0.19f, 0.20f, 1f);
    [Min(0f)] public float thickFogDensity = 0.04f;

    [Header("Rain")]
    public ParticleSystem rainSystem;
    [Min(0f)] public float heavyRainRate = 1500f;

    public WeatherType CurrentWeather { get; private set; }

    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (rainSystem == null && Camera.main != null)
        {
            Transform rainTransform = Camera.main.transform.Find("CameraRainParticles");
            if (rainTransform != null)
                rainSystem = rainTransform.GetComponent<ParticleSystem>();
        }

        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = 0f;
        RenderSettings.fog = false;

        if (rainSystem != null)
        {
            ParticleSystem.EmissionModule emission = rainSystem.emission;
            emission.rateOverTime = 0f;
            rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        CurrentWeather = WeatherType.Clear;
        if (startingWeather != WeatherType.Clear)
            TransitionTo(startingWeather);
    }

    /// <summary>Starts a smooth transition to the requested weather state.</summary>
    public void TransitionTo(WeatherType target)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(target));
    }

    public void SetClear() => TransitionTo(WeatherType.Clear);
    public void SetFog() => TransitionTo(WeatherType.Fog);
    public void SetHeavyRain() => TransitionTo(WeatherType.HeavyRain);

    private IEnumerator TransitionRoutine(WeatherType target)
    {
        float startDensity = RenderSettings.fogDensity;
        float startRainRate = GetRainRate();
        float targetDensity = target == WeatherType.Fog ? thickFogDensity : 0f;
        float targetRainRate = target == WeatherType.HeavyRain ? heavyRainRate : 0f;

        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fog = target == WeatherType.Fog || startDensity > 0f;

        if (rainSystem != null && targetRainRate > 0f)
        {
            rainSystem.gameObject.SetActive(true);
            ParticleSystem.EmissionModule emission = rainSystem.emission;
            emission.enabled = true;
            if (!rainSystem.isPlaying)
                rainSystem.Play(true);
        }

        float duration = Mathf.Max(0f, transitionDuration);
        if (duration <= 0f)
        {
            ApplyTransitionValues(targetDensity, targetRainRate);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t); // smoothstep

                ApplyTransitionValues(
                    Mathf.Lerp(startDensity, targetDensity, t),
                    Mathf.Lerp(startRainRate, targetRainRate, t));
                yield return null;
            }

            ApplyTransitionValues(targetDensity, targetRainRate);
        }

        RenderSettings.fog = target == WeatherType.Fog;
        if (rainSystem != null && targetRainRate <= 0f)
        {
            ParticleSystem.EmissionModule emission = rainSystem.emission;
            emission.enabled = false;
            rainSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        CurrentWeather = target;
        transitionRoutine = null;
    }

    private void ApplyTransitionValues(float fogDensity, float rainRate)
    {
        RenderSettings.fogDensity = fogDensity;
        if (rainSystem == null)
            return;

        ParticleSystem.EmissionModule emission = rainSystem.emission;
        emission.rateOverTime = rainRate;
    }

    private float GetRainRate()
    {
        if (rainSystem == null)
            return 0f;

        return rainSystem.emission.rateOverTime.constant;
    }
}
