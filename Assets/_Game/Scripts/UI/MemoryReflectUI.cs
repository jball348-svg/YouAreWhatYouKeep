// MemoryReflectUI.cs
// The full memory reflection screen.
// Opened with Tab during normal play.
// Also opened automatically when slots are full and a new memory is offered.
// In "replace mode" the player MUST choose a memory to let go before closing.
// Attach to the MemoryReflect object under Screens.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MemoryReflectUI : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES — assign in Inspector
    // -------------------------------------------------------
    [Header("Layout")]
    public Transform memoryCardContainer;   // Where memory cards are spawned
    public GameObject memoryCardPrefab;     // The card template

    [Header("Detail Panel")]
    [Tooltip("Shows description of selected memory")]
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDescription;
    public TextMeshProUGUI detailCategory;
    public Button forgetButton;
    public GameObject detailPanel;         // Hide when nothing selected

    [Header("Replace Mode")]
    [Tooltip("Shown when slots are full and player must choose")]
    public GameObject replaceModeNotice;
    public TextMeshProUGUI replaceModeText;

    [Header("Header")]
    public TextMeshProUGUI slotCountText;  // "4 / 6 memories"
    public Button closeButton;

    [Header("Feel")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 4f;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private bool isVisible = false;
    private bool isReplaceMode = false;
    private MemoryData pendingMemory = null;   // Memory waiting to be kept
    private MemoryInstance selectedMemory = null;

    private List<MemoryCardUI> spawnedCards = new List<MemoryCardUI>();
    private float targetAlpha = 0f;

    // -------------------------------------------------------
    // AWAKE
    // -------------------------------------------------------
    private void Awake()
    {
        forgetButton.onClick.AddListener(OnForgetPressed);
        closeButton.onClick.AddListener(() => UIManager.Instance.CloseReflectScreen());
    }

    // -------------------------------------------------------
    // UPDATE — fade
    // -------------------------------------------------------
    private void Update()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    // -------------------------------------------------------
    // SHOW / HIDE
    // -------------------------------------------------------
    public void Show()
    {
        isVisible = true;
        isReplaceMode = false;
        pendingMemory = null;
        targetAlpha = 1f;
        gameObject.SetActive(true);

        replaceModeNotice.SetActive(false);
        detailPanel.SetActive(false);
        closeButton.gameObject.SetActive(true);

        RefreshMemories();
    }

    public void ShowInReplaceMode(MemoryData offeredMemory)
    {
        isVisible = true;
        isReplaceMode = true;
        pendingMemory = offeredMemory;
        targetAlpha = 1f;
        gameObject.SetActive(true);

        // Hide close button — player MUST make a choice
        closeButton.gameObject.SetActive(false);

        replaceModeNotice.SetActive(true);
        replaceModeText.text = $"You want to hold onto \"{offeredMemory.memoryTitle}\" — " +
                               $"but you have no room left.\n\nChoose a memory to let go.";

        detailPanel.SetActive(false);
        RefreshMemories();
    }

    public void Hide()
    {
        isVisible = false;
        targetAlpha = 0f;

        // Deactivate after fade — handled simply by leaving gameObject active
        // Full fade-out before deactivation handled via coroutine if needed later
    }

    // -------------------------------------------------------
    // REFRESH — rebuild all memory cards
    // -------------------------------------------------------
    public void RefreshMemories()
    {
        // Clear existing cards
        foreach (Transform child in memoryCardContainer)
            Destroy(child.gameObject);
        spawnedCards.Clear();

        if (MemorySystem.Instance == null) return;

        var memories = MemorySystem.Instance.GetAllMemories();

        // Update slot count header
        slotCountText.text = $"{memories.Count} / {MemorySystem.Instance.GetSlotCount()} memories kept";

        // Spawn a card for each held memory
        foreach (var memory in memories)
        {
            GameObject cardObj = Instantiate(memoryCardPrefab, memoryCardContainer);
            MemoryCardUI card = cardObj.GetComponent<MemoryCardUI>();
            card.Initialise(memory, OnCardSelected);
            spawnedCards.Add(card);
        }

        // Clear detail panel if selected memory no longer exists
        if (selectedMemory != null && !memories.Contains(selectedMemory))
        {
            selectedMemory = null;
            detailPanel.SetActive(false);
        }
    }

    // -------------------------------------------------------
    // CARD SELECTED
    // -------------------------------------------------------
    private void OnCardSelected(MemoryInstance memory)
    {
        selectedMemory = memory;

        detailPanel.SetActive(true);
        detailTitle.text = memory.Title;
        detailDescription.text = memory.Description;
        detailCategory.text = memory.Category.ToString().ToUpper();

        // In replace mode, the forget button reads differently
        forgetButton.GetComponentInChildren<TextMeshProUGUI>().text =
            isReplaceMode ? "Let this go" : "Forget this";

        // Mark as reflected on
        memory.hasBeenReflectedOn = true;

        // Highlight selected card
        foreach (var card in spawnedCards)
            card.SetSelected(card.BoundMemory == memory);
    }

    // -------------------------------------------------------
    // FORGET PRESSED
    // -------------------------------------------------------
private void OnForgetPressed()
{
    if (selectedMemory == null) return;

    if (isReplaceMode && pendingMemory != null)
    {
        MemorySystem.Instance.ReplaceMemory(selectedMemory, pendingMemory);
        selectedMemory = null;
        pendingMemory = null;
        isReplaceMode = false;
        UIManager.Instance.CloseReflectScreen();
    }
    else
    {
        // Forget and close — don't leave player on a broken state
        MemorySystem.Instance.ForgetMemory(selectedMemory);
        selectedMemory = null;
        UIManager.Instance.CloseReflectScreen();
    }
}
}