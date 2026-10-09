using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class BuildTrainingGround
{
    const string ROOT = "Assets/GeneratedTrainingGround";

    static Material grassMat;
    static Material dirtMat;
    static Material asphaltMat;
    static Material concreteMat;
    static Material concreteDarkMat;
    static Material metalMat;
    static Material metalDarkMat;
    static Material glassMat;
    static Material roadLineMat;
    static Material warningMat;
    static Material rubberMat;

    [MenuItem("26247/BUILD TRAINING GROUND")]
    public static void Build()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode before building.");
            return;
        }

        Debug.Log("========== BUILDING 26247 TRAINING GROUND ==========");

        EnsureFolders();
        CreateMaterials();

        // ----------------------------------------------------
        // REMOVE OLD GENERATED MAP
        // ----------------------------------------------------

        GameObject old = GameObject.Find("TrainingGround");

        if (old != null)
            Object.DestroyImmediate(old);

        // ----------------------------------------------------
        // ROOT
        // ----------------------------------------------------

        GameObject root = new GameObject("TrainingGround");

        GameObject environment = CreateEmpty("Environment", root.transform);
        GameObject ground = CreateEmpty("Ground", environment.transform);
        GameObject roads = CreateEmpty("Roads", environment.transform);
        GameObject walls = CreateEmpty("Walls", environment.transform);
        GameObject buildings = CreateEmpty("Buildings", environment.transform);
        GameObject containers = CreateEmpty("Containers", environment.transform);
        GameObject barriers = CreateEmpty("Barriers", environment.transform);
        GameObject fence = CreateEmpty("Fence", environment.transform);
        GameObject tower = CreateEmpty("WatchTower", environment.transform);
        GameObject props = CreateEmpty("TrainingProps", environment.transform);

        // ----------------------------------------------------
        // GROUND
        // ----------------------------------------------------

        CreateBlock(
            "Ground",
            new Vector3(0f, -0.25f, 0f),
            new Vector3(220f, 0.5f, 180f),
            grassMat,
            ground.transform
        );

        CreateBlock(
            "DirtTrainingArea",
            new Vector3(-55f, 0.04f, 38f),
            new Vector3(55f, 0.08f, 34f),
            dirtMat,
            ground.transform
        );

        CreateBlock(
            "DirtVehicleArea",
            new Vector3(62f, 0.04f, -55f),
            new Vector3(45f, 0.08f, 30f),
            dirtMat,
            ground.transform
        );

        // ----------------------------------------------------
        // ROADS
        // ----------------------------------------------------

        CreateRoads(roads.transform);

        // ----------------------------------------------------
        // WALLS
        // ----------------------------------------------------

        CreateWalls(walls.transform);

        // ----------------------------------------------------
        // BUILDINGS
        // ----------------------------------------------------

        CreateBuilding(
            "CommandBuilding",
            new Vector3(68f, 0f, 54f),
            new Vector3(34f, 14f, 26f),
            buildings.transform
        );

        CreateBuilding(
            "OperationsBuilding",
            new Vector3(-70f, 0f, -48f),
            new Vector3(32f, 12f, 24f),
            buildings.transform
        );

        CreateBuilding(
            "TrainingBuilding",
            new Vector3(62f, 0f, 5f),
            new Vector3(28f, 10f, 20f),
            buildings.transform
        );

        // ----------------------------------------------------
        // CONTAINERS
        // ----------------------------------------------------

        CreateContainer(
            "Container_A",
            new Vector3(-28f, 1.3f, 55f),
            0f,
            containers.transform
        );

        CreateContainer(
            "Container_B",
            new Vector3(-28f, 1.3f, 68f),
            0f,
            containers.transform
        );

        CreateContainer(
            "Container_C",
            new Vector3(-10f, 1.3f, 61f),
            90f,
            containers.transform
        );

        // ----------------------------------------------------
        // BARRIERS
        // ----------------------------------------------------

        CreateBarrierLine(
            new Vector3(-2f, 0f, -33f),
            7,
            5f,
            barriers.transform
        );

        CreateBarrierLine(
            new Vector3(35f, 0f, 23f),
            6,
            5f,
            barriers.transform
        );

        CreateCoverBlock(
            "Cover_A",
            new Vector3(-35f, 1.2f, 5f),
            new Vector3(8f, 2.4f, 1.2f),
            barriers.transform
        );

        CreateCoverBlock(
            "Cover_B",
            new Vector3(-35f, 1.2f, 12f),
            new Vector3(4f, 2.4f, 1.2f),
            barriers.transform
        );

        CreateCoverBlock(
            "Cover_C",
            new Vector3(18f, 1.2f, -5f),
            new Vector3(10f, 2.4f, 1.2f),
            barriers.transform
        );

        // ----------------------------------------------------
        // FENCES
        // ----------------------------------------------------

        CreateFence(
            new Vector3(-105f, 0f, -88f),
            new Vector3(0f, 0f, 176f),
            fence.transform
        );

        CreateFence(
            new Vector3(105f, 0f, -88f),
            new Vector3(0f, 0f, 176f),
            fence.transform
        );

        // ----------------------------------------------------
        // WATCH TOWER
        // ----------------------------------------------------

        CreateWatchTower(
            new Vector3(-82f, 0f, 72f),
            tower.transform
        );

        // ----------------------------------------------------
        // TRAINING PROPS
        // ----------------------------------------------------

        CreateTrainingProps(props.transform);

        // ----------------------------------------------------
        // LIGHTING
        // ----------------------------------------------------

        CreateLighting(root.transform);

        // ----------------------------------------------------
        // CAMERA
        // ----------------------------------------------------

        CreateCamera(root.transform);

        // ----------------------------------------------------
        // SAVE
        // ----------------------------------------------------

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;

        Debug.Log("========== TRAINING GROUND COMPLETE ==========");
    }

    // ============================================================
    // FOLDERS
    // ============================================================

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(ROOT))
            AssetDatabase.CreateFolder(
                "Assets",
                "GeneratedTrainingGround"
            );

        if (!AssetDatabase.IsValidFolder(ROOT + "/Materials"))
            AssetDatabase.CreateFolder(
                ROOT,
                "Materials"
            );

        if (!AssetDatabase.IsValidFolder(ROOT + "/Textures"))
            AssetDatabase.CreateFolder(
                ROOT,
                "Textures"
            );
    }

    // ============================================================
    // MATERIALS
    // ============================================================

    static void CreateMaterials()
    {
        grassMat = MakeMaterial(
            "Grass",
            new Color(0.20f, 0.29f, 0.13f),
            0f,
            0.15f,
            8f
        );

        dirtMat = MakeMaterial(
            "Dirt",
            new Color(0.25f, 0.16f, 0.09f),
            0f,
            0.1f,
            6f
        );

        asphaltMat = MakeMaterial(
            "Asphalt",
            new Color(0.055f, 0.06f, 0.065f),
            0f,
            0.18f,
            10f
        );

        concreteMat = MakeMaterial(
            "Concrete",
            new Color(0.46f, 0.47f, 0.44f),
            0f,
            0.25f,
            5f
        );

        concreteDarkMat = MakeMaterial(
            "ConcreteDark",
            new Color(0.24f, 0.25f, 0.24f),
            0f,
            0.2f,
            5f
        );

        metalMat = MakeMaterial(
            "MilitaryMetal",
            new Color(0.19f, 0.23f, 0.17f),
            0.65f,
            0.3f,
            4f
        );

        metalDarkMat = MakeMaterial(
            "DarkMetal",
            new Color(0.055f, 0.06f, 0.055f),
            0.8f,
            0.2f,
            3f
        );

        glassMat = MakeMaterial(
            "Glass",
            new Color(0.025f, 0.075f, 0.10f),
            0.2f,
            0.7f,
            1f
        );

        roadLineMat = MakeSolidMaterial(
            "RoadMarkings",
            new Color(0.80f, 0.76f, 0.58f),
            0f,
            0.3f
        );

        warningMat = MakeSolidMaterial(
            "Warning",
            new Color(0.7f, 0.4f, 0.05f),
            0f,
            0.25f
        );

        rubberMat = MakeSolidMaterial(
            "Rubber",
            new Color(0.02f, 0.02f, 0.018f),
            0f,
            0.08f
        );
    }

    static Material MakeMaterial(
        string name,
        Color color,
        float metallic,
        float smoothness,
        float tiling
    )
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        string path =
            ROOT + "/Materials/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = name;

            AssetDatabase.CreateAsset(
                mat,
                path
            );
        }

        SetColor(mat, color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        Texture2D tex =
            CreateNoiseTexture(
                name + "_Texture",
                color
            );

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture(
                "_BaseMap",
                tex
            );

            mat.SetTextureScale(
                "_BaseMap",
                new Vector2(tiling, tiling)
            );
        }
        else if (mat.HasProperty("_MainTex"))
        {
            mat.SetTexture(
                "_MainTex",
                tex
            );

            mat.SetTextureScale(
                "_MainTex",
                new Vector2(tiling, tiling)
            );
        }

        EditorUtility.SetDirty(mat);

        return mat;
    }

    static Material MakeSolidMaterial(
        string name,
        Color color,
        float metallic,
        float smoothness
    )
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        string path =
            ROOT + "/Materials/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = name;

            AssetDatabase.CreateAsset(
                mat,
                path
            );
        }

        SetColor(mat, color);

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        EditorUtility.SetDirty(mat);

        return mat;
    }

    static void SetColor(
        Material mat,
        Color color
    )
    {
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
    }

    // ============================================================
    // TEXTURE
    // ============================================================

    static Texture2D CreateNoiseTexture(
        string name,
        Color baseColor
    )
    {
        string path =
            ROOT + "/Textures/" + name + ".asset";

        Texture2D existing =
            AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (existing != null)
            return existing;

        const int size = 128;

        Texture2D tex =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                true
            );

        tex.wrapMode =
            TextureWrapMode.Repeat;

        tex.filterMode =
            FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = (float)x / size;
                float fy = (float)y / size;

                float n1 =
                    Mathf.PerlinNoise(
                        fx * 7f,
                        fy * 7f
                    );

                float n2 =
                    Mathf.PerlinNoise(
                        fx * 20f + 30f,
                        fy * 20f + 15f
                    );

                float n =
                    n1 * 0.75f +
                    n2 * 0.25f;

                float brightness =
                    Mathf.Lerp(
                        0.72f,
                        1.12f,
                        n
                    );

                Color c =
                    baseColor * brightness;

                c.a = 1f;

                tex.SetPixel(
                    x,
                    y,
                    c
                );
            }
        }

        tex.Apply();

        AssetDatabase.CreateAsset(
            tex,
            path
        );

        return tex;
    }

    // ============================================================
    // ROADS
    // ============================================================

    static void CreateRoads(
        Transform parent
    )
    {
        CreateBlock(
            "MainRoad",
            new Vector3(0f, 0.02f, -2f),
            new Vector3(210f, 0.08f, 13f),
            asphaltMat,
            parent
        );

        CreateBlock(
            "SecondaryRoad",
            new Vector3(15f, 0.03f, 48f),
            new Vector3(12f, 0.08f, 95f),
            asphaltMat,
            parent
        );

        // Horizontal lane markings
        for (float x = -96f; x <= 96f; x += 12f)
        {
            CreateBlock(
                "LaneMark",
                new Vector3(x, 0.08f, -2f),
                new Vector3(5.5f, 0.025f, 0.20f),
                roadLineMat,
                parent
            );
        }

        // Vertical lane markings
        for (float z = 5f; z <= 92f; z += 12f)
        {
            CreateBlock(
                "LaneMark",
                new Vector3(15f, 0.09f, z),
                new Vector3(0.20f, 0.025f, 5.5f),
                roadLineMat,
                parent
            );
        }

        // Curbs
        CreateBlock(
            "CurbNorth",
            new Vector3(0f, 0.23f, 6.9f),
            new Vector3(210f, 0.4f, 0.45f),
            concreteDarkMat,
            parent
        );

        CreateBlock(
            "CurbSouth",
            new Vector3(0f, 0.23f, -10.9f),
            new Vector3(210f, 0.4f, 0.45f),
            concreteDarkMat,
            parent
        );
    }

    // ============================================================
    // WALLS
    // ============================================================

    static void CreateWalls(
        Transform parent
    )
    {
        CreateBlock(
            "NorthWall",
            new Vector3(0f, 2f, 88f),
            new Vector3(210f, 4f, 1.2f),
            concreteMat,
            parent
        );

        CreateBlock(
            "SouthWall",
            new Vector3(0f, 2f, -88f),
            new Vector3(210f, 4f, 1.2f),
            concreteMat,
            parent
        );

        CreateBlock(
            "WestWall",
            new Vector3(-104f, 2f, 0f),
            new Vector3(1.2f, 4f, 176f),
            concreteMat,
            parent
        );

        CreateBlock(
            "EastWall",
            new Vector3(104f, 2f, 0f),
            new Vector3(1.2f, 4f, 176f),
            concreteMat,
            parent
        );

        // Internal cover walls
        CreateBlock(
            "InternalWallA",
            new Vector3(-48f, 2f, -25f),
            new Vector3(30f, 4f, 1f),
            concreteMat,
            parent
        );

        CreateBlock(
            "InternalWallB",
            new Vector3(-12f, 2f, 28f),
            new Vector3(1f, 4f, 30f),
            concreteMat,
            parent
        );

        CreateBlock(
            "InternalWallC",
            new Vector3(44f, 2f, 38f),
            new Vector3(1f, 4f, 24f),
            concreteMat,
            parent
        );
    }

    // ============================================================
    // BUILDINGS
    // ============================================================

    static void CreateBuilding(
        string name,
        Vector3 position,
        Vector3 size,
        Transform parent
    )
    {
        GameObject root =
            new GameObject(name);

        root.transform.position =
            position;

        root.transform.SetParent(
            parent
        );

        float w = size.x;
        float h = size.y;
        float d = size.z;

        CreateBlock(
            "BuildingBody",
            position +
            new Vector3(
                0f,
                h * 0.5f,
                0f
            ),
            new Vector3(w, h, d),
            concreteMat,
            root.transform
        );

        CreateBlock(
            "Roof",
            position +
            new Vector3(
                0f,
                h + 0.35f,
                0f
            ),
            new Vector3(
                w + 1f,
                0.7f,
                d + 1f
            ),
            concreteDarkMat,
            root.transform
        );

        // Door
        CreateBlock(
            "Door",
            position +
            new Vector3(
                0f,
                2.2f,
                -d * 0.506f
            ),
            new Vector3(
                3.2f,
                4.4f,
                0.15f
            ),
            metalDarkMat,
            root.transform
        );

        // Front windows
        float[] xs =
        {
            -w * 0.32f,
            0f,
            w * 0.32f
        };

        foreach (float x in xs)
        {
            CreateBlock(
                "Window",
                position +
                new Vector3(
                    x,
                    h * 0.56f,
                    -d * 0.507f
                ),
                new Vector3(
                    4.2f,
                    2.8f,
                    0.12f
                ),
                glassMat,
                root.transform
            );
        }

        // Side windows
        CreateBlock(
            "SideWindow",
            position +
            new Vector3(
                w * 0.507f,
                h * 0.55f,
                -d * 0.25f
            ),
            new Vector3(
                0.12f,
                2.8f,
                4f
            ),
            glassMat,
            root.transform
        );

        CreateBlock(
            "SideWindow",
            position +
            new Vector3(
                w * 0.507f,
                h * 0.55f,
                d * 0.25f
            ),
            new Vector3(
                0.12f,
                2.8f,
                4f
            ),
            glassMat,
            root.transform
        );

        // Rooftop equipment
        CreateBlock(
            "RooftopEquipment",
            position +
            new Vector3(
                -w * 0.28f,
                h + 1.3f,
                0f
            ),
            new Vector3(
                5f,
                2f,
                4f
            ),
            metalMat,
            root.transform
        );

        MarkStatic(root);
    }

    // ============================================================
    // CONTAINERS
    // ============================================================

    static void CreateContainer(
        string name,
        Vector3 position,
        float rotation,
        Transform parent
    )
    {
        GameObject root =
            new GameObject(name);

        root.transform.position =
            position;

        root.transform.rotation =
            Quaternion.Euler(
                0f,
                rotation,
                0f
            );

        root.transform.SetParent(
            parent
        );

        // Container body
        CreateBlock(
            "Body",
            position,
            new Vector3(
                12f,
                2.6f,
                2.6f
            ),
            metalMat,
            root.transform
        );

        // Roof
        CreateBlock(
            "Roof",
            position +
            Vector3.up * 1.4f,
            new Vector3(
                12.2f,
                0.15f,
                2.8f
            ),
            metalDarkMat,
            root.transform
        );

        // Door
        CreateBlock(
            "RearDoor",
            position +
            new Vector3(
                6.08f,
                0f,
                0f
            ),
            new Vector3(
                0.12f,
                2.3f,
                2.4f
            ),
            metalDarkMat,
            root.transform
        );

        // Vertical ribs
        for (int i = 0; i < 8; i++)
        {
            float x =
                -5.2f + i * 1.5f;

            CreateBlock(
                "Rib",
                position +
                new Vector3(
                    x,
                    0f,
                    -1.34f
                ),
                new Vector3(
                    0.10f,
                    2.5f,
                    0.10f
                ),
                metalDarkMat,
                root.transform
            );
        }

        MarkStatic(root);
    }

    // ============================================================
    // BARRIERS
    // ============================================================

    static void CreateBarrierLine(
        Vector3 start,
        int count,
        float spacing,
        Transform parent
    )
    {
        for (int i = 0; i < count; i++)
        {
            CreateCoverBlock(
                "ConcreteBarrier",
                start +
                new Vector3(
                    i * spacing,
                    1f,
                    0f
                ),
                new Vector3(
                    3.8f,
                    2f,
                    1.3f
                ),
                parent
            );
        }
    }

    static void CreateCoverBlock(
        string name,
        Vector3 position,
        Vector3 scale,
        Transform parent
    )
    {
        CreateBlock(
            name,
            position,
            scale,
            concreteMat,
            parent
        );
    }

    // ============================================================
    // FENCE
    // ============================================================

    static void CreateFence(
        Vector3 start,
        Vector3 direction,
        Transform parent
    )
    {
        float length =
            direction.magnitude;

        Vector3 dir =
            direction.normalized;

        int posts =
            Mathf.RoundToInt(
                length / 5f
            );

        for (int i = 0; i <= posts; i++)
        {
            Vector3 p =
                start +
                dir * (i * 5f);

            CreateBlock(
                "FencePost",
                p +
                Vector3.up * 3f,
                new Vector3(
                    0.28f,
                    6f,
                    0.28f
                ),
                metalDarkMat,
                parent
            );
        }

        for (int rail = 0; rail < 3; rail++)
        {
            float y =
                1.2f +
                rail * 2f;

            Vector3 midpoint =
                start +
                dir * (length * 0.5f);

            Vector3 scale;

            if (Mathf.Abs(dir.x) >
                Mathf.Abs(dir.z))
            {
                scale =
                    new Vector3(
                        length,
                        0.16f,
                        0.16f
                    );
            }
            else
            {
                scale =
                    new Vector3(
                        0.16f,
                        0.16f,
                        length
                    );
            }

            CreateBlock(
                "FenceRail",
                midpoint +
                Vector3.up * y,
                scale,
                metalDarkMat,
                parent
            );
        }

        // vertical wires / mesh supports
        for (int i = 0; i < posts * 2; i++)
        {
            Vector3 p =
                start +
                dir *
                (i *
                (length /
                (posts * 2f)));

            CreateBlock(
                "FenceVertical",
                p +
                Vector3.up * 3f,
                new Vector3(
                    0.07f,
                    5.5f,
                    0.07f
                ),
                metalDarkMat,
                parent
            );
        }
    }

    // ============================================================
    // WATCH TOWER
    // ============================================================

    static void CreateWatchTower(
        Vector3 position,
        Transform parent
    )
    {
        GameObject root =
            new GameObject(
                "WatchTower"
            );

        root.transform.position =
            position;

        root.transform.SetParent(
            parent
        );

        Vector3[] legs =
        {
            new Vector3(-2.8f, 4.5f, -2.8f),
            new Vector3( 2.8f, 4.5f, -2.8f),
            new Vector3(-2.8f, 4.5f,  2.8f),
            new Vector3( 2.8f, 4.5f,  2.8f)
        };

        foreach (Vector3 leg in legs)
        {
            CreateBlock(
                "TowerLeg",
                position + leg,
                new Vector3(
                    0.55f,
                    9f,
                    0.55f
                ),
                metalMat,
                root.transform
            );
        }

        CreateBlock(
            "TowerPlatform",
            position +
            Vector3.up * 9f,
            new Vector3(
                8f,
                0.5f,
                8f
            ),
            metalMat,
            root.transform
        );

        // Cabin
        CreateBlock(
            "Cabin",
            position +
            Vector3.up * 11.8f,
            new Vector3(
                5.5f,
                3f,
                5.5f
            ),
            metalMat,
            root.transform
        );

        // Windows
        CreateBlock(
            "FrontWindow",
            position +
            new Vector3(
                0f,
                12f,
                -2.8f
            ),
            new Vector3(
                3.7f,
                1.7f,
                0.12f
            ),
            glassMat,
            root.transform
        );

        CreateBlock(
            "SideWindow",
            position +
            new Vector3(
                2.8f,
                12f,
                0f
            ),
            new Vector3(
                0.12f,
                1.7f,
                3.7f
            ),
            glassMat,
            root.transform
        );

        // Roof
        CreateBlock(
            "TowerRoof",
            position +
            new Vector3(
                0f,
                13.4f,
                0f
            ),
            new Vector3(
                6.5f,
                0.45f,
                6.5f
            ),
            metalDarkMat,
            root.transform
        );

        // Rails
        CreateBlock(
            "RailFront",
            position +
            new Vector3(
                0f,
                11f,
                -3.8f
            ),
            new Vector3(
                8f,
                4f,
                0.2f
            ),
            metalDarkMat,
            root.transform
        );

        CreateBlock(
            "RailBack",
            position +
            new Vector3(
                0f,
                11f,
                3.8f
            ),
            new Vector3(
                8f,
                4f,
                0.2f
            ),
            metalDarkMat,
            root.transform
        );

        CreateBlock(
            "RailLeft",
            position +
            new Vector3(
                -3.8f,
                11f,
                0f
            ),
            new Vector3(
                0.2f,
                4f,
                8f
            ),
            metalDarkMat,
            root.transform
        );

        CreateBlock(
            "RailRight",
            position +
            new Vector3(
                3.8f,
                11f,
                0f
            ),
            new Vector3(
                0.2f,
                4f,
                8f
            ),
            metalDarkMat,
            root.transform
        );

        // Ladder
        CreateBlock(
            "Ladder",
            position +
            new Vector3(
                2.1f,
                4.5f,
                -3.2f
            ),
            new Vector3(
                0.18f,
                9f,
                2f
            ),
            metalDarkMat,
            root.transform
        );

        MarkStatic(root);
    }

    // ============================================================
    // PROPS
    // ============================================================

    static void CreateTrainingProps(
        Transform parent
    )
    {
        // Tire stack
        for (int i = 0; i < 5; i++)
        {
            GameObject tire =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );

            tire.name =
                "TrainingTire";

            tire.transform.position =
                new Vector3(
                    -72f + i * 2.1f,
                    0.75f,
                    18f
                );

            tire.transform.localScale =
                new Vector3(
                    1.5f,
                    0.35f,
                    1.5f
                );

            tire.transform.rotation =
                Quaternion.Euler(
                    90f,
                    0f,
                    0f
                );

            tire.GetComponent<Renderer>()
                .sharedMaterial =
                rubberMat;

            tire.transform.SetParent(
                parent
            );

            MarkStatic(tire);
        }

        // Warning posts
        for (int i = 0; i < 4; i++)
        {
            CreateBlock(
                "WarningPost",
                new Vector3(
                    -15f + i * 10f,
                    1.5f,
                    70f
                ),
                new Vector3(
                    0.25f,
                    3f,
                    0.25f
                ),
                warningMat,
                parent
            );
        }

        // Drone launch / landing pad
        CreateBlock(
            "DroneTrainingPad",
            new Vector3(
                20f,
                0.10f,
                58f
            ),
            new Vector3(
                24f,
                0.18f,
                24f
            ),
            concreteDarkMat,
            parent
        );

        // Pad border
        CreateBlock(
            "PadBorderNorth",
            new Vector3(
                20f,
                0.22f,
                69.5f
            ),
            new Vector3(
                24f,
                0.05f,
                0.3f
            ),
            roadLineMat,
            parent
        );

        CreateBlock(
            "PadBorderSouth",
            new Vector3(
                20f,
                0.22f,
                46.5f
            ),
            new Vector3(
                24f,
                0.05f,
                0.3f
            ),
            roadLineMat,
            parent
        );

        CreateBlock(
            "PadBorderEast",
            new Vector3(
                31.5f,
                0.22f,
                58f
            ),
            new Vector3(
                0.3f,
                0.05f,
                24f
            ),
            roadLineMat,
            parent
        );

        CreateBlock(
            "PadBorderWest",
            new Vector3(
                8.5f,
                0.22f,
                58f
            ),
            new Vector3(
                0.3f,
                0.05f,
                24f
            ),
            roadLineMat,
            parent
        );
    }

    // ============================================================
    // LIGHTING
    // ============================================================

    static void CreateLighting(
        Transform parent
    )
    {
        Light[] lights =
            Object.FindObjectsByType<Light>(
                FindObjectsSortMode.None
            );

        foreach (Light l in lights)
        {
            if (l != null)
                Object.DestroyImmediate(
                    l.gameObject
                );
        }

        // Sun
        GameObject sunObj =
            new GameObject("Sun");

        sunObj.transform.rotation =
            Quaternion.Euler(
                48f,
                -35f,
                0f
            );

        sunObj.transform.SetParent(
            parent
        );

        Light sun =
            sunObj.AddComponent<Light>();

        sun.type =
            LightType.Directional;

        sun.intensity =
            1.25f;

        sun.shadows =
            LightShadows.Soft;

        sun.shadowStrength =
            0.85f;

        sun.shadowBias =
            0.05f;

        RenderSettings.sun =
            sun;

        // Ambient
        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Trilight;

        RenderSettings.ambientIntensity =
            0.75f;

        // Fog
        RenderSettings.fog =
            true;

        RenderSettings.fogMode =
            FogMode.ExponentialSquared;

        RenderSettings.fogDensity =
            0.0025f;

        RenderSettings.fogColor =
            new Color(
                0.43f,
                0.48f,
                0.45f
            );

        // Skybox
        Shader skyShader =
            Shader.Find("Skybox/Procedural");

        if (skyShader != null)
        {
            Material sky =
                new Material(
                    skyShader
                );

            sky.name =
                "TrainingGroundSky";

            RenderSettings.skybox =
                sky;

            RenderSettings.reflectionIntensity =
                0.8f;
        }
    }

    // ============================================================
    // CAMERA
    // ============================================================

    static void CreateCamera(
        Transform parent
    )
    {
        Camera cam =
            Camera.main;

        if (cam == null)
        {
            GameObject cameraObj =
                new GameObject(
                    "Main Camera"
                );

            cameraObj.tag =
                "MainCamera";

            cam =
                cameraObj.AddComponent<Camera>();

            cameraObj.AddComponent<AudioListener>();
        }

        cam.transform.position =
            new Vector3(
                -82f,
                8f,
                -70f
            );

        cam.transform.rotation =
            Quaternion.Euler(
                12f,
                38f,
                0f
            );

        cam.fieldOfView =
            70f;

        cam.nearClipPlane =
            0.05f;

        cam.farClipPlane =
            500f;

        cam.transform.SetParent(
            parent
        );
    }

    // ============================================================
    // GENERIC BLOCK
    // ============================================================

    static GameObject CreateBlock(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent
    )
    {
        GameObject go =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        go.name =
            name;

        go.transform.position =
            position;

        go.transform.localScale =
            scale;

        if (material != null)
        {
            Renderer renderer =
                go.GetComponent<Renderer>();

            renderer.sharedMaterial =
                material;
        }

        go.transform.SetParent(
            parent
        );

        MarkStatic(go);

        return go;
    }

    static GameObject CreateEmpty(
        string name,
        Transform parent
    )
    {
        GameObject go =
            new GameObject(name);

        go.transform.SetParent(
            parent
        );

        return go;
    }

    static void MarkStatic(
        GameObject go
    )
    {
        go.isStatic = true;
    }
}
