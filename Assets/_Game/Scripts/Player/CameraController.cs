// CameraController.cs
// Subtle camera life — bob, tilt, and trait-driven perception.
// Calm trait makes movement quieter and more contemplative.
// Fragile trait adds slight edge-proximity unease.
// Curious trait very slightly widens effective FOV.
// Attach to PlayerCamera.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // -------------------------------------------------------
    // BOB
    // -------------------------------------------------------
    [Header("Breathing / Bob")]
    public float bobAmplitude = 0.04f;
    public float bobFrequency = 1.8f;
    public float bobSmoothing = 8f;

    // -------------------------------------------------------
    // TILT
    // -------------------------------------------------------
    [Header("Movement Tilt")]
    public float strafeTiltAmount = 1.5f;
    public float strafeTiltSmoothing = 6f;

    // -------------------------------------------------------
    // TRAIT MODIFIERS
    // -------------------------------------------------------
    [Header("Trait Perception Modifiers")]
    [Tooltip("Calm reduces bob amplitude — stiller, more present")]
    public float calmBobReduction = 0.03f;

    [Tooltip("Fragile adds subtle camera sway near edges")]
    public float fragileEdgeSway = 0.8f;

    [Tooltip("Curious slightly increases FOV")]
    public float curiousFOVBonus = 8f;

    [Tooltip("Base FOV — traits offset from this")]
    public float baseFOV = 60f;

    // -------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------
    private Vector3 baseLocalPosition;
    private float bobTimer = 0f;
    private float currentBobAmount = 0f;
    private float targetBobAmount = 0f;
    private float currentTilt = 0f;
    private float currentFOV;
    private Camera cam;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        baseLocalPosition = transform.localPosition;
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = baseFOV;
            currentFOV = baseFOV;
        }
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        HandleBob(isMoving);
        HandleTilt(moveInput.x);
        HandleTraitPerception();
    }

    // -------------------------------------------------------
    // BOB
    // -------------------------------------------------------
    private void HandleBob(bool isMoving)
    {
        // Calm trait reduces bob — movement feels stiller
        float effectiveBobAmplitude = bobAmplitude;
        if (IdentitySystem.Instance != null)
        {
            float calmStrength = IdentitySystem.Instance
                .GetTraitStrength(TraitType.Calm);
            effectiveBobAmplitude -= calmBobReduction * calmStrength * 2f;
            effectiveBobAmplitude = Mathf.Max(0f, effectiveBobAmplitude);
        }

        targetBobAmount = isMoving ? effectiveBobAmplitude : 0f;
        currentBobAmount = Mathf.Lerp(currentBobAmount, targetBobAmount,
            bobSmoothing * Time.deltaTime);

        if (isMoving)
            bobTimer += Time.deltaTime * bobFrequency;

        float bobOffset = Mathf.Sin(bobTimer * Mathf.PI * 2f) * currentBobAmount;
        transform.localPosition = baseLocalPosition + new Vector3(0f, bobOffset, 0f);
    }

    // -------------------------------------------------------
    // TILT
    // -------------------------------------------------------
    private void HandleTilt(float strafeInput)
    {
        float targetTilt = -strafeInput * strafeTiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt,
            strafeTiltSmoothing * Time.deltaTime);

        Vector3 currentAngles = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(currentAngles.x,
            currentAngles.y, currentTilt);
    }

    // -------------------------------------------------------
    // TRAIT PERCEPTION
    // FOV and subtle effects driven by traits
    // -------------------------------------------------------
    private void HandleTraitPerception()
    {
        if (cam == null) return;
        if (IdentitySystem.Instance == null) return;

        float targetFOV = baseFOV;

        // Curious widens view slightly — the world feels bigger
        float curiousStrength = IdentitySystem.Instance
            .GetTraitStrength(TraitType.Curious);
        targetFOV += curiousFOVBonus * curiousStrength * 2f;

        // Fragile narrows view very slightly — the world feels closer, more intense
        float fragileStrength = IdentitySystem.Instance
            .GetTraitStrength(TraitType.Fragile);
        targetFOV -= (fragileStrength * 2f) * 3f;

        targetFOV = Mathf.Clamp(targetFOV, 50f, 80f);
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, 1.5f * Time.deltaTime);
        cam.fieldOfView = currentFOV;
    }

    // -------------------------------------------------------
    // PUBLIC — called by Phase 7 for environmental reactions
    // -------------------------------------------------------
    public void TriggerEmotionalMoment(float intensity = 1f)
    {
        // Gentle push-in when something significant happens
        StartCoroutine(EmotionalMomentCoroutine(intensity));
    }

    private System.Collections.IEnumerator EmotionalMomentCoroutine(float intensity)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = baseLocalPosition;
        Vector3 pushPos = baseLocalPosition + Vector3.forward * (0.08f * intensity);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
            transform.localPosition = Vector3.Lerp(startPos, pushPos, t);
            yield return null;
        }

        transform.localPosition = baseLocalPosition;
    }
}