// EmotionalResponseSystem.cs
// Drives post processing in response to the player's memory state.
// The world's colour, light, and atmosphere reflect what the player has lived.
//
// ARCHITECTURE:
// - Reads MemorySystem on change events
// - Calculates a target post processing state from held memories
// - Smoothly interpolates the actual post processing toward that target
// - Also responds to immediate moment triggers via TriggerMomentResponse()
//
// POST PROCESSING TARGETS:
// - Color Adjustments: saturation, colour filter tint, contrast
// - Bloom: intensity
// - Vignette: intensity
//
// DEPENDENCIES:
// - Requires the GlobalPostProcessVolume prefab in the _Persistent scene
// - Requires URP post processing (com.unity.render-pipelines.universal)
// - MemoryData.worldTintContribution drives the colour tint per memory

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class EmotionalResponseSystem : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static EmotionalResponseSystem Instance { get; private set; }

    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("References")]
    [Tooltip("The GlobalPostProcessVolume from the _Persistent scene")]
    public Volume postProcessVolume;

    // -------------------------------------------------------
    // BASE STATE — what the world looks like with no memories
    // -------------------------------------------------------
    [Header("Base World State (no memories)")]
    public float baseSaturation = -15f;      // slightly desaturated
    public float baseContrast = 5f;
    public float baseBloomIntensity = 0.3f;
    public float baseVignetteIntensity = 0.25f;
    public Color baseColourFilter = Color.white;

    // -------------------------------------------------------
    // PEAK STATE — world at maximum emotional weight
    // -------------------------------------------------------
    [Header("Peak World State (all memories full)")]
    public float peakSaturation = 20f;       // richer, warmer
    public float peakContrast = 15f;
    public float peakBloomIntensity = 0.8f;
    public float peakVignetteIntensity = 0.35f;

    // -------------------------------------------------------
    // MOMENT RESPONSE — immediate flash when memory is taken
    // -------------------------------------------------------
    [Header("Moment Response")]
    [Tooltip("Bloom spike when a memory is kept")]
    public float momentBloomSpike = 1.5f;

    [Tooltip("How long the bloom spike lasts")]
    public float momentBloomDuration = 1.8f;

    [Tooltip("Time dilation when a moment is taken — subtle slowdown")]
    [Range(0.5f, 1f)]
    public float momentTimeDilation = 0.75f;

    [Tooltip("How long time dilation lasts")]
    public float momentTimeDilationDuration = 0.8f;

    [Header("Feel")]
    public float transitionSpeed = 0.8f;    // how fast world state shifts

    // -------------------------------------------------------
    // PRIVATE — post processing component references
    // -------------------------------------------------------
    private ColorAdjustments colorAdjustments;
    private Bloom bloom;
    private Vignette vignette;

    // Current target values (interpolated toward)
    private float targetSaturation;
    private float targetContrast;
    private float targetBloomIntensity;
    private float targetVignetteIntensity;
    private Color targetColourFilter;

    // Whether a moment response is currently playing
    private bool momentResponseActive = false;

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
        // Get post processing component references
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out bloom);
            postProcessVolume.profile.TryGet(out vignette);
        }
        else
        {
            Debug.LogWarning("[EmotionalResponseSystem] No post process volume assigned.");
        }

        // Set initial state
        SetToBaseState();

        // Subscribe to memory events
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoryKept += OnMemoryKept;
            MemorySystem.Instance.OnMemoryForgotten += _ => RecalculateWorldState();
        }
    }

    private void OnDestroy()
    {
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoryKept -= OnMemoryKept;
            MemorySystem.Instance.OnMemoryForgotten -= _ => RecalculateWorldState();
        }
    }

    // -------------------------------------------------------
    // UPDATE — smooth interpolation toward target state
    // -------------------------------------------------------
    private void Update()
    {
        if (momentResponseActive) return; // moment response controls PP directly

        float speed = transitionSpeed * Time.deltaTime;

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(
                colorAdjustments.saturation.value, targetSaturation, speed);

            colorAdjustments.contrast.value = Mathf.Lerp(
                colorAdjustments.contrast.value, targetContrast, speed);

            colorAdjustments.colorFilter.value = Color.Lerp(
                colorAdjustments.colorFilter.value, targetColourFilter, speed);
        }

        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(
                bloom.intensity.value, targetBloomIntensity, speed);
        }

        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value, targetVignetteIntensity, speed);
        }
    }

    // -------------------------------------------------------
    // MEMORY EVENTS
    // -------------------------------------------------------
    private void OnMemoryKept(MemoryInstance memory)
    {
        RecalculateWorldState();
        TriggerMomentResponse(memory);
    }

    // -------------------------------------------------------
    // RECALCULATE WORLD STATE
    // Called whenever memories change.
    // Derives target post processing values from current memory set.
    // -------------------------------------------------------
    private void RecalculateWorldState()
    {
        if (MemorySystem.Instance == null) return;

        var memories = MemorySystem.Instance.GetAllMemories();
        int slotCount = MemorySystem.Instance.GetSlotCount();

        if (memories.Count == 0)
        {
            SetToBaseState();
            return;
        }

        // How full are the slots? 0-1
        float fullness = (float)memories.Count / slotCount;

        // Blend saturation and contrast toward peak based on fullness
        targetSaturation = Mathf.Lerp(baseSaturation, peakSaturation, fullness);
        targetContrast = Mathf.Lerp(baseContrast, peakContrast, fullness);
        targetBloomIntensity = Mathf.Lerp(baseBloomIntensity, peakBloomIntensity, fullness);
        targetVignetteIntensity = Mathf.Lerp(
            baseVignetteIntensity, peakVignetteIntensity, fullness);

        // Colour filter: blend all memory tints together weighted by emotional weight
        Color blendedTint = Color.white;
        float totalWeight = 0f;

        foreach (var memory in memories)
        {
            float weight = memory.EmotionalWeight * memory.vividness;
            blendedTint = Color.Lerp(blendedTint, memory.data.worldTintContribution, 
                weight / (totalWeight + weight + 0.001f));
            totalWeight += weight;
        }

        targetColourFilter = blendedTint;
    }

    // -------------------------------------------------------
    // MOMENT RESPONSE
    // Immediate reaction when a memory is taken —
    // bloom spike + subtle time dilation
    // -------------------------------------------------------
    private void TriggerMomentResponse(MemoryInstance memory)
    {
        StartCoroutine(MomentResponseCoroutine(memory));
    }

    private IEnumerator MomentResponseCoroutine(MemoryInstance memory)
    {
        momentResponseActive = true;

        // Time dilation — the world briefly slows
        Time.timeScale = momentTimeDilation;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Bloom spike
        float startBloom = bloom != null ? bloom.intensity.value : baseBloomIntensity;
        float elapsed = 0f;

        while (elapsed < momentBloomDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled — works during time dilation
            float t = elapsed / momentBloomDuration;

            // Spike up then decay — feels like a breath
            float curve = Mathf.Sin(t * Mathf.PI);
            if (bloom != null)
                bloom.intensity.value = startBloom + (momentBloomSpike * curve);

            // Restore time scale during first portion
            if (elapsed >= momentTimeDilationDuration)
            {
                Time.timeScale = Mathf.Lerp(
                    momentTimeDilation, 1f,
                    (elapsed - momentTimeDilationDuration) /
                    (momentBloomDuration - momentTimeDilationDuration)
                );
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }

            yield return null;
        }

        // Restore
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        momentResponseActive = false;

        // Now let Update() smoothly handle the rest
        RecalculateWorldState();
    }

    // -------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------
    private void SetToBaseState()
    {
        targetSaturation = baseSaturation;
        targetContrast = baseContrast;
        targetBloomIntensity = baseBloomIntensity;
        targetVignetteIntensity = baseVignetteIntensity;
        targetColourFilter = baseColourFilter;
    }

    // Called by future systems (Phase 7 world echo, Phase 6 traits)
    public void PushEmotionalState(float saturationBoost, float bloomBoost, float duration)
    {
        StartCoroutine(PushStateCoroutine(saturationBoost, bloomBoost, duration));
    }

    private IEnumerator PushStateCoroutine(float satBoost, float bloomBoost, float duration)
    {
        float prevSat = targetSaturation;
        float prevBloom = targetBloomIntensity;

        targetSaturation += satBoost;
        targetBloomIntensity += bloomBoost;

        yield return new WaitForSeconds(duration);

        targetSaturation = prevSat;
        targetBloomIntensity = prevBloom;
    }
}