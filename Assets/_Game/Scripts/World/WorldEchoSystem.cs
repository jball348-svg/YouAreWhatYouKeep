// WorldEchoSystem.cs
// Places remember the player. The world holds the shape of your presence.
// When a memory is formed somewhere, that location becomes subtly warmer.
// Lingering somewhere without taking a memory still leaves a trace.
//
// ARCHITECTURE:
// - Tracks "echo points" — world positions where significant things happened
// - Each echo has a strength that fades over time
// - PlayerController reports position each frame
// - WorldEchoSystem checks proximity to echoes and drives atmosphere
// - Communicates warmth to EmotionalResponseSystem via PushEmotionalState
//
// DESIGN INTENT:
// The world should feel inhabited by your past self.
// Walk back through somewhere you lingered — something is different.
// Not obvious. Just felt.

using UnityEngine;
using System.Collections.Generic;

public class WorldEchoSystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static WorldEchoSystem Instance { get; private set; }

    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Echo Settings")]
    [Tooltip("How close the player needs to be to feel an echo")]
    public float echoFeelRadius = 5f;

    [Tooltip("How quickly echoes fade over game time (strength per hour)")]
    public float echoDecayPerHour = 0.1f;

    [Tooltip("Minimum echo strength before it's removed")]
    public float minimumEchoStrength = 0.05f;

    [Header("Linger Tracking")]
    [Tooltip("How long player must stay in one spot to create a linger echo (seconds)")]
    public float lingerThreshold = 8f;

    [Tooltip("How far player can move and still count as lingering")]
    public float lingerMovementTolerance = 1.5f;

    [Header("Atmosphere Response")]
    [Tooltip("How strongly echoes push the emotional response system")]
    public float echoAtmosphereStrength = 8f;

    // -------------------------------------------------------
    // ECHO DATA
    // -------------------------------------------------------
    [System.Serializable]
    public class WorldEcho
    {
        public Vector3 position;
        public float strength;          // 0-1
        public string memoryTitle;      // which memory was formed here
        public Color echoColour;        // memory's colour
        public float timeCreated;       // game time when created
        public EchoType type;
    }

    public enum EchoType
    {
        MemoryFormed,   // player took a memory here
        Lingered,       // player stayed here a long time
        Significant     // something important happened (Phase 8)
    }

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private List<WorldEcho> activeEchoes = new List<WorldEcho>();

    // Linger tracking
    private Vector3 lastPlayerPosition;
    private float lingerTimer = 0f;
    private bool isLingering = false;

    // Proximity tracking
    private float currentEchoStrength = 0f;
    private float targetEchoStrength = 0f;

    // -------------------------------------------------------
    // AWAKE & START
    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (MemorySystem.Instance != null)
            MemorySystem.Instance.OnMemoryKept += OnMemoryKept;

        if (TimeSystem.Instance != null)
            TimeSystem.Instance.OnHourChanged += OnHourChanged;
    }

    private void OnDestroy()
    {
        if (MemorySystem.Instance != null)
            MemorySystem.Instance.OnMemoryKept -= OnMemoryKept;

        if (TimeSystem.Instance != null)
            TimeSystem.Instance.OnHourChanged -= OnHourChanged;
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying()) return;

        UpdateLingerTracking();
        UpdateEchoProximity();
        ApplyEchoAtmosphere();
    }

    // -------------------------------------------------------
    // LINGER TRACKING
    // Player staying still builds a linger echo
    // -------------------------------------------------------
    private void UpdateLingerTracking()
    {
        // Find player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        float distanceMoved = Vector3.Distance(playerPos, lastPlayerPosition);

        if (distanceMoved < lingerMovementTolerance)
        {
            lingerTimer += Time.deltaTime;

            if (lingerTimer >= lingerThreshold && !isLingering)
            {
                isLingering = true;
                RegisterLingerEcho(playerPos);
            }
        }
        else
        {
            lingerTimer = 0f;
            isLingering = false;
            lastPlayerPosition = playerPos;
        }
    }

    // -------------------------------------------------------
    // ECHO PROXIMITY
    // How strongly does the player feel nearby echoes?
    // -------------------------------------------------------
    private void UpdateEchoProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        float strongestEcho = 0f;

        foreach (var echo in activeEchoes)
        {
            float distance = Vector3.Distance(playerPos, echo.position);

            if (distance < echoFeelRadius)
            {
                // Proximity factor: stronger when closer
                float proximity = 1f - (distance / echoFeelRadius);
                float felt = proximity * echo.strength;
                strongestEcho = Mathf.Max(strongestEcho, felt);
            }
        }

        targetEchoStrength = strongestEcho;
        currentEchoStrength = Mathf.Lerp(
            currentEchoStrength, targetEchoStrength, 2f * Time.deltaTime);
    }

    // -------------------------------------------------------
    // APPLY ECHO ATMOSPHERE
    // Push emotional response when player is near an echo
    // -------------------------------------------------------
    private void ApplyEchoAtmosphere()
    {
        if (currentEchoStrength < 0.1f) return;
        if (EmotionalResponseSystem.Instance == null) return;

        // Very gentle continuous push — not a spike
        // Only pushes when near an echo, fades when player moves away
        float pushStrength = currentEchoStrength * echoAtmosphereStrength;

        // Apply as a very small per-frame nudge rather than a timed push
        // This creates a continuous warmth rather than repeated spikes
        if (currentEchoStrength > 0.3f)
        {
            EmotionalResponseSystem.Instance.PushEmotionalState(
                saturationBoost: pushStrength * 0.1f,
                bloomBoost: pushStrength * 0.02f,
                duration: 0.5f
            );
        }
    }

    // -------------------------------------------------------
    // REGISTER ECHOES
    // -------------------------------------------------------
    private void OnMemoryKept(MemoryInstance memory)
    {
        // Find player position when memory was taken
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        RegisterEcho(
            player.transform.position,
            EchoType.MemoryFormed,
            memory.data.memoryColour,
            memory.Title
        );

        Debug.Log($"[WorldEcho] Memory echo registered: {memory.Title}");
    }

    private void RegisterLingerEcho(Vector3 position)
    {
        RegisterEcho(position, EchoType.Lingered, Color.white, "Lingered here");
        Debug.Log($"[WorldEcho] Linger echo registered at {position}");
    }

    private void RegisterEcho(Vector3 position, EchoType type,
        Color colour, string title)
    {
        // Don't stack echoes too close together
        foreach (var existing in activeEchoes)
        {
            if (Vector3.Distance(existing.position, position) < 2f)
            {
                // Strengthen existing echo instead
                existing.strength = Mathf.Min(1f, existing.strength + 0.3f);
                return;
            }
        }

        var echo = new WorldEcho
        {
            position = position,
            strength = 0.8f,
            memoryTitle = title,
            echoColour = colour,
            timeCreated = TimeSystem.Instance != null
                ? TimeSystem.Instance.CurrentHour : 0f,
            type = type
        };

        activeEchoes.Add(echo);
    }

    // -------------------------------------------------------
    // ECHO DECAY
    // Called each game hour by TimeSystem
    // -------------------------------------------------------
    private void OnHourChanged(float hour)
    {
        for (int i = activeEchoes.Count - 1; i >= 0; i--)
        {
            activeEchoes[i].strength -= echoDecayPerHour;

            if (activeEchoes[i].strength <= minimumEchoStrength)
                activeEchoes.RemoveAt(i);
        }
    }

    // -------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------

    // Is the player currently near any echo?
    public bool IsNearEcho()
    {
        return currentEchoStrength > 0.2f;
    }

    // Strongest echo the player currently feels
    public float GetCurrentEchoStrength()
    {
        return currentEchoStrength;
    }

    // All active echoes — used by Phase 8 ending to map lived experience
    public List<WorldEcho> GetAllEchoes()
    {
        return new List<WorldEcho>(activeEchoes);
    }

    // Register a significant echo from outside (Phase 8)
    public void RegisterSignificantEcho(Vector3 position, Color colour, string title)
    {
        RegisterEcho(position, EchoType.Significant, colour, title);
    }

    // Draw all echo positions in Scene view
    private void OnDrawGizmos()
    {
        foreach (var echo in activeEchoes)
        {
            Color gizmoCol = echo.echoColour;
            gizmoCol.a = echo.strength * 0.5f;
            Gizmos.color = gizmoCol;
            Gizmos.DrawWireSphere(echo.position, echoFeelRadius * echo.strength);
        }
    }
}