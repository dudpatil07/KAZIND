using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

/// <summary>Head tracking and Touch controls; attaches at runtime, preserving the desktop scene.</summary>
[DefaultExecutionOrder(-200)]
public sealed class QuestPlayerRig : MonoBehaviour
{
    public static QuestPlayerRig Instance { get; private set; }
    public static bool Active => Instance != null && Instance.initialized;
    public bool APressed { get; private set; }
    public bool BPressed { get; private set; }
    public bool YPressed { get; private set; }
    public bool TriggerPressed { get; private set; }
    public int MenuStep { get; private set; }
    public bool HeadTracked { get; private set; }

    private readonly List<XRInputSubsystem> inputs = new List<XRInputSubsystem>();
    private FPSPlayerController player;
    private CharacterController body;
    private Camera head;
    private XRInputSubsystem tracking;
    private Vector3 desktopCameraPosition;
    private Quaternion desktopCameraRotation;
    private float originalHeight;
    private Vector3 originalCenter;
    private float footOffset;
    private float verticalSpeed;
    private bool initialized;
    private bool heldA, heldB, heldY, heldTrigger, menuLatched, turnLatched;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        SceneManager.sceneLoaded -= Attach;
        SceneManager.sceneLoaded += Attach;
    }

    private static void Attach(Scene scene, LoadSceneMode mode)
    {
        if (Instance != null) return;
        FPSPlayerController controller = FindFirstObjectByType<FPSPlayerController>();
        if (controller != null && controller.GetComponent<QuestPlayerRig>() == null)
            controller.gameObject.AddComponent<QuestPlayerRig>();
    }

    private void Awake()
    {
        Instance = this;
        player = GetComponent<FPSPlayerController>();
        body = GetComponent<CharacterController>();
        head = player.playerCamera != null ? player.playerCamera : Camera.main;
    }

    private void OnEnable() => Application.onBeforeRender += UpdateHeadPose;
    private void OnDisable()
    {
        Application.onBeforeRender -= UpdateHeadPose;
        if (!initialized) return;
        initialized = false;
        if (head != null)
        {
            head.transform.localPosition = desktopCameraPosition;
            head.transform.localRotation = desktopCameraRotation;
        }
        if (body != null) { body.height = originalHeight; body.center = originalCenter; }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!initialized)
        {
            if (!XRSettings.isDeviceActive || head == null || body == null) return;
            SubsystemManager.GetSubsystems(inputs);
            tracking = inputs.Find(input => input.running);
            if (tracking == null) return;
            tracking.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            desktopCameraPosition = head.transform.localPosition;
            desktopCameraRotation = head.transform.localRotation;
            originalHeight = body.height;
            originalCenter = body.center;
            footOffset = body.center.y - body.height * 0.5f;
            head.nearClipPlane = 0.05f;
            initialized = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }

        UpdateHeadPose();
        bool canRead = Application.isFocused && HeadTracked;
        InputDevice left = canRead ? InputDevices.GetDeviceAtXRNode(XRNode.LeftHand) : default;
        InputDevice right = canRead ? InputDevices.GetDeviceAtXRNode(XRNode.RightHand) : default;
        APressed = Rising(right, CommonUsages.primaryButton, ref heldA);
        BPressed = Rising(right, CommonUsages.secondaryButton, ref heldB);
        YPressed = Rising(left, CommonUsages.secondaryButton, ref heldY);
        TriggerPressed = Rising(right, CommonUsages.triggerButton, ref heldTrigger);
        left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 move);
        right.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 turn);
        float menuAxis = Mathf.Abs(move.y) >= Mathf.Abs(move.x) ? -move.y : move.x;
        MenuStep = 0;
        if (Mathf.Abs(menuAxis) < 0.3f) menuLatched = false;
        if (!menuLatched && Mathf.Abs(menuAxis) > 0.7f)
        {
            MenuStep = menuAxis > 0 ? 1 : -1;
            menuLatched = true;
        }

        if (!player.GameplayInputEnabled || !canRead)
        {
            verticalSpeed = 0;
            turnLatched = Mathf.Abs(turn.x) > 0.3f;
            return;
        }
        if (Mathf.Abs(turn.x) < 0.3f) turnLatched = false;
        if (!turnLatched && Mathf.Abs(turn.x) > 0.7f)
        {
            // Turn around the user's head, not the centre of the play area.
            transform.RotateAround(head.transform.position, Vector3.up, Mathf.Sign(turn.x) * 30f);
            turnLatched = true;
        }

        // Keep the collision capsule under the tracked head, including room-scale movement.
        Vector3 localHead = transform.InverseTransformPoint(head.transform.position);
        body.height = Mathf.Clamp(localHead.y - footOffset, 0.8f, 2.4f);
        body.center = new Vector3(localHead.x, footOffset + body.height * 0.5f, localHead.z);
        Vector3 forward = Vector3.ProjectOnPlane(head.transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forward);
        move = move.magnitude < 0.2f ? Vector2.zero : Vector2.ClampMagnitude(move, 1f);
        Vector3 velocity = (forward * move.y + rightDirection * move.x) * 2f;
        verticalSpeed = body.isGrounded ? -2f : verticalSpeed + Physics.gravity.y * Time.deltaTime;
        velocity.y = verticalSpeed;
        if (body.enabled) body.Move(velocity * Time.deltaTime);
    }

    private void UpdateHeadPose()
    {
        if (!initialized || head == null) return;
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        HeadTracked = device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked;
        if (!HeadTracked) return;
        bool floor = tracking != null && tracking.GetTrackingOriginMode() == TrackingOriginModeFlags.Floor;
        if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            head.transform.localPosition = position + Vector3.up * (footOffset + (floor ? 0f : 1.65f));
        if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            head.transform.localRotation = rotation;
    }

    private static bool Rising(InputDevice device, InputFeatureUsage<bool> usage, ref bool held)
    {
        bool down = device.TryGetFeatureValue(usage, out bool value) && value;
        bool pressed = down && !held;
        held = down;
        return pressed;
    }
}
