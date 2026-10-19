using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Replaces the flat silhouette dressing with terrain, vegetation and range vehicles.</summary>
public static class UpgradeRangeSurroundings
{
    const string RootName = "RealisticRangeSurroundingsV2";
    const string OldRootName = "RealisticRangeSurroundings";
    const string LegacyRootName = "CinematicRangeSurroundings";
    const string MaterialFolder = "Assets/GeneratedTrainingGround/Materials";

    [MenuItem("26247/PLAIN CONCRETE GROUND FINISH")]
    static void ApplyGroundFinish()
    {
        if (Application.isPlaying) return;
        Transform groundRoot = GameObject.Find("TrainingGround/Environment/Ground")?.transform;
        Transform roadRoot = GameObject.Find("TrainingGround/Environment/Roads")?.transform;
        if (groundRoot == null || roadRoot == null) return;

        var concrete = GetFlatMaterial("RangeConcreteGround.mat", new Color(0.48f, 0.47f, 0.43f));
        ApplyMaterialToChildren(groundRoot, concrete);
        foreach (Transform road in roadRoot)
        {
            if (road.name == "LaneMark")
            {
                road.gameObject.SetActive(false);
                continue;
            }

            var renderer = road.GetComponent<Renderer>();
            if (renderer == null) continue;
            renderer.sharedMaterial = concrete;
            EditorUtility.SetDirty(renderer);
        }

        // The old organic disks had hard, dark edges. Use the same slab finish so they blend into the yard.
        Transform scenery = GameObject.Find("RealisticRangeSurroundingsV2")?.transform;
        if (scenery != null)
        {
            foreach (Transform item in scenery)
            {
                if (item.name != "NaturalTerrainPatch") continue;
                var renderer = item.GetComponent<Renderer>();
                if (renderer == null) continue;
                renderer.sharedMaterial = concrete;
                EditorUtility.SetDirty(renderer);
            }
        }

        RenderSettings.ambientIntensity = 1.2f;
        RenderSettings.ambientGroundColor = new Color(0.30f, 0.30f, 0.27f);
        if (RenderSettings.sun != null) RenderSettings.sun.shadowStrength = 0.56f;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    static void ApplyMaterialToChildren(Transform root, Material material)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }
    }

    [MenuItem("26247/PLAIN CONCRETE WALL FINISH")]
    static void ApplyWallFinish()
    {
        if (Application.isPlaying) return;
        Transform wallRoot = GameObject.Find("TrainingGround/Environment/Walls")?.transform;
        if (wallRoot == null) return;

        const string overlayName = "ConcretePanelJoints";
        Transform existingOverlay = wallRoot.Find(overlayName);
        if (existingOverlay != null) Object.DestroyImmediate(existingOverlay.gameObject);
        var overlay = new GameObject(overlayName);
        overlay.transform.SetParent(wallRoot, false);

        var concrete = GetPlainConcreteMaterial();
        var joint = GetFlatMaterial("WallJoint.mat", new Color(0.34f, 0.35f, 0.33f));
        foreach (Transform wall in wallRoot)
        {
            if (wall == overlay.transform) continue;
            var renderers = wall.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.sharedMaterial = concrete;
                EditorUtility.SetDirty(renderer);
            }

            if (wall.GetComponent<Renderer>() == null) continue;
            Bounds bounds = wall.GetComponent<Renderer>().bounds;
            bool runsAlongX = bounds.size.x >= bounds.size.z;
            float span = runsAlongX ? bounds.size.x : bounds.size.z;
            int joints = Mathf.FloorToInt(span / 8f);
            for (int i = 1; i < joints; i++)
            {
                float along = -span * 0.5f + span * i / joints;
                Vector3 front = bounds.center + (runsAlongX ? new Vector3(along, 0f, bounds.extents.z + 0.018f) : new Vector3(bounds.extents.x + 0.018f, 0f, along));
                Vector3 back = bounds.center + (runsAlongX ? new Vector3(along, 0f, -bounds.extents.z - 0.018f) : new Vector3(-bounds.extents.x - 0.018f, 0f, along));
                Vector3 faceScale = runsAlongX
                    ? new Vector3(0.045f, bounds.size.y * 0.96f, 0.035f)
                    : new Vector3(0.035f, bounds.size.y * 0.96f, 0.045f);
                CreateJoint(overlay.transform, front, faceScale, joint);
                CreateJoint(overlay.transform, back, faceScale, joint);
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    static void CreateJoint(Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        var seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seam.name = "ConcreteExpansionJoint";
        seam.transform.SetParent(parent, false);
        seam.transform.position = position;
        seam.transform.localScale = scale;
        seam.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(seam.GetComponent<Collider>());
        seam.isStatic = true;
    }

    static Material GetPlainConcreteMaterial()
    {
        const string path = "Assets/GeneratedTrainingGround/Materials/WallConcrete.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = "WallConcrete" };
            AssetDatabase.CreateAsset(material, path);
        }
        SetMaterialColor(material, new Color(0.49f, 0.48f, 0.44f));
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.16f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static Material GetFlatMaterial(string filename, Color color)
    {
        string path = MaterialFolder + "/" + filename;
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = filename.Replace(".mat", "") };
            AssetDatabase.CreateAsset(material, path);
        }
        SetMaterialColor(material, color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.12f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    [MenuItem("26247/UPGRADE RANGE SURROUNDINGS")]
    public static void Apply()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode before upgrading the surroundings.");
            return;
        }

        DestroyNamed(OldRootName);
        DestroyNamed(LegacyRootName);
        DestroyNamed(RootName);
        Transform oldBackdrop = GameObject.Find("SceneryBackdrop")?.transform;
        if (oldBackdrop != null) oldBackdrop.gameObject.SetActive(false);

        var root = new GameObject(RootName);
        root.isStatic = true;
        Material grass = MaterialAsset("NaturalGrass.mat", new Color(0.29f, 0.33f, 0.20f), 0f, 0.12f);
        Material earth = MaterialAsset("NaturalEarth.mat", new Color(0.35f, 0.27f, 0.17f), 0f, 0.08f);
        Material rock = MaterialAsset("NaturalRock.mat", new Color(0.32f, 0.31f, 0.27f), 0f, 0.12f);
        Material olive = MaterialAsset("RangeVehicleOlive.mat", new Color(0.25f, 0.29f, 0.19f), 0.16f, 0.28f);
        Material rubber = MaterialAsset("RangeVehicleRubber.mat", new Color(0.075f, 0.08f, 0.073f), 0.02f, 0.12f);
        Material metal = MaterialAsset("RangeVehicleMetal.mat", new Color(0.42f, 0.40f, 0.33f), 0.42f, 0.4f);
        Material glass = MaterialAsset("RangeVehicleGlass.mat", new Color(0.12f, 0.19f, 0.20f), 0.28f, 0.62f);

        CreateOuterTerrain(root.transform, grass, earth, rock);
        CreateTerrainPatches(root.transform, grass, earth, rock);
        CreateVegetation(root.transform, grass, rock);
        CreateTank(root.transform, new Vector3(-48f, 0f, 52f), 18f, olive, rubber, metal);
        CreateTank(root.transform, new Vector3(73f, 0f, -62f), -12f, olive, rubber, metal);
        CreateRangeProps(root.transform, olive, rubber, metal, glass);
        CreateFieldCamp(root.transform, olive, rubber, metal, glass, earth);
        SetNaturalDaylight();
        BrightenExistingGroundMaterials();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Natural terrain, range vehicles and brighter outdoor lighting added. Press Play to view the updated scene.");
    }

    static void CreateOuterTerrain(Transform parent, Material grass, Material earth, Material rock)
    {
        // Wide low ridges sit beyond the compound, with irregular profiles instead of flat cutout peaks.
        CreateRidge(parent, new Vector3(-12f, 0f, 132f), 220f, 72f, 21f, grass);
        CreateRidge(parent, new Vector3(-142f, 0f, 20f), 70f, 225f, 15f, earth);
        CreateRidge(parent, new Vector3(145f, 0f, 5f), 72f, 235f, 18f, grass);
        CreateRidge(parent, new Vector3(5f, 0f, -128f), 230f, 70f, 13f, rock);
    }

    static void CreateRidge(Transform parent, Vector3 center, float width, float depth, float height, Material material)
    {
        const int xCount = 30, zCount = 18;
        var vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[xCount * zCount * 6];
        for (int z = 0; z <= zCount; z++)
        for (int x = 0; x <= xCount; x++)
        {
            float u = (float)x / xCount, v = (float)z / zCount;
            float nx = u * 2f - 1f, nz = v * 2f - 1f;
            float edge = Mathf.Clamp01(1f - Mathf.Max(nx * nx, nz * nz));
            float noise = Mathf.PerlinNoise(u * 6.7f + 2f, v * 5.3f + 7f);
            float y = height * edge * (0.24f + noise * 0.58f);
            int i = z * (xCount + 1) + x;
            vertices[i] = new Vector3(nx * width * 0.5f, y, nz * depth * 0.5f);
            uv[i] = new Vector2(u * 7f, v * 7f);
        }
        int ti = 0;
        for (int z = 0; z < zCount; z++)
        for (int x = 0; x < xCount; x++)
        {
            int a = z * (xCount + 1) + x, b = a + 1, c = a + xCount + 1, d = c + 1;
            triangles[ti++] = a; triangles[ti++] = c; triangles[ti++] = b;
            triangles[ti++] = b; triangles[ti++] = c; triangles[ti++] = d;
        }
        var mesh = new Mesh { name = "RollingRangeTerrain" };
        mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles; mesh.RecalculateNormals();
        var hill = new GameObject("RollingTerrainRidge");
        hill.transform.SetParent(parent, false); hill.transform.position = center;
        hill.AddComponent<MeshFilter>().sharedMesh = mesh;
        hill.AddComponent<MeshRenderer>().sharedMaterial = material;
        hill.isStatic = true;
    }

    static void CreateTerrainPatches(Transform parent, Material grass, Material earth, Material rock)
    {
        // Organic dirt and gravel clearings add distinct terrain types to open ground at the edges.
        Patch(parent, new Vector3(-84f, 0.012f, 18f), 21f, 13f, earth, 5f);
        Patch(parent, new Vector3(-48f, 0.013f, 68f), 24f, 11f, grass, 17f);
        Patch(parent, new Vector3(-6f, 0.014f, -73f), 23f, 10f, rock, 29f);
        Patch(parent, new Vector3(92f, 0.015f, 8f), 13f, 25f, earth, 41f);
        Patch(parent, new Vector3(-92f, 0.016f, -70f), 14f, 21f, grass, 53f);
        // Bare earth and loose gravel in the existing vehicle drill area.
        Patch(parent, new Vector3(55f, 0.02f, -56f), 25f, 18f, earth, 67f);
        Patch(parent, new Vector3(61f, 0.025f, -57f), 18f, 11f, rock, 79f);
    }

    static void Patch(Transform parent, Vector3 center, float rx, float rz, Material material, float seed)
    {
        const int segments = 32;
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float wobble = 0.84f + Mathf.PerlinNoise(Mathf.Cos(angle) + seed, Mathf.Sin(angle) + seed) * 0.28f;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * rx * wobble, 0f, Mathf.Sin(angle) * rz * wobble);
            int next = (i + 1) % segments;
            triangles[i * 3] = 0; triangles[i * 3 + 1] = next + 1; triangles[i * 3 + 2] = i + 1;
        }
        var mesh = new Mesh { name = "NaturalTerrainPatch" };
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals();
        var patch = new GameObject("NaturalTerrainPatch");
        patch.transform.SetParent(parent, false); patch.transform.position = center;
        patch.AddComponent<MeshFilter>().sharedMesh = mesh;
        patch.AddComponent<MeshRenderer>().sharedMaterial = material;
        patch.isStatic = true;
    }

    static void CreateVegetation(Transform parent, Material foliage, Material rock)
    {
        var random = new System.Random(927);
        for (int i = 0; i < 65; i++)
        {
            float x = RandomRange(random, -102f, 102f), z = RandomRange(random, -82f, 82f);
            if (Mathf.Abs(x) < 38f && Mathf.Abs(z) < 35f) continue; // preserve the central route and sightlines
            if (Mathf.Abs(z + 2f) < 10f || (Mathf.Abs(x - 15f) < 9f && z > 0f)) continue; // leave both paved lanes clear
            if (x > 37f && z < -36f) continue; // keep tank drill area clear
            float radius = RandomRange(random, 0.2f, 0.55f);
            var shrub = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shrub.name = "LowRangeScrub"; shrub.transform.SetParent(parent, false);
            shrub.transform.position = new Vector3(x, radius * 0.55f, z);
            shrub.transform.localScale = new Vector3(radius * 2.2f, radius, radius * 1.8f);
            shrub.GetComponent<Renderer>().sharedMaterial = foliage;
            Object.DestroyImmediate(shrub.GetComponent<Collider>()); shrub.isStatic = true;
        }

        // Scattered weathered stones are kept low so they do not obstruct drone visibility.
        for (int i = 0; i < 38; i++)
        {
            float x = RandomRange(random, -103f, 103f), z = RandomRange(random, -84f, 84f);
            if (Mathf.Abs(x) < 40f && Mathf.Abs(z) < 38f) continue;
            if (Mathf.Abs(z + 2f) < 10f || (Mathf.Abs(x - 15f) < 9f && z > 0f)) continue;
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "WeatheredRangeStone"; stone.transform.SetParent(parent, false);
            stone.transform.position = new Vector3(x, 0.26f, z);
            stone.transform.localScale = new Vector3(RandomRange(random, 0.7f, 1.9f), RandomRange(random, 0.35f, 0.75f), RandomRange(random, 0.6f, 1.5f));
            stone.transform.rotation = Quaternion.Euler(RandomRange(random, -8f, 8f), RandomRange(random, 0f, 360f), RandomRange(random, -8f, 8f));
            stone.GetComponent<Renderer>().sharedMaterial = rock;
            Object.DestroyImmediate(stone.GetComponent<Collider>()); stone.isStatic = true;
        }
    }

    static void CreateTank(Transform parent, Vector3 position, float yaw, Material paint, Material rubber, Material metal)
    {
        var tank = new GameObject("ParkedTrainingTank");
        tank.transform.SetParent(parent, false); tank.transform.position = position;
        tank.transform.rotation = Quaternion.Euler(0f, yaw, 0f); tank.isStatic = true;
        // Layered, sloped armor and a low silhouette give the hull more believable proportions.
        Box(tank.transform, "ArmoredLowerHull", new Vector3(0f, 1.15f, 0f), new Vector3(6.1f, 1.1f, 9.2f), paint);
        BoxRotated(tank.transform, "SlopedFrontGlacis", new Vector3(0f, 1.55f, 4.2f), new Vector3(5.6f, 0.45f, 2.1f), paint, new Vector3(-23f, 0f, 0f));
        BoxRotated(tank.transform, "SlopedRearPlate", new Vector3(0f, 1.5f, -4.3f), new Vector3(5.6f, 0.38f, 1.6f), paint, new Vector3(18f, 0f, 0f));
        Box(tank.transform, "EngineDeck", new Vector3(0f, 1.95f, -2.8f), new Vector3(5.25f, 0.55f, 2.8f), paint);
        for (int i = 0; i < 8; i++)
            Box(tank.transform, "EngineVent", new Vector3(-1.7f + i * 0.48f, 2.24f, -2.85f), new Vector3(0.12f, 0.04f, 1.7f), rubber, RandomRange(new System.Random(i + 10), -5f, 5f));

        // Track runs, separate tread shoes, road wheels, hubs and fasteners.
        for (int side = -1; side <= 1; side += 2)
        {
            Box(tank.transform, "ContinuousTrackBelt", new Vector3(side * 3.25f, 0.67f, 0f), new Vector3(0.68f, 1.1f, 9.55f), rubber);
            for (int i = 0; i < 17; i++)
            {
                float z = -4.45f + i * 0.55f;
                Box(tank.transform, "TrackTreadTop", new Vector3(side * 3.65f, 1.23f, z), new Vector3(0.18f, 0.11f, 0.46f), metal);
                Box(tank.transform, "TrackTreadBottom", new Vector3(side * 3.65f, 0.11f, z), new Vector3(0.18f, 0.11f, 0.46f), metal);
            }
            for (int i = 0; i < 7; i++)
            {
                float z = -3.55f + i * 1.18f;
                Cylinder(tank.transform, "RoadWheelRubber", new Vector3(side * 3.57f, 0.69f, z), new Vector3(0.88f, 0.28f, 0.88f), rubber, 0f, 90f);
                Cylinder(tank.transform, "RoadWheelHub", new Vector3(side * 3.74f, 0.69f, z), new Vector3(0.55f, 0.16f, 0.55f), metal, 0f, 90f);
                Cylinder(tank.transform, "HubCap", new Vector3(side * 3.84f, 0.69f, z), new Vector3(0.23f, 0.08f, 0.23f), paint, 0f, 90f);
                for (int bolt = 0; bolt < 6; bolt++)
                {
                    float a = bolt * Mathf.PI / 3f;
                    var fastener = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    fastener.name = "WheelFastener"; fastener.transform.SetParent(tank.transform, false);
                    fastener.transform.localPosition = new Vector3(side * 3.90f, 0.69f + Mathf.Sin(a) * 0.16f, z + Mathf.Cos(a) * 0.16f);
                    fastener.transform.localScale = Vector3.one * 0.07f;
                    fastener.GetComponent<Renderer>().sharedMaterial = metal;
                    Object.DestroyImmediate(fastener.GetComponent<Collider>());
                }
            }
            Cylinder(tank.transform, "DriveSprocket", new Vector3(side * 3.58f, 0.69f, 4.1f), new Vector3(1.03f, 0.3f, 1.03f), rubber, 0f, 90f);
            Cylinder(tank.transform, "TrackIdler", new Vector3(side * 3.58f, 0.69f, -4.1f), new Vector3(1.03f, 0.3f, 1.03f), rubber, 0f, 90f);
            Box(tank.transform, "SegmentedSideSkirt", new Vector3(side * 3.52f, 1.47f, 0f), new Vector3(0.26f, 0.42f, 6.9f), paint);
            for (int i = 0; i < 4; i++)
                Box(tank.transform, "SideSkirtPanelSeam", new Vector3(side * 3.67f, 1.48f, -2.55f + i * 1.7f), new Vector3(0.025f, 0.35f, 0.045f), metal);
        }

        // Turret ring, angular turret cheeks, layered reactive armor and long detailed cannon.
        Cylinder(tank.transform, "TurretRaceRing", new Vector3(0f, 2.05f, 0.3f), new Vector3(2.3f, 0.3f, 2.3f), rubber, 0f, 0f);
        Box(tank.transform, "TurretCore", new Vector3(0f, 2.82f, 0.45f), new Vector3(4.25f, 1.35f, 4.05f), paint);
        BoxRotated(tank.transform, "TurretFrontArmor", new Vector3(0f, 3.0f, 2.35f), new Vector3(3.9f, 0.82f, 1.0f), paint, new Vector3(-18f, 0f, 0f));
        for (int side = -1; side <= 1; side += 2)
        {
            BoxRotated(tank.transform, "TurretCheekArmor", new Vector3(side * 1.95f, 2.95f, 1.0f), new Vector3(0.52f, 1.05f, 2.35f), paint, new Vector3(0f, 0f, side * 12f));
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++)
                BoxRotated(tank.transform, "ReactiveArmorBlock", new Vector3(side * 2.18f, 2.75f + row * 0.47f, -0.2f + col * 0.8f), new Vector3(0.22f, 0.38f, 0.68f), metal, new Vector3(0f, 0f, side * 8f));
        }
        Cylinder(tank.transform, "GunMantlet", new Vector3(0f, 3.0f, 2.8f), new Vector3(1.06f, 0.72f, 0.9f), paint, 90f, 0f);
        Cylinder(tank.transform, "CannonBreechCollar", new Vector3(0f, 3.02f, 3.25f), new Vector3(0.62f, 0.4f, 0.62f), metal, 90f, 0f);
        Cylinder(tank.transform, "MainCannon", new Vector3(0f, 3.02f, 5.5f), new Vector3(0.2f, 2.4f, 0.2f), paint, 90f, 0f);
        Cylinder(tank.transform, "MuzzleBrake", new Vector3(0f, 3.02f, 7.75f), new Vector3(0.38f, 0.48f, 0.38f), metal, 90f, 0f);
        for (int i = 0; i < 3; i++)
            Box(tank.transform, "MuzzleVent", new Vector3(0f, 2.88f + i * 0.13f, 7.78f), new Vector3(0.52f, 0.035f, 0.09f), rubber);

        // Hatches, periscopes, smoke launchers, storage boxes, spare wheel and aerials.
        Cylinder(tank.transform, "CommanderHatchRing", new Vector3(-1.12f, 3.56f, -0.15f), new Vector3(0.8f, 0.13f, 0.8f), metal, 0f, 0f);
        Cylinder(tank.transform, "CommanderHatch", new Vector3(-1.12f, 3.68f, -0.15f), new Vector3(0.65f, 0.16f, 0.65f), paint, 0f, 0f);
        Cylinder(tank.transform, "LoaderHatch", new Vector3(1.05f, 3.58f, -0.35f), new Vector3(0.62f, 0.12f, 0.62f), metal, 0f, 0f);
        Box(tank.transform, "OpticsHousing", new Vector3(-1.85f, 3.55f, 1.6f), new Vector3(0.45f, 0.35f, 0.7f), rubber);
        Box(tank.transform, "PeriscopeGlass", new Vector3(-1.85f, 3.68f, 1.92f), new Vector3(0.3f, 0.12f, 0.08f), metal);
        for (int i = 0; i < 6; i++)
        {
            float x = i < 3 ? -1.65f : 1.65f;
            float z = i % 3 == 0 ? 2.0f : (i % 3 == 1 ? 1.75f : 1.5f);
            Cylinder(tank.transform, "SmokeDischarger", new Vector3(x, 3.2f, z), new Vector3(0.14f, 0.32f, 0.14f), metal, -25f, 0f);
        }
        Box(tank.transform, "RearStowageBin", new Vector3(0f, 2.55f, -3.65f), new Vector3(3.7f, 0.75f, 1.25f), paint);
        Box(tank.transform, "ToolBox", new Vector3(2.45f, 2.1f, -1.3f), new Vector3(0.7f, 0.6f, 1.4f), metal);
        Cylinder(tank.transform, "SpareRoadWheel", new Vector3(-2.4f, 2.35f, -3.4f), new Vector3(0.95f, 0.3f, 0.95f), rubber, 0f, 90f);
        Line(tank.transform, "RadioAerial", new Vector3(1.5f, 3.48f, -1.4f), new Vector3(1.55f, 5.6f, -1.45f), rubber, 0.025f);
        Line(tank.transform, "RadioAerial", new Vector3(-1.8f, 3.35f, -1.0f), new Vector3(-1.95f, 4.8f, -0.7f), rubber, 0.02f);
        for (int side = -1; side <= 1; side += 2)
            AddStencil(tank.transform, "07", new Vector3(side * 2.15f, 2.85f, -1.0f), Quaternion.Euler(0f, side * 90f, 0f));

        for (int side = -1; side <= 1; side += 2)
        {
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Headlamp"; lamp.transform.SetParent(tank.transform, false);
            lamp.transform.localPosition = new Vector3(side * 2.35f, 1.65f, 5.0f);
            lamp.transform.localScale = new Vector3(0.32f, 0.28f, 0.18f);
            lamp.GetComponent<Renderer>().sharedMaterial = metal;
            Object.DestroyImmediate(lamp.GetComponent<Collider>());
            Box(tank.transform, "HeadlampGuard", new Vector3(side * 2.35f, 1.78f, 5.12f), new Vector3(0.58f, 0.1f, 0.22f), paint);
        }
    }

    static void CreateRangeProps(Transform parent, Material paint, Material rubber, Material metal, Material glass)
    {
        CreateUtilityTruck(parent, new Vector3(46f, 0f, -46f), paint, rubber, metal, glass);
        for (int i = 0; i < 5; i++)
        {
            var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "FieldSupplyCrate"; crate.transform.SetParent(parent, false);
            crate.transform.position = new Vector3(44f + i * 2.2f, 0.65f, -69f + (i % 2) * 1.8f);
            crate.transform.localScale = new Vector3(1.5f, 1.3f, 1.4f);
            crate.GetComponent<Renderer>().sharedMaterial = i % 2 == 0 ? paint : metal;
            Object.DestroyImmediate(crate.GetComponent<Collider>());
        }
        for (int i = 0; i < 4; i++)
            Cylinder(parent, "FuelDrum", new Vector3(43f + i * 1.4f, 0.65f, -64f), new Vector3(0.72f, 0.65f, 0.72f), i % 2 == 0 ? paint : metal, 0f, 0f);
    }

    static void CreateFieldCamp(Transform parent, Material paint, Material rubber, Material metal, Material glass, Material earth)
    {
        var camp = new GameObject("ForwardOperatingCamp");
        camp.transform.SetParent(parent, false);
        camp.transform.position = new Vector3(-78f, 0f, 24f);

        CreateTent(camp.transform, "CommandShelter", new Vector3(0f, 0f, 0f), paint, metal, metal);
        CreateTent(camp.transform, "CrewTent", new Vector3(15f, 0f, 1f), earth, metal, metal);
        CreateTent(camp.transform, "MedicalTent", new Vector3(1f, 0f, 16f), earth, paint, metal);
        Box(camp.transform, "OperationsTable", new Vector3(3.8f, 0.95f, 1.2f), new Vector3(2.3f, 0.16f, 1.1f), metal);
        Box(camp.transform, "FieldRadio", new Vector3(3.4f, 1.35f, 1.2f), new Vector3(0.58f, 0.65f, 0.42f), rubber);
        CreateAntenna(camp.transform, new Vector3(-5.5f, 0f, 2f), paint, metal);
        CreateAntenna(camp.transform, new Vector3(19f, 0f, 2f), paint, metal);
        CreateSandbagWall(camp.transform, new Vector3(-4f, 0f, -6f), 19, true, earth);
        CreateSandbagWall(camp.transform, new Vector3(-9f, 0f, -4f), 15, false, earth);
        CreateSandbagWall(camp.transform, new Vector3(20f, 0f, -4f), 15, false, earth);
        CreateUtilityTruck(camp.transform, new Vector3(11f, 0f, 14f), paint, rubber, metal, glass);

        for (int i = 0; i < 7; i++)
        {
            Box(camp.transform, "FieldStorageCase", new Vector3(-7f + (i % 3) * 1.2f, 0.55f, 9f + (i / 3) * 1.0f), new Vector3(1.05f, 1.0f, 0.8f), i % 2 == 0 ? paint : metal);
            if (i < 4) Cylinder(camp.transform, "FuelCan", new Vector3(5.5f + i * 0.85f, 0.48f, 10f), new Vector3(0.48f, 0.48f, 0.48f), paint, 0f, 0f);
        }
        Box(camp.transform, "PortableGenerator", new Vector3(-4f, 0.75f, 10f), new Vector3(1.8f, 1.3f, 1.1f), rubber);
        Box(camp.transform, "GeneratorControlPanel", new Vector3(-4f, 1.2f, 10.58f), new Vector3(0.9f, 0.38f, 0.08f), metal);
        CreateCampLight(camp.transform, new Vector3(7f, 0f, -4f), metal);
        CreateCampLight(camp.transform, new Vector3(23f, 0f, 10f), metal);
    }

    static void CreateTent(Transform parent, string name, Vector3 position, Material fabric, Material trim, Material metal)
    {
        var tent = new GameObject(name);
        tent.transform.SetParent(parent, false); tent.transform.localPosition = position; tent.isStatic = true;
        BoxRotated(tent.transform, "LeftCanvasRoof", new Vector3(-1.2f, 2.0f, 0f), new Vector3(3.1f, 0.12f, 5.1f), fabric, new Vector3(0f, 0f, -32f));
        BoxRotated(tent.transform, "RightCanvasRoof", new Vector3(1.2f, 2.0f, 0f), new Vector3(3.1f, 0.12f, 5.1f), fabric, new Vector3(0f, 0f, 32f));
        Triangle(tent.transform, "FrontCanvasEnd", new Vector3(-1.75f, 0.95f, 2.52f), new Vector3(1.75f, 0.95f, 2.52f), new Vector3(0f, 2.95f, 2.52f), fabric);
        Triangle(tent.transform, "RearCanvasEnd", new Vector3(-1.75f, 0.95f, -2.52f), new Vector3(1.75f, 0.95f, -2.52f), new Vector3(0f, 2.95f, -2.52f), fabric);
        Box(tent.transform, "LeftCanvasWall", new Vector3(-1.52f, 0.62f, 0f), new Vector3(0.08f, 1.24f, 4.95f), fabric);
        Box(tent.transform, "RightCanvasWall", new Vector3(1.52f, 0.62f, 0f), new Vector3(0.08f, 1.24f, 4.95f), fabric);
        Box(tent.transform, "TentDoorFlap", new Vector3(0f, 0.95f, 2.55f), new Vector3(1.15f, 1.9f, 0.06f), trim);
        Line(tent.transform, "TentRidgePole", new Vector3(0f, 2.92f, -2.6f), new Vector3(0f, 2.92f, 2.6f), metal, 0.045f);
        Line(tent.transform, "TentSupportFront", new Vector3(0f, 0f, 2.45f), new Vector3(0f, 2.95f, 2.45f), metal, 0.055f);
        Line(tent.transform, "TentSupportRear", new Vector3(0f, 0f, -2.45f), new Vector3(0f, 2.95f, -2.45f), metal, 0.055f);
        for (int side = -1; side <= 1; side += 2)
        for (int end = -1; end <= 1; end += 2)
            Line(tent.transform, "TentGuyRope", new Vector3(side * 1.4f, 1.65f, end * 1.35f), new Vector3(side * 3.3f, 0.04f, end * 3.3f), trim, 0.025f);
    }

    static void CreateSandbagWall(Transform parent, Vector3 start, int count, bool alongX, Material material)
    {
        for (int i = 0; i < count; i++)
        {
            float offset = i * 0.72f;
            float x = start.x + (alongX ? offset : 0f);
            float z = start.z + (alongX ? 0f : offset);
            Box(parent, "SandbagLower", new Vector3(x, 0.24f, z), new Vector3(0.82f, 0.42f, 0.56f), material, (i % 2) * 4f);
            if (i % 2 == 0)
                Box(parent, "SandbagUpper", new Vector3(x + (alongX ? 0.35f : 0f), 0.62f, z + (alongX ? 0f : 0.3f)), new Vector3(0.82f, 0.38f, 0.54f), material, -3f);
        }
    }

    static void CreateAntenna(Transform parent, Vector3 position, Material pole, Material metal)
    {
        Line(parent, "CommsMast", position + Vector3.up * 0.1f, position + Vector3.up * 10f, pole, 0.09f);
        for (int i = 0; i < 4; i++)
        {
            float y = 6.1f + i * 0.72f;
            Line(parent, "AntennaElement", position + new Vector3(-0.8f, y, 0f), position + new Vector3(0.8f, y, 0f), metal, 0.035f);
        }
        for (int i = 0; i < 3; i++)
            Line(parent, "MastGuyWire", position + new Vector3(0f, 5f + i, 0f), position + new Vector3((i - 1) * 2.3f, 0.04f, 3.5f), metal, 0.018f);
        Box(parent, "AntennaBase", position + new Vector3(0f, 0.48f, 0f), new Vector3(0.85f, 0.9f, 0.85f), pole);
    }

    static void CreateCampLight(Transform parent, Vector3 position, Material metal)
    {
        Line(parent, "WorkLightPole", position + Vector3.up * 0.1f, position + Vector3.up * 6f, metal, 0.09f);
        Box(parent, "WorkLightArm", position + new Vector3(0f, 5.95f, 0.55f), new Vector3(0.12f, 0.12f, 1.2f), metal);
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamp.name = "WarmCampFloodlight"; lamp.transform.SetParent(parent, false);
        lamp.transform.localPosition = position + new Vector3(0f, 5.75f, 1.05f);
        lamp.transform.localScale = new Vector3(0.42f, 0.28f, 0.24f);
        lamp.GetComponent<Renderer>().sharedMaterial = metal;
        Object.DestroyImmediate(lamp.GetComponent<Collider>());
        var light = lamp.AddComponent<Light>();
        light.type = LightType.Spot; light.color = new Color(1f, 0.77f, 0.48f); light.intensity = 2.5f; light.range = 18f; light.spotAngle = 82f;
    }

    static void CreateUtilityTruck(Transform parent, Vector3 position, Material paint, Material rubber, Material metal, Material glass)
    {
        var truck = new GameObject("FieldUtilityTruck");
        truck.transform.SetParent(parent, false); truck.transform.localPosition = position;
        Box(truck.transform, "TruckChassis", new Vector3(0f, 1.05f, 0f), new Vector3(2.8f, 0.5f, 6.8f), rubber);
        Box(truck.transform, "TruckCargoBed", new Vector3(0f, 1.65f, -1.5f), new Vector3(2.8f, 1.05f, 3.8f), paint);
        Box(truck.transform, "TruckCab", new Vector3(0f, 2.0f, 2.15f), new Vector3(2.65f, 1.85f, 2.4f), paint);
        Box(truck.transform, "TruckWindshield", new Vector3(0f, 2.4f, 3.38f), new Vector3(2.05f, 0.92f, 0.08f), glass);
        Box(truck.transform, "TruckBumper", new Vector3(0f, 0.95f, 3.6f), new Vector3(3.15f, 0.35f, 0.35f), metal);
        for (int side = -1; side <= 1; side += 2)
        for (int axle = 0; axle < 3; axle++)
        {
            float z = 2.45f - axle * 2.45f;
            Cylinder(truck.transform, "TruckTire", new Vector3(side * 1.48f, 0.48f, z), new Vector3(0.96f, 0.38f, 0.96f), rubber, 0f, 90f);
            Cylinder(truck.transform, "TruckWheelHub", new Vector3(side * 1.69f, 0.48f, z), new Vector3(0.42f, 0.12f, 0.42f), metal, 0f, 90f);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            Box(truck.transform, "SideMirrorArm", new Vector3(side * 1.6f, 2.65f, 2.7f), new Vector3(0.65f, 0.07f, 0.07f), metal);
            Box(truck.transform, "SideMirror", new Vector3(side * 1.9f, 2.68f, 2.7f), new Vector3(0.12f, 0.34f, 0.38f), rubber);
            Box(truck.transform, "DoorPanel", new Vector3(side * 1.34f, 1.95f, 2.0f), new Vector3(0.08f, 1.2f, 1.55f), paint);
            Box(truck.transform, "TruckSideWindow", new Vector3(side * 1.36f, 2.52f, 2.38f), new Vector3(0.045f, 0.62f, 0.82f), glass);
        }
        for (int i = 0; i < 8; i++)
            Box(truck.transform, "CargoBedRail", new Vector3(-1.32f + i * 0.38f, 2.2f, -1.6f), new Vector3(0.08f, 0.1f, 3.9f), metal);
        Box(truck.transform, "CanvasCargoCover", new Vector3(0f, 2.65f, -1.5f), new Vector3(2.25f, 0.25f, 3.15f), paint);
    }

    static void SetNaturalDaylight()
    {
        if (RenderSettings.sun != null)
        {
            RenderSettings.sun.transform.rotation = Quaternion.Euler(34f, -32f, 0f);
            RenderSettings.sun.color = new Color(1f, 0.93f, 0.79f);
            RenderSettings.sun.intensity = 1.2f;
            RenderSettings.sun.shadows = LightShadows.Soft;
            RenderSettings.sun.shadowStrength = 0.72f;
        }
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.48f, 0.59f, 0.69f);
        RenderSettings.ambientEquatorColor = new Color(0.45f, 0.46f, 0.40f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.23f, 0.19f);
        RenderSettings.ambientIntensity = 1.05f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.53f, 0.61f, 0.66f);
        RenderSettings.fogDensity = 0.0007f;

        var volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (var volume in volumes)
        {
            var profile = volume.sharedProfile;
            if (profile == null) continue;
            if (profile.TryGet(out ColorAdjustments grading))
            {
                grading.postExposure.Override(0.1f);
                grading.contrast.Override(4f);
                grading.saturation.Override(-2f);
                grading.colorFilter.Override(Color.white);
            }
            if (profile.TryGet(out Vignette vignette)) vignette.intensity.Override(0.06f);
        }
        if (RenderSettings.skybox != null)
        {
            Material sky = RenderSettings.skybox;
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.76f, 0.83f, 0.9f));
            if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.34f, 0.35f, 0.30f));
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 1.0f);
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.05f);
        }
    }

    static void BrightenExistingGroundMaterials()
    {
        TintMaterial("Grass.mat", new Color(0.63f, 0.65f, 0.48f));
        TintMaterial("Dirt.mat", new Color(0.72f, 0.61f, 0.43f));
        TintMaterial("Asphalt.mat", new Color(0.34f, 0.35f, 0.34f));
        TintMaterial("ConcreteDark.mat", new Color(0.43f, 0.44f, 0.41f));
    }

    static void TintMaterial(string filename, Color color)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + filename);
        if (material == null) return;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", GetDetailTexture(filename));
            material.SetTextureScale("_BaseMap", filename == "Asphalt.mat" ? new Vector2(18f, 18f) : new Vector2(10f, 10f));
        }
        EditorUtility.SetDirty(material);
    }

    static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material material, float angle = 0f)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name; item.transform.SetParent(parent, false);
        item.transform.localPosition = pos; item.transform.localScale = scale;
        item.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>()); item.isStatic = true;
    }

    static void BoxRotated(Transform parent, string name, Vector3 pos, Vector3 scale, Material material, Vector3 euler)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name; item.transform.SetParent(parent, false);
        item.transform.localPosition = pos; item.transform.localScale = scale;
        item.transform.localRotation = Quaternion.Euler(euler);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>()); item.isStatic = true;
    }

    static void Triangle(Transform parent, string name, Vector3 a, Vector3 b, Vector3 c, Material material)
    {
        var mesh = new Mesh { name = name + "Mesh" };
        mesh.vertices = new[] { a, b, c };
        mesh.triangles = new[] { 0, 1, 2, 2, 1, 0 };
        mesh.RecalculateNormals();
        var item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.AddComponent<MeshFilter>().sharedMesh = mesh;
        item.AddComponent<MeshRenderer>().sharedMaterial = material;
        item.isStatic = true;
    }

    static void AddStencil(Transform parent, string label, Vector3 position, Quaternion rotation)
    {
        var marker = new GameObject("VehicleStencil");
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = position;
        marker.transform.localRotation = rotation;
        var text = marker.AddComponent<TextMesh>();
        text.text = label;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 48;
        text.characterSize = 0.12f;
        text.color = new Color(0.78f, 0.74f, 0.57f);
    }

    static void Line(Transform parent, string name, Vector3 start, Vector3 end, Material material, float radius)
    {
        var delta = end - start;
        var item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        item.name = name; item.transform.SetParent(parent, false);
        item.transform.localPosition = (start + end) * 0.5f;
        item.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        item.transform.localScale = new Vector3(radius, delta.magnitude * 0.5f, radius);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>()); item.isStatic = true;
    }

    static void Cylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material material, float xRotation, float zRotation)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        item.name = name; item.transform.SetParent(parent, false);
        item.transform.localPosition = pos; item.transform.localScale = scale;
        item.transform.localRotation = Quaternion.Euler(xRotation, 0f, zRotation);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>()); item.isStatic = true;
    }

    static Material MaterialAsset(string filename, Color color, float metallic, float smoothness)
    {
        string path = MaterialFolder + "/" + filename;
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = filename.Replace(".mat", "") };
            AssetDatabase.CreateAsset(material, path);
        }
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        Texture2D detail = GetDetailTexture(filename);
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", detail);
            material.SetTextureScale("_BaseMap", filename.StartsWith("Natural") ? new Vector2(4f, 4f) : new Vector2(2f, 2f));
        }
        return material;
    }

    static Texture2D GetDetailTexture(string materialName)
    {
        string textureName = materialName.Replace(".mat", "Detail.asset");
        string path = "Assets/GeneratedTrainingGround/Textures/" + textureName;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture != null) return texture;

        const int size = 128;
        texture = new Texture2D(size, size, TextureFormat.RGB24, true) { name = textureName.Replace(".asset", ""), wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
        int seed = Mathf.Abs(materialName.GetHashCode() % 1000);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (float)x / size, v = (float)y / size;
            float broad = Mathf.PerlinNoise(u * 7f + seed, v * 7f + seed * 0.37f);
            float fine = Mathf.PerlinNoise(u * 31f + seed * 0.13f, v * 31f + seed * 0.71f);
            float variation = Mathf.Lerp(0.72f, 1.12f, broad * 0.72f + fine * 0.28f);
            texture.SetPixel(x, y, new Color(variation, variation, variation, 1f));
        }
        texture.Apply(true, false);
        AssetDatabase.CreateAsset(texture, path);
        return texture;
    }

    static void DestroyNamed(string name)
    {
        var old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);
    }

    static float RandomRange(System.Random random, float min, float max) => (float)(min + random.NextDouble() * (max - min));
}
