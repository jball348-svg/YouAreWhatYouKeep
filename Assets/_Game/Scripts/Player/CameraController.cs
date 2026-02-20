// CameraController.cs
// Adds subtle life to the camera — gentle breathing bob, slight tilt on movement.
// These are small details that make the world feel real without drawing attention.
// Attach this to the PlayerCamera object.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // -------------------------------------------------------
    // BREATHING BOB
    // A very subtle up/down motion when standing still or walking
    // -------------------------------------------------------
    [Header("Breathing / Bob")]
    [Tooltip("How much the camera bobs up and down while walking")]
    public float bobAmplitude = 0.04f;

    [Tooltip("How fast the bob cycle completes")]
    public float bobFrequency = 1.8f;

    [Tooltip("How quickly bob fades in and out")]
    public float bobSmoothing = 8f;

    [Header("Movement Tilt")]
    [Tooltip("How much the camera rolls when strafing left/right")]
    public float strafeTiltAmount = 1.5f;
    public float strafeTiltSmoothing = 6f;

    // -------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------
    private Vector3 baseLocalPosition;
    private float bobTimer = 0f;
    private float currentBobAmount = 0f;
    private float targetBobAmount = 0f;
    private float currentTilt = 0f;

    // Reference to the input — we read move input here too
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        // Store the starting local position as the "resting" point
        baseLocalPosition = transform.localPosition;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        HandleBob(isMoving, moveInput);
        HandleTilt(moveInput.x);
    }

    // -------------------------------------------------------
    // BOB — gentle vertical oscillation while moving
    // -------------------------------------------------------
    private void HandleBob(bool isMoving, Vector2 moveInput)
    {
        // Target bob strength: full when moving, zero when still
        targetBobAmount = isMoving ? bobAmplitude : 0f;
        currentBobAmount = Mathf.Lerp(currentBobAmount, targetBobAmount, bobSmoothing * Time.deltaTime);

        if (isMoving)
            bobTimer += Time.deltaTime * bobFrequency;

        // Calculate vertical bob offset using a sine wave
        float bobOffset = Mathf.Sin(bobTimer * Mathf.PI * 2f) * currentBobAmount;

        // Apply to local position
        transform.localPosition = baseLocalPosition + new Vector3(0f, bobOffset, 0f);
    }

    // -------------------------------------------------------
    // TILT — slight roll when strafing, feels grounded
    // -------------------------------------------------------
    private void HandleTilt(float strafeInput)
    {
        float targetTilt = -strafeInput * strafeTiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, strafeTiltSmoothing * Time.deltaTime);

        // Apply tilt to Z rotation only — X is controlled by look
        Vector3 currentAngles = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(currentAngles.x, currentAngles.y, currentTilt);
    }
}