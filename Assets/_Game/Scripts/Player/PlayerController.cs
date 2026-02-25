// PlayerController.cs
// Handles all physical movement of the player through the world.
// Designed to feel present and weighted, not snappy or gamey.
// Communicates upward to GameManager to check if movement is allowed.
//
// FIXES:
// - Wall sticking: zero-friction physics material applied at runtime
// - Stair stepping: step-up raycast nudges player over low obstacles

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
    // -------------------------------------------------------
    [Header("Movement Feel")]
    public float walkSpeed = 3.5f;
    public float acceleration = 8f;
    public float deceleration = 12f;

    [Header("Jump Feel")]
    public float jumpForce = 4f;
    public float gravityMultiplier = 2.5f;

    [Header("Look Feel")]
    public float lookSensitivity = 0.15f;
    public float verticalLookClamp = 80f;

    // -------------------------------------------------------
    // STEP CLIMBING
    // Allows the player to walk up stairs and small obstacles
    // without jumping. Tweak stepHeight to match your stair rise.
    // -------------------------------------------------------
    [Header("Step Climbing")]
    [Tooltip("Maximum height the player can step up automatically — match to your stair rise height")]
    public float stepHeight = 0.35f;

    [Tooltip("How far ahead to check for a step obstacle")]
    public float stepCheckDistance = 0.5f;

    [Tooltip("How fast the player is pushed up over a step")]
    public float stepSmooth = 8f;

    // -------------------------------------------------------
    // TRAIT MODIFIERS
    // -------------------------------------------------------
    [Header("Trait Modifiers (Phase 6)")]
    public float agileSpeedBonus = 1.5f;
    public float fearlessJumpBonus = 2f;
    public float fragileSpeedPenalty = 0.8f;
    public float fragileGravityBonus = 1.5f;
    public float fragileJumpPenalty = 1f;

    // -------------------------------------------------------
    // GROUND DETECTION
    // -------------------------------------------------------
    [Header("Ground Check")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayer;

    // -------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalLookAngle = 0f;
    private bool isGrounded;
    private bool jumpRequested;
    private Vector3 currentVelocity;

    // -------------------------------------------------------
    // AWAKE
    // -------------------------------------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        inputActions = new PlayerInputActions();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // FIX: Wall sticking
        // Create a zero-friction, zero-bounce physics material at runtime
        // and apply it to the capsule. This stops the player catching
        // on walls and geometry when moving past or jumping next to them.
        PhysicsMaterial frictionless = new PhysicsMaterial("PlayerFrictionless");
        frictionless.staticFriction = 0f;
        frictionless.dynamicFriction = 0f;
        frictionless.bounciness = 0f;
        frictionless.frictionCombine = PhysicsMaterialCombine.Minimum;
        frictionless.bounceCombine = PhysicsMaterialCombine.Minimum;
        capsule.material = frictionless;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Jump.performed -= OnJump;
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            return;

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        HandleLook();
        CheckGrounded();
    }

    // -------------------------------------------------------
    // FIXED UPDATE
    // -------------------------------------------------------
    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
            return;

        HandleMovement();
        HandleStepClimb();
        HandleJump();
        ApplyGravity();
    }

    // -------------------------------------------------------
    // LOOK
    // -------------------------------------------------------
    private void HandleLook()
    {
        float horizontalLook = lookInput.x * lookSensitivity;
        transform.Rotate(Vector3.up * horizontalLook);

        verticalLookAngle -= lookInput.y * lookSensitivity;
        verticalLookAngle = Mathf.Clamp(verticalLookAngle, -verticalLookClamp, verticalLookClamp);
        cameraRoot.localEulerAngles = new Vector3(verticalLookAngle, 0f, 0f);
    }

    // -------------------------------------------------------
    // MOVEMENT
    // -------------------------------------------------------
    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 worldDirection = transform.TransformDirection(inputDirection);

        float effectiveSpeed = walkSpeed;

        if (IdentitySystem.Instance != null)
        {
            float agileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Agile);
            effectiveSpeed += agileSpeedBonus * agileStrength * 2f;

            float fragileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Fragile);
            effectiveSpeed -= fragileSpeedPenalty * fragileStrength * 2f;
        }

        effectiveSpeed = Mathf.Max(1f, effectiveSpeed);

        Vector3 targetVelocity = worldDirection * effectiveSpeed;
        float rate = inputDirection.magnitude > 0.1f ? acceleration : deceleration;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    // -------------------------------------------------------
    // STEP CLIMBING
    // Casts two raycasts ahead of the player:
    //   Lower ray — detects an obstacle at foot level
    //   Upper ray — checks there's clear space above the obstacle
    // If lower hits and upper is clear, nudges the player upward
    // so they step over it rather than stopping dead.
    // -------------------------------------------------------
    private void HandleStepClimb()
    {
        // Only step-climb when grounded and actually moving
        if (!isGrounded) return;

        Vector3 moveDirection = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized;
        if (moveDirection.magnitude < 0.1f) return;

        Vector3 playerFeet = transform.position;
        Vector3 playerKnee = transform.position + Vector3.up * stepHeight;

        // Lower ray: is there something at foot level ahead?
        bool hitLow = Physics.Raycast(
            playerFeet + Vector3.up * 0.05f,
            moveDirection,
            stepCheckDistance,
            groundLayer
        );

        // Upper ray: is there clear space above that obstacle?
        bool hitHigh = Physics.Raycast(
            playerKnee,
            moveDirection,
            stepCheckDistance,
            groundLayer
        );

        // Step up if we're blocked low but clear high
        if (hitLow && !hitHigh)
        {
            rb.position += Vector3.up * stepSmooth * Time.fixedDeltaTime;
        }
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
                float fearlessStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Fearless);
                effectiveJumpForce += fearlessJumpBonus * fearlessStrength * 2f;

                float fragileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Fragile);
                effectiveJumpForce -= fragileJumpPenalty * fragileStrength * 2f;
            }

            effectiveJumpForce = Mathf.Max(1f, effectiveJumpForce);
            rb.AddForce(Vector3.up * effectiveJumpForce, ForceMode.VelocityChange);
            jumpRequested = false;
        }
    }

    // -------------------------------------------------------
    // GRAVITY
    // -------------------------------------------------------
    private void ApplyGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            float effectiveGravity = gravityMultiplier;

            if (IdentitySystem.Instance != null)
            {
                float fragileStrength = IdentitySystem.Instance.GetTraitStrength(TraitType.Fragile);
                effectiveGravity += fragileGravityBonus * fragileStrength * 2f;
            }

            rb.AddForce(Vector3.down * effectiveGravity, ForceMode.Acceleration);
        }
    }

    // -------------------------------------------------------
    // GROUND CHECK
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