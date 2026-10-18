using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

/// <summary>Reversible camera grading and daylight lighting; source assets and weather are untouched.</summary>
[DisallowMultipleComponent]
public sealed class ScenePresentation : MonoBehaviour
{
    public enum VisualMode { Original, Performance, Enhanced }
    public VisualMode Mode { get; private set; } = VisualMode.Enhanced;
    private UniversalAdditionalCameraData cameraData;
    private bool originalPost;
    private AntialiasingMode originalAA;
    private AntialiasingQuality originalAAQuality;
    private GameObject volumeObject;
    private VolumeProfile profile;
    private Camera targetCamera;
    private bool originalHDR;
    private LayerMask originalVolumeMask;
    private VolumeFrameworkUpdateMode originalVolumeUpdate;
    private SceneLightingPresentation lighting;

    public void Initialize(Camera camera)
    {
        if (camera == null || cameraData != null) return;
        targetCamera = camera;
        originalHDR = camera.allowHDR;
        cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null) cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        originalPost = cameraData.renderPostProcessing;
        originalAA = cameraData.antialiasing;
        originalAAQuality = cameraData.antialiasingQuality;
        originalVolumeMask = cameraData.volumeLayerMask;
        originalVolumeUpdate = camera.GetVolumeFrameworkUpdateMode();
        lighting = new SceneLightingPresentation();

        volumeObject = new GameObject("PresentationColorGrade") { hideFlags = HideFlags.DontSave };
        volumeObject.transform.SetParent(transform, false);
        // Use a layer already included in this camera's volume mask.
        int mask = cameraData.volumeLayerMask.value;
        if (mask == 0)
        {
            volumeObject.layer = 0;
        }
        else for (int i = 0; i < 32; i++)
            if ((mask & (1 << i)) != 0) { volumeObject.layer = i; break; }
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 200;
        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.hideFlags = HideFlags.DontSave;
        volume.sharedProfile = profile;

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.colorFilter.Override(Color.white);
        color.postExposure.Override(0.05f);
        color.contrast.Override(14);
        color.saturation.Override(3);
        profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);
        Bloom bloom = profile.Add<Bloom>(true);
        // Explicitly override intensity. Disabling the component would let older
        // scene volumes restore bloom and wash out the view again.
        bloom.intensity.Override(0f);
        bloom.clamp.Override(2f);
        profile.Add<ScreenSpaceLensFlare>(true).intensity.Override(0f);
        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.1f);
        vignette.smoothness.Override(0.6f);
        SetMode(Mode);
    }

    public void SetMode(VisualMode mode)
    {
        Mode = mode;
        if (cameraData == null || !isActiveAndEnabled) return;
        volumeObject.SetActive(isActiveAndEnabled);
        bool enhanced = mode == VisualMode.Enhanced;
        profile.TryGet<ColorAdjustments>(out var color);
        profile.TryGet<Tonemapping>(out var tone);
        profile.TryGet<Vignette>(out var vignette);
        color.active = tone.active = vignette.active = enhanced;
        cameraData.renderPostProcessing = mode == VisualMode.Original ? originalPost : mode == VisualMode.Enhanced;
        cameraData.antialiasing = mode == VisualMode.Original ? originalAA :
            mode == VisualMode.Enhanced ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None;
        cameraData.antialiasingQuality = mode == VisualMode.Enhanced ? AntialiasingQuality.High : originalAAQuality;
        // Keep glare suppression visible in Original mode too; disabling this
        // component restores the exact original camera mask.
        cameraData.volumeLayerMask = originalVolumeMask.value == 0 ? 1 : originalVolumeMask;
        targetCamera.allowHDR = mode == VisualMode.Enhanced || originalHDR;
        targetCamera.SetVolumeFrameworkUpdateMode(mode == VisualMode.Original ? originalVolumeUpdate : VolumeFrameworkUpdateMode.EveryFrame);
        if (mode == VisualMode.Original) lighting.Restore();
        else lighting.Apply(mode == VisualMode.Enhanced, targetCamera.transform.position);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
            SetMode((VisualMode)(((int)Mode + 1) % 3));
    }

    private void OnEnable() { if (cameraData != null) SetMode(Mode); }
    private void OnDisable()
    {
        if (volumeObject != null) volumeObject.SetActive(false);
        if (cameraData == null) return;
        cameraData.renderPostProcessing = originalPost;
        cameraData.antialiasing = originalAA;
        cameraData.antialiasingQuality = originalAAQuality;
        cameraData.volumeLayerMask = originalVolumeMask;
        if (targetCamera != null)
        {
            targetCamera.allowHDR = originalHDR;
            targetCamera.SetVolumeFrameworkUpdateMode(originalVolumeUpdate);
        }
        lighting?.Restore();
    }

    private void OnDestroy()
    {
        lighting?.Dispose();
        if (volumeObject != null) Destroy(volumeObject);
        if (profile == null) return;
        foreach (VolumeComponent component in profile.components) Destroy(component);
        Destroy(profile);
    }
}
