using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class SetupFPSMechanics
{
    [MenuItem("26247/SETUP FPS MECHANICS")]
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
        // REMOVE OLD PLAYER
        // ========================================================

        GameObject oldPlayer =
            GameObject.Find(
                "TraineePlayer"
            );

        if (oldPlayer != null)
            Object.DestroyImmediate(
                oldPlayer
            );

        // ========================================================
        // PLAYER
        // ========================================================

        GameObject player =
            new GameObject(
                "TraineePlayer"
            );

        player.transform.position =
            new Vector3(
                -72f,
                1.2f,
                -58f
            );

        CharacterController controller =
            player.AddComponent<
                CharacterController
            >();

        controller.height =
            1.8f;

        controller.radius =
            0.32f;

        controller.center =
            new Vector3(
                0f,
                0.9f,
                0f
            );

        controller.slopeLimit =
            50f;

        controller.stepOffset =
            0.35f;

        // ========================================================
        // CAMERA
        // ========================================================

        GameObject cameraObject =
            new GameObject(
                "PlayerCamera"
            );

        cameraObject.transform.SetParent(
            player.transform
        );

        cameraObject.transform.localPosition =
            new Vector3(
                0f,
                1.65f,
                0f
            );

        cameraObject.transform.localRotation =
            Quaternion.identity;

        Camera camera =
            cameraObject.AddComponent<
                Camera
            >();

        camera.fieldOfView =
            75f;

        camera.nearClipPlane =
            0.03f;

        camera.farClipPlane =
            500f;

        camera.tag =
            "MainCamera";

        cameraObject.AddComponent<
            AudioListener
        >();

        // ========================================================
        // FPS CONTROLLER
        // ========================================================

        FPSPlayerController fps =
            player.AddComponent<
                FPSPlayerController
            >();

        fps.playerCamera =
            camera;

        // ========================================================
        // SCANNER
        // ========================================================

        DroneScanner scanner =
            player.AddComponent<
                DroneScanner
            >();

        scanner.scanCamera =
            camera;

        // ========================================================
        // UI CANVAS
        // ========================================================

        GameObject canvasObject =
            GameObject.Find(
                "TrainingHUD"
            );

        if (canvasObject != null)
            Object.DestroyImmediate(
                canvasObject
            );

        canvasObject =
            new GameObject(
                "TrainingHUD"
            );

        Canvas canvas =
            canvasObject.AddComponent<
                Canvas
            >();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler =
            canvasObject.AddComponent<
                CanvasScaler
            >();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1920f,
                1080f
            );

        canvasObject.AddComponent<
            GraphicRaycaster
        >();

        // ========================================================
        // CROSSHAIR
        // ========================================================

        GameObject crosshair =
            new GameObject(
                "Crosshair"
            );

        crosshair.transform.SetParent(
            canvasObject.transform
        );

        RectTransform crossRect =
            crosshair.AddComponent<
                RectTransform
            >();

        crossRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        crossRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        crossRect.anchoredPosition =
            Vector2.zero;

        crossRect.sizeDelta =
            new Vector2(
                18f,
                18f
            );

        TextMeshProUGUI crossText =
            crosshair.AddComponent<
                TextMeshProUGUI
            >();

        crossText.text =
            "+";

        crossText.fontSize =
            24f;

        crossText.alignment =
            TextAlignmentOptions.Center;

        // ========================================================
        // SCAN STATUS
        // ========================================================

        GameObject scanObject =
            new GameObject(
                "ScanStatus"
            );

        scanObject.transform.SetParent(
            canvasObject.transform
        );

        RectTransform scanRect =
            scanObject.AddComponent<
                RectTransform
            >();

        scanRect.anchorMin =
            new Vector2(
                0.5f,
                0.12f
            );

        scanRect.anchorMax =
            new Vector2(
                0.5f,
                0.12f
            );

        scanRect.anchoredPosition =
            Vector2.zero;

        scanRect.sizeDelta =
            new Vector2(
                600f,
                50f
            );

        TextMeshProUGUI scanText =
            scanObject.AddComponent<
                TextMeshProUGUI
            >();

        scanText.text =
            "READY";

        scanText.fontSize =
            26f;

        scanText.alignment =
            TextAlignmentOptions.Center;

        // ========================================================
        // TARGET NAME
        // ========================================================

        GameObject targetObject =
            new GameObject(
                "TargetStatus"
            );

        targetObject.transform.SetParent(
            canvasObject.transform
        );

        RectTransform targetRect =
            targetObject.AddComponent<
                RectTransform
            >();

        targetRect.anchorMin =
            new Vector2(
                0.5f,
                0.07f
            );

        targetRect.anchorMax =
            new Vector2(
                0.5f,
                0.07f
            );

        targetRect.anchoredPosition =
            Vector2.zero;

        targetRect.sizeDelta =
            new Vector2(
                600f,
                40f
            );

        TextMeshProUGUI targetText =
            targetObject.AddComponent<
                TextMeshProUGUI
            >();

        targetText.text =
            "";

        targetText.fontSize =
            20f;

        targetText.alignment =
            TextAlignmentOptions.Center;

        // Connect UI
        scanner.scanText =
            scanText;

        scanner.targetText =
            targetText;

        // ========================================================
        // MOVEMENT INFO
        // ========================================================

        GameObject controlsObject =
            new GameObject(
                "Controls"
            );

        controlsObject.transform.SetParent(
            canvasObject.transform
        );

        RectTransform controlsRect =
            controlsObject.AddComponent<
                RectTransform
            >();

        controlsRect.anchorMin =
            new Vector2(
                0.02f,
                0.96f
            );

        controlsRect.anchorMax =
            new Vector2(
                0.02f,
                0.96f
            );

        controlsRect.pivot =
            new Vector2(
                0f,
                1f
            );

        controlsRect.anchoredPosition =
            Vector2.zero;

        controlsRect.sizeDelta =
            new Vector2(
                600f,
                110f
            );

        TextMeshProUGUI controlsText =
            controlsObject.AddComponent<
                TextMeshProUGUI
            >();

        controlsText.text =
            "WASD  MOVE     SHIFT  SPRINT\\n" +
            "SPACE  JUMP     LMB  SCOPE\\n" +
            "RMB / A  SCAN     ESC  UNLOCK MOUSE";

        controlsText.fontSize =
            18f;

        controlsText.alignment =
            TextAlignmentOptions.TopLeft;

        // ========================================================
        // SAVE
        // ========================================================

        Selection.activeGameObject =
            player;

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        AssetDatabase.SaveAssets();

        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "FPS MECHANICS SETUP COMPLETE"
        );

        Debug.Log(
            "WASD = Move"
        );

        Debug.Log(
            "SHIFT = Sprint"
        );

        Debug.Log(
            "SPACE = Jump"
        );

        Debug.Log(
            "LEFT CLICK = Scope / Zoom"
        );

        Debug.Log(
            "RIGHT CLICK / A = Scan"
        );

        Debug.Log(
            "ESC = Unlock Mouse"
        );

        Debug.Log(
            "========================================"
        );
    }
}
