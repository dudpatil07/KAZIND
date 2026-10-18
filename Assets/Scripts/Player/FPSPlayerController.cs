using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -25f;

    [Header("Mouse")]
    public float mouseSensitivity = 0.08f;
    public float maxLookAngle = 85f;

    [Header("Camera")]
    public Camera playerCamera;
    public float normalFOV = 75f;
    public float scopeFOV = 35f;
    public float zoomSpeed = 10f;

    private CharacterController controller;

    private float verticalVelocity;
    private float pitch;
    private float yaw;

    private bool mouseLocked;
    private bool gameplayInputEnabled = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera =
                GetComponentInChildren<Camera>();

        // Start with a valid rotation
        SanitizeTransform();

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.fieldOfView = normalFOV;
        }
    }

    void Start()
    {
        SanitizeTransform();

        yaw =
            transform.eulerAngles.y;

        if (float.IsNaN(yaw) ||
            float.IsInfinity(yaw))
        {
            yaw = 0f;
        }

        pitch = 0f;

        LockMouse();

        Debug.Log(
            "FPS CONTROLLER READY"
        );
    }

    void Update()
    {
        if (!gameplayInputEnabled)
            return;

        if (Keyboard.current == null)
            return;

        // Very important:
        // never allow NaN rotations/positions
        SanitizeTransform();

        HandleCursor();
        HandleLook();
        HandleMovement();
        HandleScope();
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        gameplayInputEnabled = enabled;

        if (enabled)
            LockMouse();
        else
            UnlockMouse();
    }

    // ============================================================
    // SANITY CHECK
    // ============================================================

    void SanitizeTransform()
    {
        Vector3 p =
            transform.position;

        Vector3 s =
            transform.localScale;

        Quaternion r =
            transform.rotation;

        bool badPosition =
            IsInvalid(p.x) ||
            IsInvalid(p.y) ||
            IsInvalid(p.z);

        bool badScale =
            IsInvalid(s.x) ||
            IsInvalid(s.y) ||
            IsInvalid(s.z);

        bool badRotation =
            IsInvalid(r.x) ||
            IsInvalid(r.y) ||
            IsInvalid(r.z) ||
            IsInvalid(r.w);

        if (badPosition)
        {
            transform.position =
                new Vector3(
                    -70f,
                    2f,
                    -55f
                );
        }

        if (badScale)
        {
            transform.localScale =
                Vector3.one;
        }

        if (badRotation)
        {
            transform.rotation =
                Quaternion.identity;

            yaw = 0f;
            pitch = 0f;
        }
    }

    bool IsInvalid(float value)
    {
        return
            float.IsNaN(value) ||
            float.IsInfinity(value);
    }

    // ============================================================
    // CURSOR
    // ============================================================

    void HandleCursor()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockMouse();
            return;
        }

        if (!mouseLocked &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            LockMouse();
        }

        if (mouseLocked &&
            Application.isFocused)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }
    }

    void LockMouse()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        mouseLocked = true;
    }

    void UnlockMouse()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        mouseLocked = false;
    }

    void OnApplicationFocus(bool focused)
    {
        if (focused && mouseLocked)
            LockMouse();
    }

    // ============================================================
    // MOUSE LOOK
    // ============================================================

    void HandleLook()
    {
        if (!mouseLocked)
            return;

        if (Mouse.current == null)
            return;

        if (playerCamera == null)
            return;

        Vector2 delta =
            Mouse.current.delta.ReadValue();

        // Prevent Linux/InputSystem NaN from
        // destroying player rotation.
        if (!IsFinite(delta.x) ||
            !IsFinite(delta.y))
        {
            return;
        }

        float mouseX =
            delta.x *
            mouseSensitivity;

        float mouseY =
            delta.y *
            mouseSensitivity;

        if (!IsFinite(mouseX) ||
            !IsFinite(mouseY))
        {
            return;
        }

        yaw += mouseX;

        pitch -= mouseY;

        pitch =
            Mathf.Clamp(
                pitch,
                -maxLookAngle,
                maxLookAngle
            );

        // Explicitly construct valid rotations
        transform.rotation =
            Quaternion.Euler(
                0f,
                yaw,
                0f
            );

        playerCamera.transform.localRotation =
            Quaternion.Euler(
                pitch,
                0f,
                0f
            );
    }

    bool IsFinite(float value)
    {
        return
            !float.IsNaN(value) &&
            !float.IsInfinity(value);
    }

    // ============================================================
    // MOVEMENT
    // ============================================================

    void HandleMovement()
    {
        if (controller == null ||
            !controller.enabled)
            return;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed)
            z += 1f;

        if (Keyboard.current.sKey.isPressed)
            z -= 1f;

        if (Keyboard.current.aKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            x += 1f;

        Vector3 input =
            new Vector3(
                x,
                0f,
                z
            );

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        bool sprinting =
            Keyboard.current.leftShiftKey.isPressed &&
            z > 0f;

        float speed =
            sprinting
            ? sprintSpeed
            : walkSpeed;

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.z;

        move *= speed;

        // --------------------------------------------------------
        // Jump / gravity
        // --------------------------------------------------------

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (
                Keyboard.current.spaceKey
                    .wasPressedThisFrame
            )
            {
                verticalVelocity =
                    Mathf.Sqrt(
                        jumpHeight *
                        -2f *
                        gravity
                    );
            }
        }
        else
        {
            verticalVelocity +=
                gravity *
                Time.deltaTime;
        }

        move.y =
            verticalVelocity;

        if (
            IsFinite(move.x) &&
            IsFinite(move.y) &&
            IsFinite(move.z)
        )
        {
            controller.Move(
                move *
                Time.deltaTime
            );
        }
    }

    // ============================================================
    // SCOPE
    // ============================================================

    void HandleScope()
    {
        if (playerCamera == null ||
            Mouse.current == null)
            return;

        bool scoped =
            mouseLocked &&
            Mouse.current.leftButton.isPressed;

        float targetFOV =
            scoped
            ? scopeFOV
            : normalFOV;

        playerCamera.fieldOfView =
            Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                zoomSpeed *
                Time.deltaTime
            );
    }

    // ============================================================
    // PUBLIC
    // ============================================================

    public bool IsSprinting()
    {
        return
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed;
    }

    public bool IsScoped()
    {
        return
            mouseLocked &&
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;
    }
}
