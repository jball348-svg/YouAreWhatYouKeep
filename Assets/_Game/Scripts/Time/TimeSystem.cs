// TimeSystem.cs
// The game's clock. Replaces the simple gameTime float in MemorySystem.
// Tracks time of day, days passed, and season.
// Ticks memory vividness down over time — old memories fade.
// Fires events when significant time boundaries are crossed.
//
// DESIGN INTENT:
// Time passes gently. A full game cycle is roughly 60-90 minutes real time.
// The player feels time through world changes, not countdowns.
// Aging is texture, not a penalty.
//
// TIME SCALE:
// 1 real second = timeScale game seconds (default 60)
// So 1 real minute = 1 game hour at default scale
// Full day = 24 game hours = 24 real minutes
// Season changes every 7 game days = ~2.8 real hours
// (adjust timeScale to taste)

using UnityEngine;
using System;
using System.Collections.Generic;

public class TimeSystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static TimeSystem Instance { get; private set; }

    // -------------------------------------------------------
    // TIME CONFIGURATION
    // -------------------------------------------------------
    [Header("Time Scale")]
    [Tooltip("How many game seconds pass per real second. 60 = 1 game hour per real minute.")]
    public float timeScale = 60f;

    [Tooltip("Starting hour of day (0-24)")]
    [Range(0f, 24f)]
    public float startingHour = 8f;

    // -------------------------------------------------------
    // VIVIDNESS DECAY
    // -------------------------------------------------------
    [Header("Memory Vividness")]
    [Tooltip("How much vividness a memory loses per game hour")]
    [Range(0f, 0.1f)]
    public float vividnessDecayPerHour = 0.02f;

    [Tooltip("Memories never fade below this vividness — some things stay with you")]
    [Range(0f, 0.5f)]
    public float minimumVividness = 0.15f;

    // -------------------------------------------------------
    // SEASON CONFIGURATION
    // -------------------------------------------------------
    [Header("Seasons")]
    [Tooltip("How many game days per season")]
    public int daysPerSeason = 7;

    // -------------------------------------------------------
    // EVENTS
    // -------------------------------------------------------
    public event Action<float> OnHourChanged;       // fires each new game hour
    public event Action<int> OnDayChanged;          // fires each new game day
    public event Action<Season> OnSeasonChanged;    // fires each new season
    public event Action<float> OnTimeOfDayUpdated;  // fires every frame (0-1 value)

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    public float CurrentHour { get; private set; }
    public int CurrentDay { get; private set; }
    public Season CurrentSeason { get; private set; }

    // 0-1 value representing position in day — useful for lighting
    public float DayProgress => CurrentHour / 24f;

    private float lastHour;
    private int lastDay;
    private Season lastSeason;

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

        CurrentHour = startingHour;
        CurrentDay = 1;
        CurrentSeason = Season.Spring;

        lastHour = Mathf.Floor(startingHour);
        lastDay = 1;
        lastSeason = Season.Spring;
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameManager.GameState.Ending)
            return; // time stops at the ending

        // Advance time
        float gameDeltaTime = Time.deltaTime * timeScale;
        CurrentHour += gameDeltaTime / 3600f; // convert seconds to hours

        // Roll over days
if (CurrentHour >= 24f)
{
    CurrentHour -= 24f;
    CurrentDay++;
    lastDay = CurrentDay;     // ← add this line
    OnDayChanged?.Invoke(CurrentDay);
    Debug.Log($"[TimeSystem] Day {CurrentDay} begins");
}

        // Check hour boundary
        float currentHourFloor = Mathf.Floor(CurrentHour);
        if (currentHourFloor != lastHour)
        {
            lastHour = currentHourFloor;
            OnHourChanged?.Invoke(currentHourFloor);
            TickVividnessDecay();
        }

        // Check season boundary
        Season newSeason = CalculateSeason();
        if (newSeason != lastSeason)
        {
            lastSeason = newSeason;
            CurrentSeason = newSeason;
            OnSeasonChanged?.Invoke(newSeason);
            Debug.Log($"[TimeSystem] Season changed to {newSeason}");
        }

        // Broadcast continuous time value for lighting systems
        OnTimeOfDayUpdated?.Invoke(DayProgress);
    }

    // -------------------------------------------------------
    // VIVIDNESS DECAY
    // Called each game hour — old memories gently fade
    // -------------------------------------------------------
    private void TickVividnessDecay()
    {
        if (MemorySystem.Instance == null) return;

        var memories = MemorySystem.Instance.GetAllMemories();

        foreach (var memory in memories)
        {
            float newVividness = Mathf.Max(
                minimumVividness,
                memory.vividness - vividnessDecayPerHour
            );

            if (!Mathf.Approximately(memory.vividness, newVividness))
            {
                memory.vividness = newVividness;
                Debug.Log($"[TimeSystem] {memory.Title} vividness: " +
                          $"{newVividness:F2}");
            }
        }

        // Notify systems that memories have changed
        // (vividness affects identity weights and colour blending)
        MemorySystem.Instance.NotifyMemoriesChanged();
    }

    // -------------------------------------------------------
    // SEASON CALCULATION
    // -------------------------------------------------------
    private Season CalculateSeason()
    {
        int seasonIndex = ((CurrentDay - 1) / daysPerSeason) % 4;
        return (Season)seasonIndex;
    }

    // -------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------

    // Get time as a formatted string — for UI or ending narration
    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(CurrentHour);
        int minutes = Mathf.FloorToInt((CurrentHour - hours) * 60f);
        return $"Day {CurrentDay}, {hours:00}:{minutes:00}";
    }

    // Is it currently night? Used by WorldEchoSystem
    public bool IsNight()
    {
        return CurrentHour < 6f || CurrentHour > 20f;
    }

    // Is it golden hour? Used by EmotionalResponseSystem
    public bool IsGoldenHour()
    {
        return (CurrentHour >= 6f && CurrentHour <= 8f) ||
               (CurrentHour >= 17f && CurrentHour <= 19f);
    }

    // Normalised time of day for lighting (0 = midnight, 0.5 = noon, 1 = midnight)
    public float GetNormalisedTimeOfDay()
    {
        return DayProgress;
    }
}

// -------------------------------------------------------
// SEASON ENUM
// -------------------------------------------------------
public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}