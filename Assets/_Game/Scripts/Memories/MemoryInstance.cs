// MemoryInstance.cs
// Represents a memory the player currently holds.
// Wraps MemoryData (the template) with runtime state.
// Not a MonoBehaviour — it's a plain C# class that lives
// inside the MemorySystem at runtime.

using UnityEngine;
using System;

[Serializable]
public class MemoryInstance
{
    // The underlying data asset
    public MemoryData data;

    // When was this memory made (game time, not real time)
    public float timeAcquired;

    // Has the player reflected on this memory (examined it in the UI)
    public bool hasBeenReflectedOn = false;

    // How vivid is this memory? Fades slightly over (game) time.
    // 1 = fresh and clear. 0 = nearly forgotten.
    // Used later by IdentitySystem and visual presentation
    [Range(0f, 1f)]
    public float vividness = 1f;

    // -------------------------------------------------------
    // CONSTRUCTOR
    // Called when a memory is first kept
    // -------------------------------------------------------
    public MemoryInstance(MemoryData sourceData, float currentGameTime)
    {
        data = sourceData;
        timeAcquired = currentGameTime;
        vividness = 1f;
        hasBeenReflectedOn = false;
    }

    // -------------------------------------------------------
    // CONVENIENCE PROPERTIES
    // Shorthand so other scripts don't have to drill into .data
    // -------------------------------------------------------
    public string Title => data.memoryTitle;
    public string Description => data.memoryDescription;
    public MemoryCategory Category => data.category;
    public float EmotionalWeight => data.emotionalWeight;
    public Color MemoryColour => data.memoryColour;
    public Sprite Icon => data.memoryIcon;
}