using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SetupDroneThreatSystem
{
    const string ROOT_NAME =
        "DroneTraining";

    [MenuItem("26247/SETUP DRONE THREAT SYSTEM")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError(
                "Stop Play Mode first."
            );

            return;
        }

        // ========================================================
        // REMOVE OLD DRONE TRAINING
        // ========================================================

        GameObject old =
            GameObject.Find(
                ROOT_NAME
            );

        if (old != null)
            Object.DestroyImmediate(
                old
            );

        // ========================================================
        // REMOVE OLD DRONE SCANNER FROM PLAYER
        // ========================================================

        GameObject player =
            GameObject.Find(
                "TraineePlayer"
            );

        if (player == null)
        {
            Debug.LogError(
                "TraineePlayer not found. Run FIX PLAYER NOW first."
            );

            return;
        }

        DroneScanner oldScanner =
            player.GetComponent<
                DroneScanner
            >();

        if (oldScanner != null)
        {
            Object.DestroyImmediate(
                oldScanner
            );
        }

        // ========================================================
        // ROOT
        // ========================================================

        GameObject root =
            new GameObject(
                ROOT_NAME
            );

        GameObject dronesRoot =
            new GameObject(
                "Drones"
            );

        dronesRoot.transform.SetParent(
            root.transform
        );

        // ========================================================
        // MANAGER
        // ========================================================

        GameObject managerObject =
            new GameObject(
                "DroneThreatManager"
            );

        managerObject.transform.SetParent(
            root.transform
        );

        DroneThreatSystem system =
            managerObject.AddComponent<
                DroneThreatSystem
            >();

        Camera cam =
            player.GetComponentInChildren<
                Camera
            >();

        system.playerCamera =
            cam;

        system.detectionTime =
            10f;

        system.maxRange =
            300f;

        system.scanTime =
            1.5f;

        // ========================================================
        // DRONES
        // ========================================================

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Cheap,
            TrainingDrone.DroneVariant.CheapScout,
            "CIVILIAN SCOUT",
            new Vector3(
                -30f,
                22f,
                25f
            ),
            new Color(
                0.32f,
                0.33f,
                0.30f
            ),
            18f,
            1.0f,
            0f
        );

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Cheap,
            TrainingDrone.DroneVariant.CheapRacer,
            "LIGHT RACING DRONE",
            new Vector3(
                20f,
                26f,
                55f
            ),
            new Color(
                0.25f,
                0.27f,
                0.25f
            ),
            22f,
            1.25f,
            2f
        );

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Cheap,
            TrainingDrone.DroneVariant.CheapHeavy,
            "UTILITY DRONE",
            new Vector3(
                55f,
                21f,
                -25f
            ),
            new Color(
                0.29f,
                0.30f,
                0.27f
            ),
            20f,
            0.9f,
            4f
        );

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Military,
            TrainingDrone.DroneVariant.MilitarySurveillance,
            "SURVEILLANCE UAV",
            new Vector3(
                50f,
                30f,
                50f
            ),
            new Color(
                0.11f,
                0.15f,
                0.105f
            ),
            32f,
            0.7f,
            1f
        );

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Military,
            TrainingDrone.DroneVariant.MilitaryHeavy,
            "HEAVY UAV",
            new Vector3(
                -55f,
                27f,
                55f
            ),
            new Color(
                0.09f,
                0.12f,
                0.085f
            ),
            28f,
            0.8f,
            3.5f
        );

        CreateDrone(
            dronesRoot.transform,
            TrainingDrone.DroneClass.Military,
            TrainingDrone.DroneVariant.MilitaryInterceptor,
            "TACTICAL UAV",
            new Vector3(
                65f,
                24f,
                10f
            ),
            new Color(
                0.075f,
                0.095f,
                0.07f
            ),
            30f,
            1.0f,
            5f
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
            "=========================================="
        );

        Debug.Log(
            "DRONE THREAT SYSTEM READY"
        );

        Debug.Log(
            "6 training drones created."
        );

        Debug.Log(
            "Aim = 10 seconds"
        );

        Debug.Log(
            "E = Scan"
        );

        Debug.Log(
            "J = Cheap"
        );

        Debug.Log(
            "K = Military"
        );

        Debug.Log(
            "Correct classification = simulated intercept"
        );

        Debug.Log(
            "=========================================="
        );
    }

    // ============================================================
    // CREATE DRONE
    // ============================================================

    static void CreateDrone(
        Transform parent,
        TrainingDrone.DroneClass droneClass,
        TrainingDrone.DroneVariant variant,
        string displayName,
        Vector3 center,
        Color bodyColor,
        float radius,
        float speed,
        float phase
    )
    {
        GameObject root =
            new GameObject(
                displayName
            );

        root.transform.position =
            center;

        root.transform.SetParent(
            parent
        );

        TrainingDrone drone =
            root.AddComponent<
                TrainingDrone
            >();

        drone.Configure(
            droneClass,
            variant,
            displayName,
            center,
            center.y,
            radius,
            speed,
            phase
        );

        // ========================================================
        // COLLIDER
        // ========================================================

        SphereCollider collider =
            root.AddComponent<
                SphereCollider
            >();

        collider.radius =
            variant == TrainingDrone.DroneVariant.CheapRacer
                ? 1.2f
                : 1.5f;

        // ========================================================
        // BODY
        // ========================================================

        CreateDroneBody(
            root.transform,
            variant,
            bodyColor
        );

        // ========================================================
        // ROTOR ARMS
        // ========================================================

        Vector3[] rotorPositions =
        {
            new Vector3(-1.35f, 0f, -1.0f),
            new Vector3( 1.35f, 0f, -1.0f),
            new Vector3(-1.35f, 0f,  1.0f),
            new Vector3( 1.35f, 0f,  1.0f)
        };

        for (int i = 0;
             i < rotorPositions.Length;
             i++)
        {
            CreateBlock(
                root.transform,
                "RotorArm",
                rotorPositions[i] *
                0.85f,
                new Vector3(
                    2.3f,
                    0.15f,
                    0.15f
                ),
                bodyColor,
                Quaternion.Euler(
                    0f,
                    45f,
                    0f
                )
            );

            GameObject rotor =
                CreateBlock(
                    root.transform,
                    "Rotor_" + i,
                    rotorPositions[i],
                    new Vector3(
                        0.65f,
                        0.08f,
                        0.65f
                    ),
                    new Color(
                        0.025f,
                        0.028f,
                        0.025f
                    ),
                    Quaternion.identity
                );

            // rotor blade
            CreateBlock(
                rotor.transform,
                "Blade",
                Vector3.zero,
                new Vector3(
                    1.05f,
                    0.035f,
                    0.07f
                ),
                new Color(
                    0.04f,
                    0.04f,
                    0.035f
                ),
                Quaternion.identity
            );
        }

        // ========================================================
        // VARIANT DETAILS
        // ========================================================

        switch (variant)
        {
            case TrainingDrone.DroneVariant.CheapScout:

                CreateBlock(
                    root.transform,
                    "Camera",
                    new Vector3(
                        0f,
                        -0.35f,
                        -0.9f
                    ),
                    new Vector3(
                        0.45f,
                        0.3f,
                        0.45f
                    ),
                    new Color(
                        0.06f,
                        0.065f,
                        0.06f
                    ),
                    Quaternion.identity
                );

                break;

            case TrainingDrone.DroneVariant.CheapRacer:

                root.transform.localScale =
                    new Vector3(
                        0.85f,
                        0.65f,
                        1.15f
                    );

                break;

            case TrainingDrone.DroneVariant.CheapHeavy:

                CreateBlock(
                    root.transform,
                    "PayloadBox",
                    new Vector3(
                        0f,
                        -0.25f,
                        0f
                    ),
                    new Vector3(
                        0.9f,
                        0.65f,
                        0.9f
                    ),
                    new Color(
                        0.17f,
                        0.18f,
                        0.16f
                    ),
                    Quaternion.identity
                );

                break;

            case TrainingDrone.DroneVariant.MilitarySurveillance:

                CreateBlock(
                    root.transform,
                    "SensorPod",
                    new Vector3(
                        0f,
                        -0.3f,
                        -1.0f
                    ),
                    new Vector3(
                        0.65f,
                        0.45f,
                        0.75f
                    ),
                    new Color(
                        0.035f,
                        0.045f,
                        0.035f
                    ),
                    Quaternion.identity
                );

                CreateBlock(
                    root.transform,
                    "TopModule",
                    new Vector3(
                        0f,
                        0.45f,
                        0f
                    ),
                    new Vector3(
                        0.75f,
                        0.25f,
                        0.75f
                    ),
                    bodyColor,
                    Quaternion.identity
                );

                break;

            case TrainingDrone.DroneVariant.MilitaryHeavy:

                root.transform.localScale =
                    new Vector3(
                        1.25f,
                        1.15f,
                        1.25f
                    );

                CreateBlock(
                    root.transform,
                    "Armor",
                    new Vector3(
                        0f,
                        0.15f,
                        0f
                    ),
                    new Vector3(
                        1.1f,
                        0.75f,
                        1.1f
                    ),
                    bodyColor,
                    Quaternion.identity
                );

                break;

            case TrainingDrone.DroneVariant.MilitaryInterceptor:

                root.transform.localScale =
                    new Vector3(
                        1.1f,
                        0.75f,
                        1.35f
                    );

                CreateBlock(
                    root.transform,
                    "NoseModule",
                    new Vector3(
                        0f,
                        0f,
                        -1.0f
                    ),
                    new Vector3(
                        0.55f,
                        0.4f,
                        0.9f
                    ),
                    new Color(
                        0.025f,
                        0.035f,
                        0.025f
                    ),
                    Quaternion.identity
                );

                CreateBlock(
                    root.transform,
                    "TopAntenna",
                    new Vector3(
                        0f,
                        0.8f,
                        0f
                    ),
                    new Vector3(
                        0.08f,
                        1.1f,
                        0.08f
                    ),
                    bodyColor,
                    Quaternion.identity
                );

                break;
        }

        // ========================================================
        // STATIC FLAG
        // ========================================================

        root.isStatic =
            false;
    }

    // ============================================================
    // BODY
    // ============================================================

    static void CreateDroneBody(
        Transform parent,
        TrainingDrone.DroneVariant variant,
        Color color
    )
    {
        PrimitiveType primitive =
            PrimitiveType.Capsule;

        Vector3 scale =
            new Vector3(
                1.25f,
                0.32f,
                1.0f
            );

        if (
            variant ==
            TrainingDrone.DroneVariant.CheapHeavy ||
            variant ==
            TrainingDrone.DroneVariant.MilitaryHeavy
        )
        {
            primitive =
                PrimitiveType.Cube;

            scale =
                new Vector3(
                    1.25f,
                    0.55f,
                    1.15f
                );
        }

        GameObject body =
            GameObject.CreatePrimitive(
                primitive
            );

        body.name =
            "DroneBody";

        body.transform.SetParent(
            parent
        );

        body.transform.localPosition =
            Vector3.zero;

        body.transform.localScale =
            scale;

        body.GetComponent<Renderer>()
            .sharedMaterial =
            CreateRuntimeMaterial(
                color
            );

        Object.DestroyImmediate(
            body.GetComponent<Collider>()
        );
    }

    // ============================================================
    // BLOCK
    // ============================================================

    static GameObject CreateBlock(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 scale,
        Color color,
        Quaternion rotation
    )
    {
        GameObject go =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        go.name =
            name;

        go.transform.SetParent(
            parent
        );

        go.transform.localPosition =
            position;

        go.transform.localScale =
            scale;

        go.transform.localRotation =
            rotation;

        go.GetComponent<Renderer>()
            .sharedMaterial =
            CreateRuntimeMaterial(
                color
            );

        Object.DestroyImmediate(
            go.GetComponent<Collider>()
        );

        return go;
    }

    // ============================================================
    // MATERIAL
    // ============================================================

    static Material CreateRuntimeMaterial(
        Color color
    )
    {
        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
            shader =
                Shader.Find("Standard");

        Material material =
            new Material(shader);

        if (material.HasProperty(
            "_BaseColor"
        ))
        {
            material.SetColor(
                "_BaseColor",
                color
            );
        }

        if (material.HasProperty(
            "_Color"
        ))
        {
            material.SetColor(
                "_Color",
                color
            );
        }

        if (material.HasProperty(
            "_Metallic"
        ))
        {
            material.SetFloat(
                "_Metallic",
                0.25f
            );
        }

        if (material.HasProperty(
            "_Smoothness"
        ))
        {
            material.SetFloat(
                "_Smoothness",
                0.4f
            );
        }

        return material;
    }
}
