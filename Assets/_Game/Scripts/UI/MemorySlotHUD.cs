// MemorySlotHUD.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MemorySlotHUD : MonoBehaviour
{
    [Header("References")]
    public Transform slotContainer;
    public GameObject slotDotPrefab;

    [Header("Colours")]
    public Color filledColour = new Color(1f, 1f, 1f, 0.9f);
    public Color emptyColour  = new Color(1f, 1f, 1f, 0.2f);

    [Header("Feel")]
    public float colourTransitionSpeed = 4f;

    private List<Image> slotDots = new List<Image>();
    private List<Color> targetColours = new List<Color>();

    private void Start()
    {
        // Delay to ensure MemorySystem singleton is ready
        Invoke(nameof(InitialBuild), 0.15f);
    }

    private void InitialBuild()
    {
        BuildSlots();

        // Subscribe here instead of OnEnable — guarantees MemorySystem exists
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoriesChanged += Refresh;
            MemorySystem.Instance.OnMemoryKept += _ => Refresh();
            MemorySystem.Instance.OnMemoryForgotten += _ => Refresh();
        }
        else
        {
            Debug.LogWarning("[MemorySlotHUD] MemorySystem not found during InitialBuild");
        }

        Refresh();
    }

    private void OnDestroy()
    {
        // Clean up subscriptions when object is destroyed
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoriesChanged -= Refresh;
            MemorySystem.Instance.OnMemoryKept -= _ => Refresh();
            MemorySystem.Instance.OnMemoryForgotten -= _ => Refresh();
        }
    }

    private void Update()
    {
        for (int i = 0; i < slotDots.Count; i++)
        {
            slotDots[i].color = Color.Lerp(
                slotDots[i].color,
                targetColours[i],
                colourTransitionSpeed * Time.deltaTime
            );
        }
    }

    private void BuildSlots()
    {
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

    public void Refresh()
    {
        if (MemorySystem.Instance == null) return;
        if (slotDots.Count == 0) return;

        int used = MemorySystem.Instance.GetUsedSlots();

        for (int i = 0; i < slotDots.Count; i++)
        {
            targetColours[i] = i < used ? filledColour : emptyColour;
        }
    }
}