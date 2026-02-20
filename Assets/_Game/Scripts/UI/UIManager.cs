// UIManager.cs
// Central UI controller. Lives on the UICanvas object.
// Listens to GameManager and MemorySystem events.
// Tells individual UI panels to show or hide.
// Nothing else should be directly activating/deactivating UI panels.

using UnityEngine;

public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------
    public static UIManager Instance { get; private set; }

    // -------------------------------------------------------
    // PANEL REFERENCES
    // Drag these in from the Inspector
    // -------------------------------------------------------
    [Header("Panels")]
    public MomentPromptUI momentPrompt;
    public MemorySlotHUD memorySlotHUD;
    public MemoryReflectUI memoryReflectUI;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private bool isReflecting = false;

    // Input
    private PlayerInputActions inputActions;

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

        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Reflect.performed += _ => ToggleReflectScreen();

        // Listen to memory system events
        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoriesChanged += OnMemoriesChanged;
            MemorySystem.Instance.OnMemorySlotsFull += OnMemorySlotsFull;
        }
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.Reflect.performed -= _ => ToggleReflectScreen();

        if (MemorySystem.Instance != null)
        {
            MemorySystem.Instance.OnMemoriesChanged -= OnMemoriesChanged;
            MemorySystem.Instance.OnMemorySlotsFull -= OnMemorySlotsFull;
        }
    }

    private void Start()
    {
        // Initial state — everything hidden except HUD
        memoryReflectUI.Hide();
        momentPrompt.Hide();
        memorySlotHUD.Refresh();
    }

    // -------------------------------------------------------
    // REFLECT SCREEN TOGGLE
    // -------------------------------------------------------
    private void ToggleReflectScreen()
    {
        if (!GameManager.Instance.IsPlaying() && !isReflecting) return;

        isReflecting = !isReflecting;

        if (isReflecting)
        {
            memoryReflectUI.Show();
            GameManager.Instance.SetGameState(GameManager.GameState.Reflecting);

            // Unlock cursor so player can click memories
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            memoryReflectUI.Hide();
            GameManager.Instance.SetGameState(GameManager.GameState.Playing);

            // Re-lock cursor for first-person play
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // -------------------------------------------------------
    // MEMORY SYSTEM CALLBACKS
    // -------------------------------------------------------
    private void OnMemoriesChanged()
    {
        memorySlotHUD.Refresh();

        // If reflect screen is open, refresh it too
        if (isReflecting)
            memoryReflectUI.RefreshMemories();
    }

    private void OnMemorySlotsFull(MemoryData offered, System.Collections.Generic.List<MemoryInstance> held)
    {
        // Open the reflect screen in "replace mode"
        // Player must choose what to forget to make room
        isReflecting = true;
        memoryReflectUI.ShowInReplaceMode(offered);
        GameManager.Instance.SetGameState(GameManager.GameState.Reflecting);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // -------------------------------------------------------
    // PUBLIC — called by MomentTrigger world objects
    // -------------------------------------------------------
    public void ShowMomentPrompt(string text)
    {
        momentPrompt.Show(text);
    }

    public void HideMomentPrompt()
    {
        momentPrompt.Hide();
    }

    // -------------------------------------------------------
    // PUBLIC — close reflect screen from a button
    // -------------------------------------------------------
    public void CloseReflectScreen()
    {
        if (isReflecting)
            ToggleReflectScreen();
    }
}