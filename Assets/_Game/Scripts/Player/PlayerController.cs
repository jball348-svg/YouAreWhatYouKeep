// PlayerController.cs
// Handles all physical movement of the player through the world.
// Designed to feel present and weighted, not snappy or gamey.
// Communicates upward to GameManager to check if movement is allowed.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("References")]
    [Tooltip("The CameraRoot object that moves up/down for vertical look")]
    public Transform cameraRoot;

    // -------------------------------------------------------
    // MOVEMENT SETTINGS
    // These are tunable in the Inspector — tweak until it feels right
    // -------------------------------------------------------
    [Header("Movement Feel")]
    [Tooltip("How fast the player walks")]
    public float walkSpeed = 3.5f;

    [Tooltip("How quickly the player reaches full speed")]
    public float acceleration = 8f;

    [Tooltip("How quickly the player stops")]
    public float deceleration = 12f;

    [Header("Jump Feel")]
    public float jumpForce = 4f;
    public float gravityMultiplier = 2.5f;

    [Header("Look Feel")]
    [Tooltip("Mouse sensitivity")]
    public float lookSensitivity = 0.15f;

    [Tooltip("How far up/down the player can look (degrees)")]
    public float verticalLookClamp = 80f;

    // -------------------------------------------------------
    // GROUND DETECTION
    // -------------------------------------------------------
    [Header("Ground Check")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayer;

    // -------------------------------------------------------
    // PRIVATE — internal state, not shown in Inspector
    // -------------------------------------------------------
    private Rigidbody rb;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalLookAngle = 0f;
    private bool isGrounded;
    private bool jumpRequested;
    private Vector3 currentVelocity; // used for smooth acceleration

    // -------------------------------------------------------
    // AWAKE & ENABLE/DISABLE
    // -------------------------------------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        // Lock and hide the cursor for first-person view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Subscribe to input events
        inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Jump.performed -= OnJump;
    }

    // -------------------------------------------------------
    // UPDATE — runs every frame, handles input reading and look
    // -------------------------------------------------------
    private void Update()
    {
        // Don't process input if the game isn't in playing state
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            return;

        // Read movement and look values each frame
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleLook();
        CheckGrounded();
    }

    // -------------------------------------------------------
    // FIXED UPDATE — physics movement happens here
    // Unity's physics runs on a fixed timestep, not per-frame
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            return;

        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    // -------------------------------------------------------
    // LOOK — rotates body left/right, camera root up/down
    // -------------------------------------------------------
    private void HandleLook()
    {
        // Horizontal look: rotate the whole player body
        float horizontalLook = lookInput.x * lookSensitivity;
        transform.Rotate(Vector3.up * horizontalLook);

        // Vertical look: tilt only the camera root
        verticalLookAngle -= lookInput.y * lookSensitivity;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, -verticalLookClamp, verticalLookClamp);
        cameraRoot.localEulerAngles = new Vector3(verticalLookAngle, 0f, 0f);
    }

    // -------------------------------------------------------
    // MOVEMENT — smooth acceleration and deceleration
    // -------------------------------------------------------
    private void HandleMovement()
    {
        // Convert 2D input into 3D world direction relative to where player faces
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        Vector3 targetVelocity = worldDirection * walkSpeed;

        // Choose acceleration or deceleration rate based on whether moving
        float rate = inputDirection.magnitude > 0.1f ? acceleration : deceleration;

        // Smoothly move toward target velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        // Apply to rigidbody — preserve Y velocity (gravity/jump)
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    // -------------------------------------------------------
    // JUMP
    // -------------------------------------------------------
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
            jumpRequested = true;
    }

    private void HandleJump()
    {
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    // -------------------------------------------------------
    // GRAVITY — extra downward force for weightier feel
    // -------------------------------------------------------
    private void ApplyGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    // -------------------------------------------------------
    // GROUND CHECK — small raycast downward from player's feet
    // -------------------------------------------------------
    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );
    }
}