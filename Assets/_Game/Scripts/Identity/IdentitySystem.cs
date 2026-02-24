// IdentitySystem.cs
// Tracks the player's trait profile — who they are becoming.
// Traits are derived from the memories they hold.
// Values shift when memories are kept or forgotten.
// Other systems read trait values to modify behaviour.
//
// ARCHITECTURE:
// - Maintains a float per TraitType (0-1 scale)
// - Subscribes to MemorySystem events
// - Recalculates all traits when memories change
// - Fires OnTraitChanged when a trait crosses a meaningful threshold
// - PlayerController and CameraController read trait values each Update
//
// DESIGN INTENT:
// Traits are not binary. They are gradients.
// A player doesn't "become Fearless" — they drift toward fearlessness.
// Traits fade when the memories reinforcing them are forgotten.
// You are what you keep — not what you once kept.

using UnityEngine;
using System.Collections.Generic;
using System;

public class IdentitySystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static IdentitySystem Instance { get; private set; }

    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Trait Calculation")]
    [Tooltip("How strongly a single memory reinforces a trait. 0-1.")]
    [Range(0f, 1f)]
    public float traitReinforcementStrength = 0.25f;

    [Tooltip("How strongly a single memory erodes a trait. 0-1.")]
    [Range(0f, 1f)]
    public float traitErosionStrength = 0.15f;

    [Tooltip("Traits drift toward 0.5 (neutral) over time when not reinforced.")]
    [Range(0f, 0.01f)]
    public float neutralDriftRate = 0.001f;

    // -------------------------------------------------------
    // EVENTS
    // -------------------------------------------------------

    // Fired when any trait changes significantly
    // Other systems subscribe to react to identity shifts
    public event Action<TraitType, float> OnTraitChanged;

    // Fired when a trait crosses a major threshold (0.3, 0.6, 0.9)
    // Used by Phase 7 for world echo reactions to identity
    public event Action<TraitType, float> OnTraitThresholdCrossed;

    // -------------------------------------------------------
    // PRIVATE STATE
    // -------------------------------------------------------
    private Dictionary<TraitType, float> traitValues
        = new Dictionary<TraitType, float>();

    private Dictionary<TraitType, float> previousTraitValues
        = new Dictionary<TraitType, float>();

    // Thresholds that trigger significant world reactions
    private readonly float[] significantThresholds = { 0.3f, 0.6f, 0.9f };

    // -------------------------------------------------------
    // AWAKE
    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitialiseTraits();
    }

    private void Start()
    {
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoryKept += OnMemoryKept;
            MemorySystem.Instance.OnMemoryForgotten += OnMemoryForgotten;
        }
    }

    private void OnDestroy()
    {
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoryKept -= OnMemoryKept;
            MemorySystem.Instance.OnMemoryForgotten -= OnMemoryForgotten;
        }
    }

    // -------------------------------------------------------
    // UPDATE — gentle neutral drift
    // Traits slowly return toward 0.5 if not reinforced
    // This means identity requires maintenance — you must
    // keep living certain ways to stay who you are
    // -------------------------------------------------------
private void Update()
{
    if (neutralDriftRate <= 0f) return;

    foreach (TraitType trait in System.Enum.GetValues(typeof(TraitType)))
    {
        float current = traitValues[trait];
        float drifted = Mathf.MoveTowards(current, 0.5f,
            neutralDriftRate * Time.deltaTime);

        if (!Mathf.Approximately(current, drifted))
            traitValues[trait] = drifted;
    }
}

    // -------------------------------------------------------
    // INITIALISE — all traits start at 0.5 (neutral)
    // -------------------------------------------------------
    private void InitialiseTraits()
    {
        foreach (TraitType trait in System.Enum.GetValues(typeof(TraitType)))
        {
            traitValues[trait] = 0.5f;
            previousTraitValues[trait] = 0.5f;
        }
    }

    // -------------------------------------------------------
    // MEMORY EVENTS
    // -------------------------------------------------------
    private void OnMemoryKept(MemoryInstance memory)
    {
        ApplyMemoryTraits(memory, keeping: true);
    }

    private void OnMemoryForgotten(MemoryInstance memory)
    {
        ApplyMemoryTraits(memory, keeping: false);
    }

    // -------------------------------------------------------
    // APPLY TRAIT CHANGES FROM A MEMORY
    // -------------------------------------------------------
    private void ApplyMemoryTraits(MemoryInstance memory, bool keeping)
    {
        if (memory.data == null) return;

        float direction = keeping ? 1f : -1f;

        // Reinforced traits move toward 1
        if (memory.data.reinforcedTraits != null)
        {
            foreach (var trait in memory.data.reinforcedTraits)
            {
                float change = traitReinforcementStrength
                    * memory.EmotionalWeight
                    * direction;

                ShiftTrait(trait, change);
            }
        }

        // Eroded traits move toward 0
        if (memory.data.erodedTraits != null)
        {
            foreach (var trait in memory.data.erodedTraits)
            {
                float change = traitErosionStrength
                    * memory.EmotionalWeight
                    * -direction; // erosion goes opposite direction

                ShiftTrait(trait, change);
            }
        }
    }

    // -------------------------------------------------------
    // SHIFT A SINGLE TRAIT VALUE
    // -------------------------------------------------------
    private void ShiftTrait(TraitType trait, float change)
    {
        float previous = traitValues[trait];
        float newValue = Mathf.Clamp01(previous + change);
        traitValues[trait] = newValue;

        // Check if we crossed a significant threshold
        foreach (float threshold in significantThresholds)
        {
            bool crossedUp = previous < threshold && newValue >= threshold;
            bool crossedDown = previous > threshold && newValue <= threshold;

            if (crossedUp || crossedDown)
            {
                OnTraitThresholdCrossed?.Invoke(trait, newValue);
                            }
        }

        // Fire general change event if meaningful shift
        if (Mathf.Abs(newValue - previous) > 0.01f)
        {
            OnTraitChanged?.Invoke(trait, newValue);

        }
    }

    // -------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------

    // Get current value of a trait (0-1)
    public float GetTraitValue(TraitType trait)
    {
        return traitValues.TryGetValue(trait, out float value) ? value : 0.5f;
    }

    // Check if trait is above a threshold — default 0.6 = meaningfully present
    public bool HasTrait(TraitType trait, float threshold = 0.6f)
    {
        return GetTraitValue(trait) >= threshold;
    }

    // How far above neutral (0.5) is this trait?
    // Returns 0-0.5, useful for smooth modifiers
    public float GetTraitStrength(TraitType trait)
    {
        return Mathf.Max(0f, GetTraitValue(trait) - 0.5f);
    }

    // Returns a summary of dominant traits — useful for Phase 8 ending
    public List<TraitType> GetDominantTraits(float threshold = 0.65f)
    {
        var dominant = new List<TraitType>();
        foreach (var kvp in traitValues)
        {
            if (kvp.Value >= threshold)
                dominant.Add(kvp.Key);
        }
        return dominant;
    }

    // Full snapshot — used by Phase 8 ending system
    public Dictionary<TraitType, float> GetFullProfile()
    {
        return new Dictionary<TraitType, float>(traitValues);
    }
}