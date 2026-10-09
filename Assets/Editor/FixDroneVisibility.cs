using UnityEngine;
using UnityEditor;

public static class FixDroneVisibility
{
    [MenuItem("26247/FIX DRONE VISIBILITY")]
    public static void Fix()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Stop Play Mode first.");
            return;
        }

        GameObject player =
            GameObject.Find("TraineePlayer");

        if (player == null)
        {
            Debug.LogError("TraineePlayer not found.");
            return;
        }

        TrainingDrone[] drones =
            Object.FindObjectsByType<TrainingDrone>(
                FindObjectsSortMode.None
            );

        if (drones.Length == 0)
        {
            Debug.LogError(
                "No TrainingDrone objects found. Run 26247 -> SETUP DRONE THREAT SYSTEM first."
            );

            return;
        }

        Vector3 playerPos =
            player.transform.position;

        Vector3 forward =
            player.transform.forward;

        Vector3 right =
            player.transform.right;

        // Visible test positions around player
        Vector3[] positions =
        {
            playerPos + forward * 32f + Vector3.up * 18f,
            playerPos + forward * 55f + right * 20f + Vector3.up * 24f,
            playerPos + forward * 65f - right * 25f + Vector3.up * 20f,
            playerPos - right * 45f + Vector3.up * 25f,
            playerPos + right * 50f + Vector3.up * 28f,
            playerPos + forward * 85f + Vector3.up * 32f
        };

        for (int i = 0; i < drones.Length; i++)
        {
            TrainingDrone drone =
                drones[i];

            if (drone == null)
                continue;

            drone.gameObject.SetActive(true);

            // Make drone substantially easier to see
            drone.transform.localScale =
                Vector3.one *
                1.8f;

            // Place drone in visible training airspace
            Vector3 position =
                positions[
                    i % positions.Length
                ];

            drone.transform.position =
                position;

            // Make every renderer visible
            Renderer[] renderers =
                drone.GetComponentsInChildren<Renderer>(
                    true
                );

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;

                Material material =
                    renderer.sharedMaterial;

                if (material != null)
                {
                    if (material.HasProperty("_BaseColor"))
                    {
                        Color c =
                            drone.droneClass ==
                            TrainingDrone.DroneClass.Military
                            ? new Color(
                                0.18f,
                                0.24f,
                                0.16f
                            )
                            : new Color(
                                0.42f,
                                0.44f,
                                0.38f
                            );

                        material.SetColor(
                            "_BaseColor",
                            c
                        );
                    }

                    if (material.HasProperty("_Color"))
                    {
                        Color c =
                            drone.droneClass ==
                            TrainingDrone.DroneClass.Military
                            ? new Color(
                                0.18f,
                                0.24f,
                                0.16f
                            )
                            : new Color(
                                0.42f,
                                0.44f,
                                0.38f
                            );

                        material.SetColor(
                            "_Color",
                            c
                        );
                    }
                }
            }

            AddVisibilityLight(
                drone.gameObject,
                i
            );
        }

        // Make camera see everything at useful range
        Camera cam =
            player.GetComponentInChildren<Camera>();

        if (cam != null)
        {
            cam.farClipPlane = 500f;
        }

        EditorUtility.SetDirty(player);

        AssetDatabase.SaveAssets();

        Debug.Log(
            "========================================"
        );
        Debug.Log(
            "DRONE VISIBILITY FIXED"
        );
        Debug.Log(
            "Drones placed around player."
        );
        Debug.Log(
            "Drone size increased."
        );
        Debug.Log(
            "Renderers enabled."
        );
        Debug.Log(
            "========================================"
        );
    }

    static void AddVisibilityLight(
        GameObject drone,
        int index
    )
    {
        Transform old =
            drone.transform.Find(
                "VisibilityLight"
            );

        if (old != null)
            Object.DestroyImmediate(
                old.gameObject
            );

        GameObject lightObject =
            new GameObject(
                "VisibilityLight"
            );

        lightObject.transform.SetParent(
            drone.transform
        );

        lightObject.transform.localPosition =
            new Vector3(
                0f,
                0.15f,
                -0.4f
            );

        Light light =
            lightObject.AddComponent<Light>();

        light.type =
            LightType.Point;

        light.range =
            5f;

        light.intensity =
            1.5f;

        light.shadows =
            LightShadows.None;

        light.color =
            (index % 2 == 0)
            ? new Color(
                0.3f,
                1f,
                0.5f
            )
            : new Color(
                1f,
                0.35f,
                0.15f
            );
    }
}
