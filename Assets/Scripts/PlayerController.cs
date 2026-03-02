using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Scene Reference")]
    public Transform cameraTransform; // Drag Main Camera here

    [Header("Tuning (must assign)")]
    public PlayerTuningSO tuning; // Drag PlayerTuning_Default here

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Look Clamp")]
    public float pitchClamp = 80f;

    private CharacterController cc;
    private PlayerInputActions actions;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float pitch;

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
        actions.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        actions.Disable();
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (tuning == null)
        {
            Debug.LogError("PlayerController: tuning is not assigned. Drag PlayerTuning_Default into the Tuning field.");
            return;
        }

        HandleMove();
        HandleLook();
    }

    void HandleMove()
    {
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        // Decide speed (walk/run/crouch)
        float speed = tuning.walkSpeed;

        bool shiftHeld = Keyboard.current != null &&
                 (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        bool ctrlHeld = Keyboard.current != null &&
                        (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

        if (shiftHeld)
        {
            speed = tuning.runSpeed;
        }

        // crouch overrides run if both pressed
        if (ctrlHeld)
        {
            speed = tuning.crouchSpeed;
        }

        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float yaw = lookInput.x * tuning.mouseSensitivity;
        float pitchDelta = lookInput.y * tuning.mouseSensitivity;

        transform.Rotate(Vector3.up * yaw);

        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}