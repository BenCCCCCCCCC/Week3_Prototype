using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Scene Reference")]
    public Transform cameraTransform;

    [Header("Player Stats (must assign)")]
    public PlayerStatsSO playerStats;

    [Header("Input")]
    public bool enablePlayerInput = true;

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Look Clamp")]
    public float pitchClamp = 80f;

    [Header("Runtime Status")]
    public float externalSpeedMultiplier = 1f;
    public bool IsInvincible { get; set; } = false;

    private CharacterController cc;
    private PlayerInputActions actions;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float pitch;

    private bool forcedMoveActive = false;
    private Vector3 forcedMoveDirection = Vector3.forward;
    private float forcedMoveSpeed = 0f;

    private bool useManualSpeedOverride = false;
    private float manualSpeedOverride = 0f;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        actions = new PlayerInputActions();

        actions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        actions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        actions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        actions.Player.Look.canceled += _ => lookInput = Vector2.zero;
    }

    void OnEnable()
    {
        if (enablePlayerInput)
        {
            actions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            actions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDisable()
    {
        if (actions != null)
        {
            actions.Disable();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        if (actions != null)
        {
            actions.Dispose();
        }
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        enablePlayerInput = enabled;

        if (actions == null)
        {
            return;
        }

        if (enabled)
        {
            actions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            actions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }
    }

    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerController: playerStats is not assigned.");
            return;
        }

        HandleMove();
        HandleLook();
    }

    void HandleMove()
    {
        if (cc == null) return;
        if (!cc.enabled) return;

        Vector2 currentMoveInput = enablePlayerInput ? moveInput : Vector2.zero;

        Vector3 inputDirection = transform.right * currentMoveInput.x + transform.forward * currentMoveInput.y;

        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        if (!forcedMoveActive && inputDirection.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = inputDirection.normalized;
        }

        float currentSpeed = GetCurrentMoveSpeed();

        Vector3 planarVelocity;
        if (forcedMoveActive)
        {
            planarVelocity = forcedMoveDirection.normalized * forcedMoveSpeed;
        }
        else
        {
            planarVelocity = inputDirection.normalized * currentSpeed;
        }

        if (cc.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = planarVelocity + Vector3.up * verticalVelocity;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        if (!enablePlayerInput) return;
        if (cameraTransform == null) return;

        float yaw = lookInput.x * playerStats.mouseSensitivity;
        float pitchDelta = lookInput.y * playerStats.mouseSensitivity;

        transform.Rotate(Vector3.up * yaw);

        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public float GetDefaultMoveSpeed()
    {
        if (playerStats == null) return 0f;

        float speed = playerStats.walkSpeed;

        bool shiftHeld = Keyboard.current != null &&
                         (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        bool ctrlHeld = Keyboard.current != null &&
                        (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

        if (shiftHeld)
        {
            speed = playerStats.runSpeed;
        }

        if (ctrlHeld)
        {
            speed = playerStats.crouchSpeed;
        }

        return speed;
    }

    public float GetBaseMoveSpeed()
    {
        return GetDefaultMoveSpeed();
    }

    public float GetCurrentMoveSpeed()
    {
        if (useManualSpeedOverride)
        {
            return manualSpeedOverride;
        }

        return GetDefaultMoveSpeed() * externalSpeedMultiplier;
    }

    public void SetSpeed(float newSpeed)
    {
        useManualSpeedOverride = true;
        manualSpeedOverride = newSpeed;

        float defaultSpeed = GetDefaultMoveSpeed();
        if (defaultSpeed > 0.001f)
        {
            externalSpeedMultiplier = newSpeed / defaultSpeed;
        }
        else
        {
            externalSpeedMultiplier = 1f;
        }
    }

    public void RestoreDefaultSpeed()
    {
        useManualSpeedOverride = false;
        manualSpeedOverride = 0f;
        externalSpeedMultiplier = 1f;
    }

    public Vector3 GetCurrentForwardOrMoveDirection()
    {
        if (LastMoveDirection.sqrMagnitude > 0.001f)
        {
            return LastMoveDirection.normalized;
        }

        return transform.forward;
    }

    public void StartForcedMove(Vector3 worldDirection, float speed)
    {
        if (worldDirection.sqrMagnitude < 0.001f)
        {
            worldDirection = transform.forward;
        }

        forcedMoveActive = true;
        forcedMoveDirection = worldDirection.normalized;
        forcedMoveSpeed = speed;
        LastMoveDirection = forcedMoveDirection;
    }

    public void StopForcedMove()
    {
        forcedMoveActive = false;
        forcedMoveSpeed = 0f;
    }
}