// MomentTrigger.cs
// World object that offers a memory when the player enters its zone.
// Has optional visual presence — a soft coloured glow that pulses gently.
// The glow colour comes from the memory's own memoryColour field.
// visibleInWorld can be toggled per-trigger for testing vs final polish.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class MomentTrigger : MonoBehaviour
{
    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Memory")]
    public MemoryData memoryData;

    [Header("Trigger Zone")]
    public float triggerRadius = 3f;

    [Header("Interaction")]
    public string promptText = "Experience this moment";
    public float lingerTime = 0f;
    public bool consumeOnUse = true;

    [Header("Visual Presence")]
    [Tooltip("Show a coloured glow in the world. Use for testing or as final design.")]
    public bool visibleInWorld = true;

    [Tooltip("How large the glow light radius is")]
    public float glowRadius = 2.5f;

    [Tooltip("How bright the glow is. 0.3-0.8 works well.")]
    [Range(0f, 2f)]
    public float glowIntensity = 0.5f;

    [Tooltip("Speed of the pulse breathing effect")]
    public float pulseSpeed = 1.2f;

    [Tooltip("How much the glow breathes in/out. 0 = no pulse.")]
    [Range(0f, 0.5f)]
    public float pulseAmount = 0.15f;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    private float lingerTimer = 0f;
    private PlayerInputActions inputActions;

    // Visual components
    private Light glowLight;
    private float baseIntensity;
    private float pulseTimer;

    // -------------------------------------------------------
    // EVENTS
    // -------------------------------------------------------
    public event System.Action<string> OnPlayerEnterRange;
    public event System.Action OnPlayerExitRange;
    public event System.Action OnMomentExperienced;

    // -------------------------------------------------------
    // AWAKE
    // -------------------------------------------------------
    private void Awake()
    {
        inputActions = new PlayerInputActions();

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;

        SetupVisuals();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    // -------------------------------------------------------
    // VISUAL SETUP
    // Creates a point light using the memory's colour
    // -------------------------------------------------------
    private void SetupVisuals()
    {
        if (!visibleInWorld) return;
        if (memoryData == null) return;

        // Create a child object to hold the light
        // so it can be positioned independently if needed
        GameObject lightObj = new GameObject("MomentGlow");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        // Add a point light
        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = memoryData.memoryColour;
        glowLight.intensity = glowIntensity;
        glowLight.range = glowRadius;
        glowLight.shadows = LightShadows.None; // no shadows — keeps it soft

        baseIntensity = glowIntensity;
        pulseTimer = Random.Range(0f, Mathf.PI * 2f); // randomise start phase
                                                        // so multiple triggers
                                                        // don't pulse in sync
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (!playerInRange || hasBeenUsed) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying()) return;

        HandleInteraction();
        HandlePulse();
    }

    private void HandleInteraction()
    {
        if (lingerTime > 0f)
        {
            lingerTimer += Time.deltaTime;
            if (lingerTimer >= lingerTime)
                ExperienceMoment();
            return;
        }

        if (inputActions.Player.Interact.WasPressedThisFrame())
            ExperienceMoment();
    }

    // -------------------------------------------------------
    // PULSE — gentle breathing light effect
    // -------------------------------------------------------
    private void HandlePulse()
    {
        if (glowLight == null) return;

        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = Mathf.Sin(pulseTimer) * pulseAmount;
        glowLight.intensity = baseIntensity + pulse;
    }

    // -------------------------------------------------------
    // TRIGGER DETECTION
    // -------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        lingerTimer = 0f;

        // Brighten the glow when player enters
        if (glowLight != null)
        {
            baseIntensity = glowIntensity * 1.6f;
            glowLight.range = glowRadius * 1.3f;
        }

        UIManager.Instance?.ShowMomentPrompt(promptText);
        OnPlayerEnterRange?.Invoke(promptText);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        lingerTimer = 0f;

        // Return glow to normal
        if (glowLight != null)
        {
            baseIntensity = glowIntensity;
            glowLight.range = glowRadius;
        }

        UIManager.Instance?.HideMomentPrompt();
        OnPlayerExitRange?.Invoke();
    }

    // -------------------------------------------------------
    // EXPERIENCE
    // -------------------------------------------------------
    private void ExperienceMoment()
    {
        if (memoryData == null)
        {
            Debug.LogWarning($"[MomentTrigger] No MemoryData assigned on {gameObject.name}");
            return;
        }

        UIManager.Instance?.HideMomentPrompt();
        MemorySystem.Instance?.OfferMemory(memoryData);
        OnMomentExperienced?.Invoke();

        if (consumeOnUse)
        {
            hasBeenUsed = true;

            // Fade out the light before disabling
            if (glowLight != null)
                StartCoroutine(FadeOutAndDisable());
            else
                gameObject.SetActive(false);
        }
    }

    // -------------------------------------------------------
    // FADE OUT
    // -------------------------------------------------------
    private System.Collections.IEnumerator FadeOutAndDisable()
    {
        float duration = 1.2f;
        float elapsed = 0f;
        float startIntensity = glowLight.intensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            glowLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / duration);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // EDITOR GIZMO
    // -------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Color gizmoColour = memoryData != null
            ? memoryData.memoryColour
            : new Color(1f, 0.8f, 0.4f, 1f);

        gizmoColour.a = 0.25f;
        Gizmos.color = gizmoColour;
        Gizmos.DrawSphere(transform.position, triggerRadius);

        gizmoColour.a = 0.8f;
        Gizmos.color = gizmoColour;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}