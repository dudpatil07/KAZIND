using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1000)]
public class DroneThreatSystem : MonoBehaviour
{
    public enum State
    {
        Idle,
        Detecting,
        Detected,
        Scanning,
        Classification
    }

    [Header("Detection")]
    public float detectionTime = 10f;
    public float maxRange = 300f;

    [Header("Scanning")]
    public float scanTime = 1.5f;

    [Header("References")]
    public Camera playerCamera;

    [Header("Training")]
    public int score = 0;

    private State state =
        State.Idle;

    private TrainingDrone currentTarget;

    private float detectionTimer;
    private float scanTimer;

    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera =
                GetComponentInParent<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera =
                Camera.main;
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================

    void Update()
    {
        if (
            playerCamera == null ||
            Keyboard.current == null
        )
            return;

        switch (state)
        {
            case State.Idle:
            case State.Detecting:

                HandleDetection();

                break;

            case State.Detected:

                HandleDetected();

                break;

            case State.Scanning:

                HandleScanning();

                break;

            case State.Classification:

                HandleClassification();

                break;
        }
    }

    // ============================================================
    // AIMED DRONE
    // ============================================================

    TrainingDrone GetAimedDrone()
    {
        Ray ray =
            playerCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        RaycastHit hit;

        if (
            Physics.Raycast(
                ray,
                out hit,
                maxRange,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            return
                hit.collider.GetComponentInParent<
                    TrainingDrone
                >();
        }

        return null;
    }

    // ============================================================
    // DETECTION
    // ============================================================

    void HandleDetection()
    {
        TrainingDrone target =
            GetAimedDrone();

        if (target == null)
        {
            currentTarget =
                null;

            detectionTimer =
                0f;

            state =
                State.Idle;

            return;
        }

        if (target != currentTarget)
        {
            currentTarget =
                target;

            detectionTimer =
                0f;

            state =
                State.Detecting;
        }

        detectionTimer +=
            Time.deltaTime;

        if (
            detectionTimer >=
            detectionTime
        )
        {
            detectionTimer =
                detectionTime;

            state =
                State.Detected;
        }
    }

    // ============================================================
    // DETECTED
    // ============================================================

    void HandleDetected()
    {
        TrainingDrone target =
            GetAimedDrone();

        if (target != currentTarget)
        {
            ResetTarget();
            return;
        }

        if (
            Keyboard.current.eKey
                .wasPressedThisFrame
        )
        {
            scanTimer =
                0f;

            state =
                State.Scanning;
        }
    }

    // ============================================================
    // SCANNING
    // ============================================================

    void HandleScanning()
    {
        TrainingDrone target =
            GetAimedDrone();

        if (target != currentTarget)
        {
            ResetTarget();
            return;
        }

        scanTimer +=
            Time.deltaTime;

        if (
            scanTimer >=
            scanTime
        )
        {
            scanTimer =
                scanTime;

            state =
                State.Classification;
        }
    }

    // ============================================================
    // CLASSIFICATION
    // ============================================================

    void HandleClassification()
    {
        TrainingDrone target =
            GetAimedDrone();

        if (target != currentTarget)
        {
            ResetTarget();
            return;
        }

        // J = CHEAP
        if (
            Keyboard.current.jKey
                .wasPressedThisFrame
        )
        {
            AttemptClassification(
                TrainingDrone.DroneClass.Cheap
            );

            return;
        }

        // K = MILITARY
        if (
            Keyboard.current.kKey
                .wasPressedThisFrame
        )
        {
            AttemptClassification(
                TrainingDrone.DroneClass.Military
            );
        }
    }

    // ============================================================
    // RESULT
    // ============================================================

    void AttemptClassification(
        TrainingDrone.DroneClass selectedClass
    )
    {
        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }

        bool correct =
            selectedClass ==
            currentTarget.droneClass;

        if (correct)
        {
            score += 100;

            Vector3 blastPosition =
                currentTarget.transform.position;

            Debug.Log(
                "CORRECT: " +
                currentTarget.GetClassText()
            );

            // Explosion
            DroneExplosionFX.Play(
                blastPosition
            );

            // Remove drone
            currentTarget.gameObject
                .SetActive(false);

            currentTarget =
                null;

            detectionTimer =
                0f;

            scanTimer =
                0f;

            state =
                State.Idle;
        }
        else
        {
            score -= 50;

            Debug.Log(
                "WRONG CLASSIFICATION"
            );

            // Drone survives
            currentTarget =
                null;

            detectionTimer =
                0f;

            scanTimer =
                0f;

            state =
                State.Idle;
        }
    }

    // ============================================================
    // RESET
    // ============================================================

    void ResetTarget()
    {
        currentTarget =
            null;

        detectionTimer =
            0f;

        scanTimer =
            0f;

        state =
            State.Idle;
    }

    // ============================================================
    // SCOPE DETECTION
    // ============================================================

    bool IsScoped()
    {
        if (Mouse.current == null)
            return false;

        bool mouseScope =
            Mouse.current.leftButton.isPressed;

        bool keyScope =
            Keyboard.current != null &&
            Keyboard.current.oKey.isPressed;

        return
            mouseScope ||
            keyScope;
    }

    // ============================================================
    // GUI
    // ============================================================

