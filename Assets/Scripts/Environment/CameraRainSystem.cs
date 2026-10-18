using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraRainSystem : MonoBehaviour
{
    private const string RainObjectName = "CameraRainParticles";

    [Header("Rain volume")]
    [Min(1f)] public float coverage = 24f;
    [Min(1f)] public float spawnHeight = 14f;
    [Min(0.1f)] public float particleLifetime = 2f;

    [Header("Rain appearance")]
    public Color rainColor = new Color(0.72f, 0.78f, 0.84f, 0.55f);
    [Min(0.001f)] public float dropSize = 0.035f;
    [Min(0f)] public float stretchLength = 2f;

    [Header("World collision")]
    [Range(1, 256)] public int maxCollisionShapes = 64;
    [Range(0f, 1f)] public float bounce = 0.25f;
    [Range(0f, 1f)] public float collisionDampen = 0.1f;

    private ParticleSystem rain;

    private void Awake()
    {
        Camera targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("CameraRainSystem needs a MainCamera in the scene.", this);
            enabled = false;
            return;
        }

        Transform existing = targetCamera.transform.Find(RainObjectName);
        if (existing != null)
        {
            rain = existing.GetComponent<ParticleSystem>();
            if (rain != null)
                return;
        }

        GameObject rainObject = new GameObject(RainObjectName);
        rainObject.transform.SetParent(targetCamera.transform, false);
        rainObject.transform.localPosition = Vector3.up * spawnHeight;
        rainObject.transform.localRotation = Quaternion.identity;

        rain = rainObject.AddComponent<ParticleSystem>();
        ConfigureRain(rain);
    }

    private void ConfigureRain(ParticleSystem system)
    {
        ParticleSystem.MainModule main = system.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = particleLifetime;
        main.startSpeed = 0f;
        main.startSize = dropSize;
        main.startColor = rainColor;
        main.gravityModifier = 1f;
        main.maxParticles = Mathf.CeilToInt(1500f * particleLifetime * 1.25f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.enabled = true;
        emission.rateOverTime = 1500f;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(coverage, 0.5f, coverage);

        ParticleSystem.CollisionModule collision = system.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.quality = ParticleSystemCollisionQuality.Medium;
        collision.maxCollisionShapes = maxCollisionShapes;
        collision.radiusScale = 0.1f;
        collision.bounce = bounce;
        collision.dampen = collisionDampen;
        collision.lifetimeLoss = 0.4f;
        collision.sendCollisionMessages = false;

        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.1f;
        renderer.lengthScale = stretchLength;
        renderer.cameraVelocityScale = 0f;

        system.Play();
    }
}
