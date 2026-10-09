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
        if (Keyboard.current == null ||
            Mouse.current == null)
            return;

        bool scanPressed =
            Keyboard.current.aKey.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame;

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
            currentTarget =
                hit.collider.gameObject;

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

        if (!IsSameTarget(
            hit.collider.gameObject,
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

        Transform root =
            target.transform.root;

        return root.name;
    }

    bool IsSameTarget(
        GameObject a,
        GameObject b
    )
    {
        if (a == null || b == null)
            return false;

        return
            a == b ||
            a.transform.root ==
            b.transform.root;
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
