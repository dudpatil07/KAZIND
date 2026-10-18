using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class DroneScanner : MonoBehaviour
{
    [Header("Scan")]
    public float scanRange = 250f;
    public float scanDuration = 2.5f;
    public LayerMask droneLayer = ~0;

    [Header("Camera")]
    public Camera scanCamera;

    [Header("UI")]
    public TextMeshProUGUI scanText;
    public TextMeshProUGUI targetText;

    private float scanTimer;
    private bool scanning;
    private GameObject currentTarget;

    void Awake()
    {
        if (scanCamera == null)
            scanCamera =
                Camera.main;

        SetUI(
            "READY",
            ""
        );
    }

    void Update()
    {
        bool scanPressed =
            (Keyboard.current != null &&
             Keyboard.current.fKey.wasPressedThisFrame) ||
            (Mouse.current != null &&
             Mouse.current.rightButton.wasPressedThisFrame);

        if (scanPressed)
            StartScan();

        if (scanning)
            ContinueScan();
    }

    // ==========================================================
    // START SCAN
    // ==========================================================

    void StartScan()
    {
        if (scanCamera == null)
            scanCamera = Camera.main;

        if (scanCamera == null)
            return;

        Ray ray =
            scanCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        RaycastHit hit;

        if (Physics.Raycast(
            ray,
            out hit,
            scanRange,
            droneLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            TrainingDrone drone =
                hit.collider.GetComponentInParent<TrainingDrone>();

            if (drone == null)
            {
                scanning = false;
                currentTarget = null;
                SetUI("NO DRONE", "");
                return;
            }

            currentTarget = drone.gameObject;

            scanTimer =
                0f;

            scanning =
                true;

            SetUI(
                "SCANNING...",
                GetTargetName(currentTarget)
            );
        }
        else
        {
            scanning = false;
            currentTarget = null;

            SetUI(
                "NO TARGET",
                ""
            );
        }
    }

    // ==========================================================
    // CONTINUE
    // ==========================================================

    void ContinueScan()
    {
        if (scanCamera == null)
            return;

        if (currentTarget == null ||
            !currentTarget.activeInHierarchy)
        {
            CancelScan();
            return;
        }

        // Make sure we are still looking at the target
        Ray ray =
            scanCamera.ViewportPointToRay(
                new Vector3(
                    0.5f,
                    0.5f,
                    0f
                )
            );

        RaycastHit hit;

        if (!Physics.Raycast(
            ray,
            out hit,
            scanRange,
            droneLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            CancelScan();
            return;
        }

        TrainingDrone hitDrone =
            hit.collider.GetComponentInParent<TrainingDrone>();

        if (hitDrone == null || !IsSameTarget(
            hitDrone.gameObject,
            currentTarget
        ))
        {
            CancelScan();
            return;
        }

        scanTimer +=
            Time.deltaTime;

        float remaining =
            Mathf.Max(
                0f,
                scanDuration - scanTimer
            );

        SetUI(
            "SCANNING  " +
            remaining.ToString("0.0") +
            "s",
            GetTargetName(currentTarget)
        );

        if (scanTimer >= scanDuration)
        {
            FinishScan();
        }
    }

    // ==========================================================
    // FINISH
    // ==========================================================

    void FinishScan()
    {
        scanning = false;

        string targetName =
            GetTargetName(
                currentTarget
            );

        SetUI(
            "✓ DRONE IDENTIFIED",
            targetName
        );

        Debug.Log(
            "Drone scan complete: " +
            targetName
        );
    }

    // ==========================================================
    // CANCEL
    // ==========================================================

    void CancelScan()
    {
        scanning =
            false;

        currentTarget =
            null;

        SetUI(
            "SCAN LOST",
            ""
        );
    }

    // ==========================================================
    // TARGET NAME
    // ==========================================================

    string GetTargetName(
        GameObject target
    )
    {
        if (target == null)
            return "";

        TrainingDrone drone =
            target.GetComponent<TrainingDrone>();

        return drone != null && !string.IsNullOrWhiteSpace(drone.displayName)
            ? drone.displayName
            : target.name;
    }

    bool IsSameTarget(
        GameObject a,
        GameObject b
    )
    {
        if (a == null || b == null)
            return false;

        return a == b;
    }

    // ==========================================================
    // UI
    // ==========================================================

    void SetUI(
        string status,
        string target
    )
    {
        if (scanText != null)
            scanText.text =
                status;

        if (targetText != null)
            targetText.text =
                target;
    }
}
