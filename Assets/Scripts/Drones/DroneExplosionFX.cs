using UnityEngine;

public static class DroneExplosionFX
{
    public static void Play(Vector3 position)
    {
        GameObject root =
            new GameObject("DroneExplosion");

        root.transform.position = position;

        // ========================================================
        // FLASH
        // ========================================================

        GameObject flash =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

        flash.name = "ExplosionFlash";
        flash.transform.SetParent(root.transform);
        flash.transform.localPosition = Vector3.zero;
        flash.transform.localScale = Vector3.one * 0.6f;

        Collider flashCollider =
            flash.GetComponent<Collider>();

        if (flashCollider != null)
            Object.DestroyImmediate(flashCollider);

        Renderer renderer =
            flash.GetComponent<Renderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material =
            new Material(shader);

        Color flashColor =
            new Color(
                1f,
                0.28f,
                0.03f,
                1f
            );

        if (material.HasProperty("_BaseColor"))
            material.SetColor(
                "_BaseColor",
                flashColor
            );

        if (material.HasProperty("_Color"))
            material.SetColor(
                "_Color",
                flashColor
            );

        renderer.sharedMaterial = material;

        // ========================================================
        // PARTICLES
        // ========================================================

        GameObject particleObject =
            new GameObject(
                "ExplosionParticles"
            );

        particleObject.transform.SetParent(
            root.transform
        );

        particleObject.transform.localPosition =
            Vector3.zero;

        ParticleSystem ps =
            particleObject.AddComponent<
                ParticleSystem
            >();

        // Main
        ParticleSystem.MainModule main =
            ps.main;

        main.duration = 0.9f;
        main.loop = false;

        main.startLifetime =
            new ParticleSystem.MinMaxCurve(
                0.25f,
                0.75f
            );

        main.startSpeed =
            new ParticleSystem.MinMaxCurve(
                4f,
                12f
            );

        main.startSize =
            new ParticleSystem.MinMaxCurve(
                0.12f,
                0.4f
            );

        main.maxParticles = 80;

        main.startColor =
            new ParticleSystem.MinMaxGradient(
                new Color(
                    1f,
                    0.20f,
                    0.03f,
                    1f
                ),
                new Color(
                    1f,
                    0.78f,
                    0.18f,
                    1f
                )
            );

        // Emission
        ParticleSystem.EmissionModule emission =
            ps.emission;

        emission.enabled = true;
        emission.rateOverTime = 0f;

        emission.SetBursts(
            new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(
                    0f,
                    65
                )
            }
        );

        // Shape
        ParticleSystem.ShapeModule shape =
            ps.shape;

        shape.enabled = true;

        // Unity 6 uses shapeType
        shape.shapeType =
            ParticleSystemShapeType.Sphere;

        shape.radius = 0.25f;

        // Color over lifetime
        ParticleSystem.ColorOverLifetimeModule color =
            ps.colorOverLifetime;

        color.enabled = true;

        Gradient gradient =
            new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(
                    new Color(
                        1f,
                        0.8f,
                        0.2f
                    ),
                    0f
                ),

                new GradientColorKey(
                    new Color(
                        0.85f,
                        0.08f,
                        0.01f
                    ),
                    1f
                )
            },

            new GradientAlphaKey[]
            {
                new GradientAlphaKey(
                    1f,
                    0f
                ),

                new GradientAlphaKey(
                    0f,
                    1f
                )
            }
        );

        color.color =
            new ParticleSystem.MinMaxGradient(
                gradient
            );

        // Size over lifetime
        ParticleSystem.SizeOverLifetimeModule size =
            ps.sizeOverLifetime;

        size.enabled = true;

        AnimationCurve sizeCurve =
            new AnimationCurve(
                new Keyframe(
                    0f,
                    1.4f
                ),
                new Keyframe(
                    1f,
                    0f
                )
            );

        size.size =
            new ParticleSystem.MinMaxCurve(
                1f,
                sizeCurve
            );

        ps.Play();

        // ========================================================
        // EXPLOSION LIGHT
        // ========================================================

        GameObject lightObject =
            new GameObject(
                "ExplosionLight"
            );

        lightObject.transform.SetParent(
            root.transform
        );

        lightObject.transform.localPosition =
            Vector3.zero;

        Light light =
            lightObject.AddComponent<Light>();

        light.type =
            LightType.Point;

        light.color =
            new Color(
                1f,
                0.24f,
                0.04f
            );

        light.range = 16f;
        light.intensity = 12f;
        light.shadows = LightShadows.None;

        // ========================================================
        // ANIMATOR
        // ========================================================

        ExplosionAnimator animator =
            root.AddComponent<
                ExplosionAnimator
            >();

        animator.flash =
            flash.transform;

        animator.light =
            light;

        Object.Destroy(
            root,
            2f
        );
    }
}

public class ExplosionAnimator : MonoBehaviour
{
    public Transform flash;
    public Light light;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                timer / 0.65f
            );

        // Expand flash
        if (flash != null)
        {
            float size =
                Mathf.Lerp(
                    0.6f,
                    5.5f,
                    t
                );

            flash.localScale =
                Vector3.one * size;
        }

        // Fade light
        if (light != null)
        {
            light.intensity =
                Mathf.Lerp(
                    12f,
                    0f,
                    t
                );
        }
    }
}
