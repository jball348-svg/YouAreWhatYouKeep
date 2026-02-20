// MomentTrigger.cs
// Place this on any world object to make it a potential memory moment.
// When the player enters the trigger zone, a prompt appears.
// If the player interacts, the memory is offered to MemorySystem.
// 
// This is designed to be placed on ENV_ prefabs so moments
// are baked into the environment, not floated as pickups.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class MomentTrigger : MonoBehaviour
{
    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Memory")]
    [Tooltip("The memory this moment can create")]
    public MemoryData memoryData;

    [Header("Trigger Zone")]
    [Tooltip("How close the player needs to be to be offered this moment")]
    public float triggerRadius = 3f;

    [Header("Presentation")]
    [Tooltip("Text shown as interaction prompt. e.g. 'Sit for a while'")]
    public string promptText = "Experience this moment";

    [Tooltip("How long to linger before the memory is offered (seconds)")]
    public float lingerTime = 0f;

    [Header("One Time")]
    [Tooltip("Once experienced, this moment disappears. Keeps world feeling finite.")]
    public bool consumeOnUse = true;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    private float lingerTimer = 0f;
    private bool isLingering = false;

    private PlayerInputActions inputActions;

    // -------------------------------------------------------
    // EVENTS
    // UI subscribes to these to show/hide prompt
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

        // Set up the sphere collider as a trigger
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (!playerInRange || hasBeenUsed) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying()) return;

        // Handle linger timer if required
        if (lingerTime > 0f)
        {
            isLingering = true;
            lingerTimer += Time.deltaTime;

            if (lingerTimer >= lingerTime)
                ExperienceMoment();

            return;
        }

        // Otherwise wait for interact button
        if (inputActions.Player.Interact.WasPressedThisFrame())
            ExperienceMoment();
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

        OnPlayerEnterRange?.Invoke(promptText);
        Debug.Log($"[MomentTrigger] Player in range of: {memoryData?.memoryTitle}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        isLingering = false;
        lingerTimer = 0f;

        OnPlayerExitRange?.Invoke();
    }

    // -------------------------------------------------------
    // EXPERIENCE THE MOMENT
    // -------------------------------------------------------
    private void ExperienceMoment()
    {
        if (memoryData == null)
        {
            Debug.LogWarning($"[MomentTrigger] No MemoryData assigned on {gameObject.name}");
            return;
        }

        // Offer to the memory system
        MemorySystem.Instance?.OfferMemory(memoryData);

        OnMomentExperienced?.Invoke();

        if (consumeOnUse)
        {
            hasBeenUsed = true;
            // Fade/disable world object — visual handled by WorldEchoSystem in Phase 7
            // For now just disable the trigger
            gameObject.SetActive(false);
        }
    }

    // -------------------------------------------------------
    // EDITOR VISUALISATION
    // Draws the trigger radius as a sphere in the Scene view
    // so you can see it without running the game
    // -------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.4f, 0.3f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(1f, 0.8f, 0.4f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}