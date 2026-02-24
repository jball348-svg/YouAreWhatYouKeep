// EndingTrigger.cs
// Place this in the world to give the player a way to end the game.
// Could be a doorway, a horizon point, a specific place.
// Does not force — the player chooses to walk through it.
// Also exposes a manual trigger for testing via Inspector button.

using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Player must press Interact (E) rather than just walking in")]
    public bool requireInteract = true;

    public string promptText = "Leave this place";

    [Header("Glow")]
    public Color triggerColour = new Color(0.9f, 0.85f, 0.7f);
    public float glowIntensity = 0.4f;

    private bool playerInRange = false;
    private PlayerInputActions inputActions;
    private Light glowLight;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        var col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 3f;

        // Warm glow
        var lightObj = new GameObject("EndingGlow");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = triggerColour;
        glowLight.intensity = glowIntensity;
        glowLight.range = 4f;
        glowLight.shadows = LightShadows.None;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        if (!playerInRange) return;

        if (requireInteract)
        {
            if (inputActions.Player.Interact.WasPressedThisFrame())
                TriggerEnding();
        }
        else
        {
            TriggerEnding();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        UIManager.Instance?.ShowMomentPrompt(promptText);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        UIManager.Instance?.HideMomentPrompt();
    }

    private void TriggerEnding()
    {
        UIManager.Instance?.HideMomentPrompt();
        EndingSystem.Instance?.TriggerEnding();
        gameObject.SetActive(false);
    }

    // Inspector button for testing without walking to trigger
    [ContextMenu("Trigger Ending Now")]
    public void TriggerEndingFromInspector()
    {
        EndingSystem.Instance?.TriggerEnding();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(triggerColour.r, triggerColour.g, triggerColour.b, 0.3f);
        Gizmos.DrawSphere(transform.position, 3f);
    }
}