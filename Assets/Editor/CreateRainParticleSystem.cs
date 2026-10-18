using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateRainParticleSystem
{
    private const string RainObjectName = "CameraRainParticles";
    private const float EmissionRate = 1500f;

    [MenuItem("Drone Simulator/Weather/Create Camera Rain System")]
    public static void CreateOrConfigureRain()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("A camera tagged MainCamera is required to create the rain system.");
            return;
        }

        Transform rainTransform = mainCamera.transform.Find(RainObjectName);
        GameObject rainObject;
        ParticleSystem rain;

        if (rainTransform != null)
        {
            rainObject = rainTransform.gameObject;
            rain = rainObject.GetComponent<ParticleSystem>();
            if (rain == null)
                rain = Undo.AddComponent<ParticleSystem>(rainObject);
            Undo.RecordObject(rain, "Configure Camera Rain System");
        }
        else
        {
            rainObject = new GameObject(RainObjectName);
            Undo.RegisterCreatedObjectUndo(rainObject, "Create Camera Rain System");
            rainObject.transform.SetParent(mainCamera.transform, false);
            rainObject.transform.localPosition = Vector3.up * 14f;
            rainObject.transform.localRotation = Quaternion.identity;
            rain = Undo.AddComponent<ParticleSystem>(rainObject);
        }

        Configure(rain);
        EditorUtility.SetDirty(rain);
        EditorSceneManager.MarkSceneDirty(rainObject.scene);
        Selection.activeGameObject = rainObject;
        Debug.Log("Camera rain system created: 1,500 particles/second, stretched billboards, world collision.", rainObject);
    }

    private static void Configure(ParticleSystem system)
    {
        ParticleSystem.MainModule main = system.main;
        main.duration = 1f;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 2f;
        main.startSpeed = 0f;
        main.startSize = 0.035f;
        main.startColor = new Color(0.72f, 0.78f, 0.84f, 0.55f);
        main.gravityModifier = 1f;
        main.maxParticles = 3750;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = false;
        emission.rateOverTime = EmissionRate;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(24f, 0.5f, 24f);

        ParticleSystem.CollisionModule collision = system.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.quality = ParticleSystemCollisionQuality.Medium;
        collision.maxCollisionShapes = 64;
        collision.radiusScale = 0.1f;
        collision.bounce = 0.25f;
        collision.dampen = 0.1f;
        collision.lifetimeLoss = 0.4f;
        collision.sendCollisionMessages = false;

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.1f;
        renderer.lengthScale = 2f;
        renderer.cameraVelocityScale = 0f;

        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
