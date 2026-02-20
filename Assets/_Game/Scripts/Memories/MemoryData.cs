// MemoryData.cs
// A ScriptableObject that defines what a single memory IS.
// This is data only — no behaviour lives here.
// Create instances via right-click → Memory → New Memory in the Project panel.
// Every possible memory in the game gets its own asset in Assets/_Game/ScriptableObjects/Memories/

using UnityEngine;

[CreateAssetMenu(fileName = "MEM_NewMemory", menuName = "Memory/New Memory")]
public class MemoryData : ScriptableObject
{
    // -------------------------------------------------------
    // IDENTITY
    // -------------------------------------------------------
    [Header("Identity")]
    [Tooltip("Short poetic phrase shown in memory slot. e.g. 'Swam at midnight'")]
    public string memoryTitle = "Unnamed Memory";

    [Tooltip("Longer reflection shown when the player examines this memory")]
    [TextArea(3, 6)]
    public string memoryDescription = "";

    // -------------------------------------------------------
    // CLASSIFICATION
    // Used later by IdentitySystem to understand who the player is becoming
    // -------------------------------------------------------
    [Header("Classification")]
    public MemoryCategory category;
    
    [Tooltip("Emotional weight — how strongly this memory shapes identity. 0-1.")]
    [Range(0f, 1f)]
    public float emotionalWeight = 0.5f;

    // -------------------------------------------------------
    // WORLD ECHO
    // How this memory subtly changes the world around the player
    // Designed now, implemented in Phase 7
    // -------------------------------------------------------
    [Header("World Echo (implemented in Phase 7)")]
    [Tooltip("Subtle colour shift this memory adds to the world")]
    public Color worldTintContribution = Color.white;

    [Tooltip("Audio layer this memory unlocks in the ambient mix")]
    public AudioClip ambientLayer;

    // -------------------------------------------------------
    // IDENTITY TRAITS
    // What keeping this memory nudges the player toward
    // Designed now, wired up in Phase 6
    // -------------------------------------------------------
    [Header("Identity Traits (implemented in Phase 6)")]
    [Tooltip("Traits this memory reinforces if kept")]
    public TraitType[] reinforcedTraits;

    [Tooltip("Traits this memory quietly erodes if kept")]
    public TraitType[] erodedTraits;

    // -------------------------------------------------------
    // PRESENTATION
    // -------------------------------------------------------
    [Header("Presentation")]
    [Tooltip("Icon shown in the memory slot UI")]
    public Sprite memoryIcon;

    [Tooltip("Colour used to tint this memory's UI card")]
    public Color memoryColour = new Color(0.8f, 0.75f, 0.7f, 1f);
}

// -------------------------------------------------------
// SUPPORTING ENUMS
// Defined here for now — will move to their own file
// when the lists grow large
// -------------------------------------------------------

public enum MemoryCategory
{
    Nature,
    Solitude,
    Connection,
    Risk,
    Creation,
    Loss,
    Wonder,
    Stillness
}

public enum TraitType
{
    Fearless,
    Fragile,
    Curious,
    Calm,
    Aware,
    Warm,
    Agile,
    Melancholic,
    Resilient,
    Open
}