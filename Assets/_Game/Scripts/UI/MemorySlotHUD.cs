// MemorySlotHUD.cs
// A minimal HUD element showing memory slot usage.
// Deliberately understated — dots or small shapes, not a bar.
// The player should be AWARE of their slots, not managed by them.
// Attach to the MemorySlotHUD object under HUD.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MemorySlotHUD : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("References")]
    [Tooltip("Parent object that slot indicators are spawned inside")]
    public Transform slotContainer;

    [Tooltip("Prefab for a single slot dot — a simple Image")]
    public GameObject slotDotPrefab;

    // -------------------------------------------------------
    // VISUALS
    // -------------------------------------------------------
    [Header("Colours")]
    public Color filledColour = new Color(0.9f, 0.85f, 0.75f, 0.9f);   // warm cream
    public Color emptyColour  = new Color(0.9f, 0.85f, 0.75f, 0.2f);   // same, very faint

    [Header("Feel")]
    public float colourTransitionSpeed = 4f;

    // -------------------------------------------------------
    // PRIVATE
    // -------------------------------------------------------
    private List<Image> slotDots = new List<Image>();
    private List<Color> targetColours = new List<Color>();

    // -------------------------------------------------------
    // START
    // -------------------------------------------------------
    private void Start()
    {
        BuildSlots();
        Refresh();
    }

    private void Update()
    {
        // Smoothly animate dot colours
        for (int i = 0; i < slotDots.Count; i++)
        {
            slotDots[i].color = Color.Lerp(
                slotDots[i].color,
                targetColours[i],
                colourTransitionSpeed * Time.deltaTime
            );
        }
    }

    // -------------------------------------------------------
    // BUILD — creates one dot per slot
    // -------------------------------------------------------
    private void BuildSlots()
    {
        // Clear any existing
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        slotDots.Clear();
        targetColours.Clear();

        int slotCount = MemorySystem.Instance != null
            ? MemorySystem.Instance.GetSlotCount()
            : 6;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject dot = Instantiate(slotDotPrefab, slotContainer);
            Image img = dot.GetComponent<Image>();
            img.color = emptyColour;
            slotDots.Add(img);
            targetColours.Add(emptyColour);
        }
    }

    // -------------------------------------------------------
    // REFRESH — called whenever memories change
    // -------------------------------------------------------
    public void Refresh()
    {
        if (MemorySystem.Instance == null) return;

        int used = MemorySystem.Instance.GetUsedSlots();

        for (int i = 0; i < slotDots.Count; i++)
        {
            targetColours[i] = i < used ? filledColour : emptyColour;
        }
    }
}