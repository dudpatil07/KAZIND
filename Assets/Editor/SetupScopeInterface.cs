using UnityEngine;
using UnityEditor;

public static class SetupScopeInterface
{
    [MenuItem("26247/SETUP SCOPE INTERFACE")]
    public static void Setup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError(
                "Stop Play Mode first."
            );

            return;
        }

        GameObject playerObject =
            GameObject.Find(
                "TraineePlayer"
            );

        if (playerObject == null)
        {
            Debug.LogError(
                "TraineePlayer not found. Run FIX PLAYER NOW first."
            );

            return;
        }

        FPSPlayerController player =
            playerObject.GetComponent<
                FPSPlayerController
            >();

        if (player == null)
        {
            Debug.LogError(
                "FPSPlayerController not found."
            );

            return;
        }

        // --------------------------------------------------------
        // Find camera
        // --------------------------------------------------------

        Camera cam =
            playerObject.GetComponentInChildren<
                Camera
            >();

        if (cam == null)
        {
            Debug.LogError(
                "Player camera not found."
            );

            return;
        }

        // --------------------------------------------------------
        // Remove old scope
        // --------------------------------------------------------

        ScopeOverlay old =
            cam.GetComponent<
                ScopeOverlay
            >();

        if (old != null)
            Object.DestroyImmediate(
                old
            );

        // --------------------------------------------------------
        // Add scope
        // --------------------------------------------------------

        ScopeOverlay scope =
            cam.gameObject.AddComponent<
                ScopeOverlay
            >();

        scope.player =
            player;

        scope.scopeSize =
            0.82f;

        scope.reticleThickness =
            2f;

        scope.vignetteAlpha =
            0.88f;

        scope.showScopeInfo =
            true;

        // --------------------------------------------------------
        // Make sure zoom values are correct
        // --------------------------------------------------------

        player.normalFOV =
            75f;

        player.scopeFOV =
            30f;

        player.zoomSpeed =
            12f;

        // --------------------------------------------------------
        // Save
        // --------------------------------------------------------

        EditorUtility.SetDirty(
            playerObject
        );

        AssetDatabase.SaveAssets();

        Selection.activeGameObject =
            cam.gameObject;

        Debug.Log(
            "======================================"
        );

        Debug.Log(
            "SCOPE INTERFACE READY"
        );

        Debug.Log(
            "LMB = 2.5x scope"
        );

        Debug.Log(
            "Circular optic enabled"
        );

        Debug.Log(
            "Reticle enabled"
        );

        Debug.Log(
            "Range marks enabled"
        );

        Debug.Log(
            "======================================"
        );
    }
}
