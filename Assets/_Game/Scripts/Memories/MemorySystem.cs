// MemorySystem.cs
// The central manager for everything memory-related.
// Lives in the _Persistent scene. Never unloads.
// Other systems ask this what memories the player holds,
// and it broadcasts events when memories are gained or lost.

using UnityEngine;
using System.Collections.Generic;
using System;

public class MemorySystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static MemorySystem Instance { get; private set; }

    // -------------------------------------------------------
    // CONFIGURATION
    // -------------------------------------------------------
    [Header("Memory Slots")]
    [Tooltip("How many memories the player can hold at once. 5-8 recommended.")]
    [Range(3, 10)]
    public int maxMemorySlots = 6;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private List<MemoryInstance> heldMemories = new List<MemoryInstance>();

    // Tracks game time — used to timestamp memories
    // We'll replace this with a proper TimeSystem in Phase 7
    private float gameTime = 0f;

    // -------------------------------------------------------
    // EVENTS
    // Other systems subscribe to these to react to memory changes
    // without MemorySystem needing to know about them
    // -------------------------------------------------------

    // Fired when a memory is successfully kept
    public event Action<MemoryInstance> OnMemoryKept;

    // Fired when a memory is let go (replaced or deliberately released)
    public event Action<MemoryInstance> OnMemoryForgotten;

    // Fired when player tries to keep a memory but slots are full
    // Passes the new memory being offered and current held memories
    public event Action<MemoryData, List<MemoryInstance>> OnMemorySlotsFull;

    // Fired any time the memory collection changes — UI listens to this
    public event Action OnMemoriesChanged;

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
    }

    private void Update()
    {
        // Tick game time — simple for now
        // Phase 7 will make this richer
        gameTime += Time.deltaTime;
    }

    // -------------------------------------------------------
    // PUBLIC: OFFER A MEMORY
    // Called by MomentTrigger when player encounters a moment.
    // Returns true if kept, false if slots full (triggers event instead).
    // -------------------------------------------------------
    public bool OfferMemory(MemoryData memoryData)
    {
        // Don't offer duplicates
        if (AlreadyHolding(memoryData))
        {
            Debug.Log($"[MemorySystem] Already holding: {memoryData.memoryTitle}");
            return false;
        }

        if (heldMemories.Count < maxMemorySlots)
        {
            // Slot available — keep it
            KeepMemory(memoryData);
            return true;
        }
        else
        {
            // Slots full — player must choose
            // Broadcast event so UI can present the choice
            OnMemorySlotsFull?.Invoke(memoryData, heldMemories);
            Debug.Log($"[MemorySystem] Slots full. Offering choice for: {memoryData.memoryTitle}");
            return false;
        }
    }

    // -------------------------------------------------------
    // PUBLIC: KEEP A MEMORY (directly)
    // Called internally or by UI after player confirms
    // -------------------------------------------------------
    public void KeepMemory(MemoryData memoryData)
    {
        var instance = new MemoryInstance(memoryData, gameTime);
        heldMemories.Add(instance);

        OnMemoryKept?.Invoke(instance);
        OnMemoriesChanged?.Invoke();


    }

    // -------------------------------------------------------
    // PUBLIC: REPLACE A MEMORY
    // Player is keeping something new by letting go of something old
    // Called by UI when player makes a choice at full capacity
    // -------------------------------------------------------
    public void ReplaceMemory(MemoryInstance toForget, MemoryData toKeep)
    {
        ForgetMemory(toForget);
        KeepMemory(toKeep);
    }

    // -------------------------------------------------------
    // PUBLIC: FORGET A MEMORY
    // -------------------------------------------------------
    public void ForgetMemory(MemoryInstance memory)
    {
        if (heldMemories.Contains(memory))
        {
            heldMemories.Remove(memory);

            OnMemoryForgotten?.Invoke(memory);
            OnMemoriesChanged?.Invoke();

                    }
    }

    // -------------------------------------------------------
    // PUBLIC: QUERIES
    // Other systems use these to read memory state
    // -------------------------------------------------------

    public List<MemoryInstance> GetAllMemories()
    {
        return new List<MemoryInstance>(heldMemories); // return a copy
    }

    public int GetSlotCount() => maxMemorySlots;
    public int GetUsedSlots() => heldMemories.Count;
    public bool HasFreeSlot() => heldMemories.Count < maxMemorySlots;
    public float GetGameTime() => gameTime;

    public bool AlreadyHolding(MemoryData data)
    {
        return heldMemories.Exists(m => m.data == data);
    }

    // Check if player has ANY memory of a certain category
    // Used later by IdentitySystem and World Echo
    public bool HasMemoryOfCategory(MemoryCategory category)
    {
        return heldMemories.Exists(m => m.Category == category);
    }

    // Get combined emotional weight of all held memories
    // Used later by IdentitySystem
    public float GetTotalEmotionalWeight()
    {
        float total = 0f;
        foreach (var m in heldMemories)
            total += m.EmotionalWeight * m.vividness;
        return total;
    }
}