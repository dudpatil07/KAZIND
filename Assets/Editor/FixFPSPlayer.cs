using UnityEngine;
using UnityEditor;

public static class FixFPSPlayer
{
    [MenuItem("26247/FIX FPS PLAYER")]
    public static void Fix()
    {
        if (Application.isPlaying)
        {
            Debug.LogError(
                "Stop Play Mode first."
            );
            return;
        }

        // --------------------------------------------------------
        // Find / create player
        // --------------------------------------------------------

        GameObject player =
            GameObject.Find(
                "TraineePlayer"
            );

        if (player == null)
        {
            player =
                new GameObject(
                    "TraineePlayer"
                );
        }

        // Put player in a safe position
        player.transform.position =
            new Vector3(
                -70f,
                3f,
                -55f
            );

        player.transform.rotation =
            Quaternion.identity;

        // --------------------------------------------------------
        // Character Controller
        // --------------------------------------------------------

        CharacterController cc =
            player.GetComponent<
                CharacterController
            >();

        if (cc == null)
            cc =
                player.AddComponent<
                    CharacterController
                >();

        cc.enabled = false;

        cc.height =
            1.8f;

        cc.radius =
            0.3f;

        cc.skinWidth =
            0.08f;

        cc.minMoveDistance =
            0.001f;

        cc.slopeLimit =
            50f;

        cc.stepOffset =
            0.35f;

        cc.center =
            new Vector3(
                0f,
                0.9f,
                0f
            );

        cc.enabled = true;

        // --------------------------------------------------------
        // Camera
        // --------------------------------------------------------

        Transform cameraChild =
            player.transform.Find(
                "PlayerCamera"
            );

        GameObject cameraObject;

        if (cameraChild != null)
        {
            cameraObject =
                cameraChild.gameObject;
        }
        else
        {
            cameraObject =
                new GameObject(
                    "PlayerCamera"
                );

            cameraObject.transform.SetParent(
                player.transform
            );
        }

        cameraObject.transform.localPosition =
            new Vector3(
                0f,
                1.62f,
                0f
            );

        cameraObject.transform.localRotation =
            Quaternion.identity;

        Camera cam =
            cameraObject.GetComponent<
                Camera
            >();

        if (cam == null)
            cam =
                cameraObject.AddComponent<
                    Camera
                >();

        cam.enabled = true;

        cam.fieldOfView =
            75f;

        cam.nearClipPlane =
            0.03f;

        cam.farClipPlane =
            500f;

        cameraObject.tag =
            "MainCamera";

        // --------------------------------------------------------
        // Remove duplicate audio listeners
        // --------------------------------------------------------

        AudioListener[] listeners =
            Object.FindObjectsByType<
                AudioListener
            >(
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < listeners.Length;
             i++)
        {
            if (listeners[i].gameObject !=
                cameraObject)
            {
                Object.DestroyImmediate(
                    listeners[i]
                );
            }
        }

        if (
            cameraObject.GetComponent<
                AudioListener
            >() == null
        )
        {
            cameraObject.AddComponent<
                AudioListener
            >();
        }

        // --------------------------------------------------------
        // Controller
        // --------------------------------------------------------

        FPSPlayerController controller =
            player.GetComponent<
                FPSPlayerController
            >();

        if (controller == null)
            controller =
                player.AddComponent<
                    FPSPlayerController
                >();

        controller.playerCamera =
            cam;

        controller.walkSpeed =
            5f;

        controller.sprintSpeed =
            8.5f;

        controller.jumpHeight =
            1.5f;

        controller.gravity =
            -25f;

        controller.normalFOV =
            75f;

        controller.scopeFOV =
            35f;

        // --------------------------------------------------------
        // Camera is player camera
        // --------------------------------------------------------

        Camera[] cameras =
            Object.FindObjectsByType<
                Camera
            >(
                FindObjectsSortMode.None
            );

        foreach (Camera c in cameras)
        {
            if (c != cam)
                c.enabled = false;
        }

        cam.enabled = true;

        Selection.activeGameObject =
            player;

        EditorUtility.SetDirty(
            player
        );

        AssetDatabase.SaveAssets();

        Debug.Log(
            "======================================"
        );

        Debug.Log(
            "FPS PLAYER FIXED"
        );

        Debug.Log(
            "Position: -70, 3, -55"
        );

        Debug.Log(
            "WASD = MOVE"
        );

        Debug.Log(
            "SHIFT = SPRINT"
        );

        Debug.Log(
            "SPACE = JUMP"
        );

        Debug.Log(
            "LEFT CLICK = LOCK + SCOPE"
        );

        Debug.Log(
            "ESC = RELEASE MOUSE"
        );

        Debug.Log(
            "======================================"
        );
    }
}
