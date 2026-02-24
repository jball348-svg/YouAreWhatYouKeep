// AudioManager.cs
// Manages all ambient audio as a layered system.
// Each memory category can unlock an audio layer that fades in.
// Layers fade out when those memories are forgotten.
// Lives in _Persistent scene, never unloads.
//
// ARCHITECTURE:
// - One AudioSource per layer, all on this object
// - Layers have target volumes, Update() smoothly fades toward them
// - MemorySystem events drive which layers are active
// - MomentResponseEffect calls TriggerMomentSting() for immediate reactions

using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static AudioManager Instance { get; private set; }

    // -------------------------------------------------------
    // BASE LAYER — always playing
    // -------------------------------------------------------
    [Header("Base Ambient")]
    [Tooltip("Always-on quiet background. The silence beneath everything.")]
    public AudioClip baseAmbientClip;

    [Range(0f, 1f)]
    public float baseAmbientVolume = 0.15f;

    // -------------------------------------------------------
    // MEMORY CATEGORY LAYERS
    // Each category has a clip and a target volume
    // -------------------------------------------------------
    [Header("Memory Category Layers")]
    public AudioLayerConfig[] categoryLayers;

    [Header("Feel")]
    [Tooltip("How slowly layers fade in and out. Higher = slower fade.")]
    public float fadeDuration = 3f;

    [Tooltip("Max volume any single layer can reach")]
    [Range(0f, 1f)]
    public float maxLayerVolume = 0.4f;

    // -------------------------------------------------------
    // MOMENT STING
    // A brief audio reaction when a memory is kept
    // -------------------------------------------------------
    [Header("Moment Reaction")]
    [Tooltip("Short subtle sound when a memory is taken")]
    public AudioClip momentStingClip;

    [Range(0f, 1f)]
    public float momentStingVolume = 0.6f;

    // -------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------
    private AudioSource baseSource;
    private AudioSource momentStingSource;
    private Dictionary<MemoryCategory, AudioSource> layerSources
        = new Dictionary<MemoryCategory, AudioSource>();
    private Dictionary<MemoryCategory, float> targetVolumes
        = new Dictionary<MemoryCategory, float>();
    private Dictionary<MemoryCategory, float> currentVolumes
        = new Dictionary<MemoryCategory, float>();

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

    private void Start()
    {
        SetupAudioSources();
        SubscribeToMemoryEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromMemoryEvents();
    }

    // -------------------------------------------------------
    // SETUP
    // -------------------------------------------------------
    private void SetupAudioSources()
    {
        // Base layer
        baseSource = CreateAudioSource("Layer_Base", baseAmbientClip, baseAmbientVolume);

        // Moment sting source (not looping)
        momentStingSource = gameObject.AddComponent<AudioSource>();
        momentStingSource.clip = momentStingClip;
        momentStingSource.loop = false;
        momentStingSource.playOnAwake = false;
        momentStingSource.volume = momentStingVolume;

        // One source per category layer
        foreach (var layer in categoryLayers)
        {
            if (layer.clip == null) continue;

            AudioSource source = CreateAudioSource(
                $"Layer_{layer.category}",
                layer.clip,
                0f  // start silent
            );

            layerSources[layer.category] = source;
            targetVolumes[layer.category] = 0f;
            currentVolumes[layer.category] = 0f;
        }
    }

    private AudioSource CreateAudioSource(string sourceName, AudioClip clip, float volume)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f; // 2D — ambient is non-positional

        if (clip != null)
            source.Play();

        return source;
    }

    // -------------------------------------------------------
    // SUBSCRIPTIONS
    // -------------------------------------------------------
    private void SubscribeToMemoryEvents()
    {
        if (MemorySystem.Instance == null) return;

        MemorySystem.Instance.OnMemoryKept += OnMemoryKept;
        MemorySystem.Instance.OnMemoryForgotten += OnMemoryForgotten;
    }

    private void UnsubscribeFromMemoryEvents()
    {
        if (MemorySystem.Instance == null) return;

        MemorySystem.Instance.OnMemoryKept -= OnMemoryKept;
        MemorySystem.Instance.OnMemoryForgotten -= OnMemoryForgotten;
    }

    // -------------------------------------------------------
    // MEMORY EVENTS
    // -------------------------------------------------------
    private void OnMemoryKept(MemoryInstance memory)
    {
        // Check if this category has a layer
        if (!layerSources.ContainsKey(memory.Category)) return;

        // Fade this layer in
        targetVolumes[memory.Category] = maxLayerVolume;

        // Play moment sting
        TriggerMomentSting();
    }

    private void OnMemoryForgotten(MemoryInstance memory)
    {
        if (!layerSources.ContainsKey(memory.Category)) return;

        // Check if player still has any memory of this category
        // If yes, keep layer playing. If no, fade it out.
        bool stillHasCategory = MemorySystem.Instance != null &&
            MemorySystem.Instance.HasMemoryOfCategory(memory.Category);

        if (!stillHasCategory)
            targetVolumes[memory.Category] = 0f;
    }

    // -------------------------------------------------------
    // UPDATE — smooth volume fades
    // -------------------------------------------------------
    private void Update()
    {
        float fadeSpeed = 1f / fadeDuration;

        foreach (var category in layerSources.Keys)
        {
            float target = targetVolumes[category];
            float current = layerSources[category].volume;
            float newVolume = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);

            layerSources[category].volume = newVolume;
            currentVolumes[category] = newVolume;
        }
    }

    // -------------------------------------------------------
    // PUBLIC — called by EmotionalResponseSystem or directly
    // -------------------------------------------------------
public void TriggerMomentSting()
{
    if (momentStingSource != null && momentStingClip != null)
    {
        // Set how many seconds into the clip to start
        // Tweak this value until it hits the right moment
        momentStingSource.time = 5f;
        momentStingSource.Play();
    }
}

    // Allows external systems to push a layer's volume temporarily
    // Used by EmotionalResponseSystem during intense moments
    public void BoostLayer(MemoryCategory category, float boostAmount, float duration)
    {
        StartCoroutine(BoostLayerCoroutine(category, boostAmount, duration));
    }

    private System.Collections.IEnumerator BoostLayerCoroutine(
        MemoryCategory category, float boostAmount, float duration)
    {
        if (!layerSources.ContainsKey(category)) yield break;

        float original = targetVolumes[category];
        targetVolumes[category] = Mathf.Min(original + boostAmount, 1f);

        yield return new WaitForSeconds(duration);

        targetVolumes[category] = original;
    }
}

// -------------------------------------------------------
// SUPPORTING DATA CLASS
// Shown as an array in Inspector — one entry per category
// -------------------------------------------------------
[System.Serializable]
public class AudioLayerConfig
{
    public MemoryCategory category;
    public AudioClip clip;
}