using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

/// <summary>Explicit Quest configuration. Does not modify or regenerate the user's scene.</summary>
public static class QuestBuildSetup
{
    [MenuItem("26247/Quest 3/Configure Android VR")]
    public static void Configure()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Install Android Build Support, SDK/NDK Tools and OpenJDK for Unity " +
                Application.unityVersion + " in Unity Hub, restart the Editor, then run Configure Android VR again.");

        ConfigureXR(BuildTargetGroup.Android);

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.kazind.dronethreatsimulator");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        EditorUserBuildSettings.buildAppBundle = false;
        AssetDatabase.SaveAssets();
        Debug.Log("Quest configured: OpenXR, Touch controllers, ARM64, Vulkan. Switch to Android in Build Profiles, " +
            "run XR Project Validation, then Build And Run with your USB-connected Quest.");
    }

    [MenuItem("26247/Quest 3/Configure Windows Quest Link")]
    public static void ConfigureLink()
    {
        ConfigureXR(BuildTargetGroup.Standalone);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        AssetDatabase.SaveAssets();
        Debug.Log("Quest Link configured. On Windows, set Meta Quest Link as the active OpenXR runtime, " +
            "launch Link inside the headset, open SampleScene, then press Play. Android Build Support is not needed for Link.");
    }

    private static void ConfigureXR(BuildTargetGroup target)
    {
        const string folder = "Assets/XR";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "XR");
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget targets))
        {
            string path = folder + "/XRGeneralSettingsPerBuildTarget.asset";
            targets = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
            if (targets == null)
            {
                targets = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(targets, path);
            }
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, targets, true);
        }
        if (!targets.HasManagerSettingsForBuildTarget(target))
            targets.CreateDefaultManagerSettingsForBuildTarget(target);
        XRGeneralSettings general = targets.SettingsForBuildTarget(target);
        general.InitManagerOnStart = true;
        general.Manager.automaticLoading = true;
        general.Manager.automaticRunning = true;
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", target))
            throw new BuildFailedException("OpenXR loader could not be assigned. Check Package Manager and the Console.");

        // OpenXR 1.15.1 exposes reading settings publicly, but creates its asset through this editor API.
        Type packageType = typeof(FeatureHelpers).Assembly.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings", true);
        packageType.GetMethod("GetOrCreateInstance", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
        FeatureHelpers.RefreshFeatures(target);
        OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(target);
        if (settings == null) throw new BuildFailedException("Could not create OpenXR settings.");
        settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
        settings.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.None;
        if (target == BuildTargetGroup.Android) EnableFeature(target, "com.unity.openxr.feature.metaquest");
        EnableFeature(target, "com.unity.openxr.feature.input.oculustouch");

        EditorUtility.SetDirty(targets);
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(general.Manager);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    private static void EnableFeature(BuildTargetGroup target, string id)
    {
        var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(target, id);
        if (feature == null) throw new BuildFailedException("Required OpenXR feature is missing: " + id);
        feature.enabled = true;
        EditorUtility.SetDirty(feature);
    }

    [MenuItem("26247/Quest 3/Build APK")]
    public static void BuildApk()
    {
        Configure();
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            throw new BuildFailedException("First switch to Android in File > Build Profiles, then choose Build APK again.");
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0) throw new BuildFailedException("Add the simulator scene to Build Profiles before building.");
        Directory.CreateDirectory("Builds/Quest");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Quest/DroneThreatSimulator.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Quest APK build failed. See the Console for details.");
        Debug.Log("Quest APK: " + Path.GetFullPath(report.summary.outputPath));
    }
}
