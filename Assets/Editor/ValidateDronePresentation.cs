using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Batch-only regression checks. Does not save or modify the user's scene.</summary>
[InitializeOnLoad]
public static class ValidateDronePresentation
{
    private const string Key = "DronePresentation.BatchValidation";

    static ValidateDronePresentation()
    {
        if (Application.isBatchMode && SessionState.GetBool(Key, false))
            EditorApplication.update += Tick;
    }

    // Run with Unity -batchmode -nographics -executeMethod ValidateDronePresentation.Run (without -quit).
    public static void Run()
    {
        if (!Application.isBatchMode)
            throw new InvalidOperationException("Use a separate batch-mode Unity process for this validation.");
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject inactive = new GameObject("InactiveSpawn");
            inactive.SetActive(false);
            inactive.transform.position = new Vector3(23, 25, 31);
            TrainingDrone source = inactive.AddComponent<TrainingDrone>();
            source.ResetForRound();
            Require(source.transform.position == new Vector3(23, 25, 31), "Reset before Awake lost the spawn position.");
            source.ConfigureAI(null, 5);
            string json = EditorJsonUtility.ToJson(source);
            GameObject copy = new GameObject("ReloadedDrone");
            copy.SetActive(false);
            TrainingDrone restored = copy.AddComponent<TrainingDrone>();
            EditorJsonUtility.FromJsonOverwrite(json, restored);
            Require(restored.SurroundSlot == 5, "Scene serialization lost the assigned slot.");
            UnityEngine.Object.DestroyImmediate(inactive);
            UnityEngine.Object.DestroyImmediate(copy);

            GameObject cameraObject = new GameObject("TestCamera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>().volumeLayerMask = 0;
            Volume oldVolume = new GameObject("LegacyBloomVolume").AddComponent<Volume>();
            oldVolume.isGlobal = true;
            oldVolume.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            oldVolume.sharedProfile.Add<Bloom>(true).intensity.Override(20f);
            oldVolume.sharedProfile.Add<ScreenSpaceLensFlare>(true).intensity.Override(1f);
            GameObject player = new GameObject("TraineePlayer");
            cameraObject.transform.SetParent(player.transform);
            Light sun = new GameObject("TestSun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.7f;
            for (int i = 0; i < 6; i++)
            {
                Light spot = new GameObject("BuildingCornerLight").AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.shadows = LightShadows.Soft;
                spot.intensity = 350;
                spot.transform.position = new Vector3(i * 10, 5, 0);
            }
            GameObject root = new GameObject("DroneTraining");
            for (int i = 0; i < 6; i++)
            {
                GameObject obj = new GameObject("TestDrone_" + i);
                obj.transform.SetParent(root.transform);
                obj.transform.position = new Vector3(i * 10 - 25, 22, 30);
                obj.AddComponent<SphereCollider>().radius = 1.5f;
                obj.AddComponent<TrainingDrone>().variant = (TrainingDrone.DroneVariant)i;
                GameObject legacy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                legacy.name = "LegacyBody";
                legacy.transform.SetParent(obj.transform, false);
                UnityEngine.Object.DestroyImmediate(legacy.GetComponent<Collider>());
            }
            GameObject manager = new GameObject("DroneThreatManager");
            manager.transform.SetParent(root.transform);
            DroneThreatSystem system = manager.AddComponent<DroneThreatSystem>();
            system.playerCamera = camera;
            SessionState.SetBool(Key, true);
            SessionState.SetInt(Key + ".Stage", 0);
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(1, exception.ToString()); }
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false)) return;
        try
        {
            int stage = SessionState.GetInt(Key + ".Stage", 0);
            if (stage == 2 && !EditorApplication.isPlaying)
            {
                Finish(0, "PASS: slots, models, graphics restore, glare suppression and offline narration.");
                return;
            }
            if (!EditorApplication.isPlaying || Time.timeSinceLevelLoad < 0.5f) return;
            DroneThreatSystem system = UnityEngine.Object.FindFirstObjectByType<DroneThreatSystem>();
            if (stage == 0)
            {
                Invoke(system, "FinishIntroVideo");
                Invoke(system, "SelectLevel", 2);
                SessionState.SetInt(Key + ".Stage", 1);
                return;
            }
            if (stage != 1 || Time.timeSinceLevelLoad < 1f) return;
            TrainingDrone[] drones = GameObject.Find("DroneTraining").GetComponentsInChildren<TrainingDrone>(true);
            Require(drones.Length == 6, "Expected six drones.");
            bool[] occupied = new bool[6];
            foreach (TrainingDrone drone in drones)
            {
                Require(!occupied[drone.SurroundSlot], "Two drones share a slot after level start.");
                occupied[drone.SurroundSlot] = true;
                DroneVisualModel visual = drone.GetComponent<DroneVisualModel>();
                Require(visual != null && visual.IsBuilt, "In-game model was not installed.");
                Require(!drone.transform.Find("LegacyBody").GetComponent<Renderer>().enabled, "Legacy mesh is still visible.");
                Require(drone.GetComponentsInChildren<Collider>().Length == 1, "Replacement model changed gameplay colliders.");
                int count = drone.transform.childCount;
                visual.Initialize(drone.variant);
                Require(drone.transform.childCount == count, "Repeated initialization duplicated the model.");
            }
            ScenePresentation presentation = system.GetComponent<ScenePresentation>();
            Light sun = GameObject.Find("TestSun").GetComponent<Light>();
            presentation.SetMode(ScenePresentation.VisualMode.Original);
            Require(Mathf.Approximately(sun.intensity, 0.7f), "Original mode did not restore sunlight.");
            CheckGlareSuppression(system.playerCamera);
            presentation.SetMode(ScenePresentation.VisualMode.Enhanced);
            Require(sun.intensity > 0.7f, "Enhanced mode did not change lighting.");
            Require(system.playerCamera.GetUniversalAdditionalCameraData().volumeLayerMask.value != 0, "Enhanced volume is invisible to the camera.");
            CheckGlareSuppression(system.playerCamera);
            VoiceGuidance voice = system.GetComponent<VoiceGuidance>();
            Require(voice != null && voice.ClipsReady, "Offline narration clips were not imported.");
            voice.SetMuted(true);
            voice.ReplayHelp();
            Require(voice.Muted && voice.CurrentSubtitle.Contains("move with W"), "Muted help lost its subtitles.");
            Require(voice.PendingCount == 0, "Help replay left stale announcements queued.");
            voice.SetContext(VoiceGuidance.Context.Hidden);
            Require(voice.CurrentSubtitle == "" && voice.PendingCount == 0, "Context transition did not clear narration.");
            voice.SetMuted(false);
            int shadowCasters = 0;
            foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.type != LightType.Directional && light.shadows != LightShadows.None) shadowCasters++;
            Require(shadowCasters <= 2, "Too many local shadow casters remain enabled.");
            presentation.SetMode(ScenePresentation.VisualMode.Performance);
            Require(!system.playerCamera.GetUniversalAdditionalCameraData().renderPostProcessing, "Performance mode still runs post-processing.");
            presentation.enabled = false;
            Require(system.playerCamera.GetUniversalAdditionalCameraData().volumeLayerMask.value == 0, "Disabling presentation did not restore the original volume mask.");
            Require(Mathf.Approximately(sun.intensity, 0.7f), "Disabling presentation did not restore lighting.");
            SessionState.SetInt(Key + ".Stage", 2);
            EditorApplication.ExitPlaymode();
        }
        catch (Exception exception) { Finish(1, exception.ToString()); }
    }

    private static void Invoke(DroneThreatSystem system, string method, params object[] args)
    {
        typeof(DroneThreatSystem).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(system, args);
    }

    private static void CheckGlareSuppression(Camera camera)
    {
        // -nographics does not construct a render pipeline, so initialize its
        // volume evaluator explicitly before testing the resolved stack.
        if (!VolumeManager.instance.isInitialized) VolumeManager.instance.Initialize();
        VolumeManager.instance.Update(camera.transform, camera.GetUniversalAdditionalCameraData().volumeLayerMask);
        Require(VolumeManager.instance.stack.GetComponent<Bloom>().intensity.value == 0,
            "The resolved volume stack still enables bloom.");
        Require(VolumeManager.instance.stack.GetComponent<ScreenSpaceLensFlare>().intensity.value == 0,
            "The resolved volume stack still enables screen-space flare.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Finish(int exitCode, string message)
    {
        SessionState.SetBool(Key, false);
        EditorApplication.update -= Tick;
        if (exitCode == 0) Debug.Log(message); else Debug.LogError(message);
        EditorApplication.Exit(exitCode);
    }
}
