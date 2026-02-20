// MemoryCardUI.cs
// A single memory card in the reflection screen.
// Displays the memory's title, colour, and selection state.
// Attach to the MemoryCard prefab.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MemoryCardUI : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES — set up in the Prefab
    // -------------------------------------------------------
    [Header("References")]
    public Image backgroundImage;
    public Image accentBar;         // Left-side coloured bar
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI categoryText;
    public Button button;

    // -------------------------------------------------------
    // VISUALS
    // -------------------------------------------------------
    [Header("Selection")]
    public Color selectedBorderColour = new Color(1f, 0.95f, 0.8f, 1f);
    public Color defaultBorderColour  = new Color(1f, 1f, 1f, 0.05f);
    public Outline outline;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    public MemoryInstance BoundMemory { get; private set; }
    private Action<MemoryInstance> onSelected;

    // -------------------------------------------------------
    // INITIALISE
    // Called by MemoryReflectUI when spawning cards
    // -------------------------------------------------------
    public void Initialise(MemoryInstance memory, Action<MemoryInstance> selectionCallback)
    {
        BoundMemory = memory;
        onSelected = selectionCallback;

        titleText.text = memory.Title;
        categoryText.text = memory.Category.ToString().ToUpper();

        // Tint the accent bar with the memory's colour
        accentBar.color = memory.MemoryColour;

        // Subtle background tint
        Color bgTint = memory.MemoryColour;
        bgTint.a = 0.08f;
        backgroundImage.color = bgTint;

        // Wire up click
        button.onClick.AddListener(() => onSelected?.Invoke(BoundMemory));

        SetSelected(false);
    }

    // -------------------------------------------------------
    // SELECTION STATE
    // -------------------------------------------------------
    public void SetSelected(bool selected)
    {
        if (outline != null)
            outline.effectColor = selected ? selectedBorderColour : defaultBorderColour;
    }
}