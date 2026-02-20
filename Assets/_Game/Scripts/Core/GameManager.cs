// GameManager.cs
// This script is the central nervous system of the game.
// It exists in the _Persistent scene and never unloads.
// Other systems talk to this to get information about the current game state.

using UnityEngine;

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------
    // SINGLETON PATTERN
    // This means there is only ever ONE GameManager in existence.
    // Any other script can reach it by writing: GameManager.Instance
    // -------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------
    // GAME STATE
    // Tracks where the player is in the overall experience
    // -------------------------------------------------------
    public enum GameState
    {
        Booting,        // Game is loading
        Playing,        // Normal play
        Reflecting,     // Player is reviewing memories
        Transitioning,  // Moving between locations
        Ending          // Final reflection sequence
    }

    public GameState CurrentState { get; private set; } = GameState.Booting;

    // -------------------------------------------------------
    // EVENTS
    // Other scripts can "subscribe" to these to be notified
    // when the game state changes — without needing to know
    // about each other. This keeps the code decoupled.
    // -------------------------------------------------------
    public event System.Action<GameState> OnGameStateChanged;

    // -------------------------------------------------------
    // AWAKE — runs once when this object first exists
    // -------------------------------------------------------
    private void Awake()
    {
        // Enforce singleton: if another GameManager exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // DontDestroyOnLoad means this object survives scene changes
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------
    // PUBLIC METHODS — other scripts call these
    // -------------------------------------------------------

    // Call this to change the game's state from anywhere
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;

        // Notify all listeners that state has changed
        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"[GameManager] State changed to: {newState}");
    }

    // Convenience check — is the player currently playing?
    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }
}