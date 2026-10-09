using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AddMountainsAndClouds
{
    const string ROOT = "Assets/GeneratedTrainingGround";

    [MenuItem("26247/ADD MOUNTAINS + CLOUDS")]
    public static void Build()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode first.");
            return;
        }

        GameObject old =
            GameObject.Find("SceneryBackdrop");

        if (old != null)
            Object.DestroyImmediate(old);

        GameObject root =
            new GameObject("SceneryBackdrop");

        // ========================================================
        // MOUNTAIN BACKDROP
        // ========================================================

        CreateMountainBackdrop(
            root.transform,
            new Vector3(0f, 0f, 120f),
            220f,
            55f,
            90f
        );

        CreateMountainBackdrop(
            root.transform,
            new Vector3(-150f, 0f, 45f),
            120f,
            45f,
            70f
        );

        CreateMountainBackdrop(
            root.transform,
            new Vector3(150f, 0f, 45f),
            120f,
            45f,
            70f
        );

        // ========================================================
        // CLOUDS
        // ========================================================

        CreateCloudLayer(
            root.transform,
            new Vector3(0f, 32f, 40f)
        );

        CreateCloudLayer(
            root.transform,
            new Vector3(-70f, 38f, 95f)
        );

        CreateCloudLayer(
            root.transform,
            new Vector3(75f, 35f, 120f)
        );

        CreateCloudLayer(
            root.transform,
            new Vector3(-130f, 42f, 70f)
        );

        // ========================================================
        // SAVE
        // ========================================================

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        AssetDatabase.SaveAssets();

        Selection.activeGameObject =
            root;

        Debug.Log(
            "=== MOUNTAINS + CLOUDS ADDED ==="
        );
    }

    // ============================================================
    // MOUNTAIN BACKDROP
    // ============================================================

    static void CreateMountainBackdrop(
        Transform parent,
        Vector3 position,
        float width,
        float height,
        float depth
    )
    {
        GameObject mountain =
            new GameObject("MountainBackdrop");

        mountain.transform.position =
            position;

        mountain.transform.SetParent(
            parent
        );

        MeshFilter filter =
            mountain.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            mountain.AddComponent<MeshRenderer>();

        Mesh mesh =
            new Mesh();

        mesh.name =
            "StaticMountainMesh";

        int segments = 32;

        Vector3[] vertices =
            new Vector3[
                (segments + 1) * 2
            ];

        int[] triangles =
            new int[
                segments * 6
            ];

        // Mountain profile
        float[] heights =
            new float[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t =
                (float)i / segments;

            float x =
                Mathf.Lerp(
                    -width * 0.5f,
                    width * 0.5f,
                    t
                );

            float n1 =
                Mathf.PerlinNoise(
                    t * 3.0f,
                    0.15f
                );

            float n2 =
                Mathf.PerlinNoise(
                    t * 7.0f + 10f,
                    0.4f
                );

            // Several mountain peaks
            float peak1 =
                Mathf.Exp(
                    -Mathf.Pow(
                        (t - 0.22f) / 0.12f,
                        2f
                    )
                );

            float peak2 =
                Mathf.Exp(
                    -Mathf.Pow(
                        (t - 0.51f) / 0.16f,
                        2f
                    )
                );

            float peak3 =
                Mathf.Exp(
                    -Mathf.Pow(
                        (t - 0.79f) / 0.11f,
                        2f
                    )
                );

            float h =
                0.20f +
                n1 * 0.22f +
                n2 * 0.08f +
                peak1 * 0.48f +
                peak2 * 0.65f +
                peak3 * 0.42f;

            heights[i] =
                h * height;

            vertices[i] =
                new Vector3(
                    x,
                    0f,
                    0f
                );

            vertices[
                segments + 1 + i
            ] =
                new Vector3(
                    x,
                    heights[i],
                    0f
                );
        }

        for (int i = 0; i < segments; i++)
        {
            int a = i;
            int b = i + 1;

            int c =
                segments + 1 + i;

            int d =
                segments + 1 + i + 1;

            int ti =
                i * 6;

            triangles[ti + 0] = a;
            triangles[ti + 1] = c;
            triangles[ti + 2] = b;

            triangles[ti + 3] = b;
            triangles[ti + 4] = c;
            triangles[ti + 5] = d;
        }

        mesh.vertices =
            vertices;

        mesh.triangles =
            triangles;

        mesh.RecalculateNormals();

        filter.sharedMesh =
            mesh;

        // --------------------------------------------------------
        // Mountain material
        // --------------------------------------------------------

        Material mat =
            CreateMountainMaterial();

        renderer.sharedMaterial =
            mat;

        mountain.isStatic =
            true;
    }

    // ============================================================
    // MOUNTAIN MATERIAL
    // ============================================================

    static Material CreateMountainMaterial()
    {
        string path =
            ROOT +
            "/Materials/MountainBackdrop.mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(
                path
            );

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
            shader =
                Shader.Find(
                    "Unlit/Color"
                );

        if (mat == null)
        {
            mat =
                new Material(shader);

            mat.name =
                "MountainBackdrop";

            AssetDatabase.CreateAsset(
                mat,
                path
            );
        }

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor(
                "_BaseColor",
                new Color(
                    0.10f,
                    0.15f,
                    0.105f,
                    1f
                )
            );
        }

        if (mat.HasProperty("_Color"))
        {
            mat.SetColor(
                "_Color",
                new Color(
                    0.10f,
                    0.15f,
                    0.105f,
                    1f
                )
            );
        }

        return mat;
    }

    // ============================================================
    // CLOUD LAYER
    // ============================================================

    static void CreateCloudLayer(
        Transform parent,
        Vector3 center
    )
    {
        GameObject root =
            new GameObject(
                "StaticCloudLayer"
            );

        root.transform.position =
            center;

        root.transform.SetParent(
            parent
        );

        for (int i = 0; i < 18; i++)
        {
            float angle =
                i * 19.4f;

            float radius =
                15f +
                (i % 6) * 8f;

            Vector3 pos =
                center +
                new Vector3(
                    Mathf.Cos(
                        angle *
                        Mathf.Deg2Rad
                    ) * radius,

                    Mathf.Sin(
                        angle *
                        0.7f *
                        Mathf.Deg2Rad
                    ) * 2.5f,

                    Mathf.Sin(
                        angle *
                        Mathf.Deg2Rad
                    ) * radius
                );

            CreateCloud(
                root.transform,
                pos,
                5f +
                (i % 5) * 2f
            );
        }
    }

    // ============================================================
    // CLOUD PUFF
    // ============================================================

    static void CreateCloud(
        Transform parent,
        Vector3 position,
        float size
    )
    {
        GameObject cloud =
            new GameObject(
                "Cloud"
            );

        cloud.transform.position =
            position;

        cloud.transform.SetParent(
            parent
        );

        MeshFilter filter =
            cloud.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            cloud.AddComponent<MeshRenderer>();

        Mesh mesh =
            CreateQuadMesh();

        filter.sharedMesh =
            mesh;

        Material mat =
            CreateCloudMaterial();

        renderer.sharedMaterial =
            mat;

        cloud.transform.localScale =
            new Vector3(
                size * 1.8f,
                size * 0.5f,
                1f
            );

        // Face the camera approximately
        cloud.transform.rotation =
            Quaternion.Euler(
                0f,
                180f,
                0f
            );

        cloud.isStatic =
            true;
    }

    // ============================================================
    // CLOUD MATERIAL
    // ============================================================

    static Material CreateCloudMaterial()
    {
        string path =
            ROOT +
            "/Materials/StaticCloud.mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(
                path
            );

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
            shader =
                Shader.Find(
                    "Unlit/Transparent"
                );

        if (mat == null)
        {
            mat =
                new Material(shader);

            mat.name =
                "StaticCloud";

            AssetDatabase.CreateAsset(
                mat,
                path
            );
        }

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor(
                "_BaseColor",
                new Color(
                    0.82f,
                    0.86f,
                    0.82f,
                    0.42f
                )
            );
        }

        if (mat.HasProperty("_Color"))
        {
            mat.SetColor(
                "_Color",
                new Color(
                    0.82f,
                    0.86f,
                    0.82f,
                    0.42f
                )
            );
        }

        // Transparent material
        if (mat.HasProperty(
            "_Surface"
        ))
        {
            mat.SetFloat(
                "_Surface",
                1f
            );

            mat.SetFloat(
                "_Blend",
                0f
            );

            mat.SetFloat(
                "_ZWrite",
                0f
            );

            mat.EnableKeyword(
                "_SURFACE_TYPE_TRANSPARENT"
            );
        }

        return mat;
    }

    // ============================================================
    // QUAD
    // ============================================================

    static Mesh CreateQuadMesh()
    {
        Mesh mesh =
            new Mesh();

        mesh.name =
            "CloudQuad";

        mesh.vertices =
            new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f),
                new Vector3( 0.5f,  0.5f, 0f)
            };

        mesh.uv =
            new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

        mesh.triangles =
            new int[]
            {
                0, 2, 1,
                1, 2, 3
            };

        mesh.RecalculateNormals();

        return mesh;
    }
}