    void OnGUI()
    {
        TrainingDrone target =
            GetAimedDrone();

        bool scoped =
            IsScoped();

        DrawCrosshair();

        // --------------------------------------------------------
        // SCORE
        // --------------------------------------------------------

        GUIStyle scoreStyle =
            new GUIStyle();

        scoreStyle.fontSize =
            20;

        scoreStyle.fontStyle =
            FontStyle.Bold;

        scoreStyle.normal.textColor =
            Color.white;

        GUI.Label(
            new Rect(
                20f,
                20f,
                300f,
                35f
            ),
            "SCORE: " +
            score,
            scoreStyle
        );

        // --------------------------------------------------------
        // NOTHING AIMED AT
        // --------------------------------------------------------

        if (target == null)
            return;

        // --------------------------------------------------------
        // TARGET INFORMATION
        // ONLY WHEN ZOOMED
        // --------------------------------------------------------

        if (scoped)
        {
            DrawTargetInfo(
                target
            );
        }

        // --------------------------------------------------------
        // STATE
        // --------------------------------------------------------

        DrawState();
    }

    // ============================================================
    // CROSSHAIR
    // ============================================================

    void DrawCrosshair()
    {
        GUIStyle style =
            new GUIStyle();

        style.alignment =
            TextAnchor.MiddleCenter;

        style.fontSize =
            24;

        style.fontStyle =
            FontStyle.Bold;

        style.normal.textColor =
            new Color(
                0.55f,
                1f,
                0.60f,
                0.95f
            );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 20f,
                Screen.height * 0.5f - 20f,
                40f,
                40f
            ),
            "+",
            style
        );
    }

    // ============================================================
    // TARGET INFO
    // ============================================================

    void DrawTargetInfo(
        TrainingDrone target
    )
    {
        float boxWidth =
            330f;

        float boxHeight =
            105f;

        float x =
            Screen.width -
            boxWidth -
            28f;

        float y =
            85f;

        // Background
        GUI.color =
            new Color(
                0.02f,
                0.05f,
                0.035f,
                0.88f
            );

        GUI.DrawTexture(
            new Rect(
                x,
                y,
                boxWidth,
                boxHeight
            ),
            Texture2D.whiteTexture
        );

        GUI.color =
            Color.white;

        GUIStyle title =
            new GUIStyle();

        title.fontSize =
            17;

        title.fontStyle =
            FontStyle.Bold;

        title.normal.textColor =
            new Color(
                0.55f,
                1f,
                0.65f
            );

        GUI.Label(
            new Rect(
                x + 16f,
                y + 10f,
                290f,
                25f
            ),
            "TARGET SIGNATURE",
            title
        );

        GUIStyle info =
            new GUIStyle();

        info.fontSize =
            15;

        info.normal.textColor =
            Color.white;

        GUI.Label(
            new Rect(
                x + 16f,
                y + 40f,
                290f,
                25f
            ),
            "AIRFRAME: " +
            target.GetAirframeType(),
            info
        );

        GUI.Label(
            new Rect(
                x + 16f,
                y + 66f,
                290f,
                25f
            ),
            "THREAT LEVEL: " +
            target.GetThreatLevel(),
            info
        );
    }

    // ============================================================
    // STATE UI
    // ============================================================

    void DrawState()
    {
        float center =
            Screen.width *
            0.5f;

        GUIStyle status =
            new GUIStyle();

        status.alignment =
            TextAnchor.MiddleCenter;

        status.fontStyle =
            FontStyle.Bold;

        status.fontSize =
            25;

        status.normal.textColor =
            Color.white;

        string text =
            "";

        switch (state)
        {
            case State.Detecting:

                float remaining =
                    Mathf.Max(
                        0f,
                        detectionTime -
                        detectionTimer
                    );

                text =
                    "DETECTING  " +
                    remaining.ToString("0.0") +
                    " s";

                break;

            case State.Detected:

                text =
                    "✓ DRONE DETECTED";

                break;

            case State.Scanning:

                float scanRemaining =
                    Mathf.Max(
                        0f,
                        scanTime -
                        scanTimer
                    );

                text =
                    "SCANNING  " +
                    scanRemaining.ToString("0.0") +
                    " s";

                break;

            case State.Classification:

                text =
                    "SCAN COMPLETE — CLASSIFY";

                break;

            default:

                text =
                    "TRACK TARGET";

                break;
        }

        GUI.Label(
            new Rect(
                center - 350f,
                45f,
                700f,
                40f
            ),
            text,
            status
        );

        GUIStyle instruction =
            new GUIStyle();

        instruction.alignment =
            TextAnchor.MiddleCenter;

        instruction.fontSize =
            18;

        instruction.normal.textColor =
            new Color(
                0.85f,
                0.90f,
                0.85f
            );

        string help =
            "";

        switch (state)
        {
            case State.Detecting:

                help =
                    "KEEP AIMING AT TARGET";

                break;

            case State.Detected:

                help =
                    "PRESS E TO SCAN";

                break;

            case State.Scanning:

                help =
                    "KEEP AIMING";

                break;

            case State.Classification:

                help =
                    "J = CHEAP       K = MILITARY";

                break;
        }

        GUI.Label(
            new Rect(
                center - 350f,
                82f,
                700f,
                30f
            ),
            help,
            instruction
        );
    }
}
