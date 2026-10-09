using UnityEngine;
using UnityEditor;

public static class SetupDroneAI
{
    [MenuItem("26247/SETUP DRONE AI")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError(
                "Stop Play Mode first."
            );

            return;
        }

        GameObject player =
            GameObject.Find(
                "TraineePlayer"
            );

        if (player == null)
        {
            Debug.LogError(
                "TraineePlayer not found."
            );

            return;
        }

        TrainingDrone[] drones =
            Object.FindObjectsByType<TrainingDrone>(
                FindObjectsSortMode.None
            );

        if (drones.Length == 0)
        {
            Debug.LogError(
                "No TrainingDrone objects found. Run SETUP DRONE THREAT SYSTEM first."
            );

            return;
        }

        // --------------------------------------------------------
        // Assign each existing drone to its own AI slot.
        // No drones are recreated.
        // --------------------------------------------------------

        for (int i = 0; i < drones.Length; i++)
        {
            TrainingDrone drone =
                drones[i];

            if (drone == null)
                continue;

            drone.gameObject.SetActive(
                true
            );

            drone.activationDistance =
                45f;

            drone.patrolRadius =
                18f +
                (i % 3) * 5f;

            drone.approachSpeed =
                6f +
                (i % 3) * 1f;

            drone.surroundSpeed =
                5.5f +
                (i % 2) * 1.5f;

            drone.surroundDistance =
                18f;

            drone.surroundHeight =
                16f +
                (i % 3) * 3f;

            drone.minimumAltitude =
                10f;

            drone.maximumAltitude =
                50f;

            drone.ConfigureAI(
                player.transform,
                i
            );

            EditorUtility.SetDirty(
                drone
            );
        }

        AssetDatabase.SaveAssets();

        Selection.activeGameObject =
            player;

        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "DRONE AI SETUP COMPLETE"
        );

        Debug.Log(
            "Player base activation radius: 45m"
        );

        Debug.Log(
            "Drones patrol while player stays in base."
        );

        Debug.Log(
            "Leaving base activates drone approach."
        );

        Debug.Log(
            "Drones then take surrounding positions."
        );

        Debug.Log(
            "Minimum drone altitude: 10m"
        );

        Debug.Log(
            "========================================"
        );
    }
}
