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
// TRAIT MODIFIERS
// These define the maximum effect each trait can have
// Actual effect = modifier * GetTraitStrength() (0-0.5 scale)
// So at full trait strength, player gets half the modifier value
// -------------------------------------------------------
[Header("Trait Modifiers (Phase 6)")]
[Tooltip("Extra speed added at full Agile trait")]
public float agileSpeedBonus = 1.5f;

[Tooltip("Extra jump force at full Fearless trait")]
public float fearlessJumpBonus = 2f;

[Tooltip("Speed reduction at full Fragile trait")]
public float fragileSpeedPenalty = 0.8f;

[Tooltip("Extra gravity at full Fragile trait — feels heavier")]
public float fragileGravityBonus = 1.5f;

[Tooltip("Jump force reduction at full Fragile trait")]
public float fragileJumpPenalty = 1f;

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
    Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    Vector3 worldDirection = transform.TransformDirection(inputDirection);

    // Apply trait modifiers to walk speed
    float effectiveSpeed = walkSpeed;

    if (IdentitySystem.Instance != null)
    {
        // Agile trait increases speed
        float agileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Agile);
        effectiveSpeed += agileSpeedBonus * agileStrength * 2f;

        // Fragile trait reduces speed slightly
        float fragileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Fragile);
        effectiveSpeed -= fragileSpeedPenalty * fragileStrength * 2f;

        // Calm trait makes movement smoother — increases deceleration
        float calmStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Calm);
        // (calm affects feel, not speed — handled via higher effective deceleration)
    }

    effectiveSpeed = Mathf.Max(1f, effectiveSpeed); // never go below 1

    Vector3 targetVelocity = worldDirection * effectiveSpeed;
    float rate = inputDirection.magnitude > 0.1f ? acceleration : deceleration;
    currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,
        rate * Time.fixedDeltaTime);

    rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y,
        currentVelocity.z);
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
        
        float effectiveJumpForce = jumpForce;

        if (IdentitySystem.Instance != null)
        {
            float fearlessStrength = IdentitySystem.Instance
                .GetTraitStrength(TraitType.Fearless);
            effectiveJumpForce += fearlessJumpBonus * fearlessStrength * 2f;

            float fragileStrength = IdentitySystem.Instance
                .GetTraitStrength(TraitType.Fragile);
            effectiveJumpForce -= (fragileJumpPenalty * fragileStrength * 2f);
        }

        effectiveJumpForce = Mathf.Max(1f, effectiveJumpForce);
        rb.AddForce(Vector3.up * effectiveJumpForce, ForceMode.VelocityChange);
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
        float effectiveGravity = gravityMultiplier;

        if (IdentitySystem.Instance != null)
        {
            // Fragile trait adds extra gravity — feels heavier, more vulnerable
            float fragileStrength = IdentitySystem.Instance
                .GetTraitStrength(TraitType.Fragile);
            effectiveGravity += fragileGravityBonus * fragileStrength * 2f;
        }

        rb.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
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