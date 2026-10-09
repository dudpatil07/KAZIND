using UnityEngine;
using UnityEditor;

public static class FixPlayerNow
{
    [MenuItem("26247/FIX PLAYER NOW")]
    public static void Fix()
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
            player =
                new GameObject(
                    "TraineePlayer"
                );
        }

        // --------------------------------------------------------
        // HARD RESET TRANSFORM
        // --------------------------------------------------------

        player.transform.position =
            new Vector3(
                -70f,
                2f,
                -55f
            );

        player.transform.rotation =
            Quaternion.identity;

        player.transform.localScale =
            Vector3.one;

        // --------------------------------------------------------
        // CHARACTER CONTROLLER
        // --------------------------------------------------------

        CharacterController cc =
            player.GetComponent<
                CharacterController
            >();

        if (cc == null)
        {
            cc =
                player.AddComponent<
                    CharacterController
                >();
        }

        cc.enabled = false;

        cc.height = 1.8f;
        cc.radius = 0.3f;

        cc.center =
            new Vector3(
                0f,
                0.9f,
                0f
            );

        cc.skinWidth = 0.08f;
        cc.stepOffset = 0.35f;
        cc.slopeLimit = 50f;

        cc.enabled = true;

        // --------------------------------------------------------
        // CAMERA
        // --------------------------------------------------------

        Transform cameraTransform =
            player.transform.Find(
                "PlayerCamera"
            );

        GameObject cameraObject;

        if (cameraTransform == null)
        {
            cameraObject =
                new GameObject(
                    "PlayerCamera"
                );

            cameraObject.transform.SetParent(
                player.transform
            );
        }
        else
        {
            cameraObject =
                cameraTransform.gameObject;
        }

        cameraObject.transform.localPosition =
            new Vector3(
                0f,
                1.62f,
                0f
            );

        cameraObject.transform.localRotation =
            Quaternion.identity;

        cameraObject.transform.localScale =
            Vector3.one;

        Camera cam =
            cameraObject.GetComponent<
                Camera
            >();

        if (cam == null)
        {
            cam =
                cameraObject.AddComponent<
                    Camera
                >();
        }

        cam.enabled = true;
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.03f;
        cam.farClipPlane = 500f;

        cameraObject.tag =
            "MainCamera";

        // --------------------------------------------------------
        // AUDIO
        // --------------------------------------------------------

        AudioListener[] listeners =
            Object.FindObjectsByType<
                AudioListener
            >(
                FindObjectsSortMode.None
            );

        foreach (
            AudioListener listener
            in listeners
        )
        {
            if (
                listener.gameObject !=
                cameraObject
            )
            {
                Object.DestroyImmediate(
                    listener
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
        // FPS CONTROLLER
        // --------------------------------------------------------

        FPSPlayerController fps =
            player.GetComponent<
                FPSPlayerController
            >();

        if (fps == null)
        {
            fps =
                player.AddComponent<
                    FPSPlayerController
                >();
        }

        fps.playerCamera =
            cam;

        fps.walkSpeed = 5f;
        fps.sprintSpeed = 8f;
        fps.jumpHeight = 1.5f;
        fps.gravity = -25f;

        fps.mouseSensitivity = 0.08f;
        fps.normalFOV = 75f;
        fps.scopeFOV = 35f;

        // --------------------------------------------------------
        // Disable other cameras
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
            "PLAYER HARD RESET COMPLETE"
        );

        Debug.Log(
            "Position = (-70, 2, -55)"
        );

        Debug.Log(
            "Rotation = (0, 0, 0)"
        );

        Debug.Log(
            "WASD = MOVE"
        );

        Debug.Log(
            "MOUSE = LOOK"
        );

        Debug.Log(
            "SHIFT = SPRINT"
        );

        Debug.Log(
            "SPACE = JUMP"
        );

        Debug.Log(
            "LEFT CLICK = SCOPE"
        );

        Debug.Log(
            "ESC = RELEASE MOUSE"
        );

        Debug.Log(
            "======================================"
        );
    }
}
