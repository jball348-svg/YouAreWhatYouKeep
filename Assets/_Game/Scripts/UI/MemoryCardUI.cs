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

    accentBar.color = memory.MemoryColour;

    Color bgTint = memory.MemoryColour;
    bgTint.a = 0.08f;
    backgroundImage.color = bgTint;

    button.onClick.AddListener(() => onSelected?.Invoke(BoundMemory));

    // Apply vividness to card appearance
    ApplyVividness(memory.vividness);

    SetSelected(false);
}

private void ApplyVividness(float vividness)
{
    // Fade text opacity with vividness
    // Fresh memory: full opacity. Old faded memory: 50% opacity.
    float alpha = Mathf.Lerp(0.5f, 1f, vividness);

    Color titleCol = titleText.color;
    titleCol.a = alpha;
    titleText.color = titleCol;

    Color catCol = categoryText.color;
    catCol.a = alpha * 0.7f;
    categoryText.color = catCol;

    // Accent bar also fades slightly
    Color accentCol = accentBar.color;
    accentCol.a = Mathf.Lerp(0.4f, 1f, vividness);
    accentBar.color = accentCol;
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